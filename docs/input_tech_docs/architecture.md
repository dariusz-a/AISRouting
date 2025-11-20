# AisToXmlRouteConvertor Architecture (Avalonia / .NET 9)

This document defines the simplified architecture for the cross‑platform desktop application **AisToXmlRouteConvertor**, implemented with **C#**, **.NET 9**, and **Avalonia UI** using MVVM patterns.

## 1. Goals
- Select an input root folder containing AIS source data organized by MMSI (per‑vessel subfolders).
- Load ship static JSON (`<MMSI>.json`) and daily AIS position CSV files (`YYYY-MM-DD.csv`).
- Allow user to choose vessel and a time interval.
- Convert filtered AIS position reports into an optimized list of route waypoints.
- Export the route as XML (`MMSI-Start-End.xml`) matching navigation system expectations.
- Keep the implementation lean: focus only on core data load, optimize, export.
- Be cross‑platform (Windows, macOS, Linux) via Avalonia.

## 2. Target Platform & Tooling
- OS: Windows 10/11, macOS 13+, Linux (x64) – Avalonia supports cross‑platform.
- Runtime: .NET 9 (TFM `net9.0`).
- IDE: VS 2022+, Rider, VS Code (optional) with `dotnet` CLI.
- Packaging: Self‑contained publish optional for distribution.

## 3. Solution Layout (Proposed)
```
<repo_root>/
├── src/
│   ├── AisToXmlRouteConvertor/            # Single simple project (Avalonia App)
│   │   ├── AisToXmlRouteConvertor.csproj
│   │   ├── App.axaml
│   │   ├── MainWindow.axaml
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Parsers/
│   │   ├── Export/
│   │   └── ViewModels/
│   └── AisToXmlRouteConvertor.Tests/      # Unit tests (xUnit)
│       ├── AisToXmlRouteConvertor.Tests.csproj
│       └── UnitTests/
└──
```

This project uses a single `csproj` to keep everything extremely simple. Small folders inside the project provide light organization without multi‑project complexity.

## 4. Technology Stack
- UI: Avalonia (XAML dialect) with MVVM.
- MVVM Toolkit: `CommunityToolkit.Mvvm` (lightweight and compatible) or `ReactiveUI` (if reactive patterns desired). Initial choice: CommunityToolkit.Mvvm for simplicity.
- DI: `Microsoft.Extensions.DependencyInjection`.
- Logging: `Microsoft.Extensions.Logging` (console + optional file sink like `Serilog` later).
- CSV Parsing: `CsvHelper` (robust) – can fallback to simple `File.ReadLines` split approach if performance acceptable.
- JSON: `System.Text.Json`.
- XML: `System.Xml.Linq` (LINQ to XML) or `XmlWriter` for streaming output.
- Tests: xUnit or NUnit + `Microsoft.NET.Test.Sdk` + `FluentAssertions`.

## 5. Domain Models (in-app)
```csharp
public sealed record ShipStaticData(
    long Mmsi,
    string? Name,
    double? Length,
    double? Beam,
    double? Draught,
    string? CallSign,
    string? ImoNumber,
    DateTime? MinDateUtc,
    DateTime? MaxDateUtc);

public sealed record ShipState(
    DateTime TimestampUtc,
    double Latitude,
    double Longitude,
    int? NavigationalStatusIndex,
    double? RotDegPerMin,
    double? SogKnots,
    double? CogDegrees,
    int? Heading,
    double? DraughtMeters,
    int? DestinationIndex,
    long? EtaSecondsUntil);

public sealed record TimeInterval(DateTime StartUtc, DateTime EndUtc);

public sealed record RouteWaypoint(
    int Sequence,
    double Latitude,
    double Longitude,
    double? SpeedKnots,
    int? Heading,
    DateTime? EtaUtc);
```

## 6. Service Interfaces (in-app)
```csharp
// Note: To keep the app extremely simple all IO and processing helpers are static and synchronous.
// The Helper class provides the small set of operations the UI calls directly and callers wait for results.
// This avoids small-service DI overhead for a tiny utility app.

/// <summary>
/// Lightweight static helper methods used by the minimal single-project app.
/// These are synchronous helpers (no async) to keep the code simple — callers wait for results.
/// </summary>
public static class Helper
{
    // Returns list of available MMSI numbers by scanning the root folder synchronously.
    public static IReadOnlyList<long> GetAvailableMmsi(string rootPath) => throw new NotImplementedException();

    // Loads positions (renamed ShipState) for a vessel in the given interval synchronously.
    public static IReadOnlyList<ShipState> LoadShipStates(string vesselFolder, TimeInterval interval) => throw new NotImplementedException();

    // Optimizer as a static method: takes states and returns optimized waypoints.
    public static IReadOnlyList<RouteWaypoint> OptimizeTrack(IReadOnlyList<ShipState> states, TrackOptimizationParameters parameters) => throw new NotImplementedException();

    // Loads ship static JSON from vessel folder (synchronous).
    public static ShipStaticData? LoadShipStatic(string vesselFolder) => throw new NotImplementedException();

    // Export route to XML synchronously. Returns full path of exported file.
    public static string ExportRoute(IReadOnlyList<RouteWaypoint> waypoints, long mmsi, TimeInterval interval, string outputFolder) => throw new NotImplementedException();
}
```

