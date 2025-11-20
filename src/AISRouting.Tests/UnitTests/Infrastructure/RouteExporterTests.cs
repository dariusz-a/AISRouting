using System.Xml.Linq;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.UnitTests.Infrastructure
{
    [TestFixture]
    public class RouteExporterTests
    {
        private ILogger<RouteExporter> _mockLogger = null!;
        private IPathValidator _mockPathValidator = null!;
        private IFileConflictDialogService _mockConflictDialog = null!;
        private RouteExporter _exporter = null!;
        private string _testFolder = null!;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<RouteExporter>>();
            _mockPathValidator = Substitute.For<IPathValidator>();
            _mockConflictDialog = Substitute.For<IFileConflictDialogService>();
            _exporter = new RouteExporter(_mockLogger, _mockPathValidator, _mockConflictDialog);

            // Create temporary test folder
            _testFolder = Path.Combine(Path.GetTempPath(), $"AISRoutingExportTest_{Guid.NewGuid()}");
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
        public async Task ExportRouteAsync_WithValidWaypoints_CreatesXmlFile()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "test_route.xml");

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var xml = XDocument.Load(outputPath);
            xml.Root.Should().NotBeNull();
            xml.Root!.Name.LocalName.Should().Be("RouteTemplates");
            
            var routeTemplate = xml.Root.Element("RouteTemplate");
            routeTemplate.Should().NotBeNull();
            routeTemplate!.Attribute("Name")!.Value.Should().Be("205196000");
            
            var waypointElements = routeTemplate.Elements("WayPoint").ToList();
            waypointElements.Should().HaveCount(3);
        }

        [Test]
        public void ExportRouteAsync_WithEmptyWaypoints_ThrowsArgumentException()
        {
            // Arrange
            var emptyWaypoints = new List<RouteWaypoint>();
            var outputPath = Path.Combine(_testFolder, "test_route.xml");

            // Act & Assert
            var act = async () => await _exporter.ExportRouteAsync(emptyWaypoints, outputPath);
            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Waypoint list is empty*");
        }

        [Test]
        public void ExportRouteAsync_WithInvalidOutputPath_ThrowsException()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var invalidPath = Path.Combine(_testFolder, "nonexistent", "test_route.xml");
            
            _mockPathValidator
                .When(x => x.ValidateOutputFilePath(Arg.Any<string>()))
                .Do(x => throw new DirectoryNotFoundException("Directory not found"));

            // Act & Assert
            var act = async () => await _exporter.ExportRouteAsync(waypoints, invalidPath);
            act.Should().ThrowAsync<DirectoryNotFoundException>();
        }

        [Test]
        public async Task ExportRouteAsync_WithExistingFile_AndOverwriteChosen_OverwritesFile()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "test_route.xml");
            
            // Create existing file
            File.WriteAllText(outputPath, "existing content");
            
            _mockConflictDialog.ShowFileConflictDialog(outputPath)
                .Returns(FileConflictResolution.Overwrite);

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            var content = File.ReadAllText(outputPath);
            content.Should().Contain("RouteTemplates");
            content.Should().NotContain("existing content");
        }

        [Test]
        public async Task ExportRouteAsync_WithExistingFile_AndAppendSuffixChosen_CreatesNewFile()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "test_route.xml");
            
            // Create existing file
            File.WriteAllText(outputPath, "existing content");
            
            _mockConflictDialog.ShowFileConflictDialog(outputPath)
                .Returns(FileConflictResolution.AppendSuffix);

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            var content = File.ReadAllText(outputPath);
            content.Should().Be("existing content"); // Original file unchanged
            
            var newPath = Path.Combine(_testFolder, "test_route (1).xml");
            File.Exists(newPath).Should().BeTrue();
        }

        [Test]
        public void ExportRouteAsync_WithExistingFile_AndCancelChosen_ThrowsOperationCanceledException()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "test_route.xml");
            
            // Create existing file
            File.WriteAllText(outputPath, "existing content");
            
            _mockConflictDialog.ShowFileConflictDialog(outputPath)
                .Returns(FileConflictResolution.Cancel);

            // Act & Assert
            var act = async () => await _exporter.ExportRouteAsync(waypoints, outputPath);
            act.Should().ThrowAsync<OperationCanceledException>()
                .WithMessage("*Export cancelled by user*");
        }

        [Test]
        public async Task GenerateRouteXml_WithSampleWaypoints_MapsAllAttributesCorrectly()
        {
            // Arrange
            var waypoints = new List<RouteWaypoint>
            {
                new RouteWaypoint
                {
                    Index = 0,
                    Name = "205196000",
                    Lat = 55.123456,
                    Lon = 12.345678,
                    Alt = 0,
                    Speed = 12.5,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 180,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 0, 0, 0)
                }
            };
            var outputPath = Path.Combine(_testFolder, "attribute_test.xml");

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            var xml = XDocument.Load(outputPath);
            var waypointElement = xml.Root!.Element("RouteTemplate")!.Element("WayPoint")!;
            
            waypointElement.Attribute("Name")!.Value.Should().Be("205196000");
            waypointElement.Attribute("Lat")!.Value.Should().Be("55.123456");
            waypointElement.Attribute("Lon")!.Value.Should().Be("12.345678");
            waypointElement.Attribute("Alt")!.Value.Should().Be("0");
            waypointElement.Attribute("Speed")!.Value.Should().Be("12.50");
            waypointElement.Attribute("ETA")!.Value.Should().Be("0");
            waypointElement.Attribute("Delay")!.Value.Should().Be("0");
            waypointElement.Attribute("Mode")!.Value.Should().Be("Waypoint");
            waypointElement.Attribute("TrackMode")!.Value.Should().Be("Track");
            waypointElement.Attribute("Heading")!.Value.Should().Be("180");
            waypointElement.Attribute("PortXTE")!.Value.Should().Be("20");
            waypointElement.Attribute("StbdXTE")!.Value.Should().Be("20");
            waypointElement.Attribute("MinSpeed")!.Value.Should().Be("0");
            waypointElement.Attribute("MaxSpeed")!.Value.Should().Be("12.50"); // Max speed is 12.5 since that's the only speed in the list
        }

        [Test]
        public async Task GenerateRouteXml_WithVariousSpeedValues_ComputesMaxSpeedCorrectly()
        {
            // Arrange
            var waypoints = new List<RouteWaypoint>
            {
                new RouteWaypoint { Name = "205196000", Lat = 55.0, Lon = 12.0, Speed = 0, Time = DateTime.Now },
                new RouteWaypoint { Name = "205196000", Lat = 55.1, Lon = 12.1, Speed = 12.5, Time = DateTime.Now },
                new RouteWaypoint { Name = "205196000", Lat = 55.2, Lon = 12.2, Speed = 15.3, Time = DateTime.Now },
                new RouteWaypoint { Name = "205196000", Lat = 55.3, Lon = 12.3, Speed = 18.7, Time = DateTime.Now }
            };
            var outputPath = Path.Combine(_testFolder, "maxspeed_test.xml");

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            var xml = XDocument.Load(outputPath);
            var waypointElements = xml.Root!.Element("RouteTemplate")!.Elements("WayPoint");
            
            foreach (var wp in waypointElements)
            {
                wp.Attribute("MaxSpeed")!.Value.Should().Be("18.70");
            }
        }

        [Test]
        public async Task ExportRouteAsync_WithMultipleConflicts_IncrementsCorrectly()
        {
            // Arrange
            var waypoints = CreateTestWaypoints();
            var basePath = Path.Combine(_testFolder, "test_route.xml");
            
            // Create multiple existing files
            File.WriteAllText(basePath, "content 0");
            File.WriteAllText(Path.Combine(_testFolder, "test_route (1).xml"), "content 1");
            
            _mockConflictDialog.ShowFileConflictDialog(basePath)
                .Returns(FileConflictResolution.AppendSuffix);

            // Act
            await _exporter.ExportRouteAsync(waypoints, basePath);

            // Assert
            var newPath = Path.Combine(_testFolder, "test_route (2).xml");
            File.Exists(newPath).Should().BeTrue();
        }

        private List<RouteWaypoint> CreateTestWaypoints()
        {
            return new List<RouteWaypoint>
            {
                new RouteWaypoint
                {
                    Index = 0,
                    Name = "205196000",
                    Lat = 55.123456,
                    Lon = 12.345678,
                    Alt = 0,
                    Speed = 12.5,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 180,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 0, 0, 0)
                },
                new RouteWaypoint
                {
                    Index = 1,
                    Name = "205196000",
                    Lat = 55.234567,
                    Lon = 12.456789,
                    Alt = 0,
                    Speed = 15.3,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 185,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 1, 0, 0)
                },
                new RouteWaypoint
                {
                    Index = 2,
                    Name = "205196000",
                    Lat = 55.345678,
                    Lon = 12.567890,
                    Alt = 0,
                    Speed = 18.7,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 190,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 2, 0, 0)
                }
            };
        }
    }
}
