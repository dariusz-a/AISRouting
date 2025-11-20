# Feature Design:: Input Folder Control

This document outlines the technical design for the Input Folder Control feature (Feature 3.1).

#### Feature Overview
- Purpose: Provide a cross-platform UI control and supporting services that allow users to select an input folder, validate it contains MMSI subfolders, and surface errors or metadata required by downstream processing.
- Business value: prevents user errors by validating input data early, improves UX by remembering last-used paths and surfacing clear guidance, and enables downstream features (ship scanning, CSV/JSON parsing) to run reliably.
- Scope: covers folder selection UI, folder scanning service, validation rules, UX for error messages, persistence of last-used path, and test hooks for end-to-end automation.

#### Architectural Approach
- Pattern: Small feature-scope MVVM implementation using existing application architecture (Avalonia UI for views, .NET 9 for services and view-models). Keep UI thin — business logic in services and view-models.
- Principles: Single Responsibility (View handles presentation, ViewModel handles state and commands, Service performs filesystem scanning/validation), Dependency Injection for testability, and observable state for E2E tests.
- Component hierarchy:
  - View: `InputFolderView` (Avalonia XAML)
  - ViewModel: `InputFolderViewModel`
  - Service: `IInputFolderService` / `InputFolderService`
  - Store: small `AppSettings` persistence via existing settings provider
  - Tests/UI Automation: Playwright or Test Runner (project uses Playwright `.spec.ts` style E2E tests)

#### File Structure
The structure follows `docs/tech_design/application_organization.md` conventions.

`/src/Features/InputFolderControl/`
- `InputFolderView.xaml` : Avalonia view. Contains folder-picker UI and validation message areas. (Purpose: present controls, emit selection commands)
- `InputFolderView.xaml.cs` : Code-behind only for wiring events to ViewModel-friendly commands.
- `InputFolderViewModel.cs` : Exposes `SelectedPath`, `IsValid`, `ValidationMessage`, `ScanResult`, and `SelectFolderCommand`.
- `Services/IInputFolderService.cs` : Interface for folder scanning and validation operations.
- `Services/InputFolderService.cs` : Implementation that scans directory for MMSI subfolders and returns structured results.
- `Models/InputFolderScanResult.cs` : DTO containing list of discovered MMSI folders, error codes, and diagnostics.
- `Stores/AppSettings.cs` : Reads/writes `LastInputFolder` (utilizes existing settings mechanism)
- `Tests/` : Integration test helpers and test fixtures referenced by E2E tests (see centralized mocks)

Notes on naming: follow PascalCase for C# files and types, `kebab-case` or PascalCase for resource keys as established in the architecture docs.

#### Component Architecture
InputFolderView (View)
- Role: display an input folder field, a `Browse...` button, a `Scan` action, and inline validation errors.
- Design patterns: accessible labels, aria equivalents, and stable test ids for automation (see `Testing Strategy` below).
- Interaction: triggers `SelectFolderCommand` on ViewModel; shows `ValidationMessage` from ViewModel.

InputFolderViewModel
- Purpose: hold selected path state, expose commands for folder picking and triggering `IInputFolderService` scans.
- Responsibilities: validate path format, call `IInputFolderService.ScanAsync`, update `ScanResult`, persist last-used folder in `AppSettings` on success.
- Observable state: properties implement `INotifyPropertyChanged` so E2E tests can observe changes.
- Error handling: Surface user-friendly messages for missing folders, unreadable folders, and no MMSI subfolders.

IInputFolderService / InputFolderService
- Purpose: encapsulate filesystem scanning and validation. Keep I/O and parsing out of UI layer so it can be unit-tested and mocked in E2E tests.
- Methods:
  - `Task<InputFolderScanResult> ScanAsync(string path, CancellationToken ct = default)`
  - `bool PathLooksLikeInputRoot(string path)` (lightweight quick check)
- Implementation notes: Use `System.IO.EnumerationOptions` and safe exception handling to avoid blocking UI thread; provide detailed diagnostics in `InputFolderScanResult` for troubleshooting and telemetry.

Models/InputFolderScanResult
- Fields: `IReadOnlyList<string> MmsiFolderNames`, `bool HasMmsiFolders`, `IReadOnlyList<string> Errors`, `DateTime ScannedAt`.

State & Persistence
- Persist last successful input folder in `AppSettings.LastInputFolder` via existing settings provider (encrypted storage not required for this non-sensitive path). Provide migration-safe defaults and optional user reset.

Integration Points
- Downstream: Ship scanning component will consume `InputFolderScanResult.MmsiFolderNames` via an application-level service or event bus.
- Upstream/UI: The top-level shell should allow injecting a default path (used in automated tests) via environment variable or test harness.

#### Data Integration Strategy
- Data flow:
  1. User selects folder (or auto-populated from `AppSettings`).
  2. ViewModel runs basic path checks and calls `IInputFolderService.ScanAsync`.
 3. Service returns `InputFolderScanResult` with detected MMSI folders.
 4. ViewModel sets `ScanResult`, updates UI commands and persists last-used path on success.
