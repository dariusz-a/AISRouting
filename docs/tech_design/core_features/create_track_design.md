# Feature Design:: Create Track

This document outlines the technical design for the Create Track feature.

## Feature Overview

The Create Track feature generates an optimized route (sequence of waypoints) from AIS CSV records for a selected vessel and user-defined time range. It covers the full workflow described in the BDD scenarios: folder scanning and vessel selection preconditions, start/stop time selection with second resolution, robust CSV parsing, optimization to remove spurious points, handling of malformed or incomplete rows, permission gating of the Create action, and user-facing feedback/messages.

Business value: enables analysts and navigators to produce compact, exportable route templates from raw AIS logs so downstream navigation tools can consume concise, relevant waypoints instead of raw noisy telemetry.

User needs addressed:
- Quick selection of vessel and time window
- Reliable handling of noisy or malformed data
- Predictable and testable transformation from raw records → optimized waypoints
- Clear user feedback for errors and data-quality notes

High-level approach: follow the layered architecture (WPF MVVM presentation → Core services for optimization → Infrastructure for file I/O). Design emphasizes testability (observable ViewModel state, automation IDs for UI tests, centralized mock data for deterministic test fixtures) and graceful degradation for malformed input.

## Architectural Approach

Architectural patterns and principles applied:
- MVVM for separation of UI and logic (use CommunityToolkit.Mvvm ObservableObject / RelayCommand)
- Service layer with DI for ISourceDataScanner, IShipStaticDataLoader, IShipPositionLoader, ITrackOptimizer, IRouteExporter
- Single Responsibility and Dependency Inversion: each service has one responsibility and depends on interfaces
- Streaming CSV parsing and cancellation support for responsive UI

Component hierarchy and responsibilities:
- Views: `ShipSelectionView`, `TimeIntervalView`, `TrackResultsView`
- ViewModels: `MainViewModel` orchestrates flow; `ShipSelectionViewModel` and `TimeIntervalViewModel` encapsulate their parts
- Core services: `IShipPositionLoader` (load/filter records), `ITrackOptimizer` (compute RouteWaypoint list)
- Infrastructure: `SourceDataScanner`, `PositionCsvParser`, `RouteExporter`

Data flow and state management:
1. `IFolderDialogService` signals selected input root → `ISourceDataScanner.ScanInputFolder()` returns `ShipStaticData[]` bound to `AvailableVessels`.
2. User picks `SelectedVessel` and time interval (`TimeInterval.Start`/`Stop` bind to `TimeIntervalViewModel`).
3. On `CreateTrackCommand` (async): `MainViewModel` asks `IShipPositionLoader.LoadPositions(mmsi, timeInterval, token)` which returns an async stream or Task<IEnumerable<ShipDataOut>>.
4. `ITrackOptimizer.OptimizeTrack(positions, parameters)` returns `IEnumerable<RouteWaypoint>`; results assigned to `GeneratedWaypoints` observable collection and persisted for export.

Integration patterns: use DI for all services (see `App.xaml.cs` registration in `overall_architecture.md`). All file I/O and parsing live in `AISRouting.Infrastructure`; optimization lives in `AISRouting.Core` ensuring unit-testable algorithms.

User experience strategy: keep UI responsive with progress reporting, disable Create when prerequisites unmet (no vessel, invalid times, input root empty, or insufficient permissions). Show clear inline messages and non-blocking banners for warnings (malformed rows, defaults applied).

## File Structure

Follow `docs/tech_design/application_organization.md` patterns. Files added/used for this feature:

