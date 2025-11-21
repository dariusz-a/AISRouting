# Feature Design:: Process Button & Workflow (Feature 9.1)

This document outlines the technical design for the Process Button & Workflow feature (Feature 9.1). It provides an architectural, conceptual, and implementation-focused description for all scenarios governing workflow readiness, gating logic, invocation of the end‑to‑end AIS → Optimized Waypoints → XML generation pipeline, success & failure messaging, and reactive UI reset behavior. All scenarios listed under Feature 9.1 in `implementation_plan.md` are covered as a cohesive unit.

Scenarios (multi-source – deviation noted below):
- Process successfully generates XML file
- Process unavailable until all prerequisites selected
- Processing failure displays error details
- Input selection re-populates ship table and resets dependent controls
- Prevent processing when no ship selected
- Generate route for selected ship within valid time interval
- Graceful handling of transient backend processing failure

> NOTE: Feature 9.1 consolidates scenarios that reference two spec documents: `ais_to_xml_route_convertor_summary.md` and `getting_started.md`. This deviates from the ideal single BDD spec file per feature. Future alignment should unify these workflow scenarios under one feature spec file to reduce fragmentation. Until then, both are treated as authoritative sources.

## Feature Overview
The Process Button & Workflow feature governs when the user can trigger route generation and how the system performs a complete, deterministic, and observable transformation pipeline:
1. Validate all prerequisites (input folder, output folder, ship selection, valid time interval, required parsed data availability).
2. Extract filtered `ShipState` records for the selected ship within the chosen time interval.
3. Optimize raw positional data into significant waypoints via the Track Optimization algorithm (dependency on Feature 7.1).
4. Map optimized waypoints into XML structure using the specification template (dependency on Feature 8.1 / 8.2).
5. Persist XML file using naming convention `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml` in selected output folder.
6. Display success or richly detailed failure messages while preserving or restoring button enablement state.
7. Reactively reset dependent state when input folder changes.

Business Value:
- Provides a single, safe activation point for completing the conversion workflow.
- Prevents invalid or partial processing attempts (reduces support burden and error noise).
- Ensures consistent, traceable output file naming for external systems or auditing.
- Surfaces actionable failure details enabling quick user remediation (e.g., malformed CSV, permission error).
- Maintains confidence and transparency through deterministic gating states and clear success/failure surfacing.

User Needs Addressed:
- Know why the Process button is disabled (tooltip / inline helper messaging).
- Be prevented from triggering workflows with incomplete selections.
- Receive immediate, meaningful feedback on success (exact filename) or failure (error text/prefix).
- Avoid unintentional output folder auto-opening; retain control of navigation.
- Recover from transient failures and retry without restarting the application.

High-Level Approach:
- Centralize prerequisite evaluation in a dedicated `PrerequisiteEvaluator` service invoked by the `MainViewModel`.
- Introduce a `ProcessWorkflowController` orchestrator that coordinates data filtering, optimization, and XML export.
- Provide synchronous facade method `ExecuteAsync()` returning a structured result object while internally performing asynchronous (awaitable) phases without blocking UI thread.
- Use structured result types for success and failure; avoid exceptions leaking directly to UI layer.
- Maintain simplicity-first principles: minimal abstraction, clearly bounded services, easily testable pure evaluation logic.

Architectural Philosophy:
- Single Responsibility: Each pipeline stage (filtering, optimization, export) isolated behind clear interfaces.
- Deterministic gating: Enablement state computed from immutable snapshot of prerequisites.
- Transparency: Logging & diagnostics collected per run for test assertion and operator observability.
- Extensibility: Additional pipeline steps (e.g., validation pre-check, data enrichment) can be inserted in `ProcessWorkflowController` with minimal churn.

## Architectural Approach

Patterns & Principles:
- MVVM for UI state & command enabling.
- Command pattern: `ProcessCommand` bound to `MainViewModel.CanProcess`.
- Pure evaluation for prerequisites – idempotent, no side effects.
- Structured error reporting (no string concatenation logic spread across layers).
- Fail-fast at gating stage (button disabled) vs fail-fast inside pipeline (abort on unrecoverable I/O or data corruption).

