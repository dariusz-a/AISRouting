using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.IO;
using AISRouting.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.IntegrationTests
{
    [TestFixture]
    public class InputFolderScanningTests
    {
        private string _testDataPath = null!;
        private ILogger<SourceDataScanner> _scannerLogger = null!;
        private ILogger<ShipStaticDataParser> _parserLogger = null!;
        private IShipStaticDataLoader _staticLoader = null!;
        private ISourceDataScanner _scanner = null!;

        [SetUp]
        public void SetUp()
        {
            // Use the TestData folder with actual test files
            _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");

            _scannerLogger = Substitute.For<ILogger<SourceDataScanner>>();
            _parserLogger = Substitute.For<ILogger<ShipStaticDataParser>>();
            _staticLoader = new ShipStaticDataParser(_parserLogger);
            _scanner = new SourceDataScanner(_staticLoader, _scannerLogger);
        }

        [Test]
        public async Task ScanInputFolder_WithRealTestData_ReturnsVesselWithCorrectData()
        {
            // Arrange
            if (!Directory.Exists(_testDataPath))
            {
                Assert.Inconclusive("Test data folder not found. Skipping integration test.");
                return;
            }

            // Act
            var vessels = await _scanner.ScanInputFolderAsync(_testDataPath);

            // Assert
            vessels.Should().NotBeEmpty();
            var vessel = vessels.FirstOrDefault(v => v.MMSI == 205196000);
            vessel.Should().NotBeNull();
            vessel!.Name.Should().NotBeNullOrEmpty();
            vessel.MinDate.Should().Be(new DateTime(2025, 3, 15));
            vessel.MaxDate.Should().Be(new DateTime(2025, 3, 15));
        }

        [Test]
        public async Task ScanInputFolder_WithRealTestData_LoadsStaticDataFromJson()
        {
            // Arrange
            if (!Directory.Exists(_testDataPath))
            {
                Assert.Inconclusive("Test data folder not found. Skipping integration test.");
                return;
            }

            // Act
            var vessels = await _scanner.ScanInputFolderAsync(_testDataPath);

            // Assert
            var vessel = vessels.FirstOrDefault(v => v.MMSI == 205196000);
            vessel.Should().NotBeNull();
            vessel!.DisplayName.Should().NotBe("Vessel 205196000", "Static data should be loaded from JSON");
        }

        [Test]
        public async Task ScanEmptyFolder_ReturnsEmptyCollection()
        {
            // Arrange
            var emptyFolder = Path.Combine(Path.GetTempPath(), $"EmptyTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(emptyFolder);

            try
            {
                // Act
                var vessels = await _scanner.ScanInputFolderAsync(emptyFolder);

                // Assert
                vessels.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(emptyFolder);
            }
        }
    }
}
