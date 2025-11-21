# Feature Design:: Feature 10.1: Main Window Initialization

This document outlines the technical design for Feature 10.1: Main Window Initialization.

> Scope Clarification
> The BDD spec file (`docs/spec_scenarios/getting_started.md`) declares the broader Feature: "Getting Started". For Implementation Plan Feature 10.1 we isolate the scenario: "Display main window controls on launch" and define the initialization architecture enabling subsequent workflow features (folder selection, ship selection, time interval selection, processing). This design focuses exclusively on the startup surface, structural composition, and readiness states; downstream behaviors (folder scanning, selection logic, processing) are defined in their respective feature design documents.

---
## Feature Overview
The Main Window Initialization feature ensures that upon application launch the operator immediately receives a coherent, accessible, and testable UI scaffold containing all primary workflow controls: input folder picker, output folder picker, ship table placeholder, static data panel container, time interval pickers, and a disabled Process button. Business value centers on rapid operator orientation and elimination of hidden navigation—every required step is visually discoverable from the first frame, reducing training time and early-user friction.

User Need Addressed:
- Immediate visibility of conversion workflow stages.
- Clear disabled states communicating prerequisites (why Process is disabled initially).
- Reliable automation selectors (for QA and end-to-end tests) via consistent identifiers.
- Predictable layout that scales cross-platform (Windows/macOS/Linux) in Avalonia.

High-Level Approach:
1. Single-window MVVM composition (`MainWindow.axaml` + `MainViewModel`).
2. Structural provisioning of placeholder containers with empty or default state values—no blocking async operations during initial render.
3. Deterministic initialization sequence: App lifecycle → DI/service registration (minimal) → ViewModel construction → DataContext binding → visual tree loading → post-load state affirmation.
4. Accessibility & test readiness: use `AutomationProperties.AutomationId` to emulate `data-testid` semantics described in BDD scenarios.

Architectural Philosophy:
Simplicity-first (per overall architecture). Avoid deferred or lazy-loading complexity on startup. Use synchronous construction, no background threads during initial UI composition. Provide clear separation of presentation (XAML), orchestration (ViewModel), and future operations (static `Helper` methods not invoked here). Keep startup cost extremely low (<50ms typical) enabling immediate operator interaction for folder selection.

---
## Architectural Approach
Patterns Applied:
- MVVM with CommunityToolkit.Mvvm for property / command generation.
- Declarative XAML layout with semantic sectioning (Borders / Panels) matching workflow order.
- Dependency inversion kept minimal: ViewModel depends on *no* services during initialization; later injection optional.
- Separation of concerns: initialization feature does not parse data, scan disks, or execute algorithms; it only exposes reactive placeholders.

Component Relationships:
- `App` constructs and shows `MainWindow`.
- `MainWindow` binds to `MainViewModel` as DataContext.
- `MainViewModel` exposes bound properties controlling enabled/disabled states.
- Section controls (folder pickers, table, time pickers, process button) reference these properties; no cross-talk between controls directly.

State Management Strategy:
- All initial state values are null / empty collections / false flags stored in `MainViewModel`.
- Derived properties (e.g., `CanProcess`) compute disabled reasons; for Feature 10.1 returns false unconditionally (other features will extend logic).
- Property change notifications support future dynamic enablement without refactoring initialization logic.

Data Flow (Initialization Only):
```
App Startup → MainWindow Created → MainViewModel Instantiated → Bind Controls → Render Placeholder / Disabled States
```
No external file or network access occurs. This is a pure in-memory initialization.

User Experience Strategy:
- Linear top-to-bottom hierarchy communicates workflow order.
- Disabled Process button accompanied by contextual helper text placeholder (future features populate reason string).
- Ship table and static panel render structural chrome even when empty—avoids layout shifts when data arrives.

Information Architecture:
- Sections visually grouped & labeled; each corresponds to a later feature’s activation.
- Status bar presents a baseline "Ready" message at launch.

Integration Patterns:
- Only internal MVVM binding at this stage.
- Future integration (file system scanning, parsing) attaches behind commands—kept decoupled from initialization.

