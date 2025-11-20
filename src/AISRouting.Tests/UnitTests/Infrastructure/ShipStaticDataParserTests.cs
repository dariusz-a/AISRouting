using AISRouting.Core.Models;
using AISRouting.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Text.Json;

namespace AISRouting.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class ShipStaticDataParserTests
    {
        private ILogger<ShipStaticDataParser> _mockLogger = null!;
        private ShipStaticDataParser _parser = null!;
        private string _testFolder = null!;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<ShipStaticDataParser>>();
            _parser = new ShipStaticDataParser(_mockLogger);

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
        public async Task LoadStaticDataAsync_WithValidJson_ReturnsShipStaticData()
        {
            // Arrange
            var mmsi = "205196000";
            var jsonPath = Path.Combine(_testFolder, $"{mmsi}.json");
            var jsonContent = new
            {
                MMSI = 205196000,
                Name = "Test Ship",
                Length = 100.5,
                Beam = 20.3,
                Draught = 8.5,
                TypeCode = 70,
                CallSign = "TEST1",
                IMO = 1234567
            };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(jsonContent));

            // Act
            var result = await _parser.LoadStaticDataAsync(_testFolder, mmsi);

            // Assert
            result.Should().NotBeNull();
            result!.MMSI.Should().Be(205196000);
            result.Name.Should().Be("Test Ship");
            result.Length.Should().Be(100.5);
            result.Beam.Should().Be(20.3);
            result.Draught.Should().Be(8.5);
            result.FolderPath.Should().Be(_testFolder);
        }

        [Test]
        public async Task LoadStaticDataAsync_WithMissingFile_ReturnsNull()
        {
            // Arrange
            var mmsi = "999999999";

            // Act
            var result = await _parser.LoadStaticDataAsync(_testFolder, mmsi);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoadStaticDataAsync_WithMalformedJson_ReturnsNull()
        {
            // Arrange
            var mmsi = "123456789";
            var jsonPath = Path.Combine(_testFolder, $"{mmsi}.json");
            File.WriteAllText(jsonPath, "{ invalid json content }");

            // Act
            var result = await _parser.LoadStaticDataAsync(_testFolder, mmsi);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoadStaticDataAsync_SetsFolderPathProperty()
        {
            // Arrange
            var mmsi = "111111111";
            var jsonPath = Path.Combine(_testFolder, $"{mmsi}.json");
            var jsonContent = new { MMSI = 111111111 };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(jsonContent));

            // Act
            var result = await _parser.LoadStaticDataAsync(_testFolder, mmsi);

            // Assert
            result.Should().NotBeNull();
            result!.FolderPath.Should().Be(_testFolder);
        }
    }
}
