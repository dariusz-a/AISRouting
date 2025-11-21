# Feature Design:: Output Specification

This document outlines the technical design for the "Output Specification" feature (Implementation Plan reference: Feature 8.1: XML Route File Generation). It covers all scenarios:

- Generate XML with expected filename pattern
- XML content follows template
- Fail when unable to write file
- No automatic folder open after generation

The design explains architectural approach, file structure integration, component responsibilities, data flow, testing enablement, and mock data requirements for generating XML route files from optimized waypoints produced earlier in the workflow.

## Feature Overview
The Output Specification feature is responsible for transforming optimized navigation waypoints (derived from AIS positional data) into a persistable XML route file with a deterministic name and structure. Business value: it produces a standard, machine-readable artifact that downstream navigation systems or manual route planning tools can ingest without post-processing. User needs addressed:

- Assurance that exported routes use a predictable, sortable filename containing MMSI and precise time interval boundaries.
- Confidence that XML schema is consistent (matches `route_waypoint_template.xml`).
- Clear feedback when export fails due to permission or I/O problems.
- Non-intrusive UX: success messaging without forcing a folder open (respecting user workflow and privacy/security constraints).

High-level approach: A pure, synchronous export pipeline converts in-memory `RouteWaypoint` records into an XML document constructed via LINQ to XML, applying formatting rules (timestamps, numeric defaults) and writing atomically to the selected output folder. Simplicity-first principles: single static exporting helper, no DI, immediate failure reporting, minimal branching logic, isolated responsibility.

## Architectural Approach
Patterns applied:
- Single Responsibility: Exporter only serializes waypoints → XML file.
- MVVM Integration: ViewModel invokes helper method after optimization completes; exporter has no UI concerns.
- Immutable Input: Consumes completed `IReadOnlyList<RouteWaypoint>` without modifying data.
- Deterministic Construction: Filename and XML structure derived from inputs and formatting rules (no randomness or external dependencies).
- Fail Fast: Permission or write errors surfaced immediately with clear error message.
- Separation of Concerns: Optimization (previous feature) and export are decoupled; exporter does not decide which waypoints exist.

Component relationships:
- `MainViewModel` (UI orchestration) calls `Helper.ExportRoute`.
- `Helper.ExportRoute` internally delegates to specialized `XmlExporter.ExportToXml` (or directly contains logic—design supports either; recommended specialization for clarity and unit test targeting).
- Exporter builds XML using `System.Xml.Linq` to avoid string concatenation and ensure correctness.

Data flow & state management:
1. User triggers Process (earlier features) → track optimized → `List<RouteWaypoint>` prepared.
2. User has valid `TimeInterval`, selected `Mmsi`, chosen writable output folder.
3. ViewModel invokes export, receives path string, updates status message.
4. No automatic folder navigation invoked.

Integration patterns:
- Pure function style: `ExportToXml(waypoints, mmsi, interval, outputFolder)` returns full path or throws exception.
- Exceptions converted into user-facing messages at ViewModel boundary.

User experience strategy:
- On success: succinct status line “Route exported: <filename>”.
- On failure: inline or modal error “Selected folder is not writable. Choose a different folder.” matching scenario wording.
- Avoid side effects (no folder auto-open, no blocking dialogs beyond error).

## File Structure
Following `application_organization.md` patterns (Export folder for output logic). New or clarified files for this feature:

```
src/AisToXmlRouteConvertor/
└── Export/
    ├── XmlExporter.cs              # Core XML generation logic
    ├── RouteFilenameFormatter.cs   # (Optional) Isolated filename policy logic
    └── Templates/
        └── route_waypoint_template.xml  # Reference template (optional embed or docs only)

src/AisToXmlRouteConvertor/Services/Helper.cs  # Calls XmlExporter

src/AisToXmlRouteConvertor.Tests/UnitTests/
    └── XmlExporterTests.cs

src/AisToXmlRouteConvertor.Tests/TestData/
    ├── sample_waypoints.json   # Mock waypoint list fixture
    └── empty_waypoints.json    # Edge case fixture
```

