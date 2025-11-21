# Feature Design:: XML Export Validation

This document outlines the technical design for the "XML Export Validation" feature (Implementation Plan reference: Feature 8.2). It covers all scenarios:

- Output file naming pattern correctness
- Success message does not auto-open output folder

The design provides conceptual and architectural explanation, validation strategy, testability considerations, and mock data requirements, ensuring exported route artifacts meet strict naming and UX feedback standards without unintended side effects.

## Feature Overview
The XML Export Validation feature ensures that after successful route generation (implemented in Feature 8.1), two critical aspects of user-visible behavior and artifact integrity are enforced:
1. The generated XML route filename strictly follows the canonical pattern: `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml` using the user-selected time interval boundaries (not derived from internal waypoint timestamps).
2. The success feedback mechanism presents only a controlled, non-invasive message (e.g., a status label or dialog) and does not auto-open the output folder or trigger any OS shell integration.

Business Value:
- Predictable naming enables deterministic downstream ingestion, archival, sorting, and potential automation scripts.
- Disciplined UX avoids disruptive context switching, preserves user privacy (e.g., not revealing folder contents), and supports batch or iterative operations in place.

User Needs Addressed:
- Confidence that export metadata (filename) matches visible success messaging.
- Assurance that application behavior remains stable across platforms (Windows/macOS/Linux) with no spurious external UI events (no explorer/finder launches).

High-Level Approach:
- Validate filename generation via a dedicated, deterministic policy component (`RouteFilenameFormatter`).
- Ensure ViewModel/export orchestration returns filename for display but performs no navigation calls.
- Provide granular unit tests for filename pattern and UI event absence; integration tests confirm message consistency and artifact creation.

Architectural Philosophy:
- Simplicity-first, MVVM-aligned, pure functions for formatting.
- Zero global side effects; operations are synchronous and predictable.
- Explicit separation of concerns: formatting policy vs. persistence vs. UX notification.

## Architectural Approach
Patterns Applied:
- Single Responsibility: `RouteFilenameFormatter` handles only naming policy; exporter handles only XML file creation; ViewModel handles only user-facing state.
- Policy Encapsulation: Naming convention isolated—future changes (e.g., timezone representation, version tokens) localized.
- Pure Functions: Formatting uses only inputs (`mmsi`, `TimeInterval`), enabling fast, side-effect-free tests.
- Fail Fast & Determinism: If export succeeds, message references exact filename; errors from Feature 8.1 already validated earlier.
- Non-Intrusive UX: No invocation of platform shell APIs (e.g., `Process.Start` on Windows, `xdg-open` on Linux, or equivalent Avalonia helpers).

Component Relationships & Data Flow:
```
[Optimization Complete] → RouteWaypoint list
          │
          ▼
Helper.ExportRoute(waypoints, mmsi, interval, outputFolder)
          │ uses
          ▼
RouteFilenameFormatter.Build(...) → canonical file name
          │
          ▼
XmlExporter.ExportToXml(...) → writes file
          │ returns path
          ▼
MainViewModel → sets SuccessStatus (no folder auto-open)
```

User Experience Strategy:
- Display concise success status: "Track generated successfully: <filename>" (mirrors earlier feature wording style) or specialized message for validation scenario.
- Maintain UI state unchanged post-export (controls remain enabled, no forced navigation).
- Provide automation-friendly identifiers for success message UI element (e.g., `automation-id="ExportStatus"`).

Information Architecture Considerations:
- Filename embeds temporal identity implicitly; no need for additional route metadata in message.
- Discrete validation scenarios traceable via tests rather than runtime prompts.

## File Structure
Conforms to `application_organization.md`. Only additional clarification for validation concerns—no new production source files beyond those introduced for Feature 8.1 if already present.

```
src/AisToXmlRouteConvertor/
└── Export/
    ├── XmlExporter.cs                # Already contains serialization logic
    ├── RouteFilenameFormatter.cs     # Naming policy (shared by 8.1 & validated by 8.2)
    └── (no platform shell utilities; intentionally absent)

src/AisToXmlRouteConvertor/Services/Helper.cs   # Orchestrates export, returns path

src/AisToXmlRouteConvertor/ViewModels/MainViewModel.cs
    # Binds success status text to UI; does NOT open folders

src/AisToXmlRouteConvertor.Tests/UnitTests/
    ├── RouteFilenameFormatterTests.cs
    └── XmlExportValidationTests.cs

src/AisToXmlRouteConvertor.Tests/IntegrationTests/
    └── ExportWorkflowTests.cs        # Confirms no side effects (e.g., folder open)
```