```
src/AISRouting.App.WPF/
  Views/
    ShipSelectionView.xaml          # Vessel combo + static data display (AutomationId: "ship-combo")
    TimeIntervalView.xaml           # Start/Stop pickers (AutomationId: "start-picker", "stop-picker")
    TrackResultsView.xaml           # Waypoint list + status banner (AutomationId: "track-results-list")
  ViewModels/
    ShipSelectionViewModel.cs       # Load vessels, expose AvailableVessels
    TimeIntervalViewModel.cs        # Validate second-resolution times
    TrackResultsViewModel.cs        # Exposes GeneratedWaypoints and status messages
    MainViewModel.cs                # Orchestrates commands and services

src/AISRouting.Core/
  Models/
    ShipDataOut.cs
    RouteWaypoint.cs
    TimeInterval.cs
    OptimizationParameters.cs
  Services/Interfaces/
    IShipPositionLoader.cs
    ITrackOptimizer.cs
    IGeoCalculator.cs                # Haversine, bearing, perpendicular distance calculations
  Services/Implementations/
    TrackOptimizer.cs                # Multi-stage optimization pipeline
    DeviationDetector.cs             # Stage 1: Threshold-based filtering
    GeoCalculator.cs                 # Geodesic calculations for optimization

src/AISRouting.Infrastructure/
  IO/
    SourceDataScanner.cs
    PositionCsvParser.cs            # CsvHelper wrapper; yields ShipDataOut records
  Persistence/
    RouteExporter.cs

tests/
  UnitTests/
    Core/TrackOptimizerTests.cs
    Infrastructure/PositionCsvParserTests.cs
  IntegrationTests/
    CreateTrackEndToEndTests.cs     # uses centralized mock fixtures
```

Purpose comments:
- `PositionCsvParser.cs` is responsible for defensive parsing and logging of malformed rows and must expose counters (skippedRows) for UI warnings.
- `TrackResultsViewModel` exposes `IReadOnlyList<RouteWaypoint> GeneratedWaypoints` and `DataQualityNotes` for tests to assert.

## Component Architecture

Main components and roles:

- ShipSelectionView / ShipSelectionViewModel
  - Purpose: show available vessels, allow selection
  - Patterns: simple data-binding to `AvailableVessels` + command to refresh
  - Test hooks: `AutomationProperties.AutomationId="ship-combo"`, `data-test="ship-item-{mmsi}"`

- TimeIntervalView / TimeIntervalViewModel
  - Purpose: select start/stop with second resolution, validate `TimeInterval.IsValid`
  - Validation: disable Create when `!TimeInterval.IsValid` or outside vessel MinDate/MaxDate
  - Test hooks: `AutomationId` attributes on pickers

- MainViewModel
  - Coordinates CreateTrack workflow, handles permissions and UI state
  - Commands: `CreateTrackCommand`, `SelectInputFolderCommand`, `ExportRouteCommand`
  - CancellationToken support and progress reporting via `IProgress<double>` bound to progress bar

- IShipPositionLoader / PositionCsvParser
  - Purpose: enumerate CSV files for interval, stream-parse rows using CsvHelper, filter by absolute timestamp
  - Error handling: skip invalid rows, log details, return `skippedRows` count via result object
  - Edge cases: missing Heading/SOG mapped to 0 per `data_models.md`

- ITrackOptimizer / TrackOptimizer
  - Purpose: convert filtered `ShipDataOut` sequence → compact `RouteWaypoint` list using multi-stage optimization
  - Design patterns: 
    - Stage 1: Threshold-based deviation detection (Heading, Distance, SOG, ROT)
    - Stage 2: Collinearity-based simplification (fast O(n) pre-filter)
    - Stage 3: Douglas-Peucker algorithm for geometric optimization
    - Stage 4: Optional temporal spacing enforcement
  - Architectural rationale: Multi-stage approach balances speed and quality. Deviation detection provides initial filtering, collinearity removes obvious redundancy in O(n) time, Douglas-Peucker preserves route shape optimally, and temporal spacing ensures practical waypoint distribution
  - Testability: public method `OptimizeTrack(IEnumerable<ShipDataOut>, OptimizationParameters)` with deterministic outputs for unit tests; each stage independently testable

Communication patterns: ViewModels call services via injected interfaces; services return POCO models. All service results are serializable and simple for assertion in tests.

Accessibility and interaction patterns: keyboard focus, semantic labels, and AutomationProperties on controls. Provide clear error banners and tooltips (e.g., "Insufficient privileges").

## Data Integration Strategy

How data flows:
- `ISourceDataScanner` reads folder structure, produces `ShipStaticData` with `MinDate`/`MaxDate`.
- `IShipPositionLoader` identifies relevant CSV files and yields `ShipDataOut` records (Time normalized to absolute timestamps) using `PositionCsvParser.ParseCsvFile()`; filtering applied to only include records within the selected `TimeInterval`.
- `ITrackOptimizer.OptimizeTrack()` consumes the filtered sequence and emits `RouteWaypoint` objects following mapping rules in `data_models.md`.

