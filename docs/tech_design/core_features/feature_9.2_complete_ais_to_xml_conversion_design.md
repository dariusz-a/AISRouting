# Feature Design:: Complete AIS to XML Conversion (Feature 9.2)

This document outlines the technical design for the Complete AIS to XML Conversion feature (Feature 9.2). It provides a comprehensive architectural and conceptual description for all scenarios driving the full end‑to‑end transformation of raw AIS input assets (CSV + JSON) into a validated XML route file. The design addresses every scenario under Feature 9.2 in the implementation plan using `docs/spec_scenarios/application_overview.md` as the authoritative specification source.

Scenarios covered:
- Convert imported AIS CSV to XML successfully
- Reject conversion when data preview shows invalid mapping
- Handle processing failure with error message
- Fail conversion when output folder not writable (listed under Negative in spec and referenced in Iteration 8)

> Source BDD File Feature Header: `Feature: AisToXmlRouteConvertor` (scope limited here to the conversion subset — end‑to‑end file transformation). Future consolidation may split large umbrella Feature into finer-grained spec files to reduce cross-cutting scenario overlap.

## Feature Overview
The Complete AIS to XML Conversion feature represents the terminal stage of the user workflow where validated vessel static data and filtered AIS positional streams are transformed into a navigable XML route file following the prescribed naming convention and waypoint schema. While Feature 9.1 governs workflow gating and button enablement, Feature 9.2 focuses on the correctness, robustness, and observability of the conversion pipeline itself — from selecting input resources and preview validation through optimization, XML construction, and durable persistence.

Business Value:
- Produces interoperable XML route artifacts consumed by downstream navigation systems.
- Ensures reliability and deterministic naming for traceability, audit, and automation integration.
- Rejects invalid or incomplete data early, preventing corrupted route files.
- Surfaces actionable diagnostic feedback for rapid remediation of mapping or permission issues.

User Needs Addressed:
- Accurate conversion of selected AIS data within intended temporal bounds.
- Early detection of missing or malformed fields in preview stage (mapping validation).
- Clear error messages for permission or transient processing failures.
- Assurance that no partial or corrupt files remain after failed attempts.

High-Level Approach:
1. Data preview (pre-conversion) asserts mapping completeness (required columns present; non-null critical values).
2. On conversion request, the pipeline loads & filters CSV content (only now to avoid performance overhead during initial scan).
3. Track optimization collapses positional noise into significant waypoints (dependency on Feature 7.1 logic and thresholds).
4. XML generation uses route template rules (dependency on Feature 8.1 / 8.2) enforcing order, attribute formatting, naming convention.
5. Atomic file write ensures crash safety and prevents residual partial artifacts.
6. Structured success/failure result surfaces diagnostics (counts, reduction ratio, duration, failure code classification).

Architectural Philosophy:
- Simplicity-first: Single orchestrator performing synchronous steps within a single method (or small method chain) to maximize readability.
- Deterministic state transitions: Input snapshot captured before conversion; no mid-pipeline mutation of prerequisites.
- Explicit modeling of failure categories for testable assertions and user messaging consistency.
- Reuse of foundational services (parsers, optimizer, exporter) to avoid duplication.

## Architectural Approach

Patterns Applied:
- Orchestrator Pattern: A dedicated `ConversionController` coordinates discrete transformation stages.
- Single Responsibility: Each stage (mapping validation, load & filter, optimize, export) isolated behind small methods.
- Fail-Fast with Categorized Errors: Mapping or permission errors reported immediately before costly operations.
- Immutable Context: `ConversionContext` record aggregates parameters for clarity and testability.

Component Hierarchy (conversion slice):
```
MainViewModel
  └── ConversionController
        ├── PreviewValidator (MappingValidation)
        ├── CsvParser (Filtered Load)
        ├── TrackOptimizer
        ├── XmlRouteBuilder
        ├── AtomicFileWriter
        └── DiagnosticsCollector
```

