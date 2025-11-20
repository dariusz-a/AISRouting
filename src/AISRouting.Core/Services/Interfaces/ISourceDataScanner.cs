using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Scans input folder for vessel data and returns collection of discovered vessels.
    /// </summary>
    public interface ISourceDataScanner
    {
        /// <summary>
        /// Scans the input folder for vessel subfolders and returns vessel metadata.
        /// </summary>
        /// <param name="inputFolder">Path to the root input folder containing MMSI subfolders.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Collection of discovered vessels with static data and date ranges.</returns>
        Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(string inputFolder, CancellationToken cancellationToken = default);
    }
}
