# Feature Design:: Time Interval Controls (Feature 6.1)

This document outlines the technical design for the Time Interval Controls feature (Feature 6.1). It provides an architectural, conceptual, and implementation-focused description for all scenarios governing time interval selection, validation, enablement logic, and its interaction with prerequisite UI state (ship selection) and downstream processing (route generation gating). The design explicitly covers every scenario listed under Feature 6.1 in `implementation_plan.md`:

Scenarios (multi-source from spec files – see Note below):
- Time pickers enabled when ship selected
- Invalid time range disables process button
- Time outside available range blocks processing
- Show validation error when Start time after End time
- Disable processing until both times selected

> NOTE: Unlike the prompt assumption that all scenarios in a feature share a single BDD spec file, Feature 6.1 currently references two spec documents: `ais_to_xml_route_convertor_summary.md` and `getting_started.md`. This is a deviation from the expected uniform `file` value. The design treats these as a cohesive functional unit. A future alignment step should consolidate these scenarios into a single feature spec file to eliminate cross-file fragmentation.

## Feature Overview

The Time Interval Controls feature enables an operator to define a valid UTC time range over which AIS position data will be filtered during processing. It directly affects downstream data volume, correctness of waypoint generation, and gating of the Process action.

Business value:
- Prevents generation attempts with invalid temporal constraints (protects performance and correctness).
- Ensures Start/End boundaries are constrained to the vessel’s available AIS data window, avoiding empty or out-of-range loads.
- Provides immediate, accessible feedback for invalid input (improved usability and reduced trial-and-error).
- Acts as an enabling prerequisite for end‑to‑end conversion (part of overall workflow readiness evaluation).

User needs addressed:
- Clear indication of selectable time span (min/max) after vessel selection.
- Inline validation feedback without modal disruption.
- Automatic disablement of Process until both times are valid and present.
- Preservation of last valid state (no silent mutation on invalid entry).

High-level approach:
- MVVM separation: a focused TimeInterval sub-model (`TimeInterval` record) expressed via two bound DateTime pickers managed by `MainViewModel`.
- Validation is pure and synchronous; triggered on property change and on blur events from UI.
- Central evaluation method derives: validity flag, error message, and derived enabling state (`CanProcess`).
- Accessible feedback through a single observable validation message property, with test-friendly deterministic states.

Architectural philosophy:
- Simplicity-first: no background tasks, no speculative loading.
- Deterministic state transitions – every invalidation path produces a clear reason string.
- Immutable domain record for the interval; UI uses mutable wrapped properties until confirmed valid.
- Cohesion: all time logic resides inside ViewModel + a tiny static validator helper (small surface area for tests).

## Architectural Approach

Patterns applied:
- MVVM (Avalonia + CommunityToolkit.Mvvm): observable properties and relay commands.
- Single Responsibility: Time validation isolated in `TimeIntervalValidator` static class.
- Separation of Concerns: UI → ViewModel orchestration; validation → pure helper; domain representation → immutable record.
- DRY: shared validation pipeline ensures consistent messaging for all scenarios.

Component hierarchy (relevant slice):
```
MainWindow.axaml
  └── TimeIntervalSection (UserControl)
        ├── Start DateTimePicker (data-testid="time-start")
        ├── End DateTimePicker (data-testid="time-end")
        └── Validation TextBlock (data-testid="validation-inline")
```

State management strategy:
- Properties: `SelectedShip`, `StartTimeUtc`, `EndTimeUtc`, `TimeIntervalError`, `CanProcess`.
- Derived: `HasCompleteTimes`, `IsTimeRangeValid`, `IsWithinBounds`.
- A single evaluation pass sets all dependent flags; prevents inconsistent partial updates.

Integration points:
- Consumes `ShipStaticData.MinDateUtc` / `MaxDateUtc` (from JSON load after ship selection).
- Produces a validated `TimeInterval` passed to processing pipeline when Process is executed.
- Contributes to global process readiness gating along with ship selection and folder validation.

