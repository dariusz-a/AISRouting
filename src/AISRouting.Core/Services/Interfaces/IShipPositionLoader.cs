using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for loading AIS position data from CSV files.
    /// </summary>
    public interface IShipPositionLoader
    {
        /// <summary>
        /// Loads position records from CSV files within the specified time range.
        /// </summary>
        /// <param name="mmsi">The MMSI of the vessel.</param>
        /// <param name="startTime">Start of the time range (inclusive).</param>
        /// <param name="stopTime">End of the time range (inclusive).</param>
        /// <param name="inputFolderPath">Root folder containing MMSI subfolders.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ordered collection of position records within the time range.</returns>
        Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(
            int mmsi,
            DateTime startTime,
            DateTime stopTime,
            string inputFolderPath,
            IProgress<LoadProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
