using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.Parsers;
using AISRouting.Infrastructure.Validation;
using Microsoft.Extensions.Logging;

namespace AISRouting.Infrastructure.IO
{
    /// <summary>
    /// Service for loading AIS position data from CSV files.
    /// </summary>
    public class ShipPositionLoader : IShipPositionLoader
    {
        private readonly ILogger<ShipPositionLoader> _logger;
        private readonly Validation.IPathValidator _pathValidator;
        private readonly ICsvParser<ShipDataOut> _csvParser;

        public ShipPositionLoader(
            ILogger<ShipPositionLoader> logger,
            Validation.IPathValidator pathValidator,
            ICsvParser<ShipDataOut> csvParser)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
            _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        }

        public async Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(
            int mmsi,
            DateTime startTime,
            DateTime stopTime,
            string inputFolderPath,
            IProgress<LoadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            _pathValidator.ValidateInputFolderPath(inputFolderPath);
            MmsiValidator.Validate(mmsi);

            if (startTime >= stopTime)
                throw new ArgumentException("Start time must be before stop time");

            var mmsiFolder = Path.Combine(inputFolderPath, mmsi.ToString());
            if (!Directory.Exists(mmsiFolder))
                throw new DirectoryNotFoundException($"MMSI folder not found: {mmsi}");

            // Determine which CSV files to load
            var csvFiles = GetRelevantCsvFiles(mmsiFolder, startTime, stopTime);
            if (csvFiles.Count == 0)
                throw new FileNotFoundException($"No CSV files found for MMSI {mmsi} in date range");

            var allPositions = new List<ShipDataOut>();
            int processedFiles = 0;
            int skippedRecords = 0;

            foreach (var csvFile in csvFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var positions = await _csvParser.ParseFileAsync(csvFile, cancellationToken);

                    // Filter by time range and validate coordinates
                    var filtered = positions.Where(p =>
                    {
                        if (p.BaseDateTime < startTime || p.BaseDateTime > stopTime)
                            return false;

                        if (!p.IsValid)
                        {
                            skippedRecords++;
                            _logger.LogWarning("Skipping record with invalid coordinates at {Time}", p.BaseDateTime);
                            return false;
                        }

                        return true;
                    });

                    allPositions.AddRange(filtered);

                    processedFiles++;
                    progress?.Report(new LoadProgress
                    {
                        ProcessedFiles = processedFiles,
                        TotalFiles = csvFiles.Count,
                        RecordsLoaded = allPositions.Count,
                        SkippedRecords = skippedRecords
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load CSV file: {File}", csvFile);
                    // Continue processing other files
                }
            }

            if (allPositions.Count == 0)
            {
                _logger.LogWarning("No valid position records found for MMSI {MMSI} in time range {Start} to {Stop}", 
                    mmsi, startTime, stopTime);
            }

            // Sort by timestamp
            var sorted = allPositions.OrderBy(p => p.BaseDateTime).ToList();

            _logger.LogInformation(
                "Loaded {Count} position records for MMSI {MMSI} from {Start} to {Stop} (skipped {Skipped})",
                sorted.Count, mmsi, startTime, stopTime, skippedRecords);

            return sorted;
        }

        private List<string> GetRelevantCsvFiles(string mmsiFolder, DateTime start, DateTime stop)
        {
            var relevantFiles = new List<string>();

            var currentDate = start.Date;
            while (currentDate <= stop.Date)
            {
                var expectedFilename = $"{currentDate:yyyy-MM-dd}.csv";
                var filePath = Path.Combine(mmsiFolder, expectedFilename);

                if (File.Exists(filePath))
                {
                    relevantFiles.Add(filePath);
                }

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogDebug("Found {Count} CSV files for date range {Start} to {Stop}", 
                relevantFiles.Count, start.Date, stop.Date);

            return relevantFiles;
        }
    }
}