Component Relationships:
```
MainViewModel
  ├── PrerequisiteEvaluator (pure checks)
  ├── ProcessWorkflowController
  │     ├── ShipStateRepository (source of parsed raw states)
  │     ├── TrackOptimizer (Feature 7.1)
  │     ├── XmlRouteFileGenerator (Feature 8.1/8.2)
  │     └── FileSystemAbstraction / OutputPathValidator
  └── MessagingService (UI modal / inline error exposure)
```

Data Flow Summary:
1. UI changes (folder selection, ship selection, interval edits) trigger `EvaluatePrerequisites()`.
2. `CanProcess` toggles accordingly.
3. User clicks `Process!` → `ProcessWorkflowController.ExecuteAsync(context)`.
4. Controller fetches filtered states → optimizes → constructs XML → writes file → returns result.
5. ViewModel surfaces success or failure message; re-enables button for retry when appropriate.

Integration Patterns:
- Repository-based access to parsed AIS data (in-memory index keyed by MMSI plus time range). 
- Services use dependency injection (lightweight) or static factory per simplicity-first until scaling demands DI container.
- Logging directed to central logger for pipeline stage metrics (counts: raw states loaded, waypoints produced, file size). 
- Failure handshake: internal exceptions caught and converted to `ProcessingFailure` result.

User Experience Strategy:
- Hover tooltip on disabled `Process!` indicates missing prerequisites (ship, output folder, or invalid time interval).
- On success: blocking modal with filename only, no auto-open side effects.
- On failure: blocking modal or inline error banner (transient backend failure case) with actionable message.
- On input folder change: visual resets (disabled pickers, cleared selection, disabled button) for clarity.

## File Structure

Following `application_organization.md` patterns; new or extended files for Feature 9.1:
```
src/
  ViewModels/
    MainViewModel.cs                    # Adds ProcessCommand & gating evaluation
  Models/
    ProcessingContext.cs                # Aggregated immutable context passed to workflow
    ProcessingResult.cs                 # Discriminated result types (success/failure)
  Services/
    PrerequisiteEvaluator.cs            # Pure prerequisite checks
    ProcessWorkflowController.cs        # Orchestrates end-to-end pipeline
    ShipStateRepository.cs              # Existing: provides filtered ShipState sets
    TrackOptimizer.cs                   # Existing (Feature 7.1)
    XmlRouteFileGenerator.cs            # Existing (Feature 8.1)
    OutputPathValidator.cs              # File system write/access checks
    SystemClock.cs                      # Time abstraction (for test determinism)
  Utils/
    FilenameFormatter.cs                # Naming convention enforcement
    XmlTemplateLoader.cs                # Loads XML template (cached)
  Tests/
    UnitTests/
      PrerequisiteEvaluatorTests.cs
      FilenameFormatterTests.cs
    IntegrationTests/
      ProcessWorkflowSuccessTests.cs
      ProcessWorkflowFailureTests.cs
      FolderChangeResetsWorkflowTests.cs
    E2E/
      ProcessButtonWorkflowTests.cs
    Fixtures/
      ShipStateSamples.json
      ShipStaticSample.json
      RouteWaypointTemplate.xml
```

Purpose Commentary:
- `PrerequisiteEvaluator.cs`: Single method returns structured readiness state & reasons list.
- `ProcessWorkflowController.cs`: Encapsulates pipeline; ensures separation from UI concerns.
- `ProcessingContext.cs`: Prevents proliferation of parameter lists; snapshot of user selections.
- `ProcessingResult.cs`: Provides exhaustive success/failure forms for consistent UI rendering.
- `FilenameFormatter.cs`: Central naming logic to avoid drift between success message and actual file.
- `XmlTemplateLoader.cs`: Ensures template loaded once; testable fallback if missing.

## Component Architecture

### PrerequisiteEvaluator
Purpose: Determine if conditions allow processing.
Design Pattern: Pure function returning `PrerequisiteStatus` with `IsReady`, `Missing[]`, `ValidationError?`.
Communication: Invoked on any state change; does not log or raise events.
Accessibility & Testing: Deterministic output enabling direct unit assertion.