Service integration patterns:
- All I/O is asynchronous and cancellable. Use `IAsyncEnumerable<ShipDataOut>` for streaming large datasets when possible.
- Use small, focused DTOs for inter-service contracts (no UI types in Core/Infrastructure).

Error handling and edge cases:
- Malformed CSV rows: `PositionCsvParser` records row indices and messages to a `CsvParseResult` returned alongside the positions; UI shows "Some rows were ignored due to invalid format" when `skippedRows > 0`.
- Missing Heading/SOG: mapper sets Heading or Speed to 0, logs a Warning, and UI shows a data-quality note.
- No vessel selected / input root empty / missing permissions: pre-check in `MainViewModel` disables `CreateTrackCommand` and sets inline messages.

E2E testing considerations: centralize test fixtures to reproduce noisy data and malformed rows. Use deterministic timestamps and small CSVs in `tests/TestData/205196000/`.

## Track Optimization Strategy

### Multi-Stage Optimization Pipeline

The track optimization uses a four-stage pipeline designed to progressively reduce waypoint count while preserving route fidelity:

**Stage 1: Deviation-Based Filtering**
- Purpose: Remove waypoints during stable transit (constant heading, speed, position)
- Method: Threshold comparison for heading change, distance, SOG change, ROT
- Performance: O(n) - single pass
- Typical reduction: 50-80%
- Why first: Eliminates obvious redundancy quickly with minimal computation

**Stage 2: Collinearity Simplification**
- Purpose: Remove intermediate points on nearly straight line segments
- Method: Calculate bearing change between consecutive triplets; remove if < threshold (e.g., 2°)
- Performance: O(n) - single pass
- Typical reduction: 40-60% of remaining waypoints
- Why second: Fast pre-filter that dramatically reduces input to expensive geometric algorithms

**Stage 3: Douglas-Peucker Algorithm**
- Purpose: Optimal geometric simplification preserving route shape
- Method: Recursively find and retain points with maximum perpendicular distance to line segments
- Performance: O(n log n) average case
- Typical reduction: 70-90% of remaining waypoints
- Why third: Industry-standard algorithm proven for GPS track simplification; preserves visual and navigational accuracy
- Trade-off: More computationally expensive but produces optimal results for maritime routes with mixed straight and curved segments

**Stage 4: Temporal Spacing (Optional)**
- Purpose: Ensure practical waypoint distribution in time
- Method: Enforce minimum time interval between waypoints (e.g., 5 minutes)
- Performance: O(n) - single pass
- Typical reduction: 10-30%
- Why last: Applied after geometric optimization to avoid removing navigationally significant points

**Overall Pipeline Performance**: From 720 position reports → typically 35-50 waypoints (93-95% reduction)

### Douglas-Peucker Algorithm Implementation

The Douglas-Peucker algorithm is the core geometric optimization technique. It works by:

1. Given a sequence of waypoints, find the point with maximum perpendicular distance to the line connecting first and last points
2. If that distance exceeds epsilon threshold (e.g., 25 meters), split the sequence at that point and recurse on both halves
3. If all points are within threshold, keep only first and last points
4. Recursion naturally preserves turning points and route curvature

**Architectural rationale**: Douglas-Peucker is chosen because:
- Proven algorithm used in cartography, GIS, and GPS track simplification since 1973
- Guarantees that all removed points are within epsilon distance of the simplified route
- Preserves critical navigation points (course changes, waypoints near hazards)
- Single tunable parameter (perpendicular distance threshold) is intuitive for maritime applications
- Works exceptionally well for maritime routes with long straight transits interrupted by turns

**Business value**: Reduces XML file size, improves navigation system performance, focuses navigator attention on significant course changes rather than GPS noise.

## Implementation Examples

### OptimizationParameters Model

