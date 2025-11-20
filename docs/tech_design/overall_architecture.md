# Overall Architecture: Technology stack, system architecture, key decisions, iteration plan

This document covers the end-to-end architecture for the AISRouting WPF desktop application: technology stack, runtime platform, layered structure, cross-cutting concerns (DI, logging, configuration), architectural decisions, and an implementation plan broken into iterations. It includes happy path and edge/error case scenarios and cross?references other technical design documents.

## 1. Technology Stack Summary
- Platform: Windows 10/11
- Runtime: .NET 8, WPF (XAML) MVVM
- MVVM Toolkit: CommunityToolkit.Mvvm (ObservableObject, RelayCommand)
- DI / Logging: Microsoft.Extensions.DependencyInjection / Logging / Configuration
- Serialization: System.Text.Json (ship static JSON + settings)
- CSV Parsing: CsvHelper (streaming) – fallback to custom CsvParser
- XML Export: System.Xml / LINQ to XML for route template writer
- Testing: NUnit, FluentAssertions, Moq (or NSubstitute)
- Optional UI Automation: FlaUI
- Packaging / Build: dotnet CLI + GitHub Actions (windows-latest)

## 2. High-Level Architecture
Logical layers:
1. Presentation (WPF Views + ViewModels)
2. Application / Orchestration (Commands in ViewModels invoking services)
3. Domain (Models: ShipStaticData, ShipDataOut, RouteWaypoint, TimeInterval, Route)
4. Services (Core logic: scanning, loading, optimizing, exporting)
5. Infrastructure (I/O: file system, CSV, JSON, XML)
6. Cross-Cutting (Logging, Configuration, Cancellation, Progress, Error Handling)

Sequence (Happy Path – Create + Export Track):
1. User selects input folder ? Scanner discovers vessels ? UI lists vessels.
2. User selects vessel ? Static JSON parsed ? Time bounds surfaced.
3. User adjusts time interval ? Validated (start < stop, within range).
4. User clicks Create Track ? Positions loaded (filtered) ? Optimizer produces waypoints.
5. User reviews waypoints (visual list) ? Clicks Export ? XML written to output folder.
6. Success message with filename.

Edge Cases:
- Empty input folder ? Display “No vessels found” + disable Create Track.
- Malformed JSON ? Log warning, skip field, still show vessel with defaults.
- Missing CSV for selected interval ? Inform user, allow interval change.
- Optimization yields very few points ? Warn “Optimization thresholds may be too strict.”
- Export path unwritable ? Show error, keep waypoints in memory.

## 3. Component Responsibilities
(See application_organization.md for detailed project structure.)
- Views: Pure XAML layout; minimal code-behind for view-only concerns.
- ViewModels: Bindable state, commands (RelayCommand), invoke domain services asynchronously with cancellation tokens.
- Domain Models: Immutable or minimal mutable properties; simple validation helpers.
- Services: Encapsulate business rules (track thinning, deviation logic, file enumeration).
- Infrastructure: Concrete implementations of interfaces (IShipPositionLoader, IXmlRouteWriter, etc.).

## 4. Key Interfaces (Cross-Reference: data_models.md)
- ISourceDataScanner: Enumerate MMSI subfolders; return list<ShipStaticData> with min/max date hints.
- IShipStaticDataLoader: Parse <MMSI>.json to ShipStaticData.
- IShipPositionLoader: Load and filter ShipDataOut records for interval.
- ITrackOptimizer: Accept IEnumerable<ShipDataOut>, return optimized IEnumerable<RouteWaypoint>.
- IRouteExporter: Persist waypoints to XML.
- IFolderDialogService: Abstract UI folder selection (to allow unit testing).

## 5. Architectural Decisions (ADRs Summary)
1. WPF (.NET Desktop) chosen over cross-platform (WinUI, MAUI) due to Windows-only requirement and mature ecosystem.
2. MVVM Toolkit chosen for minimal boilerplate vs Prism (simplicity + lean DI integration).
3. CSV streaming parsing to handle large files without memory spikes.
4. Deviation-based optimization (simple thresholds) selected before more complex algorithms (Douglas-Peucker) for initial iteration (YAGNI + fast feedback). Future comparison possible.
5. Plain file system JSON/CSV storage retained until performance thresholds justify relational DB (SQLite) (defer complexity).
6. CancellationTokens integrated to preserve UI responsiveness for long-running operations.
7. Logging via Microsoft.Extensions.Logging to enable pluggable sinks (console, file) without framework lock-in.

