# Feature Design:: Track Optimization Algorithm

This document outlines the technical design for the Track Optimization Algorithm feature (Feature 7.1).

> Design Source Context
> Option 2 (Explicit Selection) was used. The implementation plan lists scenarios for Feature 7.1 that reference `docs/spec_scenarios/ais_to_xml_route_convertor_summary.md`. That spec file currently does NOT contain the five optimization scenarios (Optimize track, Always retain first/last, Apply thresholds, Handle empty input, Handle single position). We therefore fall back to the implementation plan's feature header for `feature_name` and treat the listed scenarios as authoritative. A future update should align the BDD spec file with these scenarios. 

## Feature Overview

The Track Optimization Algorithm converts a time‑filtered sequence of raw AIS `ShipState` position reports into a concise, navigationally meaningful sequence of `RouteWaypoint` records. Its business value is twofold:

1. Reduces noisy high-frequency AIS data (potentially thousands of reports) into a manageable route representation for XML export, improving readability and downstream system performance.
2. Preserves operationally significant moments: course changes, speed transitions, turning events, and endpoints—yielding a faithful yet compact depiction of vessel movement within the selected interval.

User Needs Addressed:
- Efficient export without manual data pruning.
- Predictable inclusion of crucial endpoints (first/last positions).
- Configurable sensitivity via `TrackOptimizationParameters` (e.g., heading, distance, speed, rate of turn thresholds).
- Deterministic behavior for edge cases (empty input, single position, all stationary positions).

High-Level Approach:
1. Accept chronologically ordered `ShipState` list (already filtered by user-selected `TimeInterval`).
2. Iterate once (O(n)) retaining first and last always; evaluate each intermediate state against last retained waypoint using threshold comparisons.
3. Convert retained `ShipState` items into `RouteWaypoint` records (sequence numbering, mapping of speed/heading, optional ETA derivation).
4. Return ordered list for XML export.

Architectural Philosophy: Simplicity-first, single-pass, stateless functional logic; pure transformation with clearly defined thresholds; no geometric overfitting (e.g., Douglas–Peucker) until future roadmap phases.

## Architectural Approach

Patterns Applied:
- Single Responsibility: `TrackOptimizer` only concerns waypoint selection.
- Pure Function Design: `Optimize` has no side effects (given inputs → computed outputs).
- Separation of Concerns: Parsing, loading, time filtering occur upstream (Helper/Parsers). Export occurs downstream (XmlExporter).
- Explicit Threshold Evaluation: Each position evaluated against last retained position; algorithm readable and testable.

Component Relationships & Data Flow:
```
LoadShipStates -> (time-filtered List<ShipState>) -> TrackOptimizer.Optimize -> List<RouteWaypoint> -> XmlExporter
```
State Management Strategy:
- All algorithm inputs are immutable data records; results constructed anew.
- No caching—each Process invocation recomputes from current interval selection.

Integration Patterns:
- Invoked via `Helper.OptimizeTrack(states, parameters)` ensuring consistent orchestration.
- Logging (if added) resides in Helper before/after optimization, keeping `TrackOptimizer` pure.

User Experience Impact:
- Displays optimization summary (e.g., "1043 positions reduced to 57 waypoints (94.5% reduction)").
- Ensures quick responsiveness even for large data sets due to O(n) complexity.

## File Structure

Additions follow `application_organization.md` conventions:
```
src/AisToXmlRouteConvertor/
  Optimization/
    TrackOptimizer.cs            # Core optimization logic
  Models/
    TrackOptimizationParameters.cs  # Threshold configuration (already defined)
  Services/
    Helper.cs                   # Calls TrackOptimizer.Optimize
  UnitTests/
    TrackOptimizerTests.cs      # Algorithm behavior tests
```

Purpose Comments:
- `Optimization/TrackOptimizer.cs`: Implements selection algorithm (public `Optimize`).
- `Models/TrackOptimizationParameters.cs`: Tunable thresholds with safe defaults.
- `Services/Helper.cs`: Facade method `OptimizeTrack` for ViewModel orchestration.
- `UnitTests/TrackOptimizerTests.cs`: Scenario coverage (empty, single, threshold triggers, stationary, aggressive vs lenient thresholds).

## Component Architecture

Primary Component: `TrackOptimizer` (static class)

Responsibilities:
- Transform `IReadOnlyList<ShipState>` into optimized `IReadOnlyList<RouteWaypoint>`.
- Guarantee inclusion of first & last valid positions when list length > 1.
- Evaluate threshold criteria: distance, heading change, speed change, rate of turn.

Design Patterns:
- Template style evaluation: Each intermediate candidate runs against predicate `MeetsThreshold(current, lastKept, parameters)`. If true, retain.
- Progressive Reference Update: `lastKept` updates only when waypoint added, providing stable comparison base.

Communication & Relationships:
- Consumes model types only; returns model types; does not reference UI or export code.

State Management & Data Flow:
```
Input states (chronological) → First retained → iterate evaluating thresholds → collect retained indices → map to RouteWaypoint sequence list.
```

