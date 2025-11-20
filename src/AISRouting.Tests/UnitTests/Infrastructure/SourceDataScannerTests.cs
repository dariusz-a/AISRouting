using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class SourceDataScannerTests
    {
        private IShipStaticDataLoader _mockStaticLoader = null!;
        private ILogger<SourceDataScanner> _mockLogger = null!;
        private SourceDataScanner _scanner = null!;
        private string _testFolder = null!;

        [SetUp]
        public void SetUp()
        {
            _mockStaticLoader = Substitute.For<IShipStaticDataLoader>();
            _mockLogger = Substitute.For<ILogger<SourceDataScanner>>();
            _scanner = new SourceDataScanner(_mockStaticLoader, _mockLogger);

            // Create temporary test folder
            _testFolder = Path.Combine(Path.GetTempPath(), $"AISRoutingTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testFolder))
            {
                Directory.Delete(_testFolder, true);
            }
        }

        [Test]
        public async Task ScanInputFolderAsync_WithValidFolder_ReturnsVessels()
        {
            // Arrange
            var mmsi = "205196000";
            var vesselFolder = Path.Combine(_testFolder, mmsi);
            Directory.CreateDirectory(vesselFolder);
            
            // Create CSV files with date format
            File.WriteAllText(Path.Combine(vesselFolder, "2025-03-15.csv"), "test");
            File.WriteAllText(Path.Combine(vesselFolder, "2025-03-16.csv"), "test");

            var staticData = new ShipStaticData
            {
                MMSI = 205196000,
                Name = "Test Vessel",
                FolderPath = vesselFolder
            };

            _mockStaticLoader.LoadStaticDataAsync(vesselFolder, mmsi, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ShipStaticData?>(staticData));

            // Act
            var result = await _scanner.ScanInputFolderAsync(_testFolder);

            // Assert
            result.Should().HaveCount(1);
            var vessel = result.First();
            vessel.MMSI.Should().Be(205196000);
            vessel.Name.Should().Be("Test Vessel");
            vessel.MinDate.Should().Be(new DateTime(2025, 3, 15));
            vessel.MaxDate.Should().Be(new DateTime(2025, 3, 16));
        }

        [Test]
        public async Task ScanInputFolderAsync_WithMissingStaticData_UsesMMSIFallback()
        {
            // Arrange
            var mmsi = "123456789";
            var vesselFolder = Path.Combine(_testFolder, mmsi);
            Directory.CreateDirectory(vesselFolder);
            File.WriteAllText(Path.Combine(vesselFolder, "2025-01-01.csv"), "test");

            _mockStaticLoader.LoadStaticDataAsync(vesselFolder, mmsi, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ShipStaticData?>(null));

            // Act
            var result = await _scanner.ScanInputFolderAsync(_testFolder);

            // Assert
            result.Should().HaveCount(1);
            var vessel = result.First();
            vessel.MMSI.Should().Be(123456789);
            vessel.DisplayName.Should().Be("Vessel 123456789");
        }

        [Test]
        public async Task ScanInputFolderAsync_WithNoCsvFiles_SetsDefaultDates()
        {
            // Arrange
            var mmsi = "999999999";
            var vesselFolder = Path.Combine(_testFolder, mmsi);
            Directory.CreateDirectory(vesselFolder);

            _mockStaticLoader.LoadStaticDataAsync(vesselFolder, mmsi, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ShipStaticData?>(null));

            // Act
            var result = await _scanner.ScanInputFolderAsync(_testFolder);

            // Assert
            result.Should().HaveCount(1);
            var vessel = result.First();
            vessel.MinDate.Should().Be(DateTime.MinValue);
            vessel.MaxDate.Should().Be(DateTime.MaxValue);
        }

        [Test]
        public void ScanInputFolderAsync_WithNonExistentFolder_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var nonExistentFolder = Path.Combine(_testFolder, "NonExistent");

            // Act & Assert
            Func<Task> act = async () => await _scanner.ScanInputFolderAsync(nonExistentFolder);
            act.Should().ThrowAsync<DirectoryNotFoundException>();
        }

        [Test]
        public async Task ScanInputFolderAsync_WithEmptyFolder_ReturnsEmptyList()
        {
            // Act
            var result = await _scanner.ScanInputFolderAsync(_testFolder);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