User experience & information architecture:
- Progressive enablement: pickers disabled until ship is selected.
- Immediate inline error on invalid sequence or range violation.
- Clear neutral helper text when valid but incomplete (e.g., end time missing).
- No modal interruption for validation failures; user iteratively corrects input.

## File Structure

Following `application_organization.md` patterns, additions / relevant feature files (new or emphasized):
```
src/AisToXmlRouteConvertor/
├── ViewModels/
│   └── MainViewModel.cs              # Holds interval state & validation
├── Models/
│   └── TimeInterval.cs               # Immutable record (already defined)
├── Services/
│   └── TimeIntervalValidator.cs      # NEW: pure validation helper
├── Views/
│   └── Controls/
│       └── TimeIntervalSection.axaml # NEW: isolated section UserControl
│       └── TimeIntervalSection.axaml.cs
├── Tests/
│   ├── UnitTests/
│   │   └── TimeIntervalValidatorTests.cs
│   └── IntegrationTests/
│       └── TimeIntervalEnablementTests.cs
└── TestData/
    └── ShipStaticDataSamples.json    # Mock static data ranges for tests
```

Purpose annotations:
- `TimeIntervalValidator.cs`: Single method with all rule checks; ensures all scenarios share same logic.
- `TimeIntervalSection.axaml`: Encapsulates UI; assigns data-test identifiers for E2E.
- `TimeIntervalValidatorTests.cs`: Unit coverage for each validation outcome path.
- `TimeIntervalEnablementTests.cs`: Integration ensuring ViewModel + UI state transitions respond to selection events.
- `ShipStaticDataSamples.json`: Centralized mock static data (aligns with centralized mock data principle; though TypeScript example in QA docs, adapted here for .NET fixtures).

## Component Architecture

### TimeIntervalSection (UserControl)
Purpose: Presents start/end pickers and validation feedback; enables accessible interaction.
Design Pattern: Passive View bound to proactive ViewModel; no code-behind logic besides initialization.
Information Architecture: Chronological ordering (Start then End); validation message directly below pickers.
User Experience Strategy: Immediate visual error; disabled processing path while invalid.
Integration Strategy: Binds to `MainViewModel` properties; no direct service calls.
Accessibility: `AutomationProperties.Name` applied (e.g., "Start Time UTC Picker").
Testing Hooks: `data-testid` implemented via attached property wrapper or mapped to `AutomationProperties.HelpText` for Playwright-like selectors.

### MainViewModel (Interval Slice)
Responsibilities:
- Maintain current candidate start/end times.
- React to ship selection (reset times to ship range bounds, clear errors).
- Invoke validator on each relevant property change.
- Expose `CanProcess` only when all prerequisites (folders, ship, valid interval) are satisfied.

Design Patterns:
- Observable properties with CommunityToolkit `[ObservableProperty]`.
- Derived property recalculation via `EvaluateTimeInterval()` method.

### TimeIntervalValidator (Static Helper)
Responsibilities:
- Encapsulate all rules:
  - Presence (both times selected).
  - Ordering (Start < End).
  - Bounds (within Min/Max when available).

Returns composite result object:
```csharp
public sealed record TimeIntervalValidationResult(
    bool IsComplete,
    bool IsOrdered,
    bool IsWithinBounds,
    string? ErrorMessage,
    bool IsValid => IsComplete && IsOrdered && IsWithinBounds
);
```

### Process Button (Indirect Relation)
Behavior: Reacts to ViewModel’s `CanProcess` which integrates interval validity; does not perform interval logic itself.

State & Data Flow Diagram:
```
SelectedShip (Min/Max)
        ↓ (ship selected event)
StartTimeUtc / EndTimeUtc (user edits)
        ↓
TimeIntervalValidator.Evaluate(start, end, min, max)
        ↓
TimeIntervalValidationResult → ViewModel properties
        ↓
TimeIntervalError + CanProcess gating
        ↓
ProcessCommand availability
```

