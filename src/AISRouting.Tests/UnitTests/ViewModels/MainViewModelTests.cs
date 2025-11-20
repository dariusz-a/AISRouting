using AISRouting.App.WPF.ViewModels;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.UnitTests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        private ISourceDataScanner _mockScanner = null!;
        private IFolderDialogService _mockFolderDialog = null!;
        private IShipPositionLoader _mockPositionLoader = null!;
        private ITrackOptimizer _mockTrackOptimizer = null!;
        private IPermissionService _mockPermissionService = null!;
        private ILogger<MainViewModel> _mockLogger = null!;
        private ShipSelectionViewModel _mockShipSelectionViewModel = null!;
        private MainViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mockScanner = Substitute.For<ISourceDataScanner>();
            _mockFolderDialog = Substitute.For<IFolderDialogService>();
            _mockPositionLoader = Substitute.For<IShipPositionLoader>();
            _mockTrackOptimizer = Substitute.For<ITrackOptimizer>();
            _mockPermissionService = Substitute.For<IPermissionService>();
            _mockPermissionService.CanCreateTrack().Returns(true);
            _mockLogger = Substitute.For<ILogger<MainViewModel>>();
            var mockShipLogger = Substitute.For<ILogger<ShipSelectionViewModel>>();
            _mockShipSelectionViewModel = new ShipSelectionViewModel(_mockScanner, _mockFolderDialog, mockShipLogger);
            _viewModel = new MainViewModel(_mockScanner, _mockFolderDialog, _mockPositionLoader, _mockTrackOptimizer, _mockPermissionService, _mockShipSelectionViewModel, _mockLogger);
        }

        [Test]
        public async Task SelectInputFolderCommand_WithCancelledDialog_DoesNotScan()
        {
            // Arrange
            _mockFolderDialog.ShowFolderBrowser(Arg.Any<string?>()).Returns((string?)null);

            // Act
            await _viewModel.SelectInputFolderCommand.ExecuteAsync(null);

            // Assert
            _viewModel.InputFolderPath.Should().BeNull();
            await _mockScanner.DidNotReceive().ScanInputFolderAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SelectInputFolderCommand_WithValidFolder_PopulatesVessels()
        {
            // Arrange
            var testFolder = @"C:\TestFolder";
            var vessels = new List<ShipStaticData>
            {
                new ShipStaticData(205196000, testFolder) { Name = "Ship 1" },
                new ShipStaticData(123456789, testFolder) { Name = "Ship 2" }
            };

            _mockFolderDialog.ShowFolderBrowser(Arg.Any<string?>()).Returns(testFolder);
            _mockScanner.ScanInputFolderAsync(testFolder, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<ShipStaticData>>(vessels));

            // Act
            await _viewModel.SelectInputFolderCommand.ExecuteAsync(null);

            // Assert
            _viewModel.InputFolderPath.Should().Be(testFolder);
            _viewModel.AvailableVessels.Should().HaveCount(2);
            _viewModel.AvailableVessels[0].Name.Should().Be("Ship 1");
            _viewModel.AvailableVessels[1].Name.Should().Be("Ship 2");
            _viewModel.FolderErrorMessage.Should().BeNull();
        }

        [Test]
        public async Task SelectInputFolderCommand_WithEmptyFolder_ShowsWarning()
        {
            // Arrange
            var testFolder = @"C:\EmptyFolder";
            _mockFolderDialog.ShowFolderBrowser(Arg.Any<string?>()).Returns(testFolder);
            _mockScanner.ScanInputFolderAsync(testFolder, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<ShipStaticData>>(new List<ShipStaticData>()));

            // Act
            await _viewModel.SelectInputFolderCommand.ExecuteAsync(null);

            // Assert
            _viewModel.AvailableVessels.Should().BeEmpty();
            _viewModel.FolderErrorMessage.Should().Be("No vessels found in input root");
        }

        [Test]
        public async Task SelectInputFolderCommand_WithDirectoryNotFoundException_ShowsError()
        {
            // Arrange
            var testFolder = @"C:\NonExistent";
            _mockFolderDialog.ShowFolderBrowser(Arg.Any<string?>()).Returns(testFolder);
            _mockScanner.ScanInputFolderAsync(testFolder, Arg.Any<CancellationToken>())
                .Returns(Task.FromException<IEnumerable<ShipStaticData>>(new DirectoryNotFoundException()));

            // Act
            await _viewModel.SelectInputFolderCommand.ExecuteAsync(null);

            // Assert
            _viewModel.FolderErrorMessage.Should().Be("Input root not accessible");
        }

        [Test]
        public async Task SelectInputFolderCommand_SetsIsScanningDuringExecution()
        {
            // Arrange
            var testFolder = @"C:\TestFolder";
            var vessels = new List<ShipStaticData>();
            bool isScanningDuringExecution = false;

            _mockFolderDialog.ShowFolderBrowser(Arg.Any<string?>()).Returns(testFolder);
            _mockScanner.ScanInputFolderAsync(testFolder, Arg.Any<CancellationToken>())
                .Returns(Task.Run(async () =>
                {
                    await Task.Delay(10); // Simulate async work
                    isScanningDuringExecution = _viewModel.IsScanning;
                    return (IEnumerable<ShipStaticData>)vessels;
                }));

            // Act
            await _viewModel.SelectInputFolderCommand.ExecuteAsync(null);

            // Assert
            isScanningDuringExecution.Should().BeTrue("IsScanning should be true during execution");
            _viewModel.IsScanning.Should().BeFalse("IsScanning should be false after completion");
        }
    }
}