Comments (purpose):
- `XmlExporter.cs`: Converts waypoints list to XML XDocument and persists file.
- `RouteFilenameFormatter.cs`: Centralizes filename generation/improves test isolation (optional—could be inside exporter for simplicity).
- `Templates/route_waypoint_template.xml`: Serves as human-readable reference; generation logic constructs dynamically (no runtime parsing required).
- Test fixtures support deterministic unit tests for file naming and XML node content.

## Component Architecture
Components and their conceptual roles:

1. `XmlExporter`
   - Purpose: Serialize waypoints list into a valid route XML file.
   - Design Pattern: Functional static utility (no state). Uses Builder pattern implicitly via LINQ to XML element composition.
   - Information Architecture: Root `<RouteTemplate Name="{MMSI}">` containing ordered `<WayPoint>` elements, each mapping sequence and navigational attributes.
   - User Experience Strategy: Indirect—exposed only via ViewModel messaging.
   - Integration: Consumes domain record list; depends on no UI assemblies.
   - Accessibility: Not applicable (non-visual component); testability via deterministic output.
   - Testing Considerations: Provide reliable selectors by attribute names (`Seq`, `Lat`, `Lon`, `Speed`, `Heading`, `ETA`). XML parsed using `XDocument` for assertions.

2. `RouteFilenameFormatter` (optional)
   - Purpose: Encapsulate the canonical naming pattern to avoid duplication.
   - Pattern: Strategy / Policy object (static method variant for simplicity).
   - Value: Single location to adapt naming conventions (e.g., future locale/time format changes).
   - Testing: Unit test verifies formatting for boundary times and zero-padding.

3. `Helper.ExportRoute`
   - Purpose: Orchestrate export: validate inputs (folder writable, waypoints not empty) then delegate to exporter.
   - Pattern: Facade.
   - Error Path: Throws or returns message sentinel; design recommends throwing domain-specific exceptions (e.g., `OutputFolderNotWritableException`).

Communication Patterns:
- Direct method calls only; no events, no messaging bus.
- ViewModel catches exceptions and sets status/error properties (observable for UI and tests).

State Management:
- No internal mutable state; all derived values computed and discarded.
- Output path string returned for post-success display.

End-to-End Testing Hooks:
- After invoking export, integration test verifies file existence and XML root name attribute.
- Negative path test manipulates folder permissions (or mocks file system using abstraction if introduced later).

## Data Integration Strategy
Input Entities:
- `IReadOnlyList<RouteWaypoint>` (ordered sequence numbers expected or derived during optimization).
- `TimeInterval` (start/end used only for filename; not embedded into XML except via first/last waypoint times).
- `long mmsi` (identifier for root element Name and filename prefix).

Transformations:
1. Filename: `<mmsi>_<StartUtcFormatted>_<EndUtcFormatted>.xml` where format = `yyyyMMdd'T'HHmmss` (no trailing `Z`—matches scenario example). Start and end derived from TimeInterval boundaries (not waypoints) to keep naming stable even if optimization changes the internal waypoint set.
2. Waypoint attributes:
   - Null `SpeedKnots` / `Heading` → `Speed="0"` / `Heading="0"` per specification; `ETA` omitted if null.
   - Timestamp formatting for ETA: `yyyyMMdd'T'HHmmss'Z'` (UTC explicit).

Error Handling:
- Empty waypoints list → throw `InvalidOperationException("No waypoints to export")` (ViewModel displays error) or treat as scenario failure (depends on business rule—design favors explicit exception).
- Non-writable folder: Pre-check write permission using `Directory.Exists` + attempt `FileStream` with `FileMode.CreateNew` inside a try/catch (dispose immediately) OR simplified `File.WriteAllText` in guarded try/catch; on failure surface scenario message.
- Partial write failure (disk full): Catch `IOException`, wrap into user-friendly message.

Edge Management:
- Duplicate sequences: Exporter assumes valid sequence ordering; if not, sequences are still emitted but tests can catch misordering earlier (optimization feature responsibility).
- Large waypoint count: Linear iteration; negligible overhead.

