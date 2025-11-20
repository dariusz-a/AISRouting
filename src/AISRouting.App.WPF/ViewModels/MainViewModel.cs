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
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private string? _inputFolderPath;

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private string? _folderErrorMessage;

        [ObservableProperty]
        private ShipStaticData? _selectedVessel;

        public ObservableCollection<ShipStaticData> AvailableVessels { get; } = new();

        public ShipSelectionViewModel ShipSelectionViewModel { get; }

        public MainViewModel(
            ISourceDataScanner scanner,
            IFolderDialogService folderDialog,
            ShipSelectionViewModel shipSelectionViewModel,
            ILogger<MainViewModel> logger)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _folderDialog = folderDialog ?? throw new ArgumentNullException(nameof(folderDialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ShipSelectionViewModel = shipSelectionViewModel ?? throw new ArgumentNullException(nameof(shipSelectionViewModel));
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
    }
}