Comments:
- `RouteFilenameFormatterTests.cs`: Asserts pattern, zero-padding, boundary times.
- `XmlExportValidationTests.cs`: Inspects message string and ensures absence of unintended calls (via mock wrappers if introduced; otherwise by design review & code scanning).
- No new UI components—reuse existing status/message area.

## Component Architecture
1. RouteFilenameFormatter
   - Purpose: Produce canonical file name from MMSI + `TimeInterval` boundaries.
   - Design Patterns: Pure policy; potential extension point.
   - Integration: Called by Helper/Exporter before XML creation.
   - Testability: Unit test verifies formatting under various edge intervals (leading zeros, month boundaries).
   - Accessibility: Not applicable (non-visual).

2. XmlExporter (from Feature 8.1)
   - Reused unchanged for serialization; validation leverages its determinism.

3. MainViewModel
   - Role: Receives returned file path; sets `ExportStatusMessage`.
   - Interaction Pattern: Property update (observable via MVVM toolkit).
   - UX Constraint: Must not call any method that triggers system file explorer.
   - Test Hooks: `ExportStatusMessage` bound with automation ID; end-to-end test inspects binding.

4. Helper.ExportRoute
   - Facade; ensures it only returns path and does not perform side effects.

Component Communication:
- Direct synchronous method calls; no event bus.
- No dependency injection—static class usage per simplicity principle.

State Management:
- Validation state externalized to tests; runtime stores only success message string.

End-to-End Testability:
- Deterministic path construction verified post-export.
- UI property assertions confirm no extraneous state changes.

## Data Integration Strategy
Inputs Used:
- `long mmsi`
- `TimeInterval interval` (StartUtc/EndUtc exact boundaries)
- Waypoints list (already validated, not needed for naming logic beyond existence check)

Transformations:
- Format start/end using `yyyyMMdd'T'HHmmss` (UTC assumed—no trailing `Z` in filename to preserve pattern readability; matches Implementation Plan examples).
- Concatenate with underscore separators and `.xml` extension.

Error / Edge Handling:
- Interval where start equals end (invalid earlier by time validation feature) never reaches export; thus not handled here.
- Missing write permission surfaces as IOException (Feature 8.1 path). Naming still attempted but file write fails gracefully.

Observability for Validation:
- Filename returned; success message uses same filename substring—matching test asserts equality.

Test Data Exposure:
- Generated filename accessible programmatically.
- No internal caching; reproducibility guaranteed.

## Implementation Examples
Policy & Usage (annotated for architectural clarity):
```csharp
// RouteFilenameFormatter.cs
using System.Globalization;
using AisToXmlRouteConvertor.Models;

namespace AisToXmlRouteConvertor.Export;

/// <summary>
/// Generates deterministic XML route filename using MMSI and selected interval boundaries.
/// Pattern: <MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml
/// </summary>
public static class RouteFilenameFormatter
{
    public static string Build(long mmsi, TimeInterval interval)
    {
        // InvariantCulture ensures cross-platform numeric stability.
        string start = interval.StartUtc.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        string end   = interval.EndUtc.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        return $"{mmsi}_{start}_{end}.xml"; // Matches spec examples exactly.
    }
}
```

ViewModel integration snippet (focused on not opening folder):
```csharp
// MainViewModel.cs (excerpt)
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? exportStatusMessage;

    private void PerformExport()
    {
        try
        {
            string path = Helper.ExportRoute(OptimizedWaypoints!, SelectedMmsi!.Value, SelectedInterval!, OutputFolder!);
            // Validation: success message references EXACT filename only; no shell commands.
            ExportStatusMessage = $"Track generated successfully: {System.IO.Path.GetFileName(path)}";
        }
        catch (IOException ex)
        {
            ExportStatusMessage = ex.Message; // Non-writable or write failure
        }
        catch (InvalidOperationException ex)
        {
            ExportStatusMessage = ex.Message; // Empty waypoints scenario
        }
    }
}
```

No code anywhere:
- Calls `Process.Start`, `ShellExecute`, `OpenFolderCommand`, or Avalonia-specific OS integration.
- This absence is intentional and asserted through code review & static analysis.

## Testing Strategy and Quality Assurance
Goals:
- Validate strict filename pattern adherence.
- Confirm success message uses same filename substring (no mismatch errors).
- Assert no folder auto-open side effects.

Unit Tests:
- `RouteFilenameFormatterTests.Build_ValidInterval_ReturnsExpectedPattern`.
- `RouteFilenameFormatterTests.Build_ZeroPadComponents_CorrectlyFormats` (covers single-digit month/day/hour/minute).
- `RouteFilenameFormatterTests.Build_DifferentIntervals_ProducesDistinctNames`.