Observability for Tests:
- XML attribute presence + order validates transformation.
- File path string aids path assertion.
- Exception types/classification enable negative test coverage.

## Implementation Examples
Below examples are illustrative; focus on clarity and rationale.

```csharp
// XmlExporter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using AisToXmlRouteConvertor.Models;

namespace AisToXmlRouteConvertor.Export;

/// <summary>
/// Serializes route waypoints into a navigation XML file with deterministic structure.
/// </summary>
public static class XmlExporter
{
    /// <summary>
    /// Exports waypoints to an XML route file.
    /// </summary>
    /// <param name="waypoints">Ordered list of optimized waypoints.</param>
    /// <param name="mmsi">Vessel MMSI.</param>
    /// <param name="interval">Selected time interval (used for filename boundaries).</param>
    /// <param name="outputFolder">Target writable folder.</param>
    /// <returns>Full path to created XML file.</returns>
    /// <exception cref="InvalidOperationException">Thrown when waypoints list is empty.</exception>
    /// <exception cref="IOException">Thrown on file system write errors.</exception>
    public static string ExportToXml(
        IReadOnlyList<RouteWaypoint> waypoints,
        long mmsi,
        TimeInterval interval,
        string outputFolder)
    {
        if (waypoints.Count == 0)
            throw new InvalidOperationException("No waypoints to export");

        EnsureWritable(outputFolder);

        string fileName = RouteFilenameFormatter.Build(mmsi, interval);
        string fullPath = Path.Combine(outputFolder, fileName);

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
        var root = new XElement("RouteTemplate", new XAttribute("Name", mmsi.ToString(CultureInfo.InvariantCulture)));

        foreach (var w in waypoints)
        {
            root.Add(ToElement(w));
        }

        doc.Add(root);

        // Atomic-ish write: write temp then move (optional future enhancement)
        doc.Save(fullPath);
        return fullPath;
    }

    private static void EnsureWritable(string folder)
    {
        if (!Directory.Exists(folder))
            throw new IOException($"Selected folder is not writable. Choose a different folder.");
        try
        {
            string probe = Path.Combine(folder, $".__probe_{Guid.NewGuid():N}");
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
        }
        catch
        {
            throw new IOException("Selected folder is not writable. Choose a different folder.");
        }
    }

    private static XElement ToElement(RouteWaypoint w)
    {
        var el = new XElement("WayPoint",
            new XAttribute("Seq", w.Sequence),
            new XAttribute("Lat", w.Latitude.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Lon", w.Longitude.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Speed", (w.SpeedKnots ?? 0).ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Heading", (w.Heading ?? 0).ToString(CultureInfo.InvariantCulture))
        );
        if (w.EtaUtc is DateTime eta)
        {
            el.Add(new XAttribute("ETA", eta.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture)));
        }
        return el;
    }
}

/// <summary>
/// Centralized filename generation policy for route exports.
/// </summary>
public static class RouteFilenameFormatter
{
    public static string Build(long mmsi, TimeInterval interval)
    {
        string start = interval.StartUtc.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        string end = interval.EndUtc.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        return $"{mmsi}_{start}_{end}.xml"; // Matches specification example
    }
}
```

Design rationale notes:
- `EnsureWritable` uses a probe file to detect permission issues early and produce scenario-specific error wording.
- `ETA` inclusion conditional ensures alignment with template: omitted when not available.
- Invariant culture guarantees consistent decimals across locales.
- Separate filename formatter future-proofs adjustments (e.g., adding version tokens).

ViewModel integration example (simplified):
```csharp
// MainViewModel excerpt (pseudo-code for clarity)
public void ExportRoute()
{
    try
    {
        var path = Helper.ExportRoute(OptimizedWaypoints, SelectedMmsi.Value, SelectedInterval, OutputFolder!);
        StatusMessage = $"Route exported: {Path.GetFileName(path)}"; // No auto-open
    }
    catch (IOException ex)
    {
        ErrorMessage = ex.Message; // Surfaces writable failure scenario text
    }
    catch (InvalidOperationException ex)
    {
        ErrorMessage = ex.Message; // Empty waypoint list
    }
}
```