State Management Strategy:
- ViewModel supplies immutable `ConversionContext` when user clicks Convert.
- Controller returns `ConversionResult` (success or failure) containing diagnostics; ViewModel updates UI accordingly.
- No shared mutable global state except repository caches already defined in previous features.

Integration Patterns:
- Reuse existing `Helper` static methods for scanning & parsing where available; wrap them in stage-specific adapters for clarity without heavy abstraction.
- Logging of key metrics (raw count, optimized count, reduction %, duration ms, failure code) at INFO or ERROR levels.
- No network calls in base design; transient failure scenario simulated via optional extension (e.g., external enrichment service stub).

User Experience Strategy:
- Conversion button begins processing only when prerequisites met (delegated to Feature 9.1 gating; not revalidated unless context changed).
- Mapping rejection uses inline or modal error with explicit missing columns list.
- Success modal shows `Track generated successfully: <filename>` (matching naming rules exactly).
- Failure modal always prefixes `Processing failed:` ensuring consistent automated parsing by test harness.

Information Architecture:
- Data preview grid lists sample rows with column headers; validation layer scans the full header set, not content rows.
- Diagnostics summary optional post-conversion panel (future enhancement) showing reduction ratio and export timestamp.

## File Structure
Complying with patterns defined in `application_organization.md`; new or extended items for Feature 9.2:
```
src/AisToXmlRouteConvertor/
  Models/
    ConversionContext.cs          # Immutable aggregation of parameters
    ConversionResult.cs           # Discriminated union (success/failure)
    ConversionDiagnostics.cs      # Counts, reduction, duration, codes
  Services/
    ConversionController.cs       # Orchestrator performing full pipeline
    PreviewValidator.cs           # Mapping and required field validation
    XmlRouteBuilder.cs            # Constructs XDocument from waypoints
    AtomicFileWriter.cs           # Safe write (temp + rename)
  Parsers/
    CsvParser.cs                  # Existing (used for filtered load)
    JsonParser.cs                 # Existing (static ship data load)
  Optimization/
    TrackOptimizer.cs             # Existing dependency
  Export/
    XmlExporter.cs                # May be wrapped or refactored to builder
  Utils/
    FilenameFormatter.cs          # Central naming pattern (shared with 9.1)
    TimeFormat.cs                 # Utility for timestamp → XML attribute formatting
  ViewModels/
    MainViewModel.cs              # Adds ConvertCommand using ConversionController
  Tests/
    UnitTests/
      PreviewValidatorTests.cs
      FilenameFormatterTests.cs
      XmlRouteBuilderTests.cs
    IntegrationTests/
      ConversionSuccessTests.cs
      ConversionMappingFailureTests.cs
      ConversionPermissionFailureTests.cs
      ConversionProcessingFailureTests.cs
    E2E/
      FullConversionWorkflowTests.cs
    Fixtures/
      PreviewSample.csv
      MissingColumnsSample.csv
      ValidShipStatic.json
      LargePositionsSample.csv
      RouteTemplateBaseline.xml
```
Comments:
- `PreviewValidator.cs`: Ensures required columns exist & critical mapping non-null expectations documented.
- `AtomicFileWriter.cs`: Guarantees absence of partially written XML on failure.
- `XmlRouteBuilder.cs`: Separation of XML construction from file write improves testability.

## Component Architecture

### ConversionController
Purpose & Role: Central orchestrator executing stages in order; returns structured result without throwing raw exceptions.
Design Patterns: Orchestrator + Template Method style sequence.
Information Architecture: Each stage returns a `StageResult` containing success flag and optional error; controller aggregates.
User Experience Strategy: Minimizes user wait by performing synchronous linear operations; potential progress instrumentation future.
Integration Strategy: Consumes existing services; no direct UI dependencies.
Code Example (outline provided below).

### PreviewValidator
Purpose: Validate header mapping before conversion to prevent semantic mismatches.
Patterns: Pure functional analysis returning `PreviewValidationResult`.
Checks:
- Presence of required columns: `TimestampUtc, Latitude, Longitude`.
- Non-empty row sample count > 0.
- Optional columns (Speed, Course, Heading) flagged if missing (warning, not error).
Accessibility & Testing: Deterministic; outputs enumerated missing list.