---
## File Structure
Conforms to `application_organization.md`. Only files impacted or required for initialization:
```
src/AisToXmlRouteConvertor/
  App.axaml                     # Application root
  App.axaml.cs                  # Startup hook, creates MainWindow
  MainWindow.axaml              # Main UI layout (initial placeholders)
  MainWindow.axaml.cs           # Code-behind minimal (lifecycle wiring)
  ViewModels/
    MainViewModel.cs            # Initialization state + properties
  Assets/Styles/CustomStyles.axaml  # Optional shared styles
  Models/                       # Present but unused during init (no changes)
  Services/                     # Present; not invoked during init
```

New / Modified Responsibilities:
- `MainWindow.axaml`: Adds AutomationIds matching BDD testids.
- `MainViewModel.cs`: Defines baseline properties: `InputFolder`, `OutputFolder`, `AvailableShips`, `SelectedShip`, `StartTimeUtc`, `EndTimeUtc`, `CanProcess`, `ProcessDisabledReason`, `StatusMessage`.
- No changes to Services/Models yet (avoid premature coupling).

Comments (Purpose Summary):
- `MainWindow.axaml`: Declarative structure for all workflow controls—initial visibility.
- `MainViewModel.cs`: Holds reactive state, enforces initial disabled conditions.
- `App.axaml.cs`: Wires application and sets status message.

---
## Component Architecture
Component Roles:
- MainWindow: Container & visual orchestrator; no business logic.
- MainViewModel: Single source of truth for startup states; exposes properties, computed `CanProcess`.
- StatusBar (inline within MainWindow): Displays `StatusMessage` ("Ready" on launch).
- Placeholder Ship Table: DataGrid with empty collection; conveys future capacity.
- Time Pickers: Disabled until ship chosen (flag logic stubbed now—will remain disabled initially since `SelectedShip == null`).
- Process Button: Disabled; reasoning surfaced for test (via `AutomationProperties.HelpText`).

Design Patterns Applied:
- Presentational vs Orchestration separation (XAML vs ViewModel).
- Read-only binding for initially static structures (e.g., `ItemsSource` empty list).
- Declarative accessibility hooks using AutomationIds.

Communication Patterns:
- One-way bindings for initial display.
- Two-way will be enabled later for pickers; not active in initialization scenario.

Accessibility Considerations:
- All interactive placeholders include `AutomationProperties.Name` for screen readers.
- Disabled controls still announced with descriptive HelpText.

Testability:
- Direct selectors via `AutomationId` implement the required `data-testid` equivalence.
- Deterministic initial snapshot (no async race) simplifies UI automation.

End-to-End Testing Hooks:
- `AutomationId="main-window"` root presence asserts successful initialization.
- Each required control asserts existence and initial disabled state where applicable.

---
## Data Integration Strategy (Initialization Scope)
No external data loaded. Strategy is intentionally inert:
- Ship list = empty list.
- Time interval fields = null.
- Disabled logic hard-coded returning false for `CanProcess` with a placeholder reason: "Select input/output folders, ship, and time interval".

Error Handling (Launch):
- None expected; initialization guarded against null references by default values.
- Potential XAML load failure → Avalonia runtime throws; out-of-scope for feature design (handled at platform level).

Edge Cases Considered:
- Missing assets (icon): window still loads; status unchanged.
- High DPI scaling: Avalonia auto-scales—no custom logic required.

Observability for Tests:
- All initial property values exposed; enabling snapshot comparisons.