## 6. Cross-Cutting Concerns
- Logging: Inject ILogger<T>; each service logs start/end, counts, anomalies (bad rows). (See security_architecture.md for sensitive data handling.)
- Configuration: AppSettings.json or user settings for threshold overrides (heading, distance, SOG, ROT). If missing, fallback to defaults.
- Error Handling: Try/catch around I/O; user-friendly messages surfaced; details logged.
- Performance: Streaming, lazy loading, minimal allocations in optimization loop (reuse math helpers).
- Progress Reporting: IProgress<ScanProgress> & IProgress<OptimizationProgress> patterns.
- Cancellation: User can cancel scan or optimization; partial results discarded safely.

## 7. Data Flow Summary (Cross-Reference Architecture.md Source)
Input Folder ? Scanner & Static Loader ? UI vessel list ? Interval selection ? Position Loader (multi-day) ? Optimizer ? Waypoint list ? Exporter ? XML file.

Edge Cases in Flow:
- Interval crosses midnight multiple days ? Loader aggregates days, filters by absolute timestamp.
- Missing Heading/SOG values (null) ? Defaults applied (0) at waypoint mapping; flagged in log.
- Duplicate/Out-of-order CSV rows ? Loader sorts by time ascending before optimization.

## 8. Optimization Algorithm Overview (Cross-Reference: data_models.md for fields)
Criteria to retain waypoint (logical OR):
- Heading change > 0.2°
- Distance > 5 m (Haversine)
- SOG change > 0.2 kn
- ROT > 0.2 deg/s
Always keep first and last points.
Potential Enhancements (future iteration): Douglas-Peucker comparison, dynamic thresholds by speed regime.

Error Cases:
- All points identical ? Returns first & last only; UI warns of low variability.
- Insufficient points (<2) ? Return original set as-is (no thinning) & log.

## 9. Security & Validation (See security_architecture.md)
- Validate MMSI numeric 9-digit pattern.
- Sanitize file paths (no traversal outside root) using Path.GetFullPath comparison.
- Avoid unvalidated user input in logs; restrict to vetted values.

## 10. Implementation Iterations (High-Level Plan)
Iteration 1: Core skeleton & folder scanning
- Create projects; implement ISourceDataScanner + static data loader; display vessels & static info.
Iteration 2: Time interval selection & position loading
- Implement IShipPositionLoader with multi-day aggregation + filtering; add validation UI.
Iteration 3: Track optimization
- Implement ITrackOptimizer thresholds; show waypoint list.
Iteration 4: XML export
- Implement IRouteExporter + conflict handling (overwrite / suffix); success & error messaging.
Iteration 5: Configurable thresholds & logging improvements
- Add user settings / config injection; enhance logging + progress UI.
Iteration 6 (Future): Advanced algorithms, mapping visualization (Mapsui), batch processing.

## 11. Risks & Mitigations (Edge Considerations)
- Large dataset performance ? Use streaming; consider pre-index cache later.
- Data quality (missing fields) ? Defaulting strategy + log counts of missing attributes.
- User confusion on sparse results ? Provide statistics (#input points, #retained).
- Export conflicts ? Provide overwrite / suffix prompt.
- Cancellation correctness ? Ensure external state (ViewModel lists) updated only after completion.

## 12. Monitoring & Diagnostics
- Counts: vessels discovered, rows parsed, waypoints produced.
- Timings: scan duration, optimization duration.
- Error tally: malformed CSV rows skipped.

## 13. Example Scenario Narratives
Happy Path: Alice selects input folder D:\AISData, picks vessel 205196000 “Evergreen Star”, interval 2024-01-01 00:00 to 2024-01-02 00:00, generates 312 waypoints, exports successfully to D:\AISRoutes.
Edge Path: Bob selects vessel with only one CSV day, chooses interval with no data (future date) ? Loader returns 0 positions; UI shows “No position reports in selected range.” Create Track disabled.

## 14. External References
- AIS Message Formats (link in Architecture.md)
- Haversine & Bearing formulas (math helpers in Core)

Cross-References:
- Data Models: data_models.md
- Organization: application_organization.md
- Security: security_architecture.md
- Layout: application_layout.md
- API Patterns / I/O: api_integration_patterns.md