### XmlRouteBuilder
Purpose: Transform optimized waypoints into XML route structure.
Patterns: Builder producing `XDocument`.
Constraints:
- Root: `<RouteTemplate Name="{MMSI}">`
- Waypoint attributes with canonical formatting; omitted ETA if null.

### AtomicFileWriter
Purpose: Durable file persistence; ensures no partial XML remains.
Pattern: Write temp, flush, atomic rename then cleanup.
Error Handling: Wraps IO exceptions into typed failure codes (`IoPermission`, `IoWriteError`).

### FilenameFormatter & TimeFormat
Purpose: Central naming & timestamp formatting reducing duplication across 9.1 and 9.2.
Design Rationale: Avoid subtle inconsistencies causing validation test failures.

### MainViewModel (Convert Slice)
Adds `ConvertCommand` invoking `ConversionController`. Delegates gating to pre-existing `CanProcess` logic (Feature 9.1). Stores last `ConversionResult` for UI display & test assertion.

End-to-End Testing Considerations:
- Reliable selectors: `data-testid="convert-btn"`, `data-testid="conversion-modal"`, `data-testid="error-banner"`.
- Observable state: `LastConversionResult` property for integration assertions.

## Data Integration Strategy

Data Flow:
```
[User clicks Convert]
  → Build ConversionContext (folders, MMSI, interval, thresholds)
  → PreviewValidator.Validate(headers, sampleRows)
     (Fail early if mapping invalid)
  → CsvParser.ParsePositions(filtered by date file pattern)
  → Filter states within TimeInterval
  → TrackOptimizer.Optimize(filteredStates, thresholds)
  → XmlRouteBuilder.Build(optimizedWaypoints, mmsi, interval)
  → AtomicFileWriter.WriteAtomic(outputFolder, filename, xmlDoc)
  → Return ConversionResult (success/failure + diagnostics)
```

Entity Relationships:
- `ShipStaticData` supplies temporal bounds checks earlier; reused only for informational summary (not mutated).
- `ShipState` sequence drives threshold comparisons in optimizer (distance, heading change, speed change).
- `RouteWaypoint` list constitutes final domain representation pre-export.

Error Handling & Edge Cases:
- Missing required columns → mapping failure (reject conversion) before loading full CSV contents.
- No positions in interval → failure `DataNotFound` (prevents empty XML).
- Optimization yields < 2 waypoints → failure `OptimizationEmpty` (unless single point interval permitted; current rule rejects to maintain route integrity).
- Output folder not writable → failure `IoPermission` (mirrors negative spec scenario).
- Atomic write exception → failure `IoWriteError` with safe cleanup.
- Unexpected exception → failure `Unhandled` with sanitized message.

Observability:
- Diagnostics: `RawCount`, `FilteredCount`, `OptimizedCount`, `ReductionPercent`, `DurationMs`.
- Logging lines for each stage; can be parsed by future telemetry pipeline.

Testability:
- Each stage pure or side-effect encapsulated; easily replaced with test doubles.
- `ConversionContext` fosters reproducible scenario permutations (e.g., synthetic missing header sets).

## Implementation Examples

### Models
```csharp
public sealed record ConversionContext(
    long Mmsi,
    string InputFolder,
    string OutputFolder,
    TimeInterval Interval,
    TrackOptimizationParameters Thresholds,
    IReadOnlyList<string> CsvHeaders,        // From preview stage
    bool OutputFolderWritable);

public abstract record ConversionResult
{
    public ConversionDiagnostics Diagnostics { get; init; } = ConversionDiagnostics.Empty();
}

public sealed record ConversionSuccess(string Filename) : ConversionResult;
public sealed record ConversionFailure(string ErrorCode, string Message) : ConversionResult;

public sealed record ConversionDiagnostics(
    int RawCount,
    int FilteredCount,
    int OptimizedCount,
    double ReductionPercent,
    long DurationMs)
{
    public static ConversionDiagnostics Empty() => new(0,0,0,0,0);
}
```