```csharp
public class OptimizationParameters
{
    // Stage 1: Deviation detection thresholds
    public double HeadingChangeThreshold { get; set; } = 0.2;  // degrees
    public double DistanceThreshold { get; set; } = 5.0;        // meters
    public double SOGChangeThreshold { get; set; } = 0.2;       // knots
    public double ROTThreshold { get; set; } = 0.2;             // deg/s
    
    // Stage 2: Collinearity threshold
    public double BearingChangeThreshold { get; set; } = 2.0;   // degrees
    
    // Stage 3: Douglas-Peucker threshold
    public double PerpendicularDistanceThreshold { get; set; } = 25.0; // meters
    
    // Stage 4: Optional temporal spacing
    public bool EnforceTemporalSpacing { get; set; } = false;
    public TimeSpan MinTemporalInterval { get; set; } = TimeSpan.FromMinutes(5);
}
```

### TrackOptimizer Multi-Stage Pipeline

```csharp
public class TrackOptimizer : ITrackOptimizer
{
    private readonly IDeviationDetector _deviationDetector;
    private readonly IGeoCalculator _geoCalculator;
    private readonly ILogger<TrackOptimizer> _logger;

    public async Task<IEnumerable<RouteWaypoint>> OptimizeTrack(
        IEnumerable<ShipDataOut> positions,
        OptimizationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var positionList = positions.ToList();
        _logger.LogInformation($"Starting optimization with {positionList.Count} position reports");
        
        // Stage 1: Deviation-based filtering
        var stage1 = ApplyDeviationDetection(positionList, parameters).ToList();
        _logger.LogInformation($"Stage 1 (Deviation): {positionList.Count} → {stage1.Count} waypoints");
        
        // Stage 2: Fast collinearity removal
        var stage2 = RemoveCollinearPoints(stage1, parameters.BearingChangeThreshold).ToList();
        _logger.LogInformation($"Stage 2 (Collinearity): {stage1.Count} → {stage2.Count} waypoints");
        
        // Stage 3: Douglas-Peucker geometric optimization
        var stage3 = DouglasPeucker(stage2, parameters.PerpendicularDistanceThreshold).ToList();
        _logger.LogInformation($"Stage 3 (Douglas-Peucker): {stage2.Count} → {stage3.Count} waypoints");
        
        // Stage 4: Optional temporal spacing
        var final = parameters.EnforceTemporalSpacing 
            ? EnforceTemporalSpacing(stage3, parameters.MinTemporalInterval).ToList()
            : stage3;
            
        if (parameters.EnforceTemporalSpacing)
            _logger.LogInformation($"Stage 4 (Temporal): {stage3.Count} → {final.Count} waypoints");
        
        _logger.LogInformation($"Optimization complete: {positionList.Count} → {final.Count} waypoints (" +
            $"{100.0 * (positionList.Count - final.Count) / positionList.Count:F1}% reduction)");
        
        return final;
    }

    private IEnumerable<RouteWaypoint> DouglasPeucker(
        List<RouteWaypoint> waypoints, 
        double epsilon)
    {
        if (waypoints.Count < 3)
            return waypoints;

        // Find point with maximum perpendicular distance from line start→end
        int maxIndex = 0;
        double maxDistance = 0.0;
        
        var start = waypoints[0];
        var end = waypoints[waypoints.Count - 1];
        
        for (int i = 1; i < waypoints.Count - 1; i++)
        {
            double distance = _geoCalculator.PerpendicularDistanceToSegment(
                waypoints[i].Lat, waypoints[i].Lon,
                start.Lat, start.Lon,
                end.Lat, end.Lon);
                
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }
        
        // If max distance exceeds threshold, recursively simplify both sides
        if (maxDistance > epsilon)
        {
            var leftSegment = waypoints.Take(maxIndex + 1).ToList();
            var rightSegment = waypoints.Skip(maxIndex).ToList();
            
            var leftResult = DouglasPeucker(leftSegment, epsilon);
            var rightResult = DouglasPeucker(rightSegment, epsilon);
            
            // Concatenate, skipping duplicate point at junction
            return leftResult.Concat(rightResult.Skip(1));
        }
        
        // All intermediate points within threshold - keep only endpoints
        return new[] { waypoints[0], waypoints[waypoints.Count - 1] };
    }

    private IEnumerable<RouteWaypoint> RemoveCollinearPoints(
        List<RouteWaypoint> waypoints,
        double bearingThreshold)
    {
        if (waypoints.Count < 3)
            return waypoints;
        
        var result = new List<RouteWaypoint> { waypoints[0] };
        
        for (int i = 1; i < waypoints.Count - 1; i++)
        {
            var prev = result[result.Count - 1];
            var current = waypoints[i];
            var next = waypoints[i + 1];
            
            double bearing1 = _geoCalculator.Bearing(
                prev.Lat, prev.Lon, current.Lat, current.Lon);
            double bearing2 = _geoCalculator.Bearing(
                current.Lat, current.Lon, next.Lat, next.Lon);
            
            // Normalize bearing difference to [0, 180]
            double bearingChange = Math.Abs(bearing2 - bearing1);
            if (bearingChange > 180)
                bearingChange = 360 - bearingChange;
            
            // Keep point if significant bearing change
            if (bearingChange > bearingThreshold)
            {
                result.Add(current);
            }
        }
        
        result.Add(waypoints[waypoints.Count - 1]);
        return result;
    }

    private IEnumerable<RouteWaypoint> EnforceTemporalSpacing(
        List<RouteWaypoint> waypoints,
        TimeSpan minInterval)
    {
        if (waypoints.Count < 2)
            return waypoints;
        
        var result = new List<RouteWaypoint> { waypoints[0] };
        DateTime lastTime = GetTimestamp(waypoints[0]);
        
        for (int i = 1; i < waypoints.Count - 1; i++)
        {
            DateTime currentTime = GetTimestamp(waypoints[i]);
            if (currentTime - lastTime >= minInterval)
            {
                result.Add(waypoints[i]);
                lastTime = currentTime;
            }
        }
        
        result.Add(waypoints[waypoints.Count - 1]); // Always keep last
        return result;
    }
    
    private DateTime GetTimestamp(RouteWaypoint wp)
    {
        // Extract timestamp from waypoint metadata or ETA field
        // Implementation depends on how timestamp is stored in RouteWaypoint
        throw new NotImplementedException("Timestamp extraction logic");
    }
}
```