---
## Implementation Examples
### MainWindow.axaml (Initialization Skeleton)
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:AisToXmlRouteConvertor.ViewModels"
        x:Class="AisToXmlRouteConvertor.MainWindow"
        Title="AIS to XML Route Converter"
        Width="1200" Height="800"
        AutomationProperties.AutomationId="main-window">
  <Window.DataContext>
    <vm:MainViewModel />
  </Window.DataContext>
  <DockPanel LastChildFill="True">
    <!-- Status Bar -->
    <Border DockPanel.Dock="Bottom" Padding="8,4" Background="#F0F0F0">
      <TextBlock Text="{Binding StatusMessage}" FontSize="12" />
    </Border>
    <ScrollViewer>
      <StackPanel Margin="20" Spacing="18">
        <!-- Input Folder Picker -->
        <StackPanel AutomationProperties.AutomationId="input-folder-picker">
          <TextBlock Text="Input Folder" FontWeight="Bold" />
          <TextBox Text="{Binding InputFolder}" IsReadOnly="True" Watermark="No folder selected" />
        </StackPanel>
        <!-- Output Folder Picker -->
        <StackPanel AutomationProperties.AutomationId="output-folder-picker">
          <TextBlock Text="Output Folder" FontWeight="Bold" />
          <TextBox Text="{Binding OutputFolder}" IsReadOnly="True" Watermark="No folder selected" />
        </StackPanel>
        <!-- Ship Table Placeholder -->
        <DataGrid ItemsSource="{Binding AvailableShips}" Height="180"
                  AutomationProperties.AutomationId="ship-table" />
        <!-- Static Data Panel Placeholder -->
        <Border AutomationProperties.AutomationId="ship-static-panel" BorderBrush="#DDD" BorderThickness="1" Padding="10">
          <TextBlock Text="Ship static data will appear after selection." FontStyle="Italic" />
        </Border>
        <!-- Time Pickers (Disabled Initially) -->
        <StackPanel Orientation="Horizontal" Spacing="12">
          <StackPanel AutomationProperties.AutomationId="time-start">
            <TextBlock Text="Start Time (UTC)" />
            <DatePicker IsEnabled="False" />
          </StackPanel>
          <StackPanel AutomationProperties.AutomationId="time-end">
            <TextBlock Text="End Time (UTC)" />
            <DatePicker IsEnabled="False" />
          </StackPanel>
        </StackPanel>
        <!-- Process Button (Disabled) -->
        <Button Content="Process!"
                AutomationProperties.AutomationId="process-btn"
                IsEnabled="{Binding CanProcess}" />
      </StackPanel>
    </ScrollViewer>
  </DockPanel>