### ProcessWorkflowController
Purpose: Perform the AIS → XML pipeline reliably.
Design Pattern: Orchestrator layering discrete stage calls; internal try/catch boundary mapping exceptions to structured failures.
Stages:
1. Data Acquisition: query filtered states from repository.
2. Optimization: call `TrackOptimizer.Optimize(states, thresholds)`.
3. XML Construction: map waypoints into XML structure (template injection).
4. Persistence: write file; verify existence & size.
5. Result Assembly.
Communication: Returns `ProcessingSuccess(filename, diagnostics)` or `ProcessingFailure(errorCode, message, diagnostics)`.
Resilience: Transient failures (I/O, network) flagged with `IsTransient=true` metadata enabling UI retry messaging.

### MainViewModel (Process Slice)
Adds:
- Properties: `CanProcess`, `ProcessInProgress`, `LastProcessingResult`.
- Command: `ProcessCommand` (async) – disables button while running.
- Method: `EvaluatePrerequisites()` updates `CanProcess` & reason tooltip.
Design Rationale: Centralizing gating logic avoids duplication across UI controls.

### FilenameFormatter
Purpose: Apply strict pattern `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml`.
Pattern Reason: Zero ambiguity, sortable, machine-parse friendly.

### XmlRouteFileGenerator
Responsibility: Accept optimized waypoints & produce XML string or XDocument; enforce template constraints (header, root name, waypoint element ordering).

### Error & Messaging Strategy
- All failures surface consistent prefix `Processing failed:`.
- Failure categories: Validation, DataNotFound, OptimizationEmpty, XmlWriteError, IoPermission, TransientBackend.
- UI mapping ensures accessible error announcement (screen reader focus to modal content).

## Data Integration Strategy

Data Entities:
- `ShipState[]`: Raw positional inputs (time, lat, lon, optional sog/cog/heading).
- `OptimizedWaypoint[]`: Reduced, semantically significant nodes.
- `ProcessingDiagnostics`: counts & duration metrics.

Filtering Mechanism:
- Query repository with `(mmsi, startUtc, endUtc)` producing ordered sequence.
- Empty sequence triggers `ProcessingFailure(DataNotFound)`; message suggests verifying interval & parsing stage.

Optimization:
- Always retain first & last states (dependency compliance with Feature 7.1).
- Apply thresholds (distance, bearing change) stored in `TrackOptimizationParameters` from earlier iteration or defaults.
- Produces at least 2 waypoints if any data exists; else failure.

XML Construction:
- Template fields: `<Route>`, `<Waypoint>` with attributes `lat`, `lon`, `timestamp` (ISO8601), optional `sog`, `cog`.
- Validation: ensure chronological ordering; duplicate timestamps pruned in optimizer stage.

Persistence:
- Pre-check output folder writability via `OutputPathValidator` (also part of prerequisites).
- Write atomic: temp file pattern `<filename>.tmp` then rename; ensures crash-safety & absence of partial files (scenario expectation: no `.tmp` leftovers).

Error Handling & Edge Cases:
- Empty filtered states → fail early before optimization.
- Optimization returns < 2 waypoints → failure with message `Insufficient waypoints after optimization`.
- XML template missing → failure `Template missing` (non-transient; user remediation required).
- Write permission denied → failure `Output folder not writable` (ties into gating; reachable only if external condition changed mid-run).
- Transient backend (e.g., external enrichment service 503) → failure flagged transient; UI re-enables button for immediate retry.

Observability for Testing:
- Diagnostics object: `{ RawCount, OptimizedCount, DurationMs, Transient, FailureCode? }`.
- Exposed via `LastProcessingResult.Diagnostics` for integration & E2E assertions.

## Implementation Examples

### Models: ProcessingContext & Result
```csharp
public sealed record ProcessingContext(
    long Mmsi,
    DateTime StartUtc,
    DateTime EndUtc,
    string OutputFolderPath,
    TrackOptimizationParameters Thresholds);

public abstract record ProcessingResult
{
    public ProcessingDiagnostics Diagnostics { get; init; } = ProcessingDiagnostics.Empty();
}

public sealed record ProcessingSuccess(string Filename) : ProcessingResult;
public sealed record ProcessingFailure(string ErrorCode, string Message, bool IsTransient) : ProcessingResult;

public sealed record ProcessingDiagnostics(int RawCount, int OptimizedCount, long DurationMs, bool IsTransient)
{
    public static ProcessingDiagnostics Empty() => new(0,0,0,false);
}
```