Accessibility & Testing Considerations:
- Deterministic output—no randomness.
- Exposes straightforward counts for UI summary and tests.
- Waypoint sequence numbers correlate with retention order—facilitates test assertions.

## Data Integration Strategy

Inputs:
- `states`: Chronologically sorted, time-filtered AIS position reports.
- `parameters`: Thresholds controlling retention sensitivity.

Processing Steps:
1. Short-Circuit Cases: Empty list → empty result; single element → single waypoint.
2. Initialize retained list with index 0.
3. For `i` in [1..n-2], evaluate thresholds vs last retained index.
4. Always append last index (n-1) if n > 1.
5. Convert retained `ShipState` entries to `RouteWaypoint` with incremental sequence numbering.

Threshold Evaluation (any criterion passing retains point):
- Distance >= `MinDistanceMeters`
- |HeadingChange| >= `MinHeadingChangeDeg` (using nullable handling; missing heading treated as 0 or skip heading criterion)
- |SpeedChange| >= `MinSogChangeKnots`
- Rate of Turn change (RotDegPerMin converted to deg/sec) >= `RotThresholdDegPerSec`

Error & Edge Case Handling:
- Null optional numeric fields do not trigger criteria; only non-null comparisons evaluated.
- All identical positions → returns first & last (two waypoints) unless only one.
- Extremely high thresholds → first & last only (valid behavior documented).

Observability for Testing:
- Count of input vs output readily available.
- Ability to derive reduction percentage.
- Waypoint sequences preserve chronological ordering.

## Implementation Examples

### Pseudocode
```text
function Optimize(states, params):
  if states.count == 0: return []
  if states.count == 1: return [toWaypoint(states[0], 1)]
  retainedIndices = [0]
  lastIndex = 0
  for i from 1 to states.count - 2:
    if MeetsThreshold(states[i], states[lastIndex], params):
      retainedIndices.add(i)
      lastIndex = i
  retainedIndices.add(states.count - 1)
  return map retainedIndices with sequence counter to RouteWaypoint
```

### C# Implementation Sketch
```csharp
public static class TrackOptimizer
{
    public static IReadOnlyList<RouteWaypoint> Optimize(
        IReadOnlyList<ShipState> states,
        TrackOptimizationParameters parameters)
    {
        if (states.Count == 0)
            return Array.Empty<RouteWaypoint>();
        if (states.Count == 1)
            return new[] { ToWaypoint(states[0], 1) };

        var retained = new List<int>(capacity: Math.Min(states.Count, 64)) { 0 };
        var lastIndex = 0;
        for (int i = 1; i < states.Count - 1; i++)
        {
            if (MeetsThreshold(states[i], states[lastIndex], parameters))
            {
                retained.Add(i);
                lastIndex = i;
            }
        }
        retained.Add(states.Count - 1);

        var waypoints = new List<RouteWaypoint>(retained.Count);
        int seq = 1;
        foreach (var idx in retained)
            waypoints.Add(ToWaypoint(states[idx], seq++));
        return waypoints;
    }

    private static bool MeetsThreshold(ShipState current, ShipState last, TrackOptimizationParameters p)
    {
        // Distance
        double distance = GeoMath.HaversineDistance(last.Latitude, last.Longitude, current.Latitude, current.Longitude);
        if (distance >= p.MinDistanceMeters) return true;

        // Heading change
        if (current.Heading.HasValue && last.Heading.HasValue)
        {
            double headingDelta = Math.Abs(NormalizeAngle(current.Heading.Value - last.Heading.Value));
            if (headingDelta >= p.MinHeadingChangeDeg) return true;
        }

        // Speed change
        if (current.SogKnots.HasValue && last.SogKnots.HasValue)
        {
            double speedDelta = Math.Abs(current.SogKnots.Value - last.SogKnots.Value);
            if (speedDelta >= p.MinSogChangeKnots) return true;
        }

        // Rate of turn change (convert deg/min → deg/sec)
        if (current.RotDegPerMin.HasValue && last.RotDegPerMin.HasValue)
        {
            double rotDeltaSec = Math.Abs((current.RotDegPerMin.Value - last.RotDegPerMin.Value) / 60.0);
            if (rotDeltaSec >= p.RotThresholdDegPerSec) return true;
        }
        return false;
    }

    private static RouteWaypoint ToWaypoint(ShipState s, int sequence)
        => new(sequence, s.Latitude, s.Longitude, s.SogKnots, s.Heading, null /* ETA derivation future */);

    private static double NormalizeAngle(int delta)
    {
        // Normalize to [-180,180]
        int value = ((delta + 540) % 360) - 180;
        return value;
    }
}
```

Design Notes:
- Capacity preallocation limits reallocations for typical small retained counts.
- Angle normalization ensures large wraparound changes (e.g., 359° → 1°) counted as 2°, not 358°.
- ETA left null pending future enrichment—explicit comment clarifies placeholder.

## Testing Strategy and Quality Assurance

Testable Design Patterns:
- Pure function enabling direct unit tests without mocks.
- Deterministic threshold checks permit boundary-focused test cases.

