# Feature Design:: Getting Started

This document outlines the technical design for the Getting Started feature.

> **Scope**: this design covers all scenarios in the Getting Started feature as defined in docs/spec_scenarios/getting_started.md:
> - Install and start AISRouting UI
> - Select input data root with vessel subfolders
> - Fail when input root empty
> - Prevent start when executable missing or corrupted

## Feature Overview

The Getting Started feature establishes the foundation for the AISRouting application. It provides the user flows and system behaviors required to:
- Install and launch the desktop application
- Select an input data root folder containing vessel subfolders and CSV/JSON files
- Surface the available vessels and basic metadata to the UI
- Validate input folder contents and present clear, actionable errors when the folder is invalid or inaccessible

Business value: a reliable and well-tested startup flow dramatically lowers the barrier to first use, prevents data-related errors from propagating into downstream features (ship selection, track creation, export), and enables automated tests and CI validation for the rest of the application.

User needs addressed:
- Quick discoverability of required input data
- Immediate feedback when the environment is misconfigured
- Clear guidance when remediation is required (missing files, unreadable folders, corrupted executable)

High-level approach and architectural philosophy:
- Follow MVVM and layered architecture: presentation (WPF) uses ViewModels that call into well-defined services in Core/Infrastructure.
- Favor small, testable services with single responsibilities (Folder scanning, static-data loading, CSV position loader, folder dialog abstraction).
- Make startup and folder selection flows resilient: validation, async background scanning, progress reporting, cancellation, and clear logging.
- Design for testability: all I/O behind interfaces, UI test hooks with data-testids or accessible names, and centralized mock data fixtures for automation.

## Architectural Approach

Architectural patterns and principles applied:
- MVVM for separation of concerns (Views ?? ViewModels ?? Services)
- Service Layer Pattern: ISourceDataScanner, IShipStaticDataLoader, IFolderDialogService
- Dependency Injection for all services (IServiceCollection via App.xaml.cs)
- Async I/O, streaming CSV parsing, and cancellation tokens for responsive UI
- Fail-fast validation at service boundaries with user-friendly messages surfaced by ViewModels

Component hierarchy and relationships (conceptual):
- MainWindow (View)
  - MainViewModel
    - ShipSelectionViewModel (child responsibilities)
    - TimeIntervalViewModel (child responsibilities)
    - TrackResultsViewModel (consumer of created tracks)
  - Services injected into ViewModels:
    - ISourceDataScanner (scans input folder, enumerates MMSI subfolders)
    - IShipStaticDataLoader (reads per-vessel static JSON files)
    - IFolderDialogService (abstracts folder selection dialog)

Data flow and state management strategy:
- User triggers Select Input Folder ? MainViewModel.SelectInputFolderCommand
- Folder dialog returned path passed to ISourceDataScanner.ScanInputFolder(path)
- ISourceDataScanner enumerates directories, calls IShipStaticDataLoader.LoadStaticData(dir, mmsi) ? builds ShipStaticData objects with MinDate/MaxDate
- MainViewModel.AvailableVessels bound to the UI (ObservableCollection<ShipStaticData>)
- UI reacts via data binding; selection populates SelectedVessel and enables downstream commands (Create Track) when valid
- Validation state maintained in ViewModels (TimeInterval.IsValid, SelectedVessel != null)

Integration patterns with existing systems:
- RouteExporter/TrackOptimizer untouched by this feature; this feature supplies the required inputs for them
- Logging and telemetry use Microsoft.Extensions.Logging across layers
- File operations are centralized in Infrastructure.IO classes for consistent error handling

User experience strategy and information architecture:
- Provide immediate visual feedback when scanning begins (progress bar / spinner) and completed (count of vessels)
- Use inline validation messages (below controls) for non-blocking guidance; use modal dialogs only for fatal errors (corrupted executable)
- Accessibility: expose control labels and data-testids for E2E tests (e.g., data-testid="input-folder-button", "ship-combo", "no-vessels-warning")

## File Structure

Follow the patterns from docs/tech_design/application_organization.md. Proposed files for this feature (new or existing locations shown):

- src/AISRouting.App.WPF/Views/ShipSelectionView.xaml
  - Purpose: UI for selecting input folder and listing available vessels.
- src/AISRouting.App.WPF/ViewModels/ShipSelectionViewModel.cs
  - Purpose: Encapsulates ship selection UI state and commands.
- src/AISRouting.App.WPF/ViewModels/MainViewModel.cs (no breaking change; ensure responsibilities split)
  - Purpose: Orchestrates top-level flow; delegates folder scanning to ISourceDataScanner.
- src/AISRouting.App.WPF/Services/FolderDialogService.cs
  - Purpose: Concrete IFolderDialogService for Ookii.Dialogs.Wpf; abstracted to allow unit tests to inject a mock.
