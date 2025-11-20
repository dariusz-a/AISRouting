using System.Collections.ObjectModel;
using System.IO;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AISRouting.App.WPF.ViewModels
{
    /// <summary>
    /// Main view model orchestrating the AISRouting application.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISourceDataScanner _scanner;
        private readonly IFolderDialogService _folderDialog;
        private readonly IShipPositionLoader _positionLoader;
        private readonly ITrackOptimizer _trackOptimizer;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<MainViewModel> _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        [ObservableProperty]
        private string? _inputFolderPath;

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private string? _folderErrorMessage;

        [ObservableProperty]
        private ShipStaticData? _selectedVessel;

        [ObservableProperty]
        private TimeInterval _timeInterval;

        [ObservableProperty]
        private ObservableCollection<RouteWaypoint> _generatedWaypoints;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private string _warningMessage;

        [ObservableProperty]
        private string _trackCreationError;

        [ObservableProperty]
        private string _dataQualityMessage;

        [ObservableProperty]
        private string _createTrackTooltip;

        public ObservableCollection<ShipStaticData> AvailableVessels { get; } = new();

        public ShipSelectionViewModel ShipSelectionViewModel { get; }

        public MainViewModel(
            ISourceDataScanner scanner,
            IFolderDialogService folderDialog,
            IShipPositionLoader positionLoader,
            ITrackOptimizer trackOptimizer,
            IPermissionService permissionService,
            ShipSelectionViewModel shipSelectionViewModel,
            ILogger<MainViewModel> logger)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _folderDialog = folderDialog ?? throw new ArgumentNullException(nameof(folderDialog));
            _positionLoader = positionLoader ?? throw new ArgumentNullException(nameof(positionLoader));
            _trackOptimizer = trackOptimizer ?? throw new ArgumentNullException(nameof(trackOptimizer));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ShipSelectionViewModel = shipSelectionViewModel ?? throw new ArgumentNullException(nameof(shipSelectionViewModel));

            _timeInterval = new TimeInterval();
            _generatedWaypoints = new ObservableCollection<RouteWaypoint>();
            _statusMessage = string.Empty;
            _warningMessage = string.Empty;
            _trackCreationError = string.Empty;
            _dataQualityMessage = string.Empty;
            _createTrackTooltip = "Create optimized track from position data";

            // Wire up property synchronization
            ShipSelectionViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ShipSelectionViewModel.SelectedVessel))
                {
                    SelectedVessel = ShipSelectionViewModel.SelectedVessel;
                }
                else if (e.PropertyName == nameof(ShipSelectionViewModel.TimeInterval))
                {
                    TimeInterval = ShipSelectionViewModel.TimeInterval;
                }
                else if (e.PropertyName == nameof(ShipSelectionViewModel.InputFolderPath))
                {
                    InputFolderPath = ShipSelectionViewModel.InputFolderPath;
                }
                else if (e.PropertyName == nameof(ShipSelectionViewModel.AvailableVessels))
                {
                    AvailableVessels.Clear();
                    foreach (var vessel in ShipSelectionViewModel.AvailableVessels)
                    {
                        AvailableVessels.Add(vessel);
                    }
                    CreateTrackCommand.NotifyCanExecuteChanged();
                }
            };
        }

        [RelayCommand]
        private async Task SelectInputFolderAsync()
        {
            var folder = _folderDialog.ShowFolderBrowser(InputFolderPath);
            if (string.IsNullOrEmpty(folder))
            {
                _logger.LogDebug("Folder selection cancelled by user");
                return;
            }

            try
            {
                IsScanning = true;
                FolderErrorMessage = null;
                InputFolderPath = folder;

                var vessels = await _scanner.ScanInputFolderAsync(folder);

                AvailableVessels.Clear();
                foreach (var vessel in vessels)
                {
                    AvailableVessels.Add(vessel);
                }

                if (AvailableVessels.Count == 0)
                {
                    FolderErrorMessage = "No vessels found in input root";
                    _logger.LogWarning("No vessels found in input folder: {Folder}", folder);
                }
                else
                {
                    _logger.LogInformation("Loaded {Count} vessels from input folder", AvailableVessels.Count);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                FolderErrorMessage = "Input root not accessible";
                _logger.LogError(ex, "Failed to access input folder: {Folder}", folder);
            }
            catch (Exception ex)
            {
                FolderErrorMessage = "Error scanning input folder";
                _logger.LogError(ex, "Unexpected error scanning input folder: {Folder}", folder);
            }
            finally
            {
                IsScanning = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreateTrack))]
        private async Task CreateTrackAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                StatusMessage = "Loading position data...";
                WarningMessage = string.Empty;
                DataQualityMessage = string.Empty;

                var positions = await _positionLoader.LoadPositionsAsync(
                    (int)SelectedVessel!.MMSI,
                    TimeInterval.Start,
                    TimeInterval.Stop,
                    InputFolderPath!,
                    progress: new Progress<LoadProgress>(p =>
                    {
                        StatusMessage = $"Loading: {p.ProcessedFiles}/{p.TotalFiles} files, {p.RecordsLoaded} records";
                        if (p.SkippedRecords > 0)
                        {
                            WarningMessage = $"Some rows were ignored due to invalid format ({p.SkippedRecords} rows skipped)";
                        }
                    }),
                    cancellationToken: _cancellationTokenSource.Token
                );

                var positionList = positions.ToList();
                StatusMessage = "Optimizing track...";

                var waypoints = await _trackOptimizer.OptimizeTrackAsync(
                    positionList,
                    options: new OptimizationOptions { ToleranceMeters = 50 },
                    progress: new Progress<OptimizationProgress>(p =>
                    {
                        StatusMessage = $"Optimizing: {p.ProcessedPoints}/{p.TotalPoints} points";
                        
                        if (p.DefaultedHeadingCount > 0 || p.DefaultedSOGCount > 0)
                        {
                            var messages = new List<string>();
                            if (p.DefaultedHeadingCount > 0)
                                messages.Add($"{p.DefaultedHeadingCount} records had missing Heading");
                            if (p.DefaultedSOGCount > 0)
                                messages.Add($"{p.DefaultedSOGCount} records had missing SOG");
                            
                            DataQualityMessage = $"Data quality note: {string.Join(", ", messages)} (defaulted to 0)";
                        }
                    }),
                    cancellationToken: _cancellationTokenSource.Token
                );

                var waypointList = waypoints.ToList();
                GeneratedWaypoints = new ObservableCollection<RouteWaypoint>(waypointList);

                var positionCount = positionList.Count;
                var reductionPercent = positionCount > 0
                    ? (100.0 - (waypointList.Count * 100.0 / positionCount))
                    : 0;

                StatusMessage = $"Track created: {waypointList.Count} waypoints from {positionCount} records ({reductionPercent:F1}% reduction)";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled";
            }
            catch (FileNotFoundException ex)
            {
                StatusMessage = "Error: No position data files found for selected time range";
                _logger.LogWarning(ex, "CSV files not found for MMSI {MMSI}", SelectedVessel?.MMSI);
            }
            catch (DirectoryNotFoundException ex)
            {
                StatusMessage = "Error: Vessel folder not found";
                _logger.LogError(ex, "MMSI folder not found");
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _logger.LogWarning(ex, "Validation error in track creation");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, "Unexpected error in track creation");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private bool CanCreateTrack()
        {
            if (!_permissionService.CanCreateTrack())
            {
                CreateTrackTooltip = "Insufficient privileges";
                return false;
            }

            if (SelectedVessel == null)
            {
                TrackCreationError = "No ship selected";
                CreateTrackTooltip = "Create optimized track from position data";
                return false;
            }

            if (!TimeInterval.IsValid)
            {
                TrackCreationError = "Invalid time interval";
                CreateTrackTooltip = "Create optimized track from position data";
                return false;
            }

            if (string.IsNullOrEmpty(InputFolderPath))
            {
                TrackCreationError = "No input folder selected";
                CreateTrackTooltip = "Create optimized track from position data";
                return false;
            }

            if (AvailableVessels.Count == 0)
            {
                TrackCreationError = "No vessels found in input root";
                CreateTrackTooltip = "Create optimized track from position data";
                return false;
            }

            TrackCreationError = string.Empty;
            CreateTrackTooltip = "Create optimized track from position data";
            return true;
        }

        [RelayCommand]
        private void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancellation requested...";
        }

        partial void OnSelectedVesselChanged(ShipStaticData? value)
        {
            TrackCreationError = string.Empty;
            CreateTrackCommand.NotifyCanExecuteChanged();
        }

        partial void OnTimeIntervalChanged(TimeInterval value)
        {
            TrackCreationError = string.Empty;
            CreateTrackCommand.NotifyCanExecuteChanged();
        }
    }
}