## Testing Strategy and Quality Assurance
Testable architecture characteristics:
- Pure static exporter with deterministic inputs → outputs reproducible for assertions.
- Filename generation isolated: unit test verifies formatting independence from system culture.
- XML attribute set predictable; attribute order irrelevant (assert via name lookup).

Test Types:
1. Unit Tests (`XmlExporterTests`):
   - `ExportToXml_ValidInput_CreatesFileWithExpectedName`
   - `ExportToXml_SetsWaypointAttributesCorrectly`
   - `ExportToXml_WritesEtaWhenPresent_OmitsWhenNull`
   - `ExportToXml_EmptyList_ThrowsInvalidOperationException`
   - `ExportToXml_NonWritableFolder_ThrowsIOException`
2. Integration Tests (extended in End-to-End): confirm route can be generated after upstream steps.

Selectors / hooks for E2E (UI level):
- Bind status label automation property: `automation-id="ExportStatus"`.
- Bind error label: `automation-id="ExportError"`.
- Button command instrumentation: log start/end events.

Positive Scenarios Coverage:
- Valid export path results in correctly named file and matching waypoint template attributes.

Negative & Edge:
- Permission failure triggers scenario message text.
- Empty waypoints list triggers domain exception.
- Null speed/heading produce `0`—assert attribute values.

Mock Data Requirements:
- Centralized fixtures: `sample_waypoints.json` (array of objects with sequence, lat, lon, speed, heading, eta) loaded by `TestDataBuilder` to produce list.
- Edge fixture: `empty_waypoints.json` → empty list for failure path.
- Helper factory: `BuildWaypoints(int count, bool withEta)` generating synthetic sequential waypoints with incremental lat/lon.
- Permission simulation: Use temp directory then remove write permission (Windows: ACL adjust or simpler approach—point to read-only directory like installed program folder; for portability, fallback to intentional invalid path).

Data Exposure for Tests:
- Return path string → direct file existence assertion.
- XML load via `XDocument.Load(path)` for waypoint attribute verification.
- Provide timestamp formatting helper reuse to avoid duplication in test expectations.

Accessibility Testing:
- Non-visual component; only ensure UI messages use clear language and are programmatically accessible via automation IDs.

## Mock Data Requirements
Centralized approach (as per QA/testing documentation expectations):

- Objects:
  ```json
  [
    { "Sequence": 1, "Latitude": 51.0, "Longitude": -3.0, "SpeedKnots": 12.5, "Heading": 90, "EtaUtc": "2025-03-15T00:00:00Z" },
    { "Sequence": 2, "Latitude": 51.0005, "Longitude": -2.9995, "SpeedKnots": 12.7, "Heading": 91, "EtaUtc": "2025-03-15T00:30:00Z" }
  ]
  ```
- Helper Functions:
  - `TestDataBuilder.LoadSampleWaypoints()` → deserializes JSON fixture.
  - `TestDataBuilder.CreateWaypoint(int seq, double lat, double lon, double? speed, int? heading, DateTime? eta)` → granular construction.
- Fixtures:
  - `sample_waypoints.json`, `waypoints_without_eta.json`, `empty_waypoints.json`.
- Exposure:
  - Exporter output path + loaded XML root for validation; tests assert count equality vs input list.

## Design Validation Checklist
✓ Uses approved static helper pattern (simplicity-first).  
✓ No deprecated components introduced.  
✓ File structure aligns with `application_organization.md`.  
✓ Deterministic filename formatting matches scenario example.  
✓ XML schema conforms to template attributes.  
✓ Error messages match BDD wording.  
✓ Test strategy covers positive, negative, edge cases.  

## Summary
The Output Specification feature converts optimized waypoint data into a standard, deterministic XML artifact while enforcing filename conventions and robust error handling. Its design maintains architectural simplicity: stateless static exporter, isolated filename policy, culture-invariant formatting, and explicit validation. The implementation supports thorough automated testing (unit and integration), minimizes coupling, and adheres strictly to the broader architecture and organizational principles. It completes the pipeline that begins with raw AIS data and ends with a usable navigation route file—without imposing extraneous UI side effects.
