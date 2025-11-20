using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for optimizing vessel tracks using simplification algorithms.
    /// </summary>
    public interface ITrackOptimizer
    {
        /// <summary>
        /// Optimizes a track by applying the Douglas-Peucker algorithm to reduce waypoint count.
        /// </summary>
        /// <param name="positions">Input position records.</param>
        /// <param name="options">Optimization options (tolerance, etc.).</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optimized collection of waypoints.</returns>
        Task<IEnumerable<RouteWaypoint>> OptimizeTrackAsync(
            IEnumerable<ShipDataOut> positions,
            OptimizationOptions? options = null,
            IProgress<OptimizationProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