- Error handling:
  - For IO exceptions: return friendly error messages and an error code for telemetry.
  - For permission errors: show a specific actionable message "Folder not readable - check permissions".
  - For empty result (no MMSI folders): surface a clear message and provide an inline link to `input data preparation` help documentation.

#### Implementation Examples
Example: `IInputFolderService` interface

```csharp
public interface IInputFolderService
{
    Task<InputFolderScanResult> ScanAsync(string path, CancellationToken ct = default);
    bool PathLooksLikeInputRoot(string path);
}
```

Example: ViewModel skeleton (C#)

```csharp
public class InputFolderViewModel : ReactiveObject
{
    private readonly IInputFolderService _folderService;
    private readonly IAppSettings _settings;

    public string SelectedPath { get; set; }
    public InputFolderScanResult ScanResult { get; private set; }
    public string ValidationMessage { get; private set; }

    public ReactiveCommand<Unit, Unit> SelectFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ScanCommand { get; }

    public InputFolderViewModel(IInputFolderService folderService, IAppSettings settings)
    {
        _folderService = folderService;
        _settings = settings;
        SelectedPath = _settings.LastInputFolder ?? string.Empty;

        SelectFolderCommand = ReactiveCommand.CreateFromTask(async () => {
            // trigger folder picker via UI interaction (platform-abstracted)
        });

        ScanCommand = ReactiveCommand.CreateFromTask(async () => {
            var result = await _folderService.ScanAsync(SelectedPath);
            ScanResult = result;
            ValidationMessage = result.HasMmsiFolders ? string.Empty : "No MMSI subfolders found";
            if (result.HasMmsiFolders) _settings.LastInputFolder = SelectedPath;
        });
    }
}
```

Example: XAML (stable test ids)

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             x:Class="App.Features.InputFolderControl.InputFolderView">
  <StackPanel Margin="12">
    <TextBlock Text="Input Folder"/>
    <StackPanel Orientation="Horizontal">
      <TextBox x:Name="InputPath" Text="{Binding SelectedPath, Mode=TwoWay}" AutomationProperties.AutomationId="input-folder-path"/>
      <Button Content="Browse..." Command="{Binding SelectFolderCommand}" AutomationProperties.AutomationId="input-folder-browse"/>
    </StackPanel>
    <Button Content="Scan" Command="{Binding ScanCommand}" AutomationProperties.AutomationId="input-folder-scan"/>
    <TextBlock Text="{Binding ValidationMessage}" Foreground="Red" AutomationProperties.AutomationId="input-folder-validation"/>
  </StackPanel>
</UserControl>
```

Design rationale: include `AutomationId` attributes for reliable selectors in E2E tests and keep the XAML minimal to preserve accessibility.

#### Testing Strategy and Quality Assurance
- Goals: Enable deterministic E2E tests (Playwright or similar) and unit tests for service and view-model logic.
- Unit tests: mock `IInputFolderService` to validate `InputFolderViewModel` behavior (e.g., persistence of `LastInputFolder`, setting `ValidationMessage`).
- Integration/E2E: use a test harness to create temporary directory structures with MMSI-named folders and run the UI flow against them.
- Test selectors: use stable `AutomationId` or `data-test-id` attributes defined in XAML for automation.

End-to-end Scenarios to cover (derived from implementation_plan.md):
- Select valid input folder and scan MMSI subfolders (happy path)
- Show error when no MMSI subfolders found (edge case)
- Remember last-used path between runs (persistence)

Test hooks and fixtures:
- Provide an environment variable `AIROUTING_TEST_INPUT_ROOT` to pre-populate `SelectedPath` in tests.
- Provide `Tests/Fixtures/InputFolderFixtures.cs` utility to create temporary folder trees and cleanup.
- Expose a test-only `IInputFolderService` implementation that points to test fixture roots.

#### Mock Data Requirements
- Centralized mock approach: reference `tests/mocks/input_folder_fixtures.ts` (or `.cs` for unit tests) which exposes helper functions:
  - `createMmsiFolder(root, mmsi)`
  - `createCsvFile(folder, name, content)`
  - `cleanupFixture(root)`
- Playwright tests should import from centralized mock module to create folder structures prior to UI interactions.
- Unit tests should use in-memory or temporary filesystem helpers from the .NET testing framework (e.g., `System.IO.Abstractions.TestingHelpers`) and mock `IInputFolderService` using `Moq` or `NSubstitute`.

#### Accessibility and UX Considerations
- Keyboard accessible folder picker and scan actions.
- Clear focus states and readable validation messages.
- Provide contextual help link to `docs/spec_scenarios/ui_input_folder.md` when validation fails.

#### Deployment and Configuration
- No special permission or platform-specific code required; use Avalonia abstractions for dialogs.
- CI: unit tests run on .NET test runner; E2E Playwright tests require a small helper to create temporary folder fixtures on the CI agent.

#### Acceptance Criteria (for reviewers)
- `InputFolderView` shows and accepts a path and exposes `Scan` behavior.
- `InputFolderService` returns a structured `InputFolderScanResult` for valid folders.
- `LastInputFolder` persists and restores between runs.
- E2E tests cover the three scenarios listed above using centralized fixtures and stable selectors.

---
> Changelog
> Created on: 2025-11-20
> - Initial design for Feature 3.1: Input Folder Control
