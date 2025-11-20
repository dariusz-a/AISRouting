# Feature Design:: Ship Selection and Static Data

This document outlines the technical design for the Ship Selection and Static Data feature

## Feature Overview

- Purpose: Provide a robust UI and services for selecting a vessel (by MMSI), presenting its
  static metadata, and selecting an available time range for track generation. This feature
  covers all scenarios in the BDD: populating the ship combo box, showing static data,
  defaulting start/stop from file timestamps, fallback names, time-range validation,
  seconds-resolution pickers, and handling missing input root.

- Business value: Enables users to identify a vessel and a valid time window quickly so
  that track generation (later feature) receives clean, validated inputs. Reduces user errors
  and provides clear feedback when input data is absent or malformed.

- High-level approach: Follow the existing MVVM, DI, and service-layer patterns. Implement
  a `ShipSelectionView` + `ShipSelectionViewModel` that depends on `ISourceDataScanner` and
  `IShipStaticDataLoader`. Use `TimeInterval` domain model from `AISRouting.Core`. All I/O
  is performed by Infrastructure services with asynchronous streaming and cancellation.

## Architectural Approach

- Patterns applied:
  - MVVM for separation of UI and logic.
  - Service Layer pattern for I/O and parsing (`ISourceDataScanner`, `IShipStaticDataLoader`).
  - Dependency Injection for testability and composability.
  - Validation at ViewModel boundaries; domain validators in Core for reusability.

- Component hierarchy and relationships:
  - `MainWindow` contains a `ShipSelectionView` region.
  - `ShipSelectionViewModel` exposes `AvailableVessels`, `SelectedVessel`, and `TimeInterval`.
  - `ISourceDataScanner.ScanInputFolder(inputRoot)` returns `IEnumerable<ShipStaticData>`.
  - `IShipStaticDataLoader.LoadStaticData(folderPath, mmsi)` loads JSON and supplies defaults.
  - `IFolderDialogService` used for folder selection; mocked in tests.

- Data flow and state management:
  1. User invokes `SelectInputFolderCommand` → `IFolderDialogService` returns path.
  2. `ShipSelectionViewModel` calls `ISourceDataScanner.ScanInputFolder(path)`.
  3. Scanner enumerates folders, uses `IShipStaticDataLoader` to create `ShipStaticData`.
  4. `AvailableVessels` collection updated (ObservableCollection) and bound to combo box.
  5. On vessel selection, `SelectedVessel` update triggers `TimeInterval` defaults set from `MinDate/MaxDate`.
  6. UI validation (Start < Stop) uses `TimeInterval.IsValid` and disables Create Track command if invalid.

- Integration patterns with existing systems:
  - Use `System.Text.Json` options and `CsvHelper` patterns already defined in `data_models.md`.
  - Follow logging and error handling conventions from `overall_architecture.md`.

## File Structure

Follow patterns from `docs/tech_design/application_organization.md` for naming and placement.

Recommended file layout (new/modified files):

```
src/AISRouting.App.WPF/
  Views/
    ShipSelectionView.xaml            # UI XAML for ship combo and static textbox
  ViewModels/
    ShipSelectionViewModel.cs         # ViewModel for selection logic and validation
  Services/
    ShipSelectionUserControl.xaml.cs  # Optional code-behind limited to view-only concerns

src/AISRouting.Core/
  Models/
    ShipStaticData.cs                 # Domain model (existing)
    TimeInterval.cs                    # Domain model (existing)
  Services/
    Interfaces/
      ISourceDataScanner.cs           # existing interface
      IShipStaticDataLoader.cs        # existing interface
    Validators/
      TimeIntervalValidator.cs        # ensures Start < Stop

src/AISRouting.Infrastructure/
  IO/
    SourceDataScanner.cs              # enumerates MMSI folders and uses loader
    JsonReader.cs / ShipStaticDataParser.cs  # existing parsers used

src/AISRouting.Tests/
  UnitTests/ViewModels/
    ShipSelectionViewModelTests.cs
  IntegrationTests/
    ShipSelectionIntegrationTests.cs  # uses TestData folder
  TestData/
    205196000/                         # sample folder with JSON and CSV files
```