Unit Test Coverage (Representative Cases):
1. `Optimize_EmptyList_ReturnsEmpty` (edge)
2. `Optimize_SingleItem_ReturnsSingleWaypoint`
3. `Optimize_AllIdenticalPositions_RetainsFirstAndLast`
4. `Optimize_DistanceThresholdTriggered_RetainsExpected`
5. `Optimize_HeadingChangeTriggered_RetainsExpected`
6. `Optimize_SpeedChangeTriggered_RetainsExpected`
7. `Optimize_RateOfTurnTriggered_RetainsExpected`
8. `Optimize_VeryHighThresholds_FirstAndLastOnly`
9. `Optimize_LowThresholds_RetainsAll`

Integration Tests:
- Placed in existing end-to-end workflow verifying reduction percentage and correct waypoint sequencing before XML export.

Negative Scenarios:
- Null optional fields (ensure they do not falsely retain points).
- Extremely sparse movement (all distances < threshold) → only endpoints.

Accessibility & Observability:
- Expose reduction summary for UI: `(originalCount, optimizedCount, reductionPercent)` computed externally from counts.

Mock Data Requirements (Centralized Approach Alignment):
- Although QA testing docs reference TypeScript Playwright patterns, for .NET unit tests we simulate centralized fixtures via `TestDataBuilder.cs` producing lists of `ShipState` with controlled deltas.
- Helper Functions:
  - `BuildLinearStates(count, startLat, startLon, latStep, lonStep, sog, heading)`
  - `BuildHeadingChangeStates(...)`
  - `BuildSpeedVarianceStates(...)`
- Fixtures embody reproducible patterns to trigger each threshold individually.

Data Exposure:
- Waypoint list count & sequences accessible directly.
- First and last timestamps preserved (assert continuity).

## Mock Data Requirements

Centralized Strategy Adaptation for .NET:
- Single factory `TestDataBuilder` supplies reusable sets.
- Structured sets named: `LinearTrackLowVariation`, `HighHeadingVarianceTrack`, `SpeedShiftTrack`, `RotShiftTrack`, `MixedVariationTrack`.
- Each exposes clearly documented construction parameters to maintain traceability between input behaviors and expected retention.

Example Fixture Builder Snippet:
```csharp
public static IReadOnlyList<ShipState> BuildHeadingChangeStates(int points)
{
    var list = new List<ShipState>(points);
    var baseTime = DateTime.UtcNow.Date;
    double lat = 51.0, lon = -1.0; int heading = 0;
    for (int i = 0; i < points; i++)
    {
        heading = (heading + 5) % 360; // Force heading change
        list.Add(new ShipState(
            TimestampUtc: baseTime.AddMinutes(i),
            Latitude: lat + i * 0.0001,
            Longitude: lon + i * 0.0001,
            NavigationalStatusIndex: 0,
            RotDegPerMin: 0.0,
            SogKnots: 12.0,
            CogDegrees: heading,
            Heading: heading,
            DraughtMeters: null,
            DestinationIndex: null,
            EtaSecondsUntil: null));
    }
    return list;
}
```

## Conceptual Explanation Recap

Architectural Philosophy: A lean, interpretable optimization layer favoring clarity and testability over aggressive geometric simplification. Business value realized by dramatically reducing export size while preserving navigationally meaningful inflection points.

Component Relationships: Optimizer sits between parsing/filtering and exporting; purely transforms lists—enabling independent evolution of parsing and export logic.

Data Flow Patterns: Sequential evaluation ensures low memory overhead; decision points codify “meaningful change” semantics via configurable thresholds.

Service Integration: Consumed through `Helper.OptimizeTrack` preserving UI orchestration simplicity.

Design Principles Applied: SRP, pure functions, minimal abstraction, explicit threshold semantics.

## Decision Validation Checklist
- ✓ Uses only approved project structure directories (`Optimization`, `Models`, `Services`).
- ✓ No deprecated components or async complexity.
- ✓ File naming follows conventions.
- ✓ Algorithm pure and unit-testable.
- ✓ Edge cases documented (empty, single, stationary).
- ✓ Threshold semantics explicitly defined.

## Future Enhancements (Roadmap Alignment)
- Optional Douglas–Peucker / Visvalingam-Whyatt algorithm integration behind parameter flag.
- Adaptive thresholds based on statistical variance of movement.
- ETA derivation using cumulative distance & speed modeling.
- Parameter UI allowing user-tunable sensitivity.

## Risks & Mitigations
| Risk | Impact | Mitigation |
|------|--------|-----------|
| Over-retention with low thresholds | Large waypoint list | Provide clear default params & potential UI warnings |
| Under-retention with high thresholds | Loss of subtle maneuvers | Always retain endpoints & allow parameter adjustment |
| Missing BDD spec sync | Spec drift | Update `ais_to_xml_route_convertor_summary.md` to include optimization scenarios |
| Performance with huge inputs | Slight delay | O(n) design; consider streaming enumerator in future |

## Summary
Feature 7.1 delivers a deterministic, single-pass optimization transforming verbose AIS position sequences into concise route waypoints with guaranteed endpoint retention and configurable sensitivity. The implementation adheres to project architectural principles, remains test-friendly, and provides a foundation for future advanced geometric simplifications and user-driven parameter tuning.
