using System.Text.Json;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AISRouting.Infrastructure.Parsers
{
    /// <summary>
    /// Parses vessel static data from JSON files.
    /// </summary>
    public class ShipStaticDataParser : IShipStaticDataLoader
    {
        private readonly ILogger<ShipStaticDataParser> _logger;

        public ShipStaticDataParser(ILogger<ShipStaticDataParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ShipStaticData?> LoadStaticDataAsync(string folderPath, string mmsi, CancellationToken cancellationToken = default)
        {
            var jsonPath = Path.Combine(folderPath, $"{mmsi}.json");

            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("Static data file not found: {JsonPath}", jsonPath);
                return null;
            }

            try
            {
                using var stream = File.OpenRead(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // First read as a dictionary to handle different property names
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var root = doc.RootElement;

                var staticData = new ShipStaticData
                {
                    FolderPath = folderPath
                };

                // Try different property name variations
                if (root.TryGetProperty("mmsi", out var mmsiElement) || root.TryGetProperty("MMSI", out mmsiElement))
                {
                    if (mmsiElement.ValueKind == JsonValueKind.String && long.TryParse(mmsiElement.GetString(), out var mmsiValue))
                        staticData.MMSI = mmsiValue;
                    else if (mmsiElement.ValueKind == JsonValueKind.Number)
                        staticData.MMSI = mmsiElement.GetInt64();
                }

                if (root.TryGetProperty("name", out var nameElement) || root.TryGetProperty("Name", out nameElement))
                    staticData.Name = nameElement.GetString();

                if (root.TryGetProperty("length", out var lengthElement) || root.TryGetProperty("Length", out lengthElement))
                {
                    if (lengthElement.ValueKind == JsonValueKind.Number)
                        staticData.Length = lengthElement.GetDouble();
                }

                if (root.TryGetProperty("width", out var beamElement) || root.TryGetProperty("Beam", out beamElement))
                {
                    if (beamElement.ValueKind == JsonValueKind.Number)
                        staticData.Beam = beamElement.GetDouble();
                }

                if (root.TryGetProperty("draught", out var draughtElement) || root.TryGetProperty("Draught", out draughtElement))
                {
                    if (draughtElement.ValueKind == JsonValueKind.Number)
                        staticData.Draught = draughtElement.GetDouble();
                }

                if (root.TryGetProperty("vesselType", out var typeElement) || root.TryGetProperty("TypeCode", out typeElement))
                {
                    if (typeElement.ValueKind == JsonValueKind.Number)
                        staticData.TypeCode = typeElement.GetInt32();
                }

                if (root.TryGetProperty("callSign", out var callSignElement) || root.TryGetProperty("CallSign", out callSignElement))
                    staticData.CallSign = callSignElement.GetString();

                if (root.TryGetProperty("imo", out var imoElement) || root.TryGetProperty("IMO", out imoElement))
                {
                    if (imoElement.ValueKind == JsonValueKind.Number)
                        staticData.IMO = imoElement.GetInt64();
                }

                _logger.LogDebug("Loaded static data for MMSI {MMSI} from {JsonPath}", mmsi, jsonPath);
                return staticData;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse static data file: {JsonPath}", jsonPath);
                return null;
            }
        }
    }
}