### PrerequisiteEvaluator
```csharp
public sealed record PrerequisiteStatus(bool IsReady, IReadOnlyList<string> MissingReasons, string? ValidationError)
{
    public static PrerequisiteStatus Ready() => new(true, Array.Empty<string>(), null);
}

public static class PrerequisiteEvaluator
{
    public static PrerequisiteStatus Evaluate(
        string? inputFolder,
        string? outputFolder,
        bool outputWritable,
        ShipStaticData? selectedShip,
        DateTime? startUtc,
        DateTime? endUtc,
        TimeIntervalValidationResult? intervalValidation)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(inputFolder)) missing.Add("Select input folder");
        if (string.IsNullOrWhiteSpace(outputFolder)) missing.Add("Select output folder");
        if (!outputWritable) missing.Add("Output folder not writable");
        if (selectedShip is null) missing.Add("Select ship");
        if (intervalValidation is null || !intervalValidation.IsComplete) missing.Add("Select Start and End time");

        if (missing.Count > 0) return new(false, missing, intervalValidation?.ErrorMessage);
        if (!(intervalValidation?.IsValid ?? false)) return new(false, missing, intervalValidation?.ErrorMessage);
        return PrerequisiteStatus.Ready();
    }
}
```

### FilenameFormatter
```csharp
public static class FilenameFormatter
{
    public static string Format(long mmsi, DateTime startUtc, DateTime endUtc)
    {
        string stamp(DateTime dt) => dt.ToString("yyyyMMdd'T'HHmmss");
        return $"{mmsi}_{stamp(startUtc)}_{stamp(endUtc)}.xml";
    }
}
```

### ProcessWorkflowController (Core Logic Outline)
```csharp
public sealed class ProcessWorkflowController
{
    private readonly ShipStateRepository _repo;
    private readonly TrackOptimizer _optimizer;
    private readonly XmlRouteFileGenerator _xmlGen;
    private readonly OutputPathValidator _outputValidator;
    private readonly ISystemClock _clock;

    public ProcessWorkflowController(
        ShipStateRepository repo,
        TrackOptimizer optimizer,
        XmlRouteFileGenerator xmlGen,
        OutputPathValidator outputValidator,
        ISystemClock clock)
    {
        _repo = repo; _optimizer = optimizer; _xmlGen = xmlGen; _outputValidator = outputValidator; _clock = clock;
    }

    public async Task<ProcessingResult> ExecuteAsync(ProcessingContext ctx, CancellationToken ct = default)
    {
        var startMs = _clock.UtcNow;
        try
        {
            if (!_outputValidator.CanWrite(ctx.OutputFolderPath))
                return new ProcessingFailure("IoPermission", "Output folder not writable", false);

            var raw = _repo.GetStates(ctx.Mmsi, ctx.StartUtc, ctx.EndUtc).ToList();
            if (raw.Count == 0)
                return new ProcessingFailure("DataNotFound", "No AIS data in selected interval", false);

            var optimized = _optimizer.Optimize(raw, ctx.Thresholds).ToList();
            if (optimized.Count < 2)
                return new ProcessingFailure("OptimizationEmpty", "Insufficient waypoints after optimization", false);

            var filename = FilenameFormatter.Format(ctx.Mmsi, ctx.StartUtc, ctx.EndUtc);
            var fullPath = Path.Combine(ctx.OutputFolderPath, filename);
            var xml = _xmlGen.Generate(optimized);
            _xmlGen.WriteAtomic(fullPath, xml); // temp+rename strategy

            var duration = (long)(_clock.UtcNow - startMs).TotalMilliseconds;
            return new ProcessingSuccess(filename)
            {
                Diagnostics = new ProcessingDiagnostics(raw.Count, optimized.Count, duration, false)
            };
        }
        catch (TransientBackendException ex)
        {
            var duration = (long)(_clock.UtcNow - startMs).TotalMilliseconds;
            return new ProcessingFailure("TransientBackend", $"Processing failed: {ex.Message}", true)
            {
                Diagnostics = new ProcessingDiagnostics(0, 0, duration, true)
            };
        }
        catch (Exception ex)
        {
            var duration = (long)(_clock.UtcNow - startMs).TotalMilliseconds;
            return new ProcessingFailure("Unhandled", $"Processing failed: {ex.Message}", false)
            {
                Diagnostics = new ProcessingDiagnostics(0, 0, duration, false)
            };
        }
    }
}
```

