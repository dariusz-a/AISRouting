using System.Globalization;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AISRouting.Infrastructure.IO
{
    /// <summary>
    /// Scans input folder for vessel subfolders and extracts metadata.
    /// </summary>
    public class SourceDataScanner : ISourceDataScanner
    {
        private readonly IShipStaticDataLoader _staticLoader;
        private readonly ILogger<SourceDataScanner> _logger;

        public SourceDataScanner(IShipStaticDataLoader staticLoader, ILogger<SourceDataScanner> logger)
        {
            _staticLoader = staticLoader ?? throw new ArgumentNullException(nameof(staticLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(string inputFolder, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(inputFolder))
            {
                _logger.LogError("Input folder not found: {InputFolder}", inputFolder);
                throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");
            }

            var results = new List<ShipStaticData>();

            foreach (var dir in Directory.EnumerateDirectories(inputFolder))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mmsiString = Path.GetFileName(dir);

                try
                {
                    // Attempt to load static data
                    var staticData = await _staticLoader.LoadStaticDataAsync(dir, mmsiString, cancellationToken);

                    // Fallback if static data file missing
                    if (staticData == null)
                    {
                        staticData = new ShipStaticData
                        {
                            MMSI = long.TryParse(mmsiString, out var mmsi) ? mmsi : 0,
                            FolderPath = dir
                        };
                    }

                    // Extract date range from CSV files
                    var (minDate, maxDate) = ExtractMinMaxDatesFromCsvFiles(dir);
                    staticData.MinDate = minDate;
                    staticData.MaxDate = maxDate;

                    results.Add(staticData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping folder {Dir} during input scan", dir);
                }
            }

            _logger.LogInformation("Scanned {Count} vessels from {InputFolder}", results.Count, inputFolder);
            return results;
        }

        private (DateTime minDate, DateTime maxDate) ExtractMinMaxDatesFromCsvFiles(string folderPath)
        {
            var dates = new List<DateTime>();

            foreach (var csvFile in Directory.EnumerateFiles(folderPath, "*.csv"))
            {
                var filename = Path.GetFileNameWithoutExtension(csvFile);

                if (DateTime.TryParseExact(filename, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                {
                    dates.Add(date);
                }
            }

            if (dates.Count == 0)
            {
                _logger.LogWarning("No valid CSV date files found in {FolderPath}", folderPath);
                return (DateTime.MinValue, DateTime.MaxValue);
            }

            return (dates.Min(), dates.Max());
        }
    }
}