- src/AISRouting.Core/Models/ShipStaticData.cs
  - Purpose: Domain model for vessel metadata (Name, MMSI, MinDate, MaxDate, sourcePath)
- src/AISRouting.Core/Services/Interfaces/ISourceDataScanner.cs
  - Purpose: Contract to scan an input folder and return enumerable ShipStaticData.
- src/AISRouting.Infrastructure/IO/SourceDataScanner.cs
  - Purpose: Implementation - enumerates directories, computes min/max dates from CSV filenames, calls IShipStaticDataLoader.
- src/AISRouting.Infrastructure/Parsers/ShipStaticDataParser.cs
  - Purpose: Parse per-vessel JSON static data into ShipStaticData model.
- src/AISRouting.Tests/IntegrationTests/InputFolderScanningTests.cs
  - Purpose: Integration tests using fixture folders under tests/TestData/ to validate scanning results.

File responsibilities and naming conventions follow Application Organization (one class per file, class name == filename). Include comments in files to explain purpose and test hooks.

Example directory tree (partial):

src/
  AISRouting.App.WPF/
    Views/
      ShipSelectionView.xaml        # Input folder control + vessel combo
    ViewModels/
      ShipSelectionViewModel.cs     # Data binding, commands, validation
  AISRouting.Core/
    Models/
      ShipStaticData.cs
    Services/Interfaces/
      ISourceDataScanner.cs
  AISRouting.Infrastructure/
    IO/
      SourceDataScanner.cs
    Parsers/
      ShipStaticDataParser.cs

Test data and fixtures:
- src/AISRouting.Tests/TestData/205196000/205196000.json
- src/AISRouting.Tests/TestData/205196000/2025-03-15.csv
- route_waypoint_template.xml (already in repo root)

Comments:
- Each new View must include data-testid attributes for critical controls to support reliable E2E testing (see QA_test_locators_instructions.md). Add these attributes only when they don't conflict with existing selectors.

## Component Architecture

Components and their roles:

1) ShipSelectionView (WPF UserControl)
- Purpose and role: present an input-folder selector button and a ComboBox listing discovered vessels. Show static attributes in a read-only area.
- Design patterns: Stateless View with bindings to ShipSelectionViewModel; follow XAML DataTemplate patterns for readability.
- Information architecture: Top-level Input Folder control, then vessel ComboBox, then static data panel.
- UX strategy: spinner/progress while scanning; show counts and inline warnings if no vessels found.
- Integration: binds to ShipSelectionViewModel.AvailableVessels and SelectedVessel.
- Accessibility: provide Labels for ComboBox and Input Folder button; ComboBox should expose automation properties (AutomationProperties.AutomationId) aligning with data-testids.
- Testability: include data-testids and named elements for Playwright or UI automation to select (e.g., Button with data-testid="select-input-folder").

2) ShipSelectionViewModel
- Role: encapsulate the state for folder selection, available vessel list, and validation state.
- Design patterns: uses ObservableObject from CommunityToolkit.Mvvm, IAsyncRelayCommand for async commands.
- Responsibilities:
  - SelectInputFolderCommand: calls IFolderDialogService.ShowDialog and then _scanner.ScanInputFolder
  - Manage AvailableVessels (ObservableCollection<ShipStaticData>)
  - Expose SelectedVessel and validation messages (NoVesselsWarning, FolderAccessError)
- Data flow: receives ShipStaticData from ISourceDataScanner and updates UI-bound collections.
- Error handling: translate exceptions from scanner into user-friendly messages and log details.
- Test hooks: expose internal ScanCancellationTokenSource for tests to cancel long scans.

3) ISourceDataScanner / SourceDataScanner
- Purpose: discover MMSI folders and build ShipStaticData with min/max dates.
- Design patterns: single responsibility service with async scanning and streaming results via IAsyncEnumerable<ShipStaticData> (or Task<IEnumerable<ShipStaticData>> for simpler consumers).
- Implementation notes:
  - Use Directory.EnumerateDirectories for streaming large folders
  - For each directory: determine MMSI from folder name, attempt to load static JSON via IShipStaticDataLoader, call ExtractMinMaxDatesFromCsvFiles
  - Robustness: wrap per-folder operations in try/catch; skip invalid folders and log warnings rather than failing the entire scan.
  - CancellationToken usage to abort long-running scans.
- Testability: unit tests should mock the file system via an abstraction (IFileSystem) or use test fixture folders.

4) IFolderDialogService / FolderDialogService
- Purpose: abstract Ookii.Dialogs.Wpf folder browser to allow injecting a mock during tests
- Pattern: simple interface with ShowFolderBrowser() ? string? (null when cancelled)
- Implementation detail: return a normalized path; validate access before returning to caller.