### MainViewModel Process Command Slice
```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool canProcess;
    [ObservableProperty] private bool processInProgress;
    [ObservableProperty] private string? processTooltip;
    [ObservableProperty] private ProcessingResult? lastProcessingResult;

    private void EvaluatePrerequisites()
    {
        var status = PrerequisiteEvaluator.Evaluate(
            InputFolderPath,
            OutputFolderPath,
            OutputFolderWritable,
            SelectedShip,
            StartTimeUtc,
            EndTimeUtc,
            LastValidationResult);

        CanProcess = status.IsReady && !processInProgress;
        processTooltip = status.IsReady ? null : string.Join("; ", status.MissingReasons.Where(r => !string.IsNullOrWhiteSpace(r)));
    }

    [RelayCommand]
    private async Task ProcessAsync()
    {
        if (!CanProcess || SelectedShip is null || LastValidationResult is null) return;
        processInProgress = true; EvaluatePrerequisites();
        var ctx = new ProcessingContext(SelectedShip.Mmsi, StartTimeUtc!.Value, EndTimeUtc!.Value, OutputFolderPath!, OptimizationParameters);
        lastProcessingResult = await _workflow.ExecuteAsync(ctx);
        processInProgress = false; EvaluatePrerequisites();
        ShowResultModal(lastProcessingResult);
    }
}
```

### Success & Failure UI Mapping (Conceptual)
- Success: `result-modal` text `Track generated successfully: <filename>`.
- Failure: prefix `Processing failed:` followed by specific error message; re-enable button unless gating prerequisites changed mid-run.

### Integration Test Outline
```csharp
[Fact]
public async Task Success_EndToEnd_GeneratesFileAndReturnsSuccess()
{
    var vm = BuildReadyViewModel();
    await vm.ProcessAsync();
    vm.LastProcessingResult.Should().BeOfType<ProcessingSuccess>();
    var success = (ProcessingSuccess)vm.LastProcessingResult!;
    File.Exists(Path.Combine(vm.OutputFolderPath!, success.Filename)).Should().BeTrue();
}
```

## Testing Strategy and Quality Assurance

Testable Design Features:
- Pure prerequisite evaluation for exhaustive matrix testing (missing combinations).
- Structured results with codes → assertion stable & language agnostic.
- Atomic write ensures no partial `.tmp` files under transient failure test.
- Clock abstraction allows deterministic duration metric assertions.

Unit Tests:
- `PrerequisiteEvaluatorTests`: all permutations (missing input, output, ship, times, invalid interval).
- `FilenameFormatterTests`: verify pattern & zero-padding.

Integration Tests:
- Success path: filtered states → optimized waypoints → XML written.
- DataNotFound path: empty repository subset.
- OptimizationEmpty path: repository returns minimal states failing threshold expansion.
- TransientBackend path: mock optimizer or external call raising `TransientBackendException`.
- IoPermission path: output validator denies write at execution time.
- Folder change reset: selecting new input folder clears selection & disables button.

E2E Tests:
- Disabled state verification with tooltips (`process-btn` + aria-describedby).
- Success modal exact filename; XML file physically present.
- Failure banner or modal displays prefix.
- Retry after transient failure: second attempt success.

Positive Scenario Mapping:
- Generate route for selected ship within valid time interval → success test + filename pattern test.

Negative & Edge Scenario Mapping:
- Prevent processing when no ship selected → gating test (ship null).
- Process unavailable until all prerequisites selected → multi-missing reasons test.
- Processing failure displays error details → synthetic exception test.
- Graceful handling of transient backend processing failure → transient flag + retry path.
- Input selection re-populates ship table and resets dependent controls → state reset integration test.