### Optimization Parameters
```csharp
public sealed record TrackOptimizationParameters(
    double MinHeadingChangeDeg = 0.2,
    double MinDistanceMeters = 5,
    double MinSogChangeKnots = 0.2,
    double RotThresholdDegPerSec = 0.2);
```

## 7. Track Optimization (Simplified Algorithm)
- Iterate chronological positions.
- Always include first + last.
- For each position compare to last kept waypoint:
  - Include if heading delta > `MinHeadingChangeDeg`.
  - Include if Haversine distance > `MinDistanceMeters`.
  - Include if |SOG change| > `MinSogChangeKnots`.
  - Include if ROT (converted to deg/sec) > `RotThresholdDegPerSec`.
- Generate `RouteWaypoint.Sequence` incrementally.
- Compute `EtaUtc` if `EtaSecondsUntil` present (TimestampUtc + offset).

Distance utility (Haversine) and bearing (optional) live in a static helper class `GeoMath` in the same project.

## 8. XML Export Structure
```
<RouteTemplate Name="{MMSI}">
  <WayPoint Seq="1" Lat="..." Lon="..." Speed="..." Heading="..." ETA="20250315T000000Z" />
  ...
</RouteTemplate>
```
- Dates in UTC: `yyyyMMdd'T'HHmmss'Z'`.
- SpeedKnots omitted or set `0` if null.
- Heading default `0` if missing.
- ETA omitted when not available.

## 9. Avalonia UI (Minimal)
Single window with panels

- `BrowseInputFolderCommand`
- `ScanVesselsCommand`
- `LoadPositionsCommand`
- `OptimizeTrackCommand`
- `BrowseOutputFolderCommand`
- `ExportRouteCommand`

State properties: `InputFolder`, `SelectedMmsi`, `AvailableMmsi`, `TimeInterval`, `Positions`, `OptimizedWaypoints`, `OutputFolder`, `StatusMessage`.

## 10. No Dependency Injection
This is a small local desktop app — we deliberately avoid dependency injection. The UI and viewmodels call the synchronous static `Helper` methods directly or construct small objects manually.

## 11. Error Handling & Validation
- Validate folder existence (`Directory.Exists`).
- Filter only subfolders with at least one `.csv` file.
- CSV malformed rows: log and skip (do not stop entire load).
- Ensure `TimeInterval.StartUtc < EndUtc`.
- On export, create output folder if missing, handle file overwrite with suffix strategy (`filename (1).xml`).

## 12. Logging Strategy
- Info: scan start/end, vessel count, positions loaded count, waypoints produced, export success.
- Warning: skipped CSV row due to parse error.
- Error: file access denied, unhandled exceptions in optimization/export.

## 13. Testing Strategy (Lean)
Unit Tests:
- GeoMath distance and heading calculations (edge cases: same point, poles, antimeridian).
- TrackOptimizer: retains first/last; thresholds behave correctly; identical points collapse.
- CSV Parsing: correct field mappings, missing columns fallback.
- XML Export: structure, attribute presence, date formatting.

Integration Tests:
- Sample data folder: end‑to‑end load → optimize → export; assert output file exists & waypoint count.

## 14. Performance Considerations (Minimal App)
- Use streaming CSV parsing (`CsvHelper` with `GetRecords`) or manual line iteration.
- Avoid loading days outside interval.
- Optimization is O(n); no complex spatial indexing required initially.

## 15. Extensibility Roadmap
- Thresholds adjustable in UI.
- Additional optimization modes (Douglas‑Peucker, Visvalingam‑Whyatt).
- Multiple vessel batch export.
- Map preview using `Avalonia.Controls.Maps` or integration with `Mapsui`.
- Additional output formats (GPX, KML, GeoJSON).

## 16. Minimal NuGet Packages
Required First Iteration:
- `Avalonia` + `Avalonia.Desktop` meta packages.
- `CommunityToolkit.Mvvm`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging.Console`
- `CsvHelper`

Testing:
- `xunit`
- `xunit.runner.visualstudio`
- `FluentAssertions`
- `Microsoft.NET.Test.Sdk`

## 17. Build & Run Commands
```bash
# Restore & build
 dotnet restore src/AisToXmlRouteConvertor.sln
 dotnet build src/AisToXmlRouteConvertor.sln -c Release

# Run (desktop)
 dotnet run --project src/AisToXmlRouteConvertor.App

# Tests
 dotnet test src/AisToXmlRouteConvertor.Tests

# Publish (self-contained example Windows x64)
 dotnet publish src/AisToXmlRouteConvertor.App -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## 18. Initial Implementation Order
1. Create solution + Avalonia App project.
2. Add Core project with models + interfaces.
3. Add Infrastructure parsing + exporter implementations.
4. Wire DI & ViewModel with basic UI bindings.
5. Implement TrackOptimizer (basic deviation logic).
6. Add XML export.
7. Provide sample data + unit tests.
8. Polish UX (status messages, error dialogs).

## 19. Non-Goals (Phase 1)
- Advanced map rendering.
- Complex caching or database persistence.
- UI automation tests.
- Live AIS streaming ingestion.

## 20. Summary
The **AisToXmlRouteConvertor** focuses on a lean, cross‑platform workflow: folder selection → vessel/time selection → load → optimize → export. The architecture isolates domain logic for extensibility while keeping initial complexity low.
