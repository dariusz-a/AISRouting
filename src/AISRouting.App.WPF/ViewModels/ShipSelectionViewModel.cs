using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AISRouting.App.WPF.ViewModels
{
    public partial class ShipSelectionViewModel : ObservableObject
    {
        private readonly ISourceDataScanner _scanner;
        private readonly IFolderDialogService _folderDialog;
        private readonly ILogger<ShipSelectionViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<ShipStaticData> _availableVessels;

        [ObservableProperty]
        private ShipStaticData? _selectedVessel;

        [ObservableProperty]
        private TimeInterval _timeInterval;

        [ObservableProperty]
        private DateTime? _t0Date;

        [ObservableProperty]
        private string _t0DateString;

        [ObservableProperty]
        private string? _t0ValidationMessage;

        [ObservableProperty]
        private bool _isT0Valid;

        [ObservableProperty]
        private string _staticDataDisplay;

        [ObservableProperty]
        private string? _validationMessage;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isInputRootValid;

        [ObservableProperty]
        private string _inputFolderPath;

        [ObservableProperty]
        private string _startTimeString;

        [ObservableProperty]
        private string _stopTimeString;

        [ObservableProperty]
        private DateTime? _startDate;

        [ObservableProperty]
        private DateTime? _stopDate;

        public ShipSelectionViewModel(
            ISourceDataScanner scanner,
            IFolderDialogService folderDialog,
            ILogger<ShipSelectionViewModel> logger)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _folderDialog = folderDialog ?? throw new ArgumentNullException(nameof(folderDialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _availableVessels = new ObservableCollection<ShipStaticData>();
            _timeInterval = new TimeInterval();
            _staticDataDisplay = string.Empty;
            _inputFolderPath = string.Empty;
            _isInputRootValid = false;
            _startTimeString = "00:00:00";
            _stopTimeString = "00:00:00";
            _t0Date = null;
            _t0DateString = string.Empty;
            _t0ValidationMessage = null;
            _isT0Valid = true;
        }

        [RelayCommand]
        private async Task SelectInputFolder()
        {
            var folderPath = _folderDialog.ShowFolderBrowser();
            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                _logger.LogInformation("Selected input folder: {FolderPath}", folderPath);

                var vessels = await _scanner.ScanInputFolderAsync(folderPath);

                AvailableVessels.Clear();
                foreach (var vessel in vessels)
                {
                    AvailableVessels.Add(vessel);
                }

                InputFolderPath = folderPath;
                IsInputRootValid = true;
                ErrorMessage = null;

                _logger.LogInformation("Loaded {Count} vessels", AvailableVessels.Count);
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Input root not accessible");
                IsInputRootValid = false;
                ErrorMessage = $"Input root not accessible: {folderPath}";
                AvailableVessels.Clear();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to input folder");
                IsInputRootValid = false;
                ErrorMessage = $"Access denied: {folderPath}";
                AvailableVessels.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning input folder");
                IsInputRootValid = false;
                ErrorMessage = $"Error scanning folder: {ex.Message}";
                AvailableVessels.Clear();
            }
        }

        partial void OnSelectedVesselChanged(ShipStaticData? value)
        {
            if (value == null)
            {
                StaticDataDisplay = string.Empty;
                return;
            }

            StaticDataDisplay = FormatStaticData(value);

            // Update both TimeInterval and the separate date properties
            TimeInterval.Start = value.MinDate;
            TimeInterval.Stop = value.MaxDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            StartDate = TimeInterval.Start;
            StopDate = TimeInterval.Stop;

            // Update time strings
            StartTimeString = TimeInterval.Start.ToString("HH:mm:ss");
            StopTimeString = TimeInterval.Stop.ToString("HH:mm:ss");

            // Initialize T0 to the start date by default (user-editable)
            T0Date = TimeInterval.Start.Date;
            T0DateString = T0Date?.ToString("yyyy-MM-dd") ?? string.Empty;

            ValidateTimeInterval();

            _logger.LogInformation("Selected vessel: {MMSI} ({Name})", value.MMSI, value.DisplayName);
        }

        partial void OnStartDateChanged(DateTime? value)
        {
            if (value.HasValue)
            {
                // Preserve the time component from TimeInterval.Start
                var time = TimeInterval.Start.TimeOfDay;
                TimeInterval.Start = value.Value.Date + time;
                ValidateTimeInterval();
                OnPropertyChanged(nameof(TimeInterval));
            }
        }

        partial void OnStopDateChanged(DateTime? value)
        {
            if (value.HasValue)
            {
                // Preserve the time component from TimeInterval.Stop
                var time = TimeInterval.Stop.TimeOfDay;
                TimeInterval.Stop = value.Value.Date + time;
                ValidateTimeInterval();
                OnPropertyChanged(nameof(TimeInterval));
            }
        }

        partial void OnStartTimeStringChanged(string value)
        {
            if (DateTime.TryParseExact(value, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var time))
            {
                TimeInterval.Start = TimeInterval.Start.Date + time.TimeOfDay;
                ValidateTimeInterval();
                OnPropertyChanged(nameof(TimeInterval));
            }
        }

        partial void OnStopTimeStringChanged(string value)
        {
            if (DateTime.TryParseExact(value, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var time))
            {
                TimeInterval.Stop = TimeInterval.Stop.Date + time.TimeOfDay;
                ValidateTimeInterval();
                OnPropertyChanged(nameof(TimeInterval));
            }
        }

        partial void OnT0DateChanged(DateTime? value)
        {
            if (value.HasValue)
            {
                T0DateString = value.Value.ToString("yyyy-MM-dd");
                // Clear validation message when a valid DateTime is set
                T0ValidationMessage = null;
                IsT0Valid = true;
                OnPropertyChanged(nameof(T0Date));
            }
        }

        partial void OnT0DateStringChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                T0ValidationMessage = "T0 must be in format YYYY-MM-DD";
                IsT0Valid = false;
                return;
            }

            if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsed))
            {
                T0Date = parsed.Date;
                T0ValidationMessage = null;
                IsT0Valid = true;
            }
            else
            {
                T0ValidationMessage = "T0 must be in format YYYY-MM-DD";
                IsT0Valid = false;
            }
        }

        private string FormatStaticData(ShipStaticData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MMSI: {data.MMSI}");
            sb.AppendLine($"Name: {data.Name ?? "N/A"}");
            sb.AppendLine($"Length: {(data.Length.HasValue ? $"{data.Length.Value:F1} m" : "N/A")}");
            sb.AppendLine($"Beam: {(data.Beam.HasValue ? $"{data.Beam.Value:F1} m" : "N/A")}");
            sb.AppendLine($"Draught: {(data.Draught.HasValue ? $"{data.Draught.Value:F1} m" : "N/A")}");

            if (data.MinDate != DateTime.MinValue && data.MaxDate != DateTime.MinValue)
            {
                sb.AppendLine($"Available Date Range: {data.MinDate:yyyy-MM-dd} to {data.MaxDate:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine("Available Date Range: N/A");
            }

            return sb.ToString();
        }

        private void ValidateTimeInterval()
        {
            if (!TimeInterval.IsValid)
            {
                ValidationMessage = "Invalid time range";
            }
            else
            {
                ValidationMessage = null;
            }
        }
    }
}
