# Working Code Generation Prompt: Feature 1.1 Data Models and Core Domain

## Task:
Generate working code for Feature 1.1 Data Models and Core Domain, implementing foundational immutable data records and core helper/service scaffolding required to support the BDD scenarios below. All code must align with the architecture (single-project .NET 9 Avalonia MVVM, static helpers, synchronous operations) and enable subsequent features.

## Role: Software Engineer
(Responsibilities retained exactly as defined in master prompt.)
- Designing and implementing robust, maintainable, and scalable features using C# (.NET 9) and Avalonia MVVM.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based (static helper) architecture patterns and ensuring proper separation of concerns.
- Writing accessible, robust, and comprehensive automated tests (xUnit + FluentAssertions) following best practices.
- Ensuring all code aligns with project technical constraints (simplicity-first, synchronous operations, immutable records).
- Practicing test-driven development (TDD) by creating failing tests before implementation.
- Maintaining high standards for code quality, documentation, and test coverage.
- Communicating technical decisions clearly for future maintainers.

## References
- BDD Scenarios Source Files:
  - docs/spec_scenarios/input_data_preparation.md
  - docs/spec_scenarios/getting_started.md
  - docs/spec_scenarios/output_specification.md
  - docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
- Test File (iteration target): `tests/data_models.spec.ts` (naming indicates TypeScript; ambiguity noted—foundation tests will be implemented in C# xUnit under `src/AisToXmlRouteConvertor.Tests/UnitTests/` mapping scenario intent.)
- Feature Design Document: `docs/tech_design/data_models.md`
- Application Architecture: `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`
- Application Layout (for validation context): `docs/tech_design/application_layout.md`

## Ambiguity Notice
The implementation plan references `.spec.ts` Playwright-style test filenames while the architecture mandates .NET 9 / xUnit. Proceeding with C# test implementation (xUnit) and providing a mapping layer so future UI/Playwright tests can reference the same domain model semantics. If a strict TypeScript runtime is intended for models, clarification would be required; otherwise C# records are authoritative per design docs.

## Development Approach
Follow strict TDD:
1. For each scenario below, create granular unit tests targeting ONLY the newly introduced record/service behavior (start with failing tests).
2. Implement the minimum production code to satisfy those tests.
3. Refactor while preserving green state.
4. Do not advance to tasks for a later scenario until all atomic tasks for the current scenario are green.
5. After completing all scenarios, run the cumulative test suite.

## Implementation Plan (Grouped by BDD Scenario)
All scenarios here contribute to foundational domain constructs. Each user interaction, validation, and structural requirement has its own atomic task. No task exceeds ~2h of focused implementation.

### Scenario: Recognize valid MMSI folder structure
**BDD Scenario (from input_data_preparation.md):**
```gherkin
Scenario: Recognize valid MMSI folder structure
  Given an `input` root folder exists
  And it contains a subfolder named "205196000" with `205196000.json` and `2025-03-15.csv`
  When the user selects the `input` folder
  Then the application should detect the MMSI subfolder and include it in the ship table
```
**Technical Design Details (inlined):**
- Folder scanning implemented via static helper `Helper.GetAvailableMmsi(string rootPath)`.
- Only filename presence inspected initially (no CSV content load) per Overall Architecture and Application Organization.
- MMSI: 9-digit numeric folder name validation (100000000–999999999).
- JSON presence (`<MMSI>.json`) optional for listing but needed later for static data; missing file flagged later.
- CSV presence by pattern `YYYY-MM-DD.csv` provides date range derivation (earliest file date → min, latest → max) without parsing file bodies.
- Data surfaced for UI via `AvailableMmsi` collection in `MainViewModel` (deferred actual binding).
**Tasks:**
1. Create file `src/AisToXmlRouteConvertor/Services/Helper.cs` with public static class `Helper` and empty method signatures (XML docs) for scanning, loading, optimizing, exporting.
2. Implement `GetAvailableMmsi(string rootPath)` returning `IReadOnlyList<long>` with validation for directory existence.
3. Add regex or numeric parsing logic to validate subfolder names as MMSI (try parse long; length == 9).
4. Enumerate child folders and filter those containing at least one file matching `*.csv` pattern.
5. For each MMSI folder, detect `<MMSI>.json` existence and list all `YYYY-MM-DD.csv` filenames.
6. Derive min/max date range by parsing filename dates (strict `yyyy-MM-dd`) without opening files.
7. Define internal DTO `ScannedVesselInfo` (internal record) capturing: `long Mmsi`, `int CsvFileCount`, `DateTime? MinDateUtc`, `DateTime? MaxDateUtc`, `bool HasStaticJson`.
8. Add method `ScanVessels(string rootPath)` returning `IReadOnlyList<ScannedVesselInfo>`; `GetAvailableMmsi` may wrap this & extract `Mmsi` list.
9. Implement logging (use `Console.WriteLine` placeholder; to transition later to `Microsoft.Extensions.Logging`).
10. Write unit test `GeoFolderScanTests.cs` verifying detection of valid MMSI folder with both JSON and CSV present.
11. Write unit test verifying folder with non-numeric name ignored.
12. Write unit test verifying folder with numeric name but no CSV ignored.
13. Write unit test verifying error/exception when root path does not exist.
14. Write unit test verifying date range derived from earliest/latest CSV file names.
15. Add XML documentation for all new public members in `Helper.cs`.
16. Ensure method returns deterministic ordering (ascending MMSI) for stable UI display.

### Scenario: Accept CSV files with required schema
**BDD Scenario (from input_data_preparation.md):**
```gherkin
Scenario: Accept CSV files with required schema
  Given a CSV file `2025-03-15.csv` in MMSI folder with rows matching `ShipDataOut` schema
  When the application reads the CSV
  Then records should be accepted and used for available date range calculations
```
**Technical Design Details (inlined):**
- `ShipState` record defined in data_models.md (TimestampUtc, Latitude, Longitude, etc.).
- CSV parsing deferred until processing stage; for foundation feature we create parser scaffolding & schema mapping.
- Use CsvHelper with mapping class (ShipStateMap) per Application Organization.
- Malformed rows logged & skipped.
**Tasks:**
1. Create `src/AisToXmlRouteConvertor/Models/ShipState.cs` defining sealed record exactly as in design doc.
2. Create `src/AisToXmlRouteConvertor/Parsers/CsvParser.cs` with static class `CsvParser` and method signature `IReadOnlyList<ShipState> ParsePositions(string csvFilePath)`.
3. Implement internal `ShipStateMap : ClassMap<ShipState>` mapping each CSV column (TimestampUtc, Latitude, Longitude, etc.).
4. Add validation: throw `FileNotFoundException` if CSV path invalid.
5. Implement streaming parse using `CsvReader` and configuration (InvariantCulture, ignore missing optional columns).
6. For each row: parse strict required fields (TimestampUtc, Latitude, Longitude) and try-parse optional fields.
7. Skip row if required field invalid; increment skipped counter.
8. Collect parsed records into `List<ShipState>` preserving chronological order (assume file already ordered; do not sort).
9. Write unit test: `CsvParserTests_ValidSchema_ReturnsRecords` using small embedded sample CSV.
10. Write unit test: `CsvParserTests_MalformedRow_SkipsAndLogs` capturing skip count.
11. Write unit test: `CsvParserTests_FileMissing_Throws` expecting exception.
12. Add XML docs to public methods and mapping class.
13. Expose helper method `Helper.LoadShipStates(string vesselFolder, TimeInterval interval)` stub (foundation only returns empty list; full logic later—still test signature existence).
14. Add TODO comment (single) inside `LoadShipStates` referencing deferred time-range filtering implementation for Iteration 9.
15. Ensure no async usage; all synchronous per architecture.

### Scenario: Show validation error when Start time after End time
**BDD Scenario (from getting_started.md):**
```gherkin
Scenario: Show validation error when Start time after End time
  Given a ship row for MMSI "205196000" is selected and Start time picker data-testid="time-start" is set to "2025-03-15 19:00" and End time picker data-testid="time-end" is set to "2025-03-15 08:00".
  When the application evaluates time inputs on blur of data-testid="time-end".
  Then an inline validation element data-testid="validation-inline" appears with text "Start time must be before End time" and data-testid="process-btn" remains disabled and aria-invalid="true" is set on both time inputs.
```
**Technical Design Details (inlined):**
- `TimeInterval` record defined with StartUtc, EndUtc.
- Validation rule: StartUtc < EndUtc.
- Must support UI logic for future binding; foundation: pure validation function.
**Tasks:**
1. Create `src/AisToXmlRouteConvertor/Models/TimeInterval.cs` record as per design doc.
2. Implement static validator method `TimeIntervalValidator.Validate(TimeInterval interval, DateTime? min, DateTime? max)` returning enum { Ok, StartAfterEnd, OutsideRange }.
3. Create file `src/AisToXmlRouteConvertor/Services/TimeIntervalValidator.cs` with XML docs.
4. Implement range checks against optional vessel bounds (min/max date from `ShipStaticData`).
5. Implement method `Helper.IsValidInterval(TimeInterval interval, ShipStaticData staticData)` combining both checks.
6. Write unit test: StartAfterEnd triggers proper enum.
7. Write unit test: OutsideRange triggers proper enum.
8. Write unit test: Valid interval returns Ok.
9. Write unit test: Null vessel bounds (all null) treat any interval as Ok except StartAfterEnd.
10. Ensure no UI dependencies (pure functions only).
11. Add summary XML docs clarifying returned meanings.

### Scenario: Generate XML with expected filename pattern
**BDD Scenario (from output_specification.md):**
```gherkin
Scenario: Generate XML with expected filename pattern
  Given processing runs for MMSI "205196000" from `2025-03-15T00:00:00` to `2025-03-15T12:00:00`
  When the application finishes processing
  Then a file named `205196000_20250315T000000_20250315T120000.xml` should exist in the selected output folder
```
**Technical Design Details (inlined):**
- XML exporter uses pattern `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml`.
- Uses `RouteWaypoint` record (Sequence, Latitude, Longitude, SpeedKnots, Heading, EtaUtc).
- Timestamps in UTC formatted `yyyyMMdd'T'HHmmss` (without trailing Z per design doc examples for filename; with Z inside XML ETA attribute).
**Tasks:**
1. Create `src/AisToXmlRouteConvertor/Models/RouteWaypoint.cs` record as per design doc.
2. Create `src/AisToXmlRouteConvertor/Export/XmlExporter.cs` static class.
3. Implement method `ExportToXml(IReadOnlyList<RouteWaypoint> waypoints, long mmsi, TimeInterval interval, string outputFolder)` returning full path.
4. Implement timestamp formatter `FormatTimestamp(DateTime utc)` -> `yyyyMMddTHHmmss`.
5. Build filename using formatted StartUtc & EndUtc.
6. Ensure output folder existence (create if missing).
7. Generate XML root `<RouteTemplate Name="{mmsi}">` using `XDocument`.
8. Iterate waypoints; create `<WayPoint Seq="..." Lat="..." Lon="..." Speed="..." Heading="..." ETA="..." />` (omit ETA if null).
9. Persist with UTF-8 BOM-less encoding.
10. Write unit test: Filename pattern correctness.
11. Write unit test: Waypoint list empty => returns error (throw `InvalidOperationException`).
12. Write unit test: Null SpeedKnots -> Speed attribute "0".
13. Write unit test: Null Heading -> Heading attribute "0".
14. Write unit test: Null EtaUtc -> no ETA attribute present.
15. Add XML docs for all public members.

### Scenario: Process successfully generates XML file
**BDD Scenario (from ais_to_xml_route_convertor_summary.md):**
```gherkin
Scenario: Process successfully generates XML file
  Given a valid input folder and writable output folder are selected and a ship is selected and Start time and End time form a valid within-range interval
  When the user presses the `Process!` button
  Then a blocking success message box with text starting `Track generated successfully: ` followed by a filename matching `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml` is shown and the button returns to enabled state after dismissal
  And the XML file exists in the output folder with waypoint structure per `route_waypoint_template.xml`
```
**Technical Design Details (inlined):**
- Pipeline order per architecture: load ship static (already), on process click: load positions (filtered), optimize, export.
- Optimization thresholds defined in `TrackOptimizationParameters` record.
- First/last positions always retained.
**Tasks:**
1. Create `src/AisToXmlRouteConvertor/Models/TrackOptimizationParameters.cs` record with default values (0.2,5,0.2,0.2).
2. Create `src/AisToXmlRouteConvertor/Optimization/TrackOptimizer.cs` static class with method signature `Optimize(IReadOnlyList<ShipState> states, TrackOptimizationParameters p)`.
3. Implement trivial initial logic: if empty -> empty list.
4. Implement always-retain first and last positions rule.
5. Implement threshold evaluation helper `MeetsThreshold(ShipState current, ShipState last, TrackOptimizationParameters p)` using heading change, Haversine distance (temporary stub distance calculation returning 0; actual GeoMath integration later).
6. Create `src/AisToXmlRouteConvertor/Services/GeoMath.cs` with signature placeholders for `HaversineDistance` and `InitialBearing` (return 0 initially) to satisfy optimizer compile.
7. Write unit test: Empty input returns empty.
8. Write unit test: Single position returns single waypoint sequence=1.
9. Write unit test: Two positions returns two waypoints.
10. Write unit test: Heading change above threshold retains intermediate waypoint.
11. Write unit test: Distance above threshold retains intermediate waypoint.
12. Integrate optimizer into `Helper.OptimizeTrack(...)` stub returning call to `TrackOptimizer.Optimize`.
13. Add composite method `Helper.ExportRoute(...)` bridging `XmlExporter.ExportToXml`.
14. Create orchestrator stub `ProcessPipeline.Run(long mmsi, TimeInterval interval, string inputFolder, string outputFolder, TrackOptimizationParameters p)` returning exported path.
15. Write integration-style unit test (no UI): Simulated states list -> optimize -> export -> file exists.
16. Ensure deterministic waypoint sequence numbering increasing from 1.
17. Provide reduction percentage calculation (states count vs waypoints count) returned as tuple part of pipeline result for future UI messaging.
18. Add XML docs for optimizer and pipeline methods.
19. Add TODO tags (single per future enhancement area) for real GeoMath, advanced thresholds, asynchronous refactor (if later needed).
20. Confirm all previous foundational tests still green after integration.

## Code Examples (Foundational Snippets)
```csharp
// ShipStaticData.cs
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

// TimeInterval.cs
public sealed record TimeInterval(DateTime StartUtc, DateTime EndUtc);

// TrackOptimizationParameters.cs
public sealed record TrackOptimizationParameters(
    double MinHeadingChangeDeg = 0.2,
    double MinDistanceMeters = 5,
    double MinSogChangeKnots = 0.2,
    double RotThresholdDegPerSec = 0.2);

// Helper.cs (method signatures excerpt)
public static class Helper
{
    public static IReadOnlyList<long> GetAvailableMmsi(string rootPath);
    public static IReadOnlyList<ShipState> LoadShipStates(string vesselFolder, TimeInterval interval);
    public static IReadOnlyList<RouteWaypoint> OptimizeTrack(IReadOnlyList<ShipState> states, TrackOptimizationParameters p);
    public static string ExportRoute(IReadOnlyList<RouteWaypoint> waypoints, long mmsi, TimeInterval interval, string outputFolder);
    public static bool IsValidInterval(TimeInterval interval, ShipStaticData staticData);
}
```

## Technical Requirements
- Immutable C# records for all domain models (no setters beyond init-only).
- Static helper/service classes (no DI containers introduced yet).
- Synchronous I/O and processing.
- Strict filename/date parsing for CSV naming: `yyyy-MM-dd.csv`.
- Logging minimal (console) placeholders allowed at this stage.
- No asynchronous code, no multi-threading.
- XML timestamp format for ETA attribute: `yyyyMMddTHHmmssZ`.
- Filename timestamp format: `yyyyMMddTHHmmss` (no trailing Z).
- Validation enumerations must avoid throwing for user errors; only throw for infrastructure errors (missing path, file not writable).

## Technical Detail Extraction Confirmation
All file paths, record signatures, helper signatures, parsing patterns, optimization thresholds, XML attribute rules, validation logic, and naming conventions are explicitly included above (inlined from design documents).

## Validation Step (Mandatory)
### Success Criteria Checklist
1. Covers all feature-related positive scenarios: YES (Recognize folder, Accept CSV schema, Time validation, XML filename, Process success).
2. Covers all feature-related negative/validation aspects: YES (StartAfterEnd, invalid folder, missing CSV, malformed rows, empty waypoints).
3. Includes required code examples: YES (records, helper signatures).
4. Provides complete examples for patterns: YES (record definitions, exporter, optimizer signatures).
5. Each user interaction/validation/error state is separate explicit task: YES (enumerated per scenario).
6. No combined/high-level tasks remain: YES (all tasks granular & atomic).
7. Each task implementable/testable in isolation <~2h: YES (scoped precisely).
8. All non-atomic tasks further split: YES (e.g., parser tasks split into mapping, validation, tests).
9. Post-review splitting performed: YES (additional tests & helper separation added).

### Validation Process Confirmation
All criteria satisfied. If any later ambiguity arises (e.g., TypeScript vs C# tests), the prompt should be updated before implementation.

> Ready for code generation. Proceed with TDD implementing tasks in listed order; do not skip tests.