### PreviewValidator
```csharp
public sealed record PreviewValidationResult(bool IsValid, IReadOnlyList<string> MissingRequiredColumns, IReadOnlyList<string> WarningColumns)
{
    public static PreviewValidationResult Success(IReadOnlyList<string> warnings) => new(true, Array.Empty<string>(), warnings);
}

public static class PreviewValidator
{
    private static readonly string[] Required = {"TimestampUtc","Latitude","Longitude"};
    private static readonly string[] Optional = {"SogKnots","CogDegrees","Heading"};

    public static PreviewValidationResult Validate(IReadOnlyList<string> headers)
    {
        var missing = Required.Where(r => !headers.Contains(r)).ToList();
        var warnings = Optional.Where(o => !headers.Contains(o)).ToList();
        return missing.Count == 0
            ? PreviewValidationResult.Success(warnings)
            : new PreviewValidationResult(false, missing, warnings);
    }
}
```

### XmlRouteBuilder
```csharp
public static class XmlRouteBuilder
{
    public static XDocument Build(IReadOnlyList<RouteWaypoint> waypoints, long mmsi, TimeInterval interval)
    {
        var root = new XElement("RouteTemplate", new XAttribute("Name", mmsi.ToString()));
        foreach (var wp in waypoints)
        {
            root.Add(new XElement("WayPoint",
                new XAttribute("Seq", wp.Sequence),
                new XAttribute("Lat", wp.Latitude),
                new XAttribute("Lon", wp.Longitude),
                new XAttribute("Speed", wp.SpeedKnots?.ToString("F1") ?? "0"),
                new XAttribute("Heading", wp.Heading?.ToString() ?? "0"),
                wp.EtaUtc is not null ? new XAttribute("ETA", TimeFormat.ToXmlStamp(wp.EtaUtc.Value)) : null));
        }
        return new XDocument(new XDeclaration("1.0","utf-8","yes"), root);
    }
}

public static class TimeFormat
{
    public static string ToXmlStamp(DateTime dtUtc) => dtUtc.ToString("yyyyMMdd'T'HHmmss'Z'");
}
```

### AtomicFileWriter
```csharp
public static class AtomicFileWriter
{
    public static void Write(string folder, string filename, XDocument doc)
    {
        Directory.CreateDirectory(folder);
        var full = Path.Combine(folder, filename);
        var temp = full + ".tmp";
        doc.Save(temp);
        if (File.Exists(full)) File.Delete(full);
        File.Move(temp, full);
    }
}
```

### ConversionController (Core Sequence)
```csharp
public sealed class ConversionController
{
    private readonly ShipStateRepository _repo; // or use Helper.LoadShipStates for simplicity

    public ConversionController(ShipStateRepository repo) => _repo = repo;

    public ConversionResult Convert(ConversionContext ctx)
    {
        var startTs = DateTime.UtcNow;
        try
        {
            if (!ctx.OutputFolderWritable)
                return Fail("IoPermission", "Selected folder is not writable. Choose a different folder.");

            var preview = PreviewValidator.Validate(ctx.CsvHeaders);
            if (!preview.IsValid)
                return Fail("MappingInvalid", $"Missing required columns: {string.Join(", ", preview.MissingRequiredColumns)}");

            var rawStates = _repo.LoadAll(ctx.Mmsi, ctx.InputFolder); // loads only needed CSVs by interval pattern internally
            var filtered = rawStates.Where(s => s.TimestampUtc >= ctx.Interval.StartUtc && s.TimestampUtc <= ctx.Interval.EndUtc).ToList();
            if (filtered.Count == 0)
                return Fail("DataNotFound", "No AIS data in selected interval.");

            var optimized = TrackOptimizer.Optimize(filtered, ctx.Thresholds).ToList();
            if (optimized.Count < 2)
                return Fail("OptimizationEmpty", "Insufficient waypoints after optimization.");

            var filename = FilenameFormatter.Format(ctx.Mmsi, ctx.Interval.StartUtc, ctx.Interval.EndUtc);
            var xml = XmlRouteBuilder.Build(optimized, ctx.Mmsi, ctx.Interval);
            AtomicFileWriter.Write(ctx.OutputFolder, filename, xml);

            var diag = new ConversionDiagnostics(
                rawStates.Count,
                filtered.Count,
                optimized.Count,
                filtered.Count == 0 ? 0 : (1 - (double)optimized.Count / filtered.Count) * 100,
                (long)(DateTime.UtcNow - startTs).TotalMilliseconds);

            return new ConversionSuccess(filename) { Diagnostics = diag };
        }
        catch (Exception ex)
        {
            return Fail("Unhandled", $"Processing failed: {ex.Message}");
        }
    }

    private ConversionFailure Fail(string code, string message) => new(code, message);
}
```

