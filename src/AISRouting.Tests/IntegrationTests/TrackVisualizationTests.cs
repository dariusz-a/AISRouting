using AISRouting.Core.Models;
using AISRouting.Core.Services.Implementations;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.IO;
using AISRouting.Infrastructure.Parsers;
using AISRouting.Infrastructure.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using ScottPlot;

namespace AISRouting.Tests.IntegrationTests
{
    [TestFixture]
    public class TrackVisualizationTests
    {
        private string _testDataPath = null!;
        private ILogger<ShipPositionLoader> _loaderLogger = null!;
        private ILogger<ShipPositionCsvParser> _parserLogger = null!;
        private ILogger<TrackOptimizer> _optimizerLogger = null!;
        private ILogger<PathValidator> _pathValidatorLogger = null!;
        private IShipPositionLoader _positionLoader = null!;
        private ITrackOptimizer _trackOptimizer = null!;
        private PathValidator _pathValidator = null!;

        [SetUp]
        public void SetUp()
        {
            _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            
            _loaderLogger = Substitute.For<ILogger<ShipPositionLoader>>();
            _parserLogger = Substitute.For<ILogger<ShipPositionCsvParser>>();
            _optimizerLogger = Substitute.For<ILogger<TrackOptimizer>>();
            _pathValidatorLogger = Substitute.For<ILogger<PathValidator>>();
            _pathValidator = new PathValidator(_pathValidatorLogger);
            
            var csvParser = new ShipPositionCsvParser(_parserLogger);
            _positionLoader = new ShipPositionLoader(_loaderLogger, _pathValidator, csvParser);
            _trackOptimizer = new TrackOptimizer(_optimizerLogger);
        }

        [Test]
        public async Task VisualizeTrackOptimization_MMSI210253000_CompareInitialAndOptimizedWaypoints()
        {
            // Arrange
            var mmsi = 210253000;
            var mmsiFolder = Path.Combine(_testDataPath, mmsi.ToString());
            
            if (!Directory.Exists(mmsiFolder))
            {
                Assert.Inconclusive($"Test data folder not found: {mmsiFolder}");
                return;
            }

            // Set time range to cover the entire day in the CSV
            var startTime = new DateTime(2024, 5, 3, 0, 0, 0, DateTimeKind.Utc);
            var stopTime = new DateTime(2024, 5, 3, 23, 59, 59, DateTimeKind.Utc);

            // Act - Load initial positions
            var initialPositions = await _positionLoader.LoadPositionsAsync(
                mmsi,
                startTime,
                stopTime,
                _testDataPath,
                cancellationToken: CancellationToken.None
            );

            var initialList = initialPositions.ToList();

            // Act - Optimize track
            var optimizedWaypoints = await _trackOptimizer.OptimizeTrackAsync(
                initialList,
                new OptimizationOptions { ToleranceMeters = 50 },
                cancellationToken: CancellationToken.None
            );

            var optimizedList = optimizedWaypoints.ToList();

            // Assert
            initialList.Should().HaveCountGreaterThanOrEqualTo(1420, "Initial positions should be loaded from CSV");
            initialList.Should().HaveCountLessThanOrEqualTo(1422, "Initial positions should match CSV row count");
            optimizedList.Should().HaveCountGreaterThanOrEqualTo(14, "Optimized waypoints should be significantly reduced");
            optimizedList.Should().HaveCountLessThanOrEqualTo(16, "Optimized waypoints should be around 15 points");

            // Create visualization
            var outputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestOutput");
            Directory.CreateDirectory(outputPath);
            var imagePath = Path.Combine(outputPath, "track_optimization_210253000.png");

            CreateInteractivePlot(initialList, optimizedList, imagePath, mmsi);

            // Verify plot was created
            File.Exists(imagePath).Should().BeTrue("Plot image should be created");
            
            TestContext.WriteLine($"Visualization saved to: {imagePath}");
            TestContext.WriteLine($"Initial positions: {initialList.Count}");
            TestContext.WriteLine($"Optimized waypoints: {optimizedList.Count}");
            TestContext.WriteLine($"Reduction: {(100.0 - (optimizedList.Count * 100.0 / initialList.Count)):F1}%");
        }

        private void CreateInteractivePlot(
            List<ShipDataOut> initialPositions,
            List<RouteWaypoint> optimizedWaypoints,
            string outputPath,
            int mmsi)
        {
            var plot = new Plot();

            // Configure plot appearance
            plot.Title($"Track Optimization - MMSI {mmsi}");
            plot.XLabel("Longitude (deg)");
            plot.YLabel("Latitude (deg)");
            plot.Legend.IsVisible = true;

            // Extract coordinates for initial positions (convert radians -> degrees)
            double[] ToDegrees(double[] arr) => arr == null ? Array.Empty<double>() : arr.Select(r => r / Math.PI * 180.0).ToArray();
            var initialLons = initialPositions.Select(p => (double)p.Lon!.Value).ToArray();
            var initialLats = initialPositions.Select(p => (double)p.Lat!.Value).ToArray();

            // Extract coordinates for optimized waypoints (convert radians -> degrees)
            var optimizedLons = ToDegrees(optimizedWaypoints.Select(w => w.Lon).ToArray());
            var optimizedLats = ToDegrees(optimizedWaypoints.Select(w => w.Lat).ToArray());

            // Plot initial positions as a line with small markers
            var initialScatter = plot.Add.Scatter(initialLons, initialLats);
            initialScatter.Label = $"Initial Positions ({initialPositions.Count} points)";
            initialScatter.Color = ScottPlot.Colors.LightBlue;
            initialScatter.LineWidth = 1;
            initialScatter.MarkerSize = 3;
            initialScatter.MarkerStyle.Shape = MarkerShape.OpenCircle;

            // Plot optimized waypoints as larger markers with a line
            var optimizedScatter = plot.Add.Scatter(optimizedLons, optimizedLats);
            optimizedScatter.Label = $"Optimized Waypoints ({optimizedWaypoints.Count} points)";
            optimizedScatter.Color = ScottPlot.Colors.Red;
            optimizedScatter.LineWidth = 2;
            optimizedScatter.MarkerSize = 8;
            optimizedScatter.MarkerStyle.Shape = MarkerShape.FilledCircle;

            // Save the plot
            plot.SavePng(outputPath, 1200, 800);

            // Also create an HTML file with interactive tooltips information
            CreateInteractiveHtml(initialPositions, optimizedWaypoints, outputPath, mmsi);
        }