## Data Integration Strategy

Input Data:
- Ship static range: `MinDateUtc`, `MaxDateUtc` (nullable; if absent treat as open range for that bound).
- User times: transient picks before validation.

Transformation:
- Convert user local entries (if UI provides localized input) to UTC consistently.
- Validation uses UTC only (domain invariant).

Error Handling:
- Null Start OR End → message: "Select both Start and End time" (scenario: Disable processing until both times selected).
- Start >= End → message: "Start time must be before End time" (scenario: Show validation error when Start time after End time; also covers equality case).
- Out of bounds → message: "Selected time is outside available data range" (scenario: Time outside available range blocks processing).

Edge Cases:
- Ship selected with identical Min/Max → enforce Start < End impossible; treat as unsatisfiable state (Process disabled; message surfaced). Optionally suggest adjusting data sources.
- User tries times exactly on bounds: allowed (inclusive interval).
- Bound missing: only enforce ordering & completeness.

Observability for tests:
- Validation result mapped to a public read-only property `LastValidationResult` (internal set) to assert state in integration tests.
- Data-test IDs on UI controls for E2E confirm enablement/disabled states and error visibility.

## Implementation Examples

### Domain Record (TimeInterval)
```csharp
namespace AisToXmlRouteConvertor.Models;

public sealed record TimeInterval(DateTime StartUtc, DateTime EndUtc)
{
    public override string ToString() => $"{StartUtc:O} - {EndUtc:O}";
}
```

### Validation Helper
```csharp
namespace AisToXmlRouteConvertor.Services;

public static class TimeIntervalValidator
{
    public static TimeIntervalValidationResult Evaluate(
        DateTime? startUtc,
        DateTime? endUtc,
        DateTime? minUtc,
        DateTime? maxUtc)
    {
        if (startUtc is null || endUtc is null)
            return new(false, false, false, "Select both Start and End time");

        if (startUtc >= endUtc)
            return new(true, false, false, "Start time must be before End time");

        if (minUtc is not null && startUtc < minUtc || maxUtc is not null && endUtc > maxUtc)
            return new(true, true, false, "Selected time is outside available data range");

        return new(true, true, true, null);
    }
}

public sealed record TimeIntervalValidationResult(
    bool IsComplete,
    bool IsOrdered,
    bool IsWithinBounds,
    string? ErrorMessage)
{
    public bool IsValid => IsComplete && IsOrdered && IsWithinBounds && ErrorMessage == null;
}
```

### ViewModel Slice
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using AisToXmlRouteConvertor.Models;
using AisToXmlRouteConvertor.Services;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ShipStaticData? selectedShip;
    [ObservableProperty] private DateTime? startTimeUtc;
    [ObservableProperty] private DateTime? endTimeUtc;
    [ObservableProperty] private string? timeIntervalError;
    [ObservableProperty] private bool canProcess; // consolidated gating
    public TimeIntervalValidationResult? LastValidationResult { get; private set; }

    partial void OnSelectedShipChanged(ShipStaticData? oldValue, ShipStaticData? newValue)
    {
        // Reset interval defaults when ship changes
        startTimeUtc = newValue?.MinDateUtc;
        endTimeUtc = newValue?.MaxDateUtc;
        EvaluateTimeInterval();
    }

    partial void OnStartTimeUtcChanged(DateTime? value) => EvaluateTimeInterval();
    partial void OnEndTimeUtcChanged(DateTime? value) => EvaluateTimeInterval();

    private void EvaluateTimeInterval()
    {
        var min = SelectedShip?.MinDateUtc;
        var max = SelectedShip?.MaxDateUtc;
        LastValidationResult = TimeIntervalValidator.Evaluate(StartTimeUtc, EndTimeUtc, min, max);
        TimeIntervalError = LastValidationResult.ErrorMessage;
        canProcess = SelectedShip is not null
                     && OutputFolderIsWritable
                     && InputFolderIsValid
                     && LastValidationResult.IsValid;
    }
}
```

### XAML Section (Simplified)
```xml
<UserControl x:Class="AisToXmlRouteConvertor.Views.Controls.TimeIntervalSection"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <StackPanel Spacing="10" DataContext="{Binding}">
    <TextBlock Text="3. Select Time Interval" FontWeight="Bold" />
    <Grid ColumnDefinitions="Auto,*">
      <TextBlock Text="Start (UTC):" Margin="0,0,8,0" />
      <DatePicker SelectedDate="{Binding StartTimeUtc}" x:Name="StartPicker" />
    </Grid>
    <Grid ColumnDefinitions="Auto,*">
      <TextBlock Text="End (UTC):" Margin="0,0,8,0" />
      <DatePicker SelectedDate="{Binding EndTimeUtc}" x:Name="EndPicker" />
    </Grid>
    <TextBlock Text="{Binding TimeIntervalError}" Foreground="Red"
               IsVisible="{Binding TimeIntervalError, Converter={StaticResource NullToBool}}"
               x:Name="ValidationMessage" />
  </StackPanel>
