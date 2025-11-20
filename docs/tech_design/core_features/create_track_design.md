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
  - Purpose: convert filtered `ShipDataOut` sequence → compact `RouteWaypoint` list
  - Design patterns: threshold-based deviation detection (Heading, Distance, SOG, ROT) with pluggable `OptimizationParameters`
  - Testability: public method `OptimizeTrack(IEnumerable<ShipDataOut>, OptimizationParameters)` with deterministic outputs for unit tests

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

## Implementation Examples

MainViewModel CreateTrack command (simplified):

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
- Unit tests: `tests/UnitTests/Core/TrackOptimizerTests.cs`, `tests/UnitTests/Infrastructure/PositionCsvParserTests.cs`.
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
- A noisy CSV sample (with spurious lat/lon jumps) to validate narrowed-window behavior
- A CSV with deliberately malformed rows (missing lat/lon or invalid values) to validate skipping and warnings
- A CSV with missing Heading and SOG cells to validate defaulting to 0

## Mock Data Requirements

- Centralized mock file: `src/mocks/mockData.ts` with typed interfaces for `ShipStaticData`, `ShipDataOut`, and helper functions `makeCsvRow(...)`.
- Fixtures in `tests/fixtures/` import and adapt mock data for each scenario.
- Unit tests use in-memory collections; integration tests copy `tests/TestData/` to a temp directory and point `IFolderDialogService` to it.

## Changelog

> **Changelog**
> Created on: 2025-11-20
> - Added design for Feature 3.1: Create Track

---

End of Create Track design document.