        private void CreateInteractiveHtml(
            List<ShipDataOut> initialPositions,
            List<RouteWaypoint> optimizedWaypoints,
            string imagePath,
            int mmsi)
        {
            var htmlPath = Path.ChangeExtension(imagePath, ".html");
            
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Track Optimization - MMSI {mmsi}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            max-width: 1400px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        h1 {{
            color: #333;
            border-bottom: 2px solid #007acc;
            padding-bottom: 10px;
        }}
        .stats {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }}
        .stat-card {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px;
            border-radius: 8px;
            text-align: center;
        }}
        .stat-value {{
            font-size: 32px;
            font-weight: bold;
            margin: 5px 0;
        }}
        .stat-label {{
            font-size: 14px;
            opacity: 0.9;
        }}
        img {{
            width: 100%;
            border: 1px solid #ddd;
            border-radius: 4px;
            margin: 20px 0;
        }}
        .waypoint-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        .waypoint-table th, .waypoint-table td {{
            padding: 10px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }}
        .waypoint-table th {{
            background-color: #007acc;
            color: white;
            font-weight: bold;
        }}
        .waypoint-table tr:hover {{
            background-color: #f5f5f5;
        }}
        .section {{
            margin: 30px 0;
        }}
        .section h2 {{
            color: #555;
            border-left: 4px solid #007acc;
            padding-left: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Track Optimization Visualization - MMSI {mmsi}</h1>
        
        <div class='stats'>
            <div class='stat-card'>
                <div class='stat-label'>Initial Positions</div>
                <div class='stat-value'>{initialPositions.Count}</div>
            </div>
            <div class='stat-card'>
                <div class='stat-label'>Optimized Waypoints</div>
                <div class='stat-value'>{optimizedWaypoints.Count}</div>
            </div>
            <div class='stat-card'>
                <div class='stat-label'>Reduction</div>
                <div class='stat-value'>{(100.0 - (optimizedWaypoints.Count * 100.0 / initialPositions.Count)):F1}%</div>
            </div>
        </div>

        <div class='section'>
            <h2>Track Visualization</h2>
            <img src='{Path.GetFileName(imagePath)}' alt='Track Optimization Plot'>
        </div>

        <div class='section'>
            <h2>Optimized Waypoints Details</h2>
            <table class='waypoint-table'>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Timestamp</th>
                        <th>Latitude</th>
                        <th>Longitude</th>
                        <th>SOG (knots)</th>
                        <th>Heading (°)</th>
                    </tr>
                </thead>
                <tbody>
";

            for (int i = 0; i < optimizedWaypoints.Count; i++)
            {
                var wp = optimizedWaypoints[i];
                html += $@"
                    <tr>
                        <td>{i + 1}</td>
                        <td>{wp.Time:yyyy-MM-dd HH:mm:ss}</td>
                        <td>{wp.Lat:F6}</td>
                        <td>{wp.Lon:F6}</td>
                        <td>{wp.Speed:F2}</td>
                        <td>{wp.Heading}</td>
                    </tr>";
            }

            html += @"
                </tbody>
            </table>
        </div>

        <div class='section'>
            <h2>Initial Position Sample (First 20 records)</h2>
            <table class='waypoint-table'>
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Time (s)</th>
                        <th>Latitude</th>
                        <th>Longitude</th>
                        <th>SOG (knots)</th>
                        <th>Heading (°)</th>
                    </tr>
                </thead>
                <tbody>
";

            for (int i = 0; i < Math.Min(20, initialPositions.Count); i++)
            {
                var pos = initialPositions[i];
                html += $@"
                    <tr>
                        <td>{i + 1}</td>
                        <td>{pos.Time}</td>
                        <td>{pos.Lat:F6}</td>
                        <td>{pos.Lon:F6}</td>
                        <td>{pos.SOG:F2}</td>
                        <td>{pos.Heading}</td>
                    </tr>";
            }

            html += $@"
                </tbody>
            </table>
            <p style='color: #666; font-style: italic;'>Showing first 20 of {initialPositions.Count} total positions</p>
        </div>
    </div>
</body>
</html>";

            File.WriteAllText(htmlPath, html);
            TestContext.WriteLine($"Interactive HTML report saved to: {htmlPath}");
        }
    }
}
