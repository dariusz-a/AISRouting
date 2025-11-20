using AISRouting.Core.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AISRouting.Infrastructure.Parsers
{
    /// <summary>
    /// CSV parser for ShipDataOut position records with error tolerance.
    /// </summary>
    public class ShipPositionCsvParser : ICsvParser<ShipDataOut>
    {
        private readonly ILogger<ShipPositionCsvParser> _logger;

        public ShipPositionCsvParser(ILogger<ShipPositionCsvParser> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ShipDataOut>> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var results = new List<ShipDataOut>();

            try
            {
                // Extract base date from filename (format: YYYY-MM-DD.csv)
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if (!DateTime.TryParseExact(fileName, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var baseDate))
                {
                    _logger.LogWarning("Unable to parse date from filename: {FileName}", fileName);
                    baseDate = DateTime.UtcNow.Date;
                }

                using var reader = new StreamReader(filePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    MissingFieldFound = null,
                    BadDataFound = context =>
                    {
                        _logger.LogWarning("Bad data found at row {Row}: {RawRecord}", context.Context?.Parser?.Row ?? 0, context.RawRecord ?? "");
                    }
                };

                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<ShipDataOutCsvMapByIndex>();

                await foreach (var record in csv.GetRecordsAsync<ShipDataOutCsvMap>(cancellationToken))
                {
                    if (record != null)
                    {
                        var shipData = new ShipDataOut
                        {
                            Time = record.Time,
                            Lat = record.Lat,
                            Lon = record.Lon,
                            NavigationalStatusIndex = record.NavigationalStatusIndex,
                            ROT = record.ROT,
                            SOG = record.SOG,
                            COG = record.COG,
                            Heading = record.Heading,
                            Draught = record.Draught,
                            DestinationIndex = record.DestinationIndex,
                            EtaSecondsUntil = record.EtaSecondsUntil,
                            BaseDate = baseDate
                        };

                        results.Add(shipData);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV file: {FilePath}", filePath);
                throw;
            }

            return results;
        }

        /// <summary>
        /// Internal class for CSV mapping to handle nullable fields.
        /// </summary>
        private class ShipDataOutCsvMap
        {
            public long Time { get; set; }
            public double? Lat { get; set; }
            public double? Lon { get; set; }
            public int? NavigationalStatusIndex { get; set; }
            public double? ROT { get; set; }
            public double? SOG { get; set; }
            public double? COG { get; set; }
            public int? Heading { get; set; }
            public double? Draught { get; set; }
            public int? DestinationIndex { get; set; }
            public long? EtaSecondsUntil { get; set; }
        }

        /// <summary>
        /// Class map for CSV files without headers, using column indexes.
        /// CSV format: Time,Lat,Lon,NavigationalStatusIndex,ROT,SOG,COG,Heading,Draught,DestinationIndex,EtaSecondsUntil
        /// </summary>
        private sealed class ShipDataOutCsvMapByIndex : ClassMap<ShipDataOutCsvMap>
        {
            public ShipDataOutCsvMapByIndex()
            {
                Map(m => m.Time).Index(0);
                Map(m => m.Lat).Index(1).Optional();
                Map(m => m.Lon).Index(2).Optional();
                Map(m => m.NavigationalStatusIndex).Index(3).Optional();
                Map(m => m.ROT).Index(4).Optional();
                Map(m => m.SOG).Index(5).Optional();
                Map(m => m.COG).Index(6).Optional();
                Map(m => m.Heading).Index(7).Optional();
                Map(m => m.Draught).Index(8).Optional();
                Map(m => m.DestinationIndex).Index(9).Optional();
                Map(m => m.EtaSecondsUntil).Index(10).Optional();
            }
        }
    }
}
