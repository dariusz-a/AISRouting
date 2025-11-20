using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AISRouting.Infrastructure.IO
{
    public class ShipStaticDataLoader : IShipStaticDataLoader
    {
        private readonly ILogger<ShipStaticDataLoader> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ShipStaticDataLoader(ILogger<ShipStaticDataLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        public async Task<ShipStaticData?> LoadStaticDataAsync(string folderPath, string mmsi, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(mmsi, out var mmsiNumber))
            {
                _logger.LogWarning("Invalid MMSI format: {MMSI}", mmsi);
                return null;
            }

            var jsonPath = Path.Combine(folderPath, $"{mmsi}.json");

            if (!File.Exists(jsonPath))
            {
                _logger.LogInformation("No static data file found for MMSI {MMSI}, using fallback", mmsi);
                return new ShipStaticData
                {
                    MMSI = mmsiNumber,
                    Name = null,
                    FolderPath = folderPath
                };
            }

            try
            {
                await using var stream = File.OpenRead(jsonPath);
                var data = await JsonSerializer.DeserializeAsync<ShipStaticData>(stream, _jsonOptions, cancellationToken);

                if (data == null)
                {
                    _logger.LogWarning("Failed to deserialize static data for MMSI {MMSI}, using fallback", mmsi);
                    return new ShipStaticData
                    {
                        MMSI = mmsiNumber,
                        Name = null,
                        FolderPath = folderPath
                    };
                }

                data.MMSI = mmsiNumber;
                data.FolderPath = folderPath;
                
                _logger.LogInformation("Loaded static data for MMSI {MMSI} with name {Name}", mmsi, data.Name ?? "N/A");
                return data;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Malformed JSON for MMSI {MMSI}, using fallback", mmsi);
                return new ShipStaticData
                {
                    MMSI = mmsiNumber,
                    Name = null,
                    FolderPath = folderPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading static data for MMSI {MMSI}", mmsi);
                throw;
            }
        }
    }
}