### GeoCalculator Implementation

```csharp
public class GeoCalculator : IGeoCalculator
{
    private const double EarthRadiusMeters = 6371000.0;

    public double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert to radians
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusMeters * c;
    }

    public double Bearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);
        
        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2) -
                Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        
        var bearing = ToDegrees(Math.Atan2(y, x));
        return (bearing + 360) % 360; // Normalize to [0, 360)
    }

    public double PerpendicularDistanceToSegment(
        double pointLat, double pointLon,
        double line1Lat, double line1Lon,
        double line2Lat, double line2Lon)
    {
        // Calculate perpendicular distance from point to line segment
        // using cross-track distance formula for spherical geometry
        
        var d13 = HaversineDistance(line1Lat, line1Lon, pointLat, pointLon) / EarthRadiusMeters;
        var brng12 = ToRadians(Bearing(line1Lat, line1Lon, line2Lat, line2Lon));
        var brng13 = ToRadians(Bearing(line1Lat, line1Lon, pointLat, pointLon));
        
        var dxt = Math.Asin(Math.Sin(d13) * Math.Sin(brng13 - brng12));
        
        return Math.Abs(dxt * EarthRadiusMeters);
    }
    
    private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private double ToDegrees(double radians) => radians * 180.0 / Math.PI;
}
```

### MainViewModel CreateTrack Command

```csharp
public async Task CreateTrackAsync(CancellationToken token)
{
    if (SelectedVessel == null) throw new InvalidOperationException("No ship selected");
    if (!TimeInterval.IsValid) return;

    var result = await _positionLoader.LoadPositionsAsync(SelectedVessel.MMSI, TimeInterval, token);
    if (result.SkippedRows > 0)
        DataQualityNotes = $"Some rows were ignored due to invalid format ({result.SkippedRows})";

    var waypoints = await _optimizer.OptimizeTrack(result.Positions, _optimizationParameters, token);
    GeneratedWaypoints = waypoints.ToList();
    
    // Display optimization statistics
    var reductionPercent = 100.0 * (result.Positions.Count() - GeneratedWaypoints.Count) / result.Positions.Count();
    StatusMessage = $"Track created: {GeneratedWaypoints.Count} waypoints " +
        $"({reductionPercent:F1}% reduction from {result.Positions.Count()} position reports)";
}
```