Purpose note for files:
- `ShipSelectionView.xaml`: Presents `AvailableVessels` combo box with `data-testid` attributes
  for automation: `data-testid="ship-combo"`, static data textbox `data-testid="ship-static"`,
  start/stop pickers `data-testid="start-picker"`/`stop-picker` and validation message `data-testid="time-error"`.
- `ShipSelectionViewModel.cs`: Contains commands `SelectInputFolderCommand`, `RefreshVesselsCommand`.
- `SourceDataScanner.cs`: Responsible for enumerating directories, extracting min/max dates, logging, and returning `ShipStaticData` instances.

## Component Architecture

1) ShipSelectionView (View)
- Purpose and Role: Provide accessible controls to select input root, pick vessel, and view static metadata and time range.
- Design Patterns: Pure XAML view, minimal code-behind; use bindings to ViewModel properties and commands.
- Integration: Binds to `ShipSelectionViewModel.SelectedVessel`, `AvailableVessels`, `TimeInterval`.
- Accessibility: Provide labels and automation ids, keyboard navigation, and proper focus scopes.
- Testing hooks: Add `data-testid` attributes and accessible names for Playwright selectors.

Example XAML snippet (conceptual):
```xml
<ComboBox ItemsSource="{Binding AvailableVessels}"
          SelectedItem="{Binding SelectedVessel}"
          DisplayMemberPath="Name"
          x:Name="ShipCombo"
          Tag="data-testid:ship-combo"/>
```

2) ShipSelectionViewModel
- Purpose and Role: Orchestrates scanning, selection, defaults, and validation.
- Responsibilities:
  - Execute folder selection and scanning async
  - Maintain `ObservableCollection<ShipStaticData> AvailableVessels`
  - Expose `ShipStaticData SelectedVessel` and update `TimeInterval` defaults
  - Validate `TimeInterval` (Start < Stop)
  - Emit user-friendly error messages for missing input root or invalid ranges

Key properties and commands:
```csharp
public ObservableCollection<ShipStaticData> AvailableVessels { get; }
public ShipStaticData? SelectedVessel { get; set; }
public TimeInterval TimeInterval { get; set; }
public IAsyncRelayCommand SelectInputFolderCommand { get; }
public IAsyncRelayCommand RefreshVesselsCommand { get; }
public IRelayCommand CreateTrackCommand { get; }
public string? ValidationMessage { get; private set; }
```

Behavioral details:
- When `SelectedVessel` set:
  - If `SelectedVessel.Name` is null or empty → show `SelectedVessel.FolderName` as display (fallback behavior implemented in view model mapping or UI DisplayMember via a converter).
  - Default `TimeInterval.Start = SelectedVessel.MinDate` and `TimeInterval.Stop = SelectedVessel.MaxDate.AddDays(1)` (per data_models.md requirement).
  - Raise property changed notifications for UI to update.

- Validation:
  - Use `TimeInterval.IsValid` property. If false, set `ValidationMessage = "Invalid time range"` and disable `CreateTrackCommand`.

Code example: setting defaults and validation
```csharp
private void OnSelectedVesselChanged()
{
    if (SelectedVessel == null) return;
    TimeInterval.Start = SelectedVessel.MinDate;
    TimeInterval.Stop = SelectedVessel.MaxDate.AddDays(1);
    ValidateTimeInterval();
}

private void ValidateTimeInterval()
{
    if (!TimeInterval.IsValid)
    {
        ValidationMessage = "Invalid time range";
        CreateTrackCommand.NotifyCanExecuteChanged();
    }
    else
    {
        ValidationMessage = null;
    }
}
```