Validation Tests (XmlExportValidationTests):
- `ExportRoute_SetsSuccessMessageWithoutFolderOpen` → ensures message contains filename and no side-effect APIs invoked (static code inspection or mock wrapper if a future abstraction is introduced).
- `ExportRoute_FilenameMatchesMessage` → splits message, extracts filename token, compares to returned path.

Integration Tests:
- `ExportWorkflowTests.FullRun_CreatesFileWithCanonicalName` → orchestrates entire pipeline including optimization stub.
- `ExportWorkflowTests_SuccessMessageStableAcrossPlatforms` → optional; ensures no OS-specific formatting anomalies.

Negative Path (leveraging existing Feature 8.1 behavior):
- Non-writable folder → message is error, no file creation, no auto-open.

Selectors / Hooks:
- `ExportStatusMessage` property bound to UI element with `automation-id="ExportStatus"` for E2E tests.

Edge Cases:
- Very close interval boundaries (seconds apart) still produce distinct start/end stamp.
- Large MMSI numeric content unaffected (treated as string pass-through).

Accessibility Testing:
- Message is textual; ensure contrast & ARIA label (future UI concerns). Current focus: structural correctness.

Mock Data Requirements:
- Use existing waypoint fixtures from Feature 8.1 tests (`sample_waypoints.json`).
- Additional test helper builds minimal set (two waypoints) with deterministic times matching interval boundaries to confirm independence of filename from internal ETA values.
- Factory: `TestDataBuilder.CreateMinimalWaypoints(interval)`.
- Fixture: `waypoints_minimal.json` for readability.

Observable Data for Assertions:
- Returned path string.
- Export status message content.

## Mock Data Requirements
Centralized approach extended:
- Fixtures: `sample_waypoints.json`, `waypoints_minimal.json` (start, end aligned to interval boundaries), `empty_waypoints.json` (for negative path).
- Helper Functions:
  - `TestDataBuilder.LoadSampleWaypoints()`.
  - `TestDataBuilder.CreateMinimalWaypoints(DateTime start, DateTime end)` → sequences 1 & 2 with lat/lon delta.
- Data Exposure:
  - Tests read fixture → list → export → assert file + message.
  - No randomness; times hardcoded for stable assertions.

Mock Data Validation Focus:
- Filename uses interval boundaries, not waypoint ETA times.
- Changing internal waypoint ETAs does not affect filename (test toggles values to confirm).

## Implementation Examples (Testing)
Illustrative unit test:
```csharp
[Fact]
public void Build_ValidInterval_ReturnsExpectedPattern()
{
    var interval = new TimeInterval(
        new DateTime(2025, 3, 15, 0, 5, 7, DateTimeKind.Utc),
        new DateTime(2025, 3, 15, 12, 6, 9, DateTimeKind.Utc));

    string name = RouteFilenameFormatter.Build(205196000, interval);

    name.Should().Be("205196000_20250315T000507_20250315T120609.xml");
}
```

Success message consistency:
```csharp
[Fact]
public void ExportRoute_FilenameMatchesSuccessMessage()
{
    var interval = new TimeInterval(
        new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2025, 3, 15, 1, 0, 0, DateTimeKind.Utc));

    var waypoints = TestDataBuilder.CreateMinimalWaypoints(interval.StartUtc, interval.EndUtc);
    string outputPath = Helper.ExportRoute(waypoints, 205196000, interval, _tempFolder);
    var vm = new MainViewModel { /* initialize needed state */ };

    vm.ExportStatusMessage = $"Track generated successfully: {System.IO.Path.GetFileName(outputPath)}";

    vm.ExportStatusMessage!.EndsWith(System.IO.Path.GetFileName(outputPath)).Should().BeTrue();
}
```

## Design Validation Checklist
✓ Filename pattern strictly enforced (`<MMSI>_<start>_<end>.xml`).
✓ ViewModel produces success message without shell operations.
✓ No deprecated patterns; aligns with existing architecture docs.
✓ Tests cover positive, edge, and negative (non-writable) cases.
✓ Mock data centralized, deterministic, reusable.
✓ Cultural and locale invariance via `InvariantCulture`.

## Summary
The XML Export Validation feature fortifies the reliability and user trust in the export process by enforcing a deterministic filename convention and ensuring restrained, predictable success messaging. Its architecture leverages pure formatting policies, static orchestration, and strict avoidance of side effects, enabling concise, robust unit and integration testing. This design adheres to project simplicity principles, integrates seamlessly into existing MVVM organization, and provides clear pathways for future extensibility (e.g., version tagging) without compromising validation guarantees.