### ViewModel Integration Snippet
```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ConversionResult? lastConversionResult;

    [RelayCommand]
    private void Convert()
    {
        if (!CanProcess || SelectedShip is null) return; // gating from Feature 9.1
        var ctx = new ConversionContext(
            SelectedShip.Mmsi,
            InputFolderPath!,
            OutputFolderPath!,
            new TimeInterval(StartTimeUtc!.Value, EndTimeUtc!.Value),
            OptimizationParameters,
            PreviewHeaders, // captured during preview stage
            OutputFolderWritable);

        lastConversionResult = _conversionController.Convert(ctx);
        DisplayConversionModal(lastConversionResult);
    }
}
```

## Testing Strategy and Quality Assurance

Design-for-Test Principles:
- Pure validation for headers (PreviewValidator) → exhaustive permutation tests.
- Deterministic naming (FilenameFormatter) → direct string equality assertions.
- Atomic write ensures absence of `.tmp` when success or immediate cleanup when failure.
- Structured failure codes allow negative scenario mapping explicitly.

Unit Tests:
- `PreviewValidatorTests`: missing required vs optional columns; all present; duplicates.
- `FilenameFormatterTests`: pattern compliance & zero padding.
- `XmlRouteBuilderTests`: waypoint ordering, attribute correctness, omitted ETA behavior.

Integration Tests:
- `ConversionSuccessTests`: full pipeline with sample data verifying file exists and waypoints count.
- `ConversionMappingFailureTests`: missing Latitude column triggers failure code `MappingInvalid` & no file created.
- `ConversionPermissionFailureTests`: simulate unwritable folder returns `IoPermission` & no temp file.
- `ConversionProcessingFailureTests`: inject optimizer throwing exception → failure `Unhandled` with prefix.

E2E Tests:
- Button disabled until prerequisites (covered in 9.1 but cross-asserted here).
- Convert action produces success modal with exact filename.
- Mapping error shows inline/modal error with missing columns list.
- Failure leaves output folder unchanged (no partial file).

Scenario Mapping:
- Convert imported AIS CSV to XML successfully → Success path verifying diagnostics plausible and filename correct.
- Reject conversion when data preview shows invalid mapping → PreviewValidator failure code `MappingInvalid`.
- Handle processing failure with error message → Force exception path; assert prefix `Processing failed:`.
- Fail conversion when output folder not writable → Permission pre-check failure code `IoPermission`.

Accessibility:
- Modal implements focus trapping; `aria-live="assertive"` for error messages.
- `data-testid` on convert button & modal content for reliable selectors.

Mock Data Requirements (Centralized):
- `PreviewSample.csv`: Minimal compliant header + 3 sample rows.
- `MissingColumnsSample.csv`: Lacks one required column for mapping failure test.
- `LargePositionsSample.csv`: Stress test pipeline & optimization reduction ratio.
- `ValidShipStatic.json`: Provide consistent static vessel metadata.
- Helper: `TestDataBuilder.CreateShipStates(count, pattern)` for generating synthetic states.
- Factory: `WaypointFactory.FromStates(states)` for building expected baseline in builder tests.

Data Exposure:
- `LastConversionResult.Diagnostics` surfaces counts for assertions.
- AtomicFileWriter exposes internal static test flag (optional) to simulate write failure for negative branch coverage.