3) SourceDataScanner (Infrastructure)
- Purpose and Role: Enumerate vessel folders under input root and construct `ShipStaticData`.
- Responsibilities:
  - Use `Directory.GetDirectories(inputRoot)` to list folders.
  - For each folder, attempt to read `<MMSI>.json` using `IShipStaticDataLoader`.
  - Extract min/max dates from CSV filenames using `ExtractMinMaxDatesFromFolder` helper.
  - Ensure robust error handling: missing JSON → fallback to folder name; malformed JSON → log warning and fallback.

Implementation notes (pseudo):
```csharp
public async Task<IEnumerable<ShipStaticData>> ScanInputFolder(string inputRoot)
{
    var dirs = Directory.GetDirectories(inputRoot);
    var results = new List<ShipStaticData>();
    foreach (var dir in dirs)
    {
        var mmsi = Path.GetFileName(dir);
        var staticData = await _staticLoader.LoadStaticData(dir, mmsi);
        var (min, max) = ExtractMinMaxDatesFromFolder(dir);
        staticData.MinDate = min;
        staticData.MaxDate = max;
        results.Add(staticData);
    }
    return results;
}
```

Edge cases:
- If `dirs` is empty or `inputRoot` inaccessible, throw a `DirectoryNotFoundException` or return an empty list and let ViewModel show "Input root not accessible" state.

## Data Integration Strategy

- Data flow recap: `IFolderDialogService` → `ISourceDataScanner` → `IShipStaticDataLoader` → `ShipStaticData` → UI bindings.

- Service integration patterns:
  - `IShipStaticDataLoader` uses `System.Text.Json` with tolerant options (AllowTrailingCommas, CaseInsensitive) and returns a valid `ShipStaticData` object with folder fallback values when fields missing.
  - `SourceDataScanner` must compute MinDate/MaxDate from CSV filenames (YYYY-MM-DD.csv) using `DateTime.ParseExact` with `CultureInfo.InvariantCulture` and treat missing CSV files as no-range (use DateTime.Now for conservative defaults or disable Create Track until resolved).

- Relationship mapping:
  - `ShipStaticData` ↔ Folder: one-to-one; folder name used for fallback display.
  - `TimeInterval` linked to chosen `ShipStaticData.MinDate/MaxDate`.

- Error handling and edge cases:
  - Missing JSON: Log warning, set Name = null and display fallback folder name.
  - Malformed CSV filenames: Skip and log; if no valid CSV files remain, mark vessel as `HasNoData` and showCreate disabled state.
  - Input root inaccessible: ViewModel displays "Input root not accessible" and disables selection controls.

- Testing and observability:
  - Expose a `bool IsScanning` and `IProgress<double>` for UI progress feedback.
  - Provide `CancellationToken` support on scanning methods.

## Implementation Examples

- Example: `IShipStaticDataLoader.LoadStaticData` implementation (conceptual)
```csharp
public async Task<ShipStaticData> LoadStaticData(string folderPath, string mmsi)
{
    var jsonPath = Path.Combine(folderPath, mmsi + ".json");
    if (!File.Exists(jsonPath))
    {
        return new ShipStaticData { MMSI = long.Parse(mmsi), Name = null, FolderPath = folderPath };
    }

    try
    {
        var json = await File.ReadAllTextAsync(jsonPath);
        var data = JsonSerializer.Deserialize<ShipStaticData>(json, _jsonOptions);
        data.FolderPath = folderPath;
        return data;
    }
    catch (JsonException ex)
    {
        _logger.LogWarning(ex, "Malformed JSON for {MMSI}", mmsi);
        return new ShipStaticData { MMSI = long.Parse(mmsi), Name = null, FolderPath = folderPath };
    }
}
```

- Example: Extracting min/max from CSV filenames
```csharp
private (DateTime min, DateTime max) ExtractMinMaxDatesFromFolder(string folder)
{
    var files = Directory.GetFiles(folder, "*.csv");
    var dates = new List<DateTime>();
    foreach (var f in files)
    {
        var name = Path.GetFileNameWithoutExtension(f);
        if (DateTime.TryParseExact(name, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            dates.Add(d);
    }
    if (!dates.Any()) return (DateTime.MinValue, DateTime.MinValue);
    return (dates.Min(), dates.Max());
}
```