State management strategy across components:
- ViewModels hold UI state; services return pure models (ShipStaticData)
- Commands disabled/enabled via CanExecute derived from validation properties
- Errors surfaced via properties (e.g., FolderErrorMessage) and logged

Accessibility and testing considerations:
- Add AutomationProperties.AutomationId and data-testids to key controls: select input folder button, ship combo, no-vessels warning, folder-error banner.
- Expose stable strings in resource files for messages so tests can assert localized messages if needed.

End-to-end testing considerations per component:
- Reliable selectors: data-testid attributes on buttons and combo box
- Observable state changes: AvailableVessels collection change triggers UI update; tests can wait for collection count > 0
- Test data requirements: fixtures with known vessel folders and CSV files

## Data Integration Strategy

How data flows through the system for this feature:
1. User selects folder path via IFolderDialogService
2. MainViewModel calls ISourceDataScanner.ScanInputFolder(path)
3. SourceDataScanner enumerates directories and for each:
   - Calls IShipStaticDataLoader.LoadStaticData(dir, mmsi)
   - Calls ExtractMinMaxDatesFromCsvFiles(dir) to compute MinDate/MaxDate
   - Builds ShipStaticData with properties: MMSI (long), DisplayName (string), MinDate, MaxDate, SourcePath
4. ViewModel receives list of ShipStaticData and exposes it to the UI

Service integration patterns and error handling:
- Use try/catch per-folder and continue scanning on non-fatal errors; accumulate warnings to show to the user after scan completes
- File access errors produce a FolderAccessError state; scanning stops only on unrecoverable errors (e.g., root path not found)
- For empty folder (no MMSI subfolders) return empty collection and set NoVesselsWarning message in ViewModel

Edge cases and mitigation:
- Permission denied on some subfolders: log warning, skip those folders, present partial results with a warning banner
- Malformed JSON static file: skip and fallback to folder-name as display name
- CSV filename date parsing failure: ignore that file and log; treat folder as missing timestamp info

Testing considerations for data flow validation:
- Integration tests using fixture directories to validate MinDate/MaxDate extraction
- Unit tests for parser behavior (ShipStaticDataParser) with malformed JSON

## Implementation Examples

The following code snippets illustrate key patterns. These are intentionally concise and include architectural comments.

ISourceDataScanner (interface):

```csharp
public interface ISourceDataScanner
{
    Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(string inputFolder, CancellationToken cancellationToken = default);
}
```

SourceDataScanner (implementation sketch):

```csharp
public class SourceDataScanner : ISourceDataScanner
{
    private readonly IShipStaticDataLoader _staticLoader;
    private readonly ILogger<SourceDataScanner> _logger;

    public async Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(string inputFolder, CancellationToken cancellationToken = default)
    {
        var results = new List<ShipStaticData>();
        if (!Directory.Exists(inputFolder))
            throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");

        foreach (var dir in Directory.EnumerateDirectories(inputFolder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mmsiString = Path.GetFileName(dir);
            try
            {
                var staticData = await _staticLoader.LoadStaticDataAsync(dir, mmsiString, cancellationToken);
                staticData.MinDate = ExtractMinDateFromCsvFiles(dir);
                staticData.MaxDate = ExtractMaxDateFromCsvFiles(dir);
                results.Add(staticData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping folder {Dir} during input scan", dir);
            }
        }

        return results;
    }
}
```

ShipSelectionViewModel (relevant portions):

```csharp
public partial class ShipSelectionViewModel : ObservableObject
{
    private readonly ISourceDataScanner _scanner;
    private readonly IFolderDialogService _folderDialog;

    public ObservableCollection<ShipStaticData> AvailableVessels { get; } = new();
    [ObservableProperty] private ShipStaticData _selectedVessel;
    [ObservableProperty] private string _folderErrorMessage;
    [ObservableProperty] private bool _isScanning;

    public IAsyncRelayCommand SelectInputFolderCommand { get; }

    public ShipSelectionViewModel(ISourceDataScanner scanner, IFolderDialogService folderDialog)
    {
        _scanner = scanner;
        _folderDialog = folderDialog;
        SelectInputFolderCommand = new AsyncRelayCommand(SelectInputFolderAsync);
    }

    private async Task SelectInputFolderAsync()
    {
        var folder = _folderDialog.ShowFolderBrowser();
        if (string.IsNullOrEmpty(folder)) return;

        try
        {
            IsScanning = true;
            var vessels = await _scanner.ScanInputFolderAsync(folder);
            AvailableVessels.Clear();
            foreach (var v in vessels) AvailableVessels.Add(v);

            if (!AvailableVessels.Any())
                FolderErrorMessage = "No vessels found in input root";
            else
                FolderErrorMessage = null;
        }
        catch (DirectoryNotFoundException)
        {
            FolderErrorMessage = "Input root not accessible";
        }
        finally
        {
            IsScanning = false;
        }
    }
}
```

