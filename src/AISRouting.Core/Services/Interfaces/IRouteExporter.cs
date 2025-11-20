using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for exporting optimized route waypoints to XML format.
    /// </summary>
    public interface IRouteExporter
    {
        /// <summary>
        /// Exports a collection of route waypoints to an XML file.
        /// </summary>
        /// <param name="waypoints">The waypoints to export.</param>
        /// <param name="outputFilePath">The full path to the output file.</param>
        /// <param name="options">Optional export configuration. If null, default options are used.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>A task representing the asynchronous export operation.</returns>
        /// <exception cref="ArgumentException">Thrown when waypoints collection is empty or path is invalid.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when output path is not writable.</exception>
        /// <exception cref="OperationCanceledException">Thrown when user cancels the export.</exception>
        Task ExportRouteAsync(
            IEnumerable<RouteWaypoint> waypoints,
            string outputFilePath,
            ExportOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