Accessibility & Observability:
- `data-testid` attributes: `process-btn`, `result-modal`, `error-banner`.
- Modal focus management tested via automation framework (focus first actionable element).

## Mock Data Requirements

Centralized Approach (per QA testing documentation principles):
- `ShipStateSamples.json`: Provides arrays of position samples across interval span; includes variations for optimization threshold tests.
- `ShipStaticSample.json`: MMSI with min/max interval values.
- `RouteWaypointTemplate.xml`: Canonical XML template used by `XmlTemplateLoader`.
- Helper Functions:
  - `FixturesLoader.LoadShipStates(mmsi, dateRange)` – filters sample sets.
  - `ShipStateFactory.CreateSequence(start, end, frequencyMinutes)` – generates synthetic sequences for threshold variation tests.
  - `TransientBackendFixture.EnableTransientNextCall()` – toggles a one-shot backend failure.

Test Data Fixtures: Promote reuse across integration and unit tests; avoid inline arrays for readability.

Data Exposure Strategy:
- Repository includes `DumpForTest()` method producing raw/optimized counts & last query parameters.
- Last processing result surfaced in ViewModel → direct assertion path.

## Implementation Examples (Additional)

### XmlRouteFileGenerator Atomic Write
```csharp
public void WriteAtomic(string fullPath, XDocument xml)
{
    var tempPath = fullPath + ".tmp";
    xml.Save(tempPath);
    if (File.Exists(fullPath)) File.Delete(fullPath);
    File.Move(tempPath, fullPath);
}
```

### TrackOptimizer (Conceptual Signature)
```csharp
public IEnumerable<OptimizedWaypoint> Optimize(IEnumerable<ShipState> states, TrackOptimizationParameters p)
{
    // Implementation from Feature 7.1 design (retain first/last, apply thresholds)
    // Returned ordered by timestamp ascending.
    throw new NotImplementedException();
}
```

## Data Flow & State Diagram (Conceptual)
```
[User Input Changes]
      ↓
PrerequisiteEvaluator → PrerequisiteStatus → CanProcess (false|true)
      ↓ (true + click)
ProcessWorkflowController.ExecuteAsync
      ↓
ShipStateRepository.Filter
      ↓
TrackOptimizer.Optimize
      ↓
XmlRouteFileGenerator.Generate + WriteAtomic
      ↓
ProcessingResult (Success/Failure)
      ↓
ViewModel → Modal/Banner UI
```

## Design Validation Checklist
✓ Single responsibility per service.
✓ No deprecated components (standard button, modal pattern only).
✓ Deterministic filename pattern centralized.
✓ Atomic file write prevents partial outputs.
✓ Transient failures distinguishable for retry logic.
✓ All Feature 9.1 scenarios mapped explicitly.
✓ Test hooks & mock data strategy defined.
✓ Adheres to simplicity-first; avoids premature over-abstraction.

## Future Extensibility Considerations
- Add progress reporting (percent of states processed) using events; current pipeline synchronous inside async method.
- Introduce cancellation token integration for long-running optimization.
- Add post-processing validation (e.g., schema of generated XML against XSD) for regulatory contexts.
- Metrics export (Prometheus) for raw vs optimized counts and median duration.

## Risks & Mitigations
- Risk: Repository inconsistency after input folder change → Mitigation: clear repository caches on folder change trigger.
- Risk: Filename collision on repeated identical interval runs → Mitigation: allow overwrite (expected); optional future uniqueness suffix config.
- Risk: Large data sets blocking UI if not awaited properly → Mitigation: keep pipeline asynchronous; consider chunked optimization if scaling required.
- Risk: Silent gating confusion → Mitigation: tooltip always lists all missing reasons; integration tests enforce behavior.

## Summary
Feature 9.1 establishes a robust, testable, and transparent processing workflow. It ensures the Process button only becomes active under complete and valid conditions, executes an orchestrated pipeline producing a correctly named XML route file, surfaces detailed success/failure outcomes, and maintains resilience against transient backend issues. The design centralizes prerequisite logic, encapsulates processing stages, and fully maps all specified scenarios while aligning with simplicity-first architectural principles and future extensibility.

Generated on: 2025-11-21