PositionCsvParser behavior (summary):
- Use `CsvHelper` with `BadDataFound` and `MissingFieldFound` hooks to log and increment `skippedRows`.
- Parse to `ShipDataOut` using `ShipDataOutMap` mapping class. Yield only valid `IsValid` records but keep counts and examples of invalid rows for diagnostics.

RouteWaypoint mapping snippet:

```csharp
public RouteWaypoint Map(ShipDataOut r)
{
    return new RouteWaypoint
    {
        Name = mmsiString,
        Lat = r.Latitude ?? throw new InvalidOperationException(),
        Lon = r.Longitude ?? throw new InvalidOperationException(),
        Alt = 0,
        Speed = r.SOG ?? 0,
        ETA = r.EtaSecondsUntil ?? 0,
        Heading = r.Heading ?? 0,
        TrackMode = "Track",
    };
}
```

All examples use types and registrations from `overall_architecture.md` (e.g., `IShipPositionLoader`, `ITrackOptimizer`) to ensure architectural compliance.

## Testing Strategy and Quality Assurance

Test-first approach: write Playwright/Integration tests that exercise the scenarios in `docs/spec_scenarios/create_track.md` before implementing logic. Unit tests cover parsing, optimization, and mapping.

Test types and locations:
- Unit tests: 
  - `tests/UnitTests/Core/TrackOptimizerTests.cs` - test each optimization stage independently
  - `tests/UnitTests/Core/GeoCalculatorTests.cs` - verify Haversine, bearing, perpendicular distance calculations
  - `tests/UnitTests/Core/DouglasPeuckerTests.cs` - test algorithm with known input/output pairs
  - `tests/UnitTests/Infrastructure/PositionCsvParserTests.cs`
- Integration tests: `tests/IntegrationTests/CreateTrackEndToEndTests.cs` — uses deterministic CSVs in `tests/TestData/205196000/`.

Testing hooks and selectors:
- Add `AutomationProperties.AutomationId` for WPF controls:
  - `ship-combo`, `start-picker`, `stop-picker`, `create-track-button`, `track-results-list`.
- ViewModels expose observable properties `GeneratedWaypoints`, `DataQualityNotes`, and `OperationStatus` to assert state in tests.

Mock data requirements (centralized approach):
- Follow `docs/tech_design/testing/QA_testing.md` centralized mock data pattern.
- Provide a `src/mocks/mockData.ts` (for Playwright fixtures) containing:
  - `mockInputRoot`: path to `tests/TestData/205196000/`
  - `mockVessel205196000` object with sample `ShipStaticData` fields
  - Helper: `getCsvForRange(start, stop)` returns small CSV content matching `ShipDataOut` schema
- Integration tests import fixtures from `tests/fixtures/` which in turn reference `src/mocks/mockData.ts`.

Example fixture usage (Playwright):
```ts
import { mockVessel205196000 } from '../../src/mocks/mockData';
export const createTrackFixture = { vessel: mockVessel205196000 };
```

Test data fixtures must include:
- A noisy CSV sample (with spurious lat/lon jumps) to validate narrowed-window behavior and Douglas-Peucker effectiveness
- A CSV with long straight segments to validate collinearity optimization
- A CSV with curved route segments to validate Douglas-Peucker shape preservation
- A CSV with deliberately malformed rows (missing lat/lon or invalid values) to validate skipping and warnings
- A CSV with missing Heading and SOG cells to validate defaulting to 0
- Known optimization test cases: given specific waypoint sequences, verify expected Douglas-Peucker output

## Mock Data Requirements

- Centralized mock file: `src/mocks/mockData.ts` with typed interfaces for `ShipStaticData`, `ShipDataOut`, and helper functions `makeCsvRow(...)`.
- Fixtures in `tests/fixtures/` import and adapt mock data for each scenario.
- Unit tests use in-memory collections; integration tests copy `tests/TestData/` to a temp directory and point `IFolderDialogService` to it.

## Changelog

> **Changelog**
> Created on: 2025-11-20
> - Added design for Feature 3.1: Create Track
> 
> Updated on: 2025-11-20
> - Added multi-stage optimization pipeline with Douglas-Peucker algorithm
> - Added GeoCalculator service for geodesic calculations
> - Expanded OptimizationParameters model with tunable thresholds
> - Added detailed implementation examples and architectural rationale

---

End of Create Track design document.