</UserControl>
```

### Process Gating Assertion Example (Integration Test)
```csharp
[Fact]
public void ProcessDisabled_WhenOnlyStartTimeProvided()
{
    var vm = BuildViewModelWithShip(min: Utc("2025-03-15T00:00:00Z"), max: Utc("2025-03-16T00:00:00Z"));
    vm.StartTimeUtc = Utc("2025-03-15T06:00:00Z");
    vm.EndTimeUtc = null;
    vm.CanProcess.Should().BeFalse();
    vm.TimeIntervalError.Should().Be("Select both Start and End time");
}
```

## Testing Strategy and Quality Assurance

Design-for-test principles:
- Pure validation logic with no side effects – unit tests cover every branch.
- ViewModel exposes validation result for direct assertions (reduces brittle UI scraping in integration tests).
- Deterministic error messages – exact string matching feasible.
- Data-test identifiers on pickers and error text enable Playwright/E2E stable selection.

Test Categories:
1. Unit – `TimeIntervalValidatorTests` (complete matrix: missing times, reversed times, out-of-range, valid).
2. ViewModel – ensures state transitions on ship change / edits produce expected gating.
3. Integration – simulate enabling conditions (folder valid, ship selected, times within range) then assert Process becomes enabled only after valid interval.
4. Negative – equality Start == End, bounds breaches, partial entry states.
5. Accessibility – ensure validation message toggles `aria-invalid` (via Avalonia automation properties) on pickers when invalid.

Positive scenarios mapped:
- Time pickers become enabled post ship selection: assert IsEnabled and default values set to min/max.
- Valid range: no error message; `CanProcess` true (assuming other prerequisites satisfied).

Negative scenarios mapped:
- Start after End: error string; `CanProcess` false.
- Out-of-bounds Start or End: bounds error; `CanProcess` false.
- Only one time selected: completeness error; `CanProcess` false.

Mock Data Requirements (centralized approach adaptation for .NET):
- Provide `ShipStaticData` fixtures in `TestData/ShipStaticDataSamples.json` and load via helper.
- Helper `ShipStaticDataFactory.Create(min, max)` yields consistent record with optional dimensions omitted for simplicity.
- Provide time interval examples list for parameterized tests (e.g., Theory data).
- Reuse same fixture for multiple tests to minimize duplication.

Example Fixture Loader:
```csharp
public static class ShipStaticDataFactory
{
    public static ShipStaticData Create(long mmsi = 205196000,
        DateTime? min = null, DateTime? max = null) => new(
            Mmsi: mmsi,
            Name: "Alice's Container Ship",
            Length: 285.0,
            Beam: 40.0,
            Draught: 14.5,
            CallSign: "ONBZ",
            ImoNumber: "IMO9234567",
            MinDateUtc: min ?? new DateTime(2025,3,15,0,0,0,DateTimeKind.Utc),
            MaxDateUtc: max ?? new DateTime(2025,3,16,0,0,0,DateTimeKind.Utc));
}
```

## Mock Data Requirements

Centralization Goals:
- Single source for vessel static range data.
- Helper ensures semantic clarity vs ad-hoc inline object creation.
- Supports extension for future test scenarios (e.g., open-ended ranges where min or max null).

Required Objects:
- Standard vessel fixture (MMSI 205196000) full date window.
- Narrow vessel fixture (3-hour window) to test Start==End unsatisfiable case.
- Out-of-range attempt examples.

Helper Functions:
- `CreateWithWindow(hoursSpan)` produce min/max relative to now for dynamic tests.
- `CreateOpenEnded(minOnly: bool)` produce null bound to test missing constraint behavior.

Exposure Strategy:
- ViewModel exposes `LastValidationResult` enabling direct consumption in integration tests.
- UI attaches `data-testid` enabling Playwright/E2E selection consistency.

## Conceptual Explanation Summary (Per Component)

Component: TimeIntervalSection
- Purpose: Collect and surface validity of Start/End bounds controlling route processing context.
- Pattern: Passive MVVM view; no logic beyond binding.
- Information Architecture: Chronological ordering; grouped under a bold header; error directly adjacent for high discoverability.
- UX Strategy: Immediate red error; disabled Process; accessible naming.
- Integration: Consumes ViewModel observables; produces no events itself.

Component: TimeIntervalValidator
- Purpose: Central truth for rule enforcement preventing duplication.
- Pattern: Pure function returning aggregate result record.
- Principles: Determinism, transparency, composability.
- Trade-offs: Simplicity over extensibility (adding complex constraints may later require refactoring to policy objects).

Component: MainViewModel Interval Slice
- Purpose: Orchestrate enabling, derive gating states, bridge between UI and domain.
- Pattern: Observable state container; derived recalculations triggered on changes.
- Design Rationale: Centralizing gating avoids scattering logic among commands.
- Trade-offs: Potential growth; mitigated by isolating evaluation logic.

## Design Validation Checklist
✓ Uses only approved components (DatePicker / TextBlock / Button).  
✓ Synchronous operations only (no async in validation).  
✓ No deprecated components introduced.  
✓ File structure conforms to `application_organization.md` patterns.  
✓ Deterministic validation messages and states mapped 1:1 to scenarios.  
✓ Mock data centralized and reusable.  
✓ Test hooks (data-testid, LastValidationResult) provided.  

## Future Extensibility Considerations
- Add time zone selector (would require conversion layer before validation; current design isolates UTC nicely).
- Introduce preset interval buttons (e.g., Last 6 Hours) – can wrap existing properties without modifying validation semantics.
- Support partial open intervals if data loading pipeline later permits unbounded end (requires adjusting completeness rule).
- Parameterize validation messages for localization.

## Risks & Mitigations
- Risk: Divergent messages if ad-hoc validation added elsewhere → Mitigation: enforce usage of `TimeIntervalValidator`.
- Risk: Large future complexity (business exceptions like blackout periods) → Mitigation: refactor validator into strategy collection when needed.
- Risk: Out-of-sync enabling logic with Process prerequisites → Mitigation: single `EvaluateTimeInterval()` sets unified `CanProcess`.

## Summary

Feature 6.1 establishes a robust, testable, and accessible time interval selection mechanism fundamental to safe AIS → XML route generation. The design emphasizes deterministic validation, clear user feedback, centralized logic, and seamless integration with existing MVVM patterns. All specified scenarios are covered with explicit rule mappings, ensuring correctness and maintainability while aligning with simplicity-first architectural principles.
