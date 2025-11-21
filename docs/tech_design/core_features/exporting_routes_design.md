# Feature Design:: Exporting Routes

This document outlines the technical design for the Exporting Routes feature

## Feature Overview
- Purpose: Export a generated track (RouteWaypoint list) into an XML file following the standard route template structure. This design covers all scenarios in the BDD: successful export with valid path, conflict resolution (overwrite or suffix), write-permission failures, and correct mapping of `WayPoint` attributes from `ShipDataOut` to `RouteWaypoint` and the XML.
- Business Value: Provides users with a portable, standards-compatible route file that can be imported into navigation systems or shared with colleagues. Enables downstream usage of optimized tracks produced by the application.
- High-level approach: Reuse existing layered architecture. The UI orchestration is in the Presentation layer (View + ViewModel). The export workflow delegates serialization and file-system concerns to `IRouteExporter` in Infrastructure. The mapping logic relies on domain models in Core (`RouteWaypoint`, `ShipDataOut`) and utility helpers (time formatting, filename generation). All file operations perform validation and permission checks before write.

**IMPORTANT: Coordinate System** - RouteWaypoint objects store Lat/Lon in **RADIANS** (not degrees). Input CSV files contain coordinates in degrees, which MUST be converted to radians during track generation. The XML export writes these radians values directly. This ensures compatibility with navigation systems that expect the route_waypoint_template.xml format.

## Architectural Approach
- Patterns applied: MVVM for UI, Service Layer for export API (`IRouteExporter`), Dependency Injection for testability, and Repository-like abstraction for filesystem operations to allow mocking in tests. Error handling follows established cross-cutting concerns: specific exceptions, user-friendly messages, and structured logging.
- Component hierarchy and relationships:
  - Presentation: `ExportDialogView.xaml` / `ExportDialogViewModel` (or `MainViewModel.ExportRouteCommand`) drives user interactions and collects `outputFolder` and conflict resolution choice.
  - Core: `RouteWaypoint` model holds serializable waypoint data.
  - Infrastructure: `IRouteExporter` and `RouteExporter` perform filename generation, conflict resolution (Overwrite / AppendSuffix / Cancel), permission checks, and XML serialization using an `XmlRouteWriter` helper.
  - Shared: `IFolderDialogService` and `IMessageBoxService` abstract dialogs and confirmations.
- Data flow and state management: ViewModel holds `GeneratedWaypoints` (ObservableCollection<RouteWaypoint>), `SelectedVessel` (ShipStaticData), and `TimeInterval`. On export command, ViewModel calls `IRouteExporter.ExportToXml(waypoints, outputFolder, mmsi, start, stop, conflictResolution)` and observes progress and exceptions.
- Integration patterns: Use DI-resolved `IRouteExporter` and `IFolderDialogService`. All I/O occurs in Infrastructure; UI only orchestrates and displays results. Use `Task`-based async operations and CancellationToken support.

## File Structure
Following `docs/tech_design/application_organization.md` conventions, add the following files and responsibilities for this feature:
src/AISRouting.App.WPF/
  Views/
    ExportDialogView.xaml            # Export options dialog UI

  ViewModels/
    ExportDialogViewModel.cs         # Binds output folder, conflict choice, and triggers export

src/AISRouting.Core/
  Models/
    RouteWaypoint.cs                 # Domain model (existing)

src/AISRouting.Infrastructure/
  Persistence/
    IRouteExporter.cs                # Interface for export behavior
    RouteExporter.cs                 # Implements export workflow (filename, conflicts, write)
    XmlRouteWriter.cs                # Low-level XML serialization using XmlWriter
    FileSystemHelper.cs              # Permission checks, safe write helpers
    TemplateLoader.cs                # Loads `route_waypoint_template.xml` if needed

tests/
  IntegrationTests/
    XmlExportValidationTests.cs      # Validates generated XML structure and attributes
  UnitTests/
    RouteExporterTests.cs            # Unit tests for conflict handling and filename generation
Purpose notes:
- `ExportDialogViewModel.cs`: Provides `IAsyncRelayCommand ExportCommand` and exposes `ExportResult` and `ErrorMessage` for UI binding.
- `RouteExporter.cs`: Orchestrates filename generation, existence checks, user prompt handling (via `IMessageBoxService`), and calls `XmlRouteWriter.Write(waypoints, path)`.
- `XmlRouteWriter.cs`: Produces the `<RouteTemplates>` root and `<RouteTemplate>` with `<WayPoint />` attributes per data model mapping. Uses `XmlWriterSettings` with UTF-8 and indentation.
- `FileSystemHelper.cs`: Implements `bool CanWriteToFolder(string path)` and `Task SafeWriteFileAsync(string path, Stream data, CancellationToken)` which writes to a temp file then atomically moves into place.

## Component Architecture

