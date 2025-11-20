using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Loads vessel static data from JSON files.
    /// </summary>
    public interface IShipStaticDataLoader
    {
        /// <summary>
        /// Loads static data for a vessel from its JSON file.
        /// </summary>
        /// <param name="folderPath">Path to the vessel's folder.</param>
        /// <param name="mmsi">MMSI identifier for the vessel.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>ShipStaticData or null if file missing or malformed.</returns>
        Task<ShipStaticData?> LoadStaticDataAsync(string folderPath, string mmsi, CancellationToken cancellationToken = default);
    }
}