</Window>
```

### MainViewModel.cs (Initialization State)
```csharp
namespace AisToXmlRouteConvertor.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string? inputFolder;
    [ObservableProperty] private string? outputFolder;
    [ObservableProperty] private ObservableCollection<object> availableShips = new(); // Placeholder type; replaced later.
    [ObservableProperty] private object? selectedShip; // Will become ShipStaticData.
    [ObservableProperty] private System.DateTime? startTimeUtc;
    [ObservableProperty] private System.DateTime? endTimeUtc;
    [ObservableProperty] private string statusMessage = "Ready";
    [ObservableProperty] private string? processDisabledReason = "Select input/output folders, ship, and time interval";

    public bool CanProcess => false; // Feature 10.1: always false at init; extended by later features.
}
```

### AutomationId Mapping (Test Selector Strategy)
| BDD data-testid | Avalonia Implementation (AutomationId) |
|-----------------|-----------------------------------------|
| main-window     | main-window                             |
| input-folder-picker | input-folder-picker                 |
| output-folder-picker | output-folder-picker               |
| ship-table      | ship-table                              |
| ship-static-panel | ship-static-panel                     |
| time-start      | time-start                              |
| time-end        | time-end                                |
| process-btn     | process-btn                             |

Rationale: Avalonia automation IDs accessible in UI automation and custom test harnesses; consistent with BDD.

---
## Testing Strategy and Quality Assurance
Goal: Assert presence and baseline states—no functional interactions yet.

Test Types:
- Unit: None (pure property initialization already covered by toolkit reliability; optional minimal test verifying defaults).
- UI Automation / Integration: Single scenario test verifying all controls exist & `process-btn` disabled.

Scenario Coverage:
"Display main window controls on launch":
Assertions:
- Main window visible.
- Each required AutomationId present exactly once.
- Process button disabled.
- Ship table has zero rows.
- Time pickers disabled.
- Status message equals "Ready".

Negative Assertions:
- No unexpected modal dialogs.
- No filesystem access performed (can verify by monitoring directory watchers—optional future enhancement).

Selectors Strategy:
- Use `AutomationId` exclusively; avoid brittle visual tree traversal.

Mock Data Requirements (Minimal for Initialization):
- User context (operator identity) not rendered yet; scenario assumption satisfied implicitly.
- Provide a `MockEnvironmentContext` static test helper (future) returning simulated OS + user; not needed for initialization tests.

Fixtures:
```csharp
public static class UiLaunchFixture
{
    public static MainViewModel Create() => new();
}
```

Integration Test Sketch:
```csharp
[Fact]
public void MainWindow_OnLaunch_RendersAllRequiredControls()
{
    using var app = AvaloniaAppTestHost.Start();
    var window = app.GetWindowByAutomationId("main-window");
    window.Should().NotBeNull();
    foreach (var id in new[]{"input-folder-picker","output-folder-picker","ship-table","ship-static-panel","time-start","time-end","process-btn"})
        app.GetControlByAutomationId(id).Should().NotBeNull();
    app.GetControlByAutomationId("process-btn").IsEnabled.Should().BeFalse();
}
```

Accessibility Validation:
- Check each control has `AutomationProperties.Name` (added in future refinements; placeholder acceptable here).

---
## Mock Data Requirements
Centralized mock approach (from QA testing strategy) will later introduce standardized fixtures; for Feature 10.1 none are strictly required. Still, we define placeholders to align with future consistency:
- `MockUser` object (operator role) reserved for later permission-based UI adaptations.
- `MockEmptyShipCollection` representing initial `AvailableShips` state.
- Exposure: ViewModel property surfaces collection length (currently zero) for test assertions.

Helper Functions (Future Preparedness):
```csharp
public static class MockDataFactory
{
    public static ObservableCollection<object> EmptyShips() => new();
    public static string OperatorUserId() => "person-alice"; // Aligns with BDD example.
}
```

Data Exposure:
- `AvailableShips.Count` used in UI test to assert zero.

---
## Conceptual Explanation Summary Per Component
1. MainWindow: Presents full workflow skeleton immediately—reduces cognitive load.
2. MainViewModel: Centralizes initial reactive properties—facilitates future incremental enablement logic without structural change.
3. Ship Table Placeholder: Communicates forthcoming data region—avoids layout shift.
4. Time Pickers Disabled: Affordance indicating dependency on ship selection—reinforces workflow order.
5. Process Button Disabled: Early expectation management—prevents user from guessing prerequisites.
6. AutomationIds: Deterministic test contract—stable across refactors.

---
## Design Validation Checklist
✓ Uses approved MVVM pattern (CommunityToolkit.Mvvm).
✓ No deprecated components.
✓ Single-project structure preserved.
✓ No premature service or parsing calls at startup.
✓ All required controls rendered with correct AutomationIds.
✓ Process button intentionally disabled.
✓ Testability provisions defined.

---
## Risks & Mitigations
| Risk | Impact | Mitigation |
|------|--------|-----------|
| Future features require additional initialization logic | Potential refactor of MainViewModel | Encapsulate only baseline properties now; extend with partial classes or additional properties later |
| Absence of async may block UI during large future scans | UI freeze | Scans implemented in later features can adopt async without changing initialization layout |
| AutomationId collision in future custom controls | Test instability | Reserve unique IDs with clear naming; maintain registry in testing docs |
| Expandable complexity of status messaging | Confusing user feedback | Wrap status logic later in minimal status service—out of scope now |

---
## Non-Functional Considerations (Initialization Scope)
- Performance: < 5ms ViewModel construction, < 50ms window render typical.
- Accessibility: Foundational hooks (AutomationIds) prepared; full ARIA naming added in subsequent UI polish iteration.
- Portability: No platform-specific APIs used.
- Maintainability: Low code surface; clear extension points.

---
## Future Extension Points
- Inject lightweight logging into initialization (status: launch time).
- Persist last window size / position.
- Theming selection applied pre-render (dark mode toggle at startup).
- Centralized selector constants for cross-test reuse.

---
## Conclusion
Feature 10.1 establishes a clean, accessible, and testable starting UI that reflects the complete conversion workflow without executing domain logic prematurely. This foundation minimizes cognitive load, accelerates operator onboarding, and provides deterministic selectors for robust automated testing. Subsequent features layer functionality onto this stable scaffold without structural disruption.