1) ExportDialogViewModel (Presentation)
- Purpose and Role: Collect export parameters (output folder, conflict strategy), validate input, and call the `IRouteExporter`.
- Design Patterns: MVVM; uses `IAsyncRelayCommand` for `ExportCommand`. Validates `SelectedVessel`, `GeneratedWaypoints`, and `TimeInterval.IsValid` before enabling the command.
- Information Architecture: Exposes properties for binding: `OutputFolder`, `ConflictResolution` (enum), `IsExporting`, `ProgressMessage`.
- Integration Strategy: Calls `IRouteExporter.ExportToXml(...)`; uses `IFolderDialogService` to open folder picker and `IMessageBoxService` for confirmations.
- Accessibility & UX: Ensure controls have `AutomationProperties.AutomationId` and `data-testid` attributes (for Playwright/E2E). Provide status messages and keyboard focus on first invalid control.
- Testing hooks: `ExportDialogViewModel` constructor receives `IRouteExporter` and `IFolderDialogService` allowing mocks. Expose `ExportResult` for assertions.

2) RouteExporter (Infrastructure)
- Purpose and Role: Responsible for converting `RouteWaypoint` collection into the XML file and handling all filesystem concerns.
- Design Patterns: Service Layer, Template Method for conflict handling. Uses `FileSystemHelper` abstraction to perform atomic writes.
- Responsibilities:
  - Validate `outputFolder` exists and is writable
  - Generate canonical filename using `GenerateFilename(mmsi, start, stop)`
  - If file exists, coordinate conflict resolution (overwrite, append suffix, cancel)
  - Compute `MaxSpeed` across waypoints and ensure attributes are set
  - Call `XmlRouteWriter.Write(waypoints, fullPath)`
  - Log success/failure and throw domain-specific exceptions for the UI to render
- Error handling: Throw `OutputPathNotWritableException`, `ExportCancelledException`, or `ExportFailedException` with context.
- Testing hooks: Make internal methods `virtual` or extract helpers for unit testing (GenerateFilename, ResolveConflictName)

3) XmlRouteWriter (Infrastructure)
- Purpose and Role: Low-level XML generation producing well-formed UTF-8 XML following the specified schema. Independent of ViewModel or UI.
- Implementation notes:
  - Use `XmlWriter` with settings: Indent=true, IndentChars="  ", Encoding=UTF8
  - Root: `<RouteTemplates>`
  - Single `<RouteTemplate Name="{MMSI}" ColorR="1" ColorG="124" ColorB="139">`
  - For each `RouteWaypoint` write `<WayPoint` with attributes `Name, Lat, Lon, Alt, Speed, ETA, Delay, Mode, TrackMode, Heading, PortXTE, StbdXTE, MinSpeed, MaxSpeed` and close `/>`
  - Escape values and format doubles with invariant culture (e.g., `lat.ToString("F6", CultureInfo.InvariantCulture)`).
- Accessibility & Testing: Include XML comment with generation metadata (timestamp, generated-by user id) to help test assertions.

## Data Integration Strategy
- Data flow: `GeneratedWaypoints` (ViewModel) -> `IRouteExporter.ExportToXml` -> `RouteExporter` maps & validates waypoints -> `XmlRouteWriter` serializes XML -> `FileSystemHelper` writes file.
- Service integration: `IRouteExporter` depends only on Core models and `FileSystemHelper`/`TemplateLoader`. It must not perform UI prompts directly; instead it raises conflict events or returns a `ConflictResolutionRequest` so the ViewModel can prompt the user via `IMessageBoxService`.
- Relationship mapping: `ShipDataOut` -> `RouteWaypoint` maps fields per `data_models.md`:
  - `Name` = MMSI string
  - `Lat` = Latitude (double)
  - `Lon` = Longitude (double)
  - `Alt` = 0
  - `Speed` = SOG or 0
  - `ETA` = EtaSecondsUntil or 0
  - `Delay` = 0
  - `Mode` = SetWaypointMode(ShipDataOut) (algorithmic; default "Cruise")
  - `TrackMode` = "Track"
  - `Heading` = Heading or 0
  - `PortXTE`/`StbdXTE` = 20
  - `MinSpeed` = 0
  - `MaxSpeed` = max SOG across all waypoints (computed prior to writing)
- Error handling & edge cases:
  - Missing Lat/Lon: Skip records or map to last-known valid position depending on business rule; default to skipping with a log warning and reducing waypoint count
  - Empty waypoints collection: Throw `InvalidOperationException` and present user-friendly message "No waypoints to export"
  - Output path not writable: Detect via `FileSystemHelper.CanWriteToFolder` and throw `OutputPathNotWritableException`
  - Filename collision: Provide explicit resolution flows: Overwrite, AppendNumericSuffix, Cancel

## Implementation Examples

1) Generate filename helper (illustrative C#):
private string GenerateFilename(string mmsi, DateTime start, DateTime stop)
{
    return $"{mmsi}-{start:yyyyMMddTHHmmss}-{stop:yyyyMMddTHHmmss}.xml";
}
Explanation: Use `InvariantCulture` formatting and the exact pattern required by BDD to ensure deterministic filenames for tests.

2) Compute MaxSpeed and map waypoints:
var maxSog = waypoints.Select(w => w.Speed).DefaultIfEmpty(0).Max();
foreach (var w in waypoints)
{
    w.MaxSpeed = maxSog;
    w.Mode ??= SetWaypointModeFrom(w); // ensure Mode populated
}
Explanation: `MaxSpeed` must be the maximum observed non-zero SOG; code uses DefaultIfEmpty to avoid exception on empty enumerables. `SetWaypointModeFrom` encapsulates logic to derive Mode.