## Mock Data Requirements

Objects & Fixtures:
- Ship static fixture: MMSI 205196000 with date range covering sample CSVs.
- Position sequences for varying optimization behavior (linear movement, heading change spikes, speed fluctuations).
- Invalid header sets enumerated in parameterized test data.

Helper Functions:
```csharp
public static class TestDataBuilder
{
    public static List<ShipState> LinearStates(DateTime start, int minutes, double lat, double lon, double dLat, double dLon)
    {
        var list = new List<ShipState>();
        for (int i=0;i<minutes;i++)
        {
            list.Add(new ShipState(
                start.AddMinutes(i),
                lat + i*dLat,
                lon + i*dLon,
                0, null, 10 + (i%3)*0.2, 90 + (i%5), 90 + (i%5), 14.5, 0, null));
        }
        return list;
    }
}
```

Exposure Strategy:
- Repository `DumpForTest(mmsi, interval)` returns raw & filtered counts.
- Builder tests directly inspect returned `XDocument` without writing to disk (fast feedback).

## Conceptual Explanation (Per Component)

Component: ConversionController
- Purpose: Single entry point for full pipeline execution.
- Pattern: Orchestrator; reduces cognitive overhead vs scattering logic.
- Information Architecture: Each stage explicit; failure short-circuits.
- UX Strategy: Fast fail for mapping & permission avoids wasted processing.
- Integration: Adapts repository & optimizer outputs without altering them.

Component: PreviewValidator
- Purpose: Ensure semantic viability of data before processing.
- Pattern: Pure validation with enumerated missing/warning sets.
- Rationale: Prevent downstream null dereferences and structural inconsistencies.
- Trade-off: Requires early header capture; minimal overhead vs late discovery.

Component: XmlRouteBuilder
- Purpose: Deterministic creation of navigable XML artifact.
- Pattern: Builder; isolates formatting & ordering logic.
- Rationale: Testable without I/O; simplifies export modifications.
- Accessibility: Ensures stable attribute ordering enabling predictable diffing.

Component: AtomicFileWriter
- Purpose: Guarantee integrity of final artifact.
- Pattern: Atomic commit; prevents partial files on crash/exception.
- Rationale: Simplifies failure rollback semantics.

## Design Validation Checklist
✓ All scenarios mapped to explicit success or failure codes.
✓ Naming & timestamp formatting centralized (no drift).
✓ No deprecated or unapproved components introduced.
✓ Separation of concerns preserved (validation vs orchestration vs construction vs persistence).
✓ Deterministic, testable outcome states (structured result objects).
✓ Mock data approach defined & aligned with centralized fixture pattern.
✓ Failure messages consistently prefixed for automation.

## Future Extensibility Considerations
- Add progressive streaming + progress reporting events (percent complete).
- Introduce pluggable export formats (GPX, KML, GeoJSON) via strategy interface.
- Incorporate schema validation (XSD) post-generation for compliance assurance.
- Add caching of parsed CSV subsets when re-running with adjusted thresholds.
- Parameterize optimization thresholds per vessel profile.

## Risks & Mitigations
- Risk: Header drift across datasets → Mitigation: expose optional header normalization map in PreviewValidator.
- Risk: Large file performance bottlenecks → Mitigation: lazy line streaming; consider segmented optimization.
- Risk: Silent partial writes on failure → Mitigation: atomic writer + existence assertions in tests.
- Risk: Inconsistent failure messaging → Mitigation: centralized failure creation utility.

## Summary
Feature 9.2 delivers a complete, robust, and testable AIS-to-XML conversion pipeline. The architecture emphasizes clarity, determinism, and maintainability: validation pre-check prevents wasted processing; orchestrated stages reuse existing domain services; atomic persistence ensures integrity; structured diagnostics support transparency and evolution. All defined scenarios (successful conversion, mapping rejection, permission failure, processing failure) are explicitly addressed with clear architectural rationale consistent with the project's simplicity-first principles.

Generated on: 2025-11-21
