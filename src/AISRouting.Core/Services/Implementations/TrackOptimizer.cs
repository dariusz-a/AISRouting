using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AISRouting.Core.Services.Implementations
{
    /// <summary>
    /// Service for optimizing vessel tracks using the Douglas-Peucker algorithm.
    /// </summary>
    public class TrackOptimizer : ITrackOptimizer
    {
        private readonly ILogger<TrackOptimizer> _logger;

        public TrackOptimizer(ILogger<TrackOptimizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RouteWaypoint>> OptimizeTrackAsync(
            IEnumerable<ShipDataOut> positions,
            OptimizationOptions? options = null,
            IProgress<OptimizationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            options ??= OptimizationOptions.Default;

            var positionList = positions.ToList();
            if (positionList.Count == 0)
                throw new ArgumentException("Position list is empty");

            _logger.LogInformation(
                "Optimizing track with {Count} positions using tolerance {Tolerance} meters",
                positionList.Count, options.ToleranceMeters);

            // Run optimization asynchronously (CPU-bound work)
            return await Task.Run(() =>
            {
                var optimized = DouglasPeuckerOptimize(positionList, options.ToleranceMeters,
                    progress, cancellationToken);

                _logger.LogInformation(
                    "Optimization complete: {Original} positions → {Optimized} waypoints",
                    positionList.Count, optimized.Count);

                return optimized;
            }, cancellationToken);
        }

        private List<RouteWaypoint> DouglasPeuckerOptimize(
            List<ShipDataOut> positions,
            double tolerance,
            IProgress<OptimizationProgress>? progress,
            CancellationToken cancellationToken)
        {
            int defaultedHeading = 0;
            int defaultedSOG = 0;

            // Convert positions to waypoints with Lat/Lon in RADIANS
            var waypoints = positions.Select((p, i) =>
            {
                if (!p.Heading.HasValue) defaultedHeading++;
                if (!p.SOG.HasValue) defaultedSOG++;

                return new RouteWaypoint
                {
                    Index = i + 1,
                    Time = p.BaseDateTime,
                    Lat = ToRadians(p.Lat ?? 0.0),  // Convert degrees to radians
                    Lon = ToRadians(p.Lon ?? 0.0),  // Convert degrees to radians
                    Speed = p.SOG ?? 0.0,
                    Heading = p.Heading ?? 0,
                    Alt = 0,
                    Delay = 0,
                    TrackMode = "Track",
                    PortXTE = 20,
                    StbdXTE = 20,
                    MinSpeed = 0
                };
            }).ToList();

            if (defaultedHeading > 0 || defaultedSOG > 0)
            {
                _logger.LogInformation(
                    "Applied defaults: {HeadingCount} records with missing Heading, {SOGCount} records with missing SOG",
                    defaultedHeading, defaultedSOG);
            }

            // Apply Douglas-Peucker algorithm
            var keepFlags = new bool[waypoints.Count];
            keepFlags[0] = true;
            keepFlags[waypoints.Count - 1] = true;

            DouglasPeuckerRecursive(waypoints, keepFlags, 0, waypoints.Count - 1,
                tolerance, progress, cancellationToken);

            // Extract kept waypoints
            var result = new List<RouteWaypoint>();
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (keepFlags[i])
                {
                    var wp = waypoints[i];
                    wp.Index = result.Count + 1; // Re-index
                    result.Add(wp);
                }
            }

            // Calculate max speed from all waypoints
            var maxSpeed = result.Any() ? result.Max(w => w.Speed) : 0.0;
            foreach (var wp in result)
            {
                wp.MaxSpeed = maxSpeed;
            }

            // Report final progress
            progress?.Report(new OptimizationProgress
            {
                ProcessedPoints = waypoints.Count,
                TotalPoints = waypoints.Count,
                DefaultedHeadingCount = defaultedHeading,
                DefaultedSOGCount = defaultedSOG
            });

            return result;
        }

        private void DouglasPeuckerRecursive(
            List<RouteWaypoint> waypoints,
            bool[] keepFlags,
            int startIndex,
            int endIndex,
            double tolerance,
            IProgress<OptimizationProgress>? progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (endIndex - startIndex <= 1)
                return;

            // Find point with maximum distance from line segment
            double maxDistance = 0;
            int maxIndex = startIndex;

            for (int i = startIndex + 1; i < endIndex; i++)
            {
                double distance = PerpendicularDistance(
                    waypoints[i], waypoints[startIndex], waypoints[endIndex]);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = i;
                }
            }

            // If max distance exceeds tolerance, keep the point and recurse
            if (maxDistance > tolerance)
            {
                keepFlags[maxIndex] = true;

                DouglasPeuckerRecursive(waypoints, keepFlags, startIndex, maxIndex,
                    tolerance, progress, cancellationToken);
                DouglasPeuckerRecursive(waypoints, keepFlags, maxIndex, endIndex,
                    tolerance, progress, cancellationToken);
            }

            // Report progress periodically
            if (endIndex % 100 == 0)
            {
                progress?.Report(new OptimizationProgress
                {
                    ProcessedPoints = endIndex,
                    TotalPoints = waypoints.Count
                });
            }
        }

        private double PerpendicularDistance(RouteWaypoint point,
            RouteWaypoint lineStart, RouteWaypoint lineEnd)
        {
            // Calculate perpendicular distance from point to line segment
            // Using Haversine formula for geographic coordinates
            // NOTE: waypoints already have Lat/Lon in radians, so no conversion needed

            const double earthRadius = 6371000; // meters

            var lat1 = lineStart.Lat;  // Already in radians
            var lon1 = lineStart.Lon;  // Already in radians
            var lat2 = lineEnd.Lat;    // Already in radians
            var lon2 = lineEnd.Lon;    // Already in radians
            var latP = point.Lat;      // Already in radians
            var lonP = point.Lon;      // Already in radians

            // Calculate distance from point to start of line
            var dLat13 = latP - lat1;
            var dLon13 = lonP - lon1;

            var a13 = Math.Sin(dLat13 / 2) * Math.Sin(dLat13 / 2) +
                      Math.Cos(lat1) * Math.Cos(latP) *
                      Math.Sin(dLon13 / 2) * Math.Sin(dLon13 / 2);
            var c13 = 2 * Math.Atan2(Math.Sqrt(a13), Math.Sqrt(1 - a13));
            var d13 = earthRadius * c13;

            // Calculate bearing from start to point
            var bearing13 = Math.Atan2(
                Math.Sin(lonP - lon1) * Math.Cos(latP),
                Math.Cos(lat1) * Math.Sin(latP) - Math.Sin(lat1) * Math.Cos(latP) * Math.Cos(lonP - lon1));

            // Calculate bearing from start to end
            var bearing12 = Math.Atan2(
                Math.Sin(lon2 - lon1) * Math.Cos(lat2),
                Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1));

            // Calculate cross-track distance
            var crossTrack = Math.Abs(Math.Asin(Math.Sin(d13 / earthRadius) * Math.Sin(bearing13 - bearing12)) * earthRadius);

            return crossTrack;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
