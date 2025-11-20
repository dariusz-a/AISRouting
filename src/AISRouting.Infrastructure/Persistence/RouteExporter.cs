using System.Globalization;
using System.Xml.Linq;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AISRouting.Infrastructure.Persistence
{
    /// <summary>
    /// Service for exporting route waypoints to XML format compatible with navigation systems.
    /// </summary>
    public class RouteExporter : IRouteExporter
    {
        private readonly ILogger<RouteExporter> _logger;
        private readonly Core.Services.Interfaces.IPathValidator _pathValidator;
        private readonly IFileConflictDialogService _conflictDialog;

        public RouteExporter(
            ILogger<RouteExporter> logger,
            Core.Services.Interfaces.IPathValidator pathValidator,
            IFileConflictDialogService conflictDialog)
        {
            _logger = logger;
            _pathValidator = pathValidator;
            _conflictDialog = conflictDialog;
        }

        public async Task ExportRouteAsync(
            IEnumerable<RouteWaypoint> waypoints,
            string outputFilePath,
            ExportOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= ExportOptions.Default;

            // Validate output path
            var outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new ArgumentException("Invalid output file path", nameof(outputFilePath));
            }

            _pathValidator.ValidateOutputFilePath(outputDirectory);

            // Validate waypoints
            var waypointList = waypoints.ToList();
            if (waypointList.Count == 0)
            {
                throw new ArgumentException("Waypoint list is empty", nameof(waypoints));
            }

            _logger.LogInformation("Starting export of {Count} waypoints to {Path}", waypointList.Count, outputFilePath);

            // Handle file conflicts
            if (File.Exists(outputFilePath))
            {
                _logger.LogWarning("File conflict detected for {Path}, prompting user", outputFilePath);
                var resolution = _conflictDialog.ShowFileConflictDialog(outputFilePath);

                switch (resolution)
                {
                    case FileConflictResolution.Cancel:
                        _logger.LogInformation("Export cancelled by user");
                        throw new OperationCanceledException("Export cancelled by user");

                    case FileConflictResolution.AppendSuffix:
                        outputFilePath = GenerateUniquePath(outputFilePath);
                        _logger.LogInformation("Generated unique filename: {Path}", outputFilePath);
                        break;

                    case FileConflictResolution.Overwrite:
                        _logger.LogInformation("User chose to overwrite existing file: {Path}", outputFilePath);
                        break;
                }
            }

            // Determine route template name (extract MMSI prefix from filename if present)
            var filename = Path.GetFileNameWithoutExtension(outputFilePath);
            var templateName = "Unknown";
            if (!string.IsNullOrEmpty(filename))
            {
                var parts = filename.Split('-');
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                    templateName = parts[0];
            }

            // Generate XML
            var xml = GenerateRouteXml(waypointList, templateName);

            // Write to file
            await File.WriteAllTextAsync(outputFilePath, xml.ToString(), cancellationToken);

            _logger.LogInformation(
                "Successfully exported {Count} waypoints to {Path}",
                waypointList.Count, outputFilePath);
        }

        private XDocument GenerateRouteXml(List<RouteWaypoint> waypoints, string templateName)
        {
            var maxSpeed = waypoints.Where(w => w.Speed > 0).Max(w => w.Speed);

            var routeTemplate = new XElement("RouteTemplate",
                new XAttribute("Name", templateName),
                new XAttribute("ColorR", "1"),
                new XAttribute("ColorG", "124"),
                new XAttribute("ColorB", "139"));

            int wpIndex = 1;
            foreach (var wp in waypoints)
            {
                // Use sequential waypoint names WP001..WP999 regardless of RouteWaypoint.Name
                var waypointName = $"WP{wpIndex:000}";

                var waypointElement = new XElement("WayPoint",
                    new XAttribute("Name", waypointName),
                    new XAttribute("Lat", wp.Lat.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("Lon", wp.Lon.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("Alt", wp.Alt),
                    new XAttribute("Speed", wp.Speed.ToString("F2", CultureInfo.InvariantCulture)),
                    new XAttribute("ETA", wp.ETA),
                    new XAttribute("Delay", wp.Delay),
                    new XAttribute("Mode", string.IsNullOrEmpty(wp.Mode) ? "Waypoint" : wp.Mode),
                    new XAttribute("TrackMode", "Track"),
                    new XAttribute("Heading", wp.Heading),
                    new XAttribute("PortXTE", wp.PortXTE),
                    new XAttribute("StbdXTE", wp.StbdXTE),
                    new XAttribute("MinSpeed", wp.MinSpeed),
                    new XAttribute("MaxSpeed", maxSpeed.ToString("F2", CultureInfo.InvariantCulture))
                );

                routeTemplate.Add(waypointElement);
                wpIndex++;
            }

            var rootElement = new XElement("RouteTemplates", routeTemplate);
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                rootElement);
        }

        private string GenerateUniquePath(string originalPath)
        {
            var dir = Path.GetDirectoryName(originalPath);
            var filenameNoExt = Path.GetFileNameWithoutExtension(originalPath);
            var ext = Path.GetExtension(originalPath);

            int suffix = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(dir!, $"{filenameNoExt} ({suffix}){ext}");
                suffix++;
            }
            while (File.Exists(newPath));

            _logger.LogInformation("Generating unique filename, original: {OriginalPath}, new: {NewPath}", originalPath, newPath);
            return newPath;
        }
    }
}
