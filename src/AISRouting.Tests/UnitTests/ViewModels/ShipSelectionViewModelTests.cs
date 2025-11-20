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
    public class ShipSelectionViewModelTests
    {
        private ISourceDataScanner _mockScanner = null!;
        private IFolderDialogService _mockFolderDialog = null!;
        private ILogger<ShipSelectionViewModel> _mockLogger = null!;
        private ShipSelectionViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mockScanner = Substitute.For<ISourceDataScanner>();
            _mockFolderDialog = Substitute.For<IFolderDialogService>();
            _mockLogger = Substitute.For<ILogger<ShipSelectionViewModel>>();
            _viewModel = new ShipSelectionViewModel(_mockScanner, _mockFolderDialog, _mockLogger);
        }

        [Test]
        public void OnSelectedVesselChanged_SetsStopTimeTo235959()
        {
            // Arrange
            var minDate = new DateTime(2025, 3, 15, 0, 0, 0);
            var maxDate = new DateTime(2025, 3, 15, 0, 0, 0);
            var vessel = new ShipStaticData(205196000, @"C:\TestFolder")
            {
                Name = "Test Ship",
                MinDate = minDate,
                MaxDate = maxDate
            };

            // Act
            _viewModel.SelectedVessel = vessel;

            // Assert
            _viewModel.TimeInterval.Start.Should().Be(minDate);
            _viewModel.TimeInterval.Stop.Should().Be(new DateTime(2025, 3, 15, 23, 59, 59));
            _viewModel.StopTimeString.Should().Be("23:59:59");
        }

        [Test]
        public void OnSelectedVesselChanged_WithMultipleDays_SetsCorrectEndTime()
        {
            // Arrange
            var minDate = new DateTime(2025, 3, 10, 0, 0, 0);
            var maxDate = new DateTime(2025, 3, 20, 0, 0, 0);
            var vessel = new ShipStaticData(205196000, @"C:\TestFolder")
            {
                Name = "Test Ship",
                MinDate = minDate,
                MaxDate = maxDate
            };

            // Act
            _viewModel.SelectedVessel = vessel;

            // Assert
            _viewModel.TimeInterval.Start.Should().Be(minDate);
            _viewModel.TimeInterval.Stop.Should().Be(new DateTime(2025, 3, 20, 23, 59, 59));
        }

        [Test]
        public void OnStartDateChanged_RaisesTimeIntervalPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TimeInterval))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.StartDate = new DateTime(2025, 3, 15);

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Test]
        public void OnStopDateChanged_RaisesTimeIntervalPropertyChanged()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TimeInterval))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.StopDate = new DateTime(2025, 3, 20);

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Test]
        public void OnStartTimeStringChanged_UpdatesTimeInterval()
        {
            // Arrange
            _viewModel.TimeInterval.Start = new DateTime(2025, 3, 15, 0, 0, 0);
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TimeInterval))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.StartTimeString = "12:30:45";

            // Assert
            _viewModel.TimeInterval.Start.Should().Be(new DateTime(2025, 3, 15, 12, 30, 45));
            propertyChangedRaised.Should().BeTrue();
        }

        [Test]
        public void OnStopTimeStringChanged_UpdatesTimeInterval()
        {
            // Arrange
            _viewModel.TimeInterval.Stop = new DateTime(2025, 3, 15, 0, 0, 0);
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.TimeInterval))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.StopTimeString = "18:45:30";

            // Assert
            _viewModel.TimeInterval.Stop.Should().Be(new DateTime(2025, 3, 15, 18, 45, 30));
            propertyChangedRaised.Should().BeTrue();
        }

        [Test]
        public void OnStartDateChanged_PreservesTimeComponent()
        {
            // Arrange
            _viewModel.TimeInterval.Start = new DateTime(2025, 3, 15, 14, 30, 45);

            // Act
            _viewModel.StartDate = new DateTime(2025, 3, 20);

            // Assert
            _viewModel.TimeInterval.Start.Should().Be(new DateTime(2025, 3, 20, 14, 30, 45));
        }

        [Test]
        public void OnStopDateChanged_PreservesTimeComponent()
        {
            // Arrange
            _viewModel.TimeInterval.Stop = new DateTime(2025, 3, 15, 18, 45, 30);

            // Act
            _viewModel.StopDate = new DateTime(2025, 3, 25);

            // Assert
            _viewModel.TimeInterval.Stop.Should().Be(new DateTime(2025, 3, 25, 18, 45, 30));
        }

        [Test]
        public void ValidateTimeInterval_WithValidRange_ClearsValidationMessage()
        {
            // Arrange & Act
            _viewModel.TimeInterval.Start = new DateTime(2025, 3, 15, 10, 0, 0);
            _viewModel.TimeInterval.Stop = new DateTime(2025, 3, 15, 20, 0, 0);
            _viewModel.StartDate = new DateTime(2025, 3, 15);

            // Assert
            _viewModel.ValidationMessage.Should().BeNullOrEmpty();
        }

        [Test]
        public void ValidateTimeInterval_WithInvalidRange_SetsValidationMessage()
        {
            // Arrange
            _viewModel.TimeInterval.Start = new DateTime(2025, 3, 15, 20, 0, 0);
            _viewModel.TimeInterval.Stop = new DateTime(2025, 3, 15, 10, 0, 0);

            // Act
            _viewModel.StartDate = new DateTime(2025, 3, 15); // Trigger validation

            // Assert
            _viewModel.ValidationMessage.Should().Be("Invalid time range");
        }

        [Test]
        public void OnStartTimeStringChanged_WithInvalidFormat_DoesNotUpdateTimeInterval()
        {
            // Arrange
            var originalStart = new DateTime(2025, 3, 15, 10, 0, 0);
            _viewModel.TimeInterval.Start = originalStart;

            // Act
            _viewModel.StartTimeString = "invalid";

            // Assert
            _viewModel.TimeInterval.Start.Should().Be(originalStart);
        }

        [Test]
        public void OnStopTimeStringChanged_WithInvalidFormat_DoesNotUpdateTimeInterval()
        {
            // Arrange
            var originalStop = new DateTime(2025, 3, 15, 20, 0, 0);
            _viewModel.TimeInterval.Stop = originalStop;

            // Act
            _viewModel.StopTimeString = "not-a-time";

            // Assert
            _viewModel.TimeInterval.Stop.Should().Be(originalStop);
        }
    }
}