## Testing Strategy and Quality Assurance

- Test types to cover:
  - Unit tests for `ShipSelectionViewModel` (mock `ISourceDataScanner`/`IShipStaticDataLoader` and `IFolderDialogService`).
  - Integration tests for `SourceDataScanner` reading `TestData/205196000` folder with sample CSV/JSON.
  - E2E Playwright tests (if UI automation present) verifying combo population, fallback display, validation messages, and seconds precision.

- Testable design patterns:
  - All services are interfaces and injected via DI, enabling mocking.
  - `IFolderDialogService` abstracted to avoid UI dialogs in tests.
  - `ShipSelectionViewModel` exposes `AvailableVessels` and `ValidationMessage` for assertions.

- Testing hooks and selectors (Playwright selectors):
  - `data-testid="ship-combo"` for the combo box
  - `data-testid="ship-static"` for static metadata textbox
  - `data-testid="start-picker"` and `data-testid="stop-picker"` for time pickers
  - `data-testid="time-error"` for validation message

- Mock Data Requirements (centralized approach):
  - Use `src/mocks/mockData.ts` as described in `QA_testing.md` for Playwright fixtures.
  - Provide fixtures:
    - `mockShip_205196000` with JSON matching `ShipStaticData` example in `data_models.md`.
    - `mockCsv_2025-03-15.csv` with at least one valid row and timestamps to assert Min/Max behavior.

- Example Unit Test (conceptual):
```csharp
[Test]
public async Task SelectVessel_SetsDefaultsAndEnablesCreateTrack()
{
    var scanner = new Mock<ISourceDataScanner>();
    scanner.Setup(s => s.ScanInputFolder(It.IsAny<string>())).ReturnsAsync(new[] {
        new ShipStaticData { MMSI = 205196000, Name = "Sea Explorer", MinDate = new DateTime(2025,3,15), MaxDate = new DateTime(2025,3,16) }
    });

    var vm = new ShipSelectionViewModel(scanner.Object, ...);
    await vm.SelectInputFolderCommand.ExecuteAsync(null);

    Assert.That(vm.AvailableVessels.Count, Is.EqualTo(1));
    vm.SelectedVessel = vm.AvailableVessels.First();
    Assert.That(vm.TimeInterval.Start, Is.EqualTo(new DateTime(2025,3,15)));
    Assert.That(vm.TimeInterval.Stop, Is.EqualTo(new DateTime(2025,3,16).AddDays(1)));
}
```

## Mock Data Requirements

- Follow `QA_testing.md` centralized mock approach. Provide the following mock objects and helpers:
  - `mockShip_205196000` (TypeScript): maps to ShipStaticData fields, including `MMSI`, `Name`, `MinDate`, `MaxDate`, `FolderPath`.
  - `getMockShipFolder(folderPath)` helper to set up a temporary folder structure for integration tests.
  - CSV fixture `2025-03-15.csv` with header and at least one valid row used to derive `MinDate/MaxDate`.

- Test data locations:
  - Unit tests: `src/AISRouting.Tests/UnitTests/TestData/` (C# serialized fixtures or created on the fly)
  - Integration tests/E2E: `tests/fixtures/` referencing `src/mocks/mockData.ts` for Playwright usage.

## Accessibility and UX Considerations

- Provide clear labeling for combo and pickers.
- Use high-contrast error messages for validation.
- Keyboard accessible combo and pickers.

## Checklist / Design Validation

- All component examples use approved components from `overall_architecture.md` (MVVM, DI). ✓
- No deprecated components used. ✓
- Error states covered (missing input root, malformed JSON, no CSV data). ✓
- Test hooks (`data-testid`) and centralized mock data references included. ✓

---
Changelog
- Created on: 2025-11-20
- Notes: Initial design document for Feature 2.1 covering all BDD scenarios and testing requirements.