View (XAML) guidance (ShipSelectionView.xaml):
- Provide Button with data-testid="select-input-folder"
- Provide ComboBox with data-testid="ship-combo" bound to AvailableVessels
- Provide TextBlock with data-testid="no-vessels-warning" bound to FolderErrorMessage and visible when non-empty

## Testing Strategy and Quality Assurance

Testing goals:
- Validate happy paths (folder with vessels, UI enables Create Track)
- Validate negative scenarios (empty folder, inaccessible path, corrupted executable)
- Ensure UI elements have stable selectors for automation

Test types and mapping to scenarios:

1) Unit Tests
- SourceDataScanner: test behavior when directories contain valid/invalid static files and CSVs
- ShipStaticDataParser: parse valid and malformed JSON
- ShipSelectionViewModel: command behavior, validation messages, and state transitions

2) Integration Tests (file-based)
- Use test fixtures under src/AISRouting.Tests/TestData/205196000 to assert scanning returns expected ShipStaticData and Min/Max dates
- Validate that malformed CSV rows do not crash the scan

3) UI/Automation Tests
- End-to-end scenario using a UI testing tool: select folder ? wait for AvailableVessels to populate ? verify ComboBox items
- Use data-testids as selectors (see QA_test_locators_instructions.md) to avoid brittle selectors

Test data management and fixtures (centralized approach):
- Reuse the TestData folder described in application_organization.md
- Centralized fixture examples:
  - tests/fixtures/inputFolders/gettingStarted/205196000/205196000.json
  - tests/fixtures/inputFolders/gettingStarted/205196000/2025-03-15.csv
- Provide helper functions for test fixtures: CreateTestInputFolder(root, fixtureName) that copies necessary files into a temp directory and returns the path

Testing hooks and selectors:
- Provide data-testids on controls documented earlier
- ViewModels expose state properties (IsScanning, AvailableVessels.Count) so tests can wait on these values

Accessibility testing:
- Ensure ComboBox and Buttons have associated Labels
- Ensure warning messages are announced to screen readers (use AutomationProperties.LiveSetting="Polite" on the warning TextBlock)

Edge case tests:
- Permission denied on subfolder: scanner skips and logs warning; test asserts warning recorded via logger mock or visible UI banner
- Non-CSV files present: ignored, scanner still returns valid vessels

## Mock Data Requirements

Follow centralized mock data approach in docs/tech_design/testing/QA_testing.md.

Required mock data objects and fixtures:
- inputFolders.gettingStarted fixture:
  - 205196000.json (Ship static data with Name, MMSI)
  - one or more CSV files (YYYY-MM-DD.csv) with sample AIS rows to validate MinDate/MaxDate extraction
- Helper functions for tests (to be placed under tests/helpers/fixtureHelpers.cs):
  - CreateTestInputFolderFromFixture(string fixtureName) -> string path
  - CreateRandomizedCsvWithTimestamps(DateTime start, DateTime end, int rows) -> CSV file used by fixtures

Test data exposure for components:
- ViewModels should allow injection of ISourceDataScanner that can be implemented by a TestSourceDataScanner returning in-memory ShipStaticData objects
- Integration tests should use file-based fixtures referenced above

Reference the centralized mock data approach:
- Use tests/TestData/ as canonical source; do not duplicate fixtures across tests
- All fixtures should be small (a few CSV rows) to keep test runs fast

## Implementation Checklist and Notes

- [x] Add ShipSelectionView and ShipSelectionViewModel following Application Organization
- [x] Add ISourceDataScanner and SourceDataScanner implementation in Infrastructure
- [x] Add FolderDialogService wrapper for Ookii.Dialogs.Wpf and register IFolderDialogService in DI
- [x] Add data-testids to UI elements for reliable automation (only in new/changed views)
- [x] Create integration tests using fixtures in src/AISRouting.Tests/TestData/
- [x] Ensure all file I/O is async and cancellation-aware

## Implementation Risks and Mitigations

- Risk: Scanning extremely large input roots could be slow. Mitigation: stream results and show incremental progress; allow cancellation.
- Risk: Partial access permission errors yield inconsistent results. Mitigation: per-folder try/catch with aggregated warnings presented to the user.
- Risk: Tests relying on file system may be flaky on CI due to permissions. Mitigation: use temporary directories with controlled permissions and small fixtures; prefer mocking where appropriate.

## Changelog

> **Changelog**
> Created on: 2025-11-20
> - Initial design for "Getting Started" feature including folder scanning, UI, and test plans


<!-- End of Getting Started feature design -->