3) Xml writing sketch (illustrative C#):
using (var writer = XmlWriter.Create(fullPath, settings))
{
    writer.WriteStartDocument();
    writer.WriteStartElement("RouteTemplates");
    writer.WriteStartElement("RouteTemplate");
    writer.WriteAttributeString("Name", mmsi);
    writer.WriteAttributeString("ColorR", "1");
    writer.WriteAttributeString("ColorG", "124");
    writer.WriteAttributeString("ColorB", "139");

    foreach (var wp in waypoints)
    {
        writer.WriteStartElement("WayPoint");
        writer.WriteAttributeString("Name", wp.Name);
        writer.WriteAttributeString("Lat", wp.Lat.ToString("F6", CultureInfo.InvariantCulture));
        // ... other attributes ...
        writer.WriteEndElement();
    }

    writer.WriteEndElement(); // RouteTemplate
    writer.WriteEndElement(); // RouteTemplates
    writer.WriteEndDocument();
}
Explain: Use `CultureInfo.InvariantCulture` to ensure decimal separators are '.' and consistent across environments. Write attributes rather than element text to match spec.

## Testing Strategy and Quality Assurance
- Testable design: All I/O is abstracted behind interfaces. ViewModels are thin orchestration layers with commands easily unit-tested. `RouteExporter` and `XmlRouteWriter` are unit-tested for mapping logic and XML output respectively.
- Unit tests:
  - `RouteExporterTests`: filename generation, conflict resolution logic (simulate File.Exists), permission checks using mocked `FileSystemHelper`.
  - `XmlRouteWriterTests`: validate XML structure, attribute names, attribute formats, and presence of `RouteTemplate` and `WayPoint` elements.
- Integration tests (in `tests/IntegrationTests/XmlExportValidationTests.cs`):
  - Full flow using `TestData/205196000` folder producing XML and asserting file exists and matches expected string patterns (MMSI, timestamps, WayPoint counts).
  - Scenario: Export failure when output path not writable — create read-only folder or mock `FileSystemHelper` and assert user-visible error message.
- End-to-end (Playwright) tests:
  - Use `data-testid` on export controls. Steps: generate track using fixtures, click Export, select temp folder, assert file creation and UI confirmation message.

Testing hooks and selectors:
- Export button: `data-testid="export-button"`
- Output folder picker: `data-testid="export-output-folder"`
- Conflict prompt: `data-testid="export-conflict-prompt"` with options `overwrite`, `suffix`, `cancel`

Mock data requirements (centralized approach):
- Reference: `docs/tech_design/testing/QA_testing.md` directives. Use the centralized test data under `tests/TestData/205196000`.
- Fixtures:
  - `export_fixture_generated_track.json` — a small set of `ShipDataOut` records mapped to expected `RouteWaypoint` entries
  - `expected_export_205196000.xml` — expected XML file for the fixture to use in assertions
  - Helper functions: `TestHelpers.CreateTempWritableFolder()`, `TestHelpers.MakeFolderReadOnly()`

## Mock Data Requirements
- Mock Data Objects:
  - `SampleShipDataOut` collection with a few records that include variations: missing Heading, missing SOG, non-zero EtaSecondsUntil, and different SOG values to validate `MaxSpeed`.
  - `SampleRouteWaypoint` list precomputed from the above collection for positive assertions.
- Helper functions (test helpers):
  - `RouteExporterTestHelper.MapCsvToWaypoints(IEnumerable<ShipDataOut>)` — produces `RouteWaypoint` list used by `XmlRouteWriterTests`.
  - `FileSystemTestHelper.CreateTempFolder(out string path)` — creates temp folder and returns path for test cleanup.
  - `XmlAssert.AreEqualIgnoringWhitespace(expectedXml, actualXml)` — compares XML structures ignoring formatting differences.
- Test Data Fixtures:
  - Place fixtures under `tests/TestData/205196000/` as per `application_organization.md`.
  - Use `route_waypoint_template.xml` from repo root as template reference if needed.
- Data Exposure for tests:
  - `RouteExporter` exposes an optional `WriteToStream(Stream s)` method for tests to capture generated XML without touching filesystem.

## Accessibility and UX Considerations
- All interactive controls must include `AutomationProperties.Name` and `AutomationProperties.AutomationId` for UI automation and accessibility.
- Export dialog includes keyboard shortcuts and focus management. Error messages are exposed via `Message` area with `data-testid="export-error"`.

## Changelog
> **Changelog**
> Updated on: 2025-11-20
> - Added design document for Feature 4.1: Exporting Routes
> - Specified file layout, component responsibilities, and testing strategy

## Next Steps
- Implement `ExportDialogViewModel`, `RouteExporter`, and `XmlRouteWriter` following patterns above.
- Add unit and integration tests described in Testing Strategy.
- Run tests and iterate on any issues found.
