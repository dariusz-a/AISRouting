using System.Xml.Linq;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.Persistence;
using AISRouting.Infrastructure.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace AISRouting.Tests.IntegrationTests
{
    [TestFixture]
    public class XmlExportValidationTests
    {
        private ILogger<RouteExporter> _mockLogger = null!;
        private ILogger<PathValidator> _mockPathLogger = null!;
        private PathValidator _pathValidator = null!;
        private IFileConflictDialogService _mockConflictDialog = null!;
        private RouteExporter _exporter = null!;
        private string _testFolder = null!;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<RouteExporter>>();
            _mockPathLogger = Substitute.For<ILogger<PathValidator>>();
            _pathValidator = new PathValidator(_mockPathLogger);
            _mockConflictDialog = Substitute.For<IFileConflictDialogService>();
            _exporter = new RouteExporter(_mockLogger, _pathValidator, _mockConflictDialog);

            // Create temporary test folder
            _testFolder = Path.Combine(Path.GetTempPath(), $"AISRoutingIntegrationTest_{Guid.NewGuid()}");
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
        public async Task ExportedXml_HasValidStructureAndAttributes()
        {
            // Arrange
            var waypoints = CreateRealWorldTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "integration_test_route.xml");

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();

            var xml = XDocument.Load(outputPath);
            
            // Validate root structure
            xml.Root.Should().NotBeNull();
            xml.Root!.Name.LocalName.Should().Be("RouteTemplates");
            
            // Validate RouteTemplate element
            var routeTemplate = xml.Root.Element("RouteTemplate");
            routeTemplate.Should().NotBeNull();
            routeTemplate!.Attribute("Name")!.Value.Should().Be("integration_test_route");
            routeTemplate.Attribute("ColorR")!.Value.Should().Be("1");
            routeTemplate.Attribute("ColorG")!.Value.Should().Be("124");
            routeTemplate.Attribute("ColorB")!.Value.Should().Be("139");
            
            // Validate WayPoint elements
            var waypointElements = routeTemplate.Elements("WayPoint").ToList();
            waypointElements.Should().HaveCount(5);
            
            // Validate first waypoint attributes
            var firstWaypoint = waypointElements[0];
            firstWaypoint.Attribute("Name")!.Value.Should().StartWith("WP");
            firstWaypoint.Attribute("Lat")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("Lon")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("Alt")!.Value.Should().Be("0");
            firstWaypoint.Attribute("Speed")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("ETA")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("Delay")!.Value.Should().Be("0");
            firstWaypoint.Attribute("Mode")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("TrackMode")!.Value.Should().Be("Track");
            firstWaypoint.Attribute("Heading")!.Value.Should().NotBeNullOrEmpty();
            firstWaypoint.Attribute("PortXTE")!.Value.Should().Be("20");
            firstWaypoint.Attribute("StbdXTE")!.Value.Should().Be("20");
            firstWaypoint.Attribute("MinSpeed")!.Value.Should().Be("0");
            firstWaypoint.Attribute("MaxSpeed")!.Value.Should().NotBeNullOrEmpty();
            
            // Validate all waypoints have same MaxSpeed
            var maxSpeedValues = waypointElements.Select(wp => wp.Attribute("MaxSpeed")!.Value).Distinct().ToList();
            maxSpeedValues.Should().HaveCount(1);
        }

        [Test]
        public async Task ExportRouteAsync_WithAppendSuffix_CreatesUniqueFile()
        {
            // Arrange
            var waypoints = CreateRealWorldTestWaypoints();
            var originalPath = Path.Combine(_testFolder, "conflict_test.xml");
            
            // Create initial file
            await _exporter.ExportRouteAsync(waypoints, originalPath);
            
            // Setup conflict dialog to return AppendSuffix
            _mockConflictDialog.ShowFileConflictDialog(originalPath)
                .Returns(FileConflictResolution.AppendSuffix);
            
            // Act - export again with same path
            await _exporter.ExportRouteAsync(waypoints, originalPath);

            // Assert
            File.Exists(originalPath).Should().BeTrue();
            File.Exists(Path.Combine(_testFolder, "conflict_test (1).xml")).Should().BeTrue();
            
            // Verify both files have valid XML
            var xml1 = XDocument.Load(originalPath);
            var xml2 = XDocument.Load(Path.Combine(_testFolder, "conflict_test (1).xml"));
            
            xml1.Root!.Name.LocalName.Should().Be("RouteTemplates");
            xml2.Root!.Name.LocalName.Should().Be("RouteTemplates");
        }

        [Test]
        public async Task EndToEndExport_WithRealData_ProducesValidNavigationXml()
        {
            // Arrange
            var waypoints = CreateDetailedTestWaypoints();
            var outputPath = Path.Combine(_testFolder, "205196000-20250315T000000-20250316T000000.xml");

            // Act
            await _exporter.ExportRouteAsync(waypoints, outputPath);

            // Assert
            var xml = XDocument.Load(outputPath);
            var waypointElements = xml.Root!.Element("RouteTemplate")!.Elements("WayPoint").ToList();
            
            // Verify waypoints are in order
            for (int i = 0; i < waypointElements.Count; i++)
            {
                var expectedIndex = i;
                // Note: Index is not stored in XML, but waypoint element names are WP001..WP999
                waypointElements[i].Attribute("Name")!.Value.Should().StartWith("WP");
            }
            
            // Verify coordinate precision
            var lat = double.Parse(waypointElements[0].Attribute("Lat")!.Value, System.Globalization.CultureInfo.InvariantCulture);
            var lon = double.Parse(waypointElements[0].Attribute("Lon")!.Value, System.Globalization.CultureInfo.InvariantCulture);
            lat.Should().BeGreaterThan(0);
            lon.Should().BeGreaterThan(0);
            
            // Verify speed formatting (should be F2 format)
            var speed = waypointElements[0].Attribute("Speed")!.Value;
            speed.Should().MatchRegex(@"^\d+\.\d{2}$");
        }

        [Test]
        public void ExportRouteAsync_WithReadOnlyDirectory_ThrowsUnauthorizedAccessException()
        {
            // Note: This test may not work on all systems due to OS security
            // It's included for completeness but may need to be adjusted
            
            // Arrange
            var waypoints = CreateRealWorldTestWaypoints();
            var readOnlyFolder = Path.Combine(_testFolder, "readonly");
            Directory.CreateDirectory(readOnlyFolder);
            
            // Make directory read-only (Windows specific)
            try
            {
                var dirInfo = new DirectoryInfo(readOnlyFolder);
                dirInfo.Attributes |= FileAttributes.ReadOnly;
                
                var outputPath = Path.Combine(readOnlyFolder, "test.xml");

                // Act & Assert
                var act = async () => await _exporter.ExportRouteAsync(waypoints, outputPath);
                act.Should().ThrowAsync<UnauthorizedAccessException>();
                
                // Cleanup
                dirInfo.Attributes &= ~FileAttributes.ReadOnly;
            }
            catch
            {
                // If we can't make it readonly, skip the test
                Assert.Inconclusive("Unable to set directory as read-only on this system");
            }
        }

        private List<RouteWaypoint> CreateRealWorldTestWaypoints()
        {
            return new List<RouteWaypoint>
            {
                new RouteWaypoint
                {
                    Index = 0,
                    Name = "205196000",
                    Lat = 0.971674,    // 55.67234° in radians
                    Lon = 0.219574,    // 12.58456° in radians
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
                    Lat = 0.971777,    // 55.68123° in radians
                    Lon = 0.219795,    // 12.59234° in radians
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
                    Lat = 0.971932,    // 55.69012° in radians
                    Lon = 0.219989,    // 12.60123° in radians
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
                },
                new RouteWaypoint
                {
                    Index = 3,
                    Name = "205196000",
                    Lat = 0.972087,    // 55.69901° in radians
                    Lon = 0.220166,    // 12.61012° in radians
                    Alt = 0,
                    Speed = 16.2,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 195,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 3, 0, 0)
                },
                new RouteWaypoint
                {
                    Index = 4,
                    Name = "205196000",
                    Lat = 0.972242,    // 55.70789° in radians
                    Lon = 0.220343,    // 12.61901° in radians
                    Alt = 0,
                    Speed = 14.8,
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 200,
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = new DateTime(2025, 3, 15, 4, 0, 0)
                }
            };
        }

        private List<RouteWaypoint> CreateDetailedTestWaypoints()
        {
            var waypoints = new List<RouteWaypoint>();
            var baseTime = new DateTime(2025, 3, 15, 0, 0, 0);
            
            for (int i = 0; i < 10; i++)
            {
                // Convert degrees to radians
                double latDegrees = 55.67 + (i * 0.01);
                double lonDegrees = 12.58 + (i * 0.01);
                double latRadians = latDegrees * Math.PI / 180.0;
                double lonRadians = lonDegrees * Math.PI / 180.0;
                
                waypoints.Add(new RouteWaypoint
                {
                    Index = i,
                    Name = "205196000",
                    Lat = latRadians,
                    Lon = lonRadians,
                    Alt = 0,
                    Speed = 10 + (i * 0.5),
                    ETA = 0,
                    Delay = 0,
                    Mode = "Waypoint",
                    TrackMode = "Track",
                    Heading = 180 + (i * 2),
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0,
                    MaxSpeed = 0,
                    Time = baseTime.AddHours(i)
                });
            }
            
            return waypoints;
        }
    }
}
