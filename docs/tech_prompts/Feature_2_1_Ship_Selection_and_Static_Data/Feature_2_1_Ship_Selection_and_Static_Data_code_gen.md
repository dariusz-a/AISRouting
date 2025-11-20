# Working Code Generation Prompt: Feature 2.1: Ship Selection and Static Data

## Task: 
Generate working code for Feature 2.1: Ship Selection and Static Data, following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using C# and WPF.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing accessible, robust, and comprehensive tests following best practices.
- Ensuring all code aligns with project technical constraints, including MVVM patterns, data relationships, and file-based storage via service layers.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in UI/UX best practices, accessibility, and robust WPF engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.
- Ensuring all generated UI code uses semantic WPF elements and includes proper accessibility attributes (e.g., AutomationProperties.Name, AutomationProperties.AutomationId) on interactive elements so that tests can reliably select them.

## References
- BDD Scenarios: `docs/spec_scenarios/ship_selection.md`
- Test File: `tests/ship_selection.spec.ts`
- Feature Design Document: `docs/tech_design/core_features/ship_selection_design.md`
- Application Architecture: `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`
- Application Layout: `docs/tech_design/application_layout.md`
- Data Models: `docs/tech_design/data_models.md`

## Development Approach

This feature implements ship selection and static data display functionality using WPF MVVM patterns. The implementation follows a service-based architecture with clear separation between presentation (ViewModels/Views), business logic (Core services), and infrastructure (IO/Parsers).

Key principles:
- All UI logic is in ViewModels; Views are pure XAML with minimal code-behind
- Services are interface-based and injected via DI for testability
- All file I/O is async and cancellation-aware
- Error handling provides clear user feedback
- Accessibility is built-in with proper labels and automation properties
- Test hooks are integrated via AutomationProperties for E2E testing

## Implementation Plan

### Scenario: Populate ship combo box from static files or folder names

**BDD Scenario:**
```gherkin
Given the input root "input_root_example" contains vessel subfolders where folder "mmsi-1" has a static data file with Name="Sea Explorer" and folder "mmsi-2" has no static name.
When the application opens the ship selection combo box and the user selects "<input_root>".
Then the combo box lists "Sea Explorer" and "205196001" and the values are selectable.
```

**Technical Design Details:**

From `ship_selection_design.md`:
- `ISourceDataScanner.ScanInputFolder(inputRoot)` returns `IEnumerable<ShipStaticData>`
- Scanner enumerates directories using `Directory.GetDirectories(inputRoot)`
- For each folder, `IShipStaticDataLoader.LoadStaticData(folderPath, mmsi)` loads JSON
- If JSON missing or malformed, fallback to folder name (MMSI)
- `ShipSelectionViewModel` maintains `ObservableCollection<ShipStaticData> AvailableVessels`
- ComboBox binds to `AvailableVessels` with `DisplayMemberPath="Name"`
- Folder enumeration is async with progress reporting

From `data_models.md`:
```csharp
public class ShipStaticData
{
    public long MMSI { get; set; }
    public string? Name { get; set; }
    public double? Length { get; set; }
    public double? Beam { get; set; }
    public double? Draught { get; set; }
    public int? TypeCode { get; set; }
    public string? CallSign { get; set; }
    public long? IMO { get; set; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
    public string FolderPath { get; set; }
}
```

From `application_organization.md`:
- Services registered in `App.xaml.cs` using `IServiceCollection`
- `ISourceDataScanner` and `IShipStaticDataLoader` are singletons
- ViewModels are transient
- Use `CommunityToolkit.Mvvm` for `ObservableObject` and `RelayCommand`

**Tasks:**

1. Create `ISourceDataScanner` interface in `src/AISRouting.Core/Services/Interfaces/ISourceDataScanner.cs` with method `Task<IEnumerable<ShipStaticData>> ScanInputFolder(string inputRoot, CancellationToken cancellationToken = default)`

2. Create `IShipStaticDataLoader` interface in `src/AISRouting.Core/Services/Interfaces/IShipStaticDataLoader.cs` with method `Task<ShipStaticData> LoadStaticData(string folderPath, string mmsi, CancellationToken cancellationToken = default)`

3. Create `ShipStaticData` domain model in `src/AISRouting.Core/Models/ShipStaticData.cs` with all properties as specified in data_models.md, including a computed property `DisplayName` that returns `Name ?? MMSI.ToString()`

4. Implement `ShipStaticDataLoader` in `src/AISRouting.Infrastructure/IO/ShipStaticDataLoader.cs`:
   - Use `System.Text.Json` with options: `PropertyNameCaseInsensitive = true`, `AllowTrailingCommas = true`, `ReadCommentHandling = Skip`
   - If JSON file doesn't exist, return `ShipStaticData` with `MMSI` from folder name and `Name = null`
   - If JSON is malformed, log warning and return fallback object
   - Set `FolderPath` property to the provided folder path

5. Implement `SourceDataScanner` in `src/AISRouting.Infrastructure/IO/SourceDataScanner.cs`:
   - Inject `IShipStaticDataLoader` and `ILogger<SourceDataScanner>`
   - Use `Directory.GetDirectories(inputRoot)` to enumerate vessel folders
   - For each folder, extract MMSI from folder name (validate 9-digit format)
   - Call `IShipStaticDataLoader.LoadStaticData(folderPath, mmsi)`
   - Call helper method `ExtractMinMaxDatesFromFolder(folderPath)` to set `MinDate` and `MaxDate`
   - Return all valid `ShipStaticData` objects

6. Implement `ExtractMinMaxDatesFromFolder` helper method in `SourceDataScanner`:
   - Use `Directory.GetFiles(folder, "*.csv")` to list CSV files
   - Parse each filename (without extension) using `DateTime.TryParseExact` with format "yyyy-MM-dd"
   - Return tuple `(DateTime min, DateTime max)` from parsed dates
   - If no valid CSV files, return `(DateTime.MinValue, DateTime.MinValue)`

7. Create `ShipSelectionViewModel` in `src/AISRouting.App.WPF/ViewModels/ShipSelectionViewModel.cs`:
   - Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
   - Inject `ISourceDataScanner`, `IFolderDialogService`, and `ILogger<ShipSelectionViewModel>`
   - Add property `ObservableCollection<ShipStaticData> AvailableVessels`
   - Add property `ShipStaticData? SelectedVessel` with `[ObservableProperty]` attribute
   - Add command `IAsyncRelayCommand SelectInputFolderCommand`
   - Implement command to show folder dialog and call scanner

8. Implement `SelectInputFolderCommand` in `ShipSelectionViewModel`:
   - Call `IFolderDialogService.ShowFolderBrowser()` to get folder path
   - If path is null or empty, return early
   - Try-catch block: call `await _scanner.ScanInputFolder(folderPath)`
   - On success, clear and repopulate `AvailableVessels` collection
   - On exception, log error and set error message property
   - Update `InputFolderPath` property for display

9. Create `ShipSelectionView.xaml` in `src/AISRouting.App.WPF/Views/ShipSelectionView.xaml`:
   - Use `UserControl` as root element
   - Add `GroupBox` with header "Vessel Selection"
   - Add `ComboBox` with `ItemsSource="{Binding AvailableVessels}"`, `SelectedItem="{Binding SelectedVessel}"`, `DisplayMemberPath="DisplayName"`
   - Set `AutomationProperties.AutomationId="ship-combo"` and `AutomationProperties.Name="Select Vessel"`
   - Add label "Select Vessel:" with accessibility binding

10. Register services in `App.xaml.cs`:
    - Add `services.AddSingleton<ISourceDataScanner, SourceDataScanner>()`
    - Add `services.AddSingleton<IShipStaticDataLoader, ShipStaticDataLoader>()`
    - Add `services.AddTransient<ShipSelectionViewModel>()`

11. Integrate `ShipSelectionView` into `MainWindow.xaml`:
    - Add `<views:ShipSelectionView DataContext="{Binding ShipSelectionViewModel}"/>` in appropriate Grid row
    - Ensure proper row definitions and margins per application_layout.md

12. Add computed property `DisplayName` to `ShipStaticData`:
    - Return `Name` if not null or empty, otherwise return `MMSI.ToString()`
    - This provides automatic fallback display for vessels without static names

---

### Scenario: Display static data after ship selection

**BDD Scenario:**
```gherkin
Given input root "input_root_example" contains vessel folder "mmsi-1" with static attributes including Name="Sea Explorer" and MMSI="mmsi-1".
When the user selects "Sea Explorer" in the ship combo box.
Then the static attributes are displayed in the large TextBox widget including Name and MMSI.
```

**Technical Design Details:**

From `ship_selection_design.md`:
- `ShipSelectionViewModel` exposes `SelectedVessel` property
- When `SelectedVessel` changes, compute formatted string for display
- TextBox shows multi-line, read-only formatted static data
- Use fixed-width font (Consolas) for alignment
- Missing fields show "N/A"

From `application_layout.md`:
```xml
<TextBox Text="{Binding StaticDataDisplay, Mode=OneWay}"
         IsReadOnly="True" TextWrapping="Wrap"
         VerticalScrollBarVisibility="Auto"
         Height="120" FontFamily="Consolas" />
```

Display format:
```
MMSI: 205196000
Name: Sea Explorer
Length: 180.5 m
Beam: 32.2 m
Draught: 8.5 m
Available Date Range: 2024-01-01 to 2024-01-31
```

**Tasks:**

1. Add property `string StaticDataDisplay` to `ShipSelectionViewModel` with `[ObservableProperty]` attribute

2. Implement `OnSelectedVesselChanged` partial method in `ShipSelectionViewModel`:
   - Called automatically when `SelectedVessel` changes (CommunityToolkit.Mvvm pattern)
   - If `SelectedVessel` is null, set `StaticDataDisplay = string.Empty` and return
   - Build formatted multi-line string with all static data fields
   - Use format: "Field: value" or "Field: N/A" for null values
   - Format numbers with appropriate precision (e.g., Length with 1 decimal)
   - Format date range: "Available Date Range: {MinDate:yyyy-MM-dd} to {MaxDate:yyyy-MM-dd}"

3. Add `TextBox` for static data display in `ShipSelectionView.xaml`:
   - Place below the `ComboBox` in a nested `GroupBox` with header "Ship Static Data"
   - Bind `Text="{Binding StaticDataDisplay, Mode=OneWay}"`
   - Set `IsReadOnly="True"`, `TextWrapping="Wrap"`, `VerticalScrollBarVisibility="Auto"`
   - Set `Height="120"`, `FontFamily="Consolas"`
   - Set `AutomationProperties.AutomationId="ship-static"` and `AutomationProperties.Name="Ship Static Data"`

4. Create helper method `FormatStaticData` in `ShipSelectionViewModel`:
   - Accept `ShipStaticData` parameter
   - Use `StringBuilder` to build formatted string
   - Return formatted multi-line string
   - Handle all nullable fields with "N/A" fallback

5. Update `OnSelectedVesselChanged` to call `FormatStaticData`:
   - `StaticDataDisplay = FormatStaticData(SelectedVessel)`
   - Ensures automatic update when selection changes

---

### Scenario: Default start/stop time values set from file timestamps

**BDD Scenario:**
```gherkin
Given vessel folder "mmsi-1" contains CSV files with earliest timestamp "ts_first" and latest "ts_last".
When the user selects vessel "205196000".
Then the StartValue defaults to "20250315T000000" and the StopValue defaults to "20250316T000000" plus 24 hours ("20250317T000000").
```

**Technical Design Details:**

From `ship_selection_design.md`:
- When `SelectedVessel` set: Default `TimeInterval.Start = SelectedVessel.MinDate` and `TimeInterval.Stop = SelectedVessel.MaxDate.AddDays(1)`
- `TimeInterval` is a domain model in `AISRouting.Core`
- `MinDate` and `MaxDate` extracted from CSV filenames by `SourceDataScanner`

From `data_models.md`:
```csharp
public class TimeInterval
{
    public DateTime Start { get; set; }
    public DateTime Stop { get; set; }
    
    public TimeSpan Duration => Stop - Start;
    public bool IsValid => Stop > Start;
}
```

**Tasks:**

1. Create `TimeInterval` domain model in `src/AISRouting.Core/Models/TimeInterval.cs`:
   - Add properties `DateTime Start` and `DateTime Stop`
   - Add computed property `TimeSpan Duration => Stop - Start`
   - Add computed property `bool IsValid => Stop > Start`

2. Add property `TimeInterval TimeInterval` to `ShipSelectionViewModel` with `[ObservableProperty]` attribute:
   - Initialize in constructor: `TimeInterval = new TimeInterval()`

3. Update `OnSelectedVesselChanged` in `ShipSelectionViewModel` to set time defaults:
   - After null check, set `TimeInterval.Start = SelectedVessel.MinDate`
   - Set `TimeInterval.Stop = SelectedVessel.MaxDate.AddDays(1)`
   - Call `OnPropertyChanged(nameof(TimeInterval))` to notify UI

4. Add `DatePicker` controls for start date in `ShipSelectionView.xaml`:
   - Create new `GroupBox` with header "Time Interval Selection"
   - Add label "Start Time:"
   - Add `DatePicker` with `SelectedDate="{Binding TimeInterval.Start, Mode=TwoWay}"`
   - Set `AutomationProperties.AutomationId="start-picker"` and `AutomationProperties.Name="Start Date"`

5. Add `TextBox` or `TimePicker` for start time with seconds resolution:
   - Add label "Time:"
   - Add `TextBox` with `Text="{Binding TimeInterval.Start, StringFormat='HH:mm:ss', Mode=TwoWay}"`
   - Set validation for time format
   - Set `AutomationProperties.AutomationId="start-time-picker"`

6. Add `DatePicker` controls for stop date in `ShipSelectionView.xaml`:
   - Add label "Stop Time:"
   - Add `DatePicker` with `SelectedDate="{Binding TimeInterval.Stop, Mode=TwoWay}"`
   - Set `AutomationProperties.AutomationId="stop-picker"` and `AutomationProperties.Name="Stop Date"`

7. Add `TextBox` or `TimePicker` for stop time with seconds resolution:
   - Add label "Time:"
   - Add `TextBox` with `Text="{Binding TimeInterval.Stop, StringFormat='HH:mm:ss', Mode=TwoWay}"`
   - Set validation for time format
   - Set `AutomationProperties.AutomationId="stop-time-picker"`

8. Create value converter `DateTimeToStringConverter` in `src/AISRouting.App.WPF/Converters/DateTimeToStringConverter.cs`:
   - Implement `IValueConverter` interface
   - Convert: format DateTime to "HH:mm:ss" string
   - ConvertBack: parse string to DateTime (preserve date component)

9. Update `ShipSelectionView.xaml` to use converter for time pickers:
   - Add converter resource in UserControl resources
   - Bind TextBox using converter for proper formatting

---

### Scenario: Show fallback when static name missing

**BDD Scenario:**
```gherkin
Given a vessel folder "mmsi-2" lacks a static name in its static file and the application lists vessels from "input_root_example".
When the ship combo is shown.
Then the folder name "205196001" is used as the displayed ship name in the combo.
```

**Technical Design Details:**

From `ship_selection_design.md`:
- If `SelectedVessel.Name` is null or empty → show `SelectedVessel.FolderName` as display
- Fallback behavior implemented via `DisplayName` computed property
- `ComboBox` uses `DisplayMemberPath="DisplayName"`

**Tasks:**

1. Ensure `DisplayName` property in `ShipStaticData` returns MMSI string when Name is null:
   - Already implemented in earlier task as `return Name ?? MMSI.ToString()`

2. Update `ShipStaticDataLoader.LoadStaticData` to handle missing Name field:
   - If JSON doesn't contain "Name" field or it's null, leave `Name` property as null
   - Ensure MMSI is always set from folder name
   - Log info message when fallback is used

3. Add unit test `ShipStaticData_DisplayName_UsesFallbackWhenNameNull` in `src/AISRouting.Tests/UnitTests/Core/ShipStaticDataTests.cs`:
   - Create `ShipStaticData` with `MMSI = 205196001` and `Name = null`
   - Assert `DisplayName == "205196001"`

4. Add integration test `LoadStaticData_MissingJson_ReturnsFallbackName` in `src/AISRouting.Tests/IntegrationTests/ShipStaticDataLoaderTests.cs`:
   - Create test folder with MMSI subfolder but no JSON file
   - Call `LoadStaticData`
   - Assert returned object has `Name = null` and `DisplayName` equals MMSI string

---

### Scenario: Validate Min/Max date range before creation

**BDD Scenario:**
```gherkin
Given vessel "mmsi-1" has CSV files with inconsistent timestamps causing Min > Max.
When the user inspects the Min/Max pickers.
Then a validation warning with text "Invalid time range" is displayed and the Create Track button is disabled until corrected.
```

**Technical Design Details:**

From `ship_selection_design.md`:
- Use `TimeInterval.IsValid` property (returns `Stop > Start`)
- If false, set `ValidationMessage = "Invalid time range"`
- Disable `CreateTrackCommand` based on validation

**Tasks:**

1. Add property `string? ValidationMessage` to `ShipSelectionViewModel` with `[ObservableProperty]` attribute

2. Create method `ValidateTimeInterval` in `ShipSelectionViewModel`:
   - Check `if (!TimeInterval.IsValid)`
   - If invalid: set `ValidationMessage = "Invalid time range"`
   - If valid: set `ValidationMessage = null`
   - Call `CreateTrackCommand.NotifyCanExecuteChanged()` if command exists

3. Update `OnSelectedVesselChanged` to call `ValidateTimeInterval()` after setting defaults

4. Add property change handler for `TimeInterval.Start` and `TimeInterval.Stop`:
   - Use `partial void OnTimeIntervalChanged()` method
   - Call `ValidateTimeInterval()` when time interval properties change

5. Add `TextBlock` for validation message in `ShipSelectionView.xaml`:
   - Place below time pickers
   - Bind `Text="{Binding ValidationMessage}"`
   - Set `Foreground="Red"` and `FontWeight="Bold"`
   - Use `Visibility` converter to show only when message is not null
   - Set `AutomationProperties.AutomationId="time-error"` and `AutomationProperties.Name="Time Range Validation Error"`

6. Create `CreateTrackCommand` placeholder in `ShipSelectionViewModel`:
   - Add `[RelayCommand(CanExecute = nameof(CanCreateTrack))]` attribute to method `CreateTrack`
   - Implement `CanCreateTrack` method returning `SelectedVessel != null && TimeInterval.IsValid`

7. Add `Button` for "Create Track" in `ShipSelectionView.xaml`:
   - Place below time interval section
   - Bind `Command="{Binding CreateTrackCommand}"`
   - Button will auto-disable when `CanCreateTrack` returns false

8. Create value converter `BoolToVisibilityConverter` in `src/AISRouting.App.WPF/Converters/BoolToVisibilityConverter.cs`:
   - Implement `IValueConverter`
   - Convert false/null to Collapsed, true to Visible
   - Use for ValidationMessage visibility

9. Add unit test `ValidateTimeInterval_InvalidRange_SetsErrorMessage`:
   - Create ViewModel
   - Set `TimeInterval.Start > TimeInterval.Stop`
   - Call `ValidateTimeInterval()`
   - Assert `ValidationMessage == "Invalid time range"`

---

### Scenario: Use seconds resolution for time pickers

**BDD Scenario:**
```gherkin
Given vessel "mmsi-1" is selected and the UI shows start/stop time pickers.
When the user opens the start time picker and sets seconds to "00".
Then the picker accepts seconds resolution and the selected timestamp shows seconds precision.
```

**Technical Design Details:**

From `application_layout.md`:
- Time format: `HH:mm:ss` (24-hour with seconds)
- Use `TextBox` with validation or custom TimePicker
- `StringFormat='HH:mm:ss'` in binding

**Tasks:**

1. Update start time `TextBox` binding in `ShipSelectionView.xaml`:
   - Change binding to use `UpdateSourceTrigger=PropertyChanged` for immediate validation
   - Add input validation using `ValidationRules`

2. Create validation rule `TimeFormatValidationRule` in `src/AISRouting.App.WPF/Validation/TimeFormatValidationRule.cs`:
   - Inherit from `ValidationRule`
   - Override `Validate` method
   - Parse input using `DateTime.TryParseExact` with format "HH:mm:ss"
   - Return `ValidationResult` with error if parsing fails

3. Apply `TimeFormatValidationRule` to time TextBox bindings:
   - Add validation rule to binding in XAML
   - Show validation error template when invalid

4. Add property `string StartTimeString` to `ShipSelectionViewModel`:
   - Format `TimeInterval.Start` as "HH:mm:ss"
   - On set, parse and update `TimeInterval.Start` preserving date
   - Validate format before updating

5. Update start time `TextBox` to bind to `StartTimeString`:
   - Use two-way binding with validation
   - Show error template on validation failure

6. Repeat steps 4-5 for `StopTimeString` property and stop time TextBox

7. Add unit test `TimeInterval_SetSeconds_PreservesSecondsResolution`:
   - Set `TimeInterval.Start` with specific seconds value
   - Format using "HH:mm:ss" and verify seconds are preserved
   - Parse back and assert seconds match

8. Create custom `TimeTextBox` user control (optional enhancement):
   - Wrap TextBox with built-in format validation
   - Provide consistent time input experience
   - Include up/down buttons for incrementing time components

---

### Scenario: Ship selection unavailable when input root missing

**BDD Scenario:**
```gherkin
Given the input root path specified by "empty_root_example" is not accessible or does not exist.
When the user opens the ship selection combo box.
Then the combo box shows an error state with text "Input root not accessible" and selection is disabled.
```

**Technical Design Details:**

From `ship_selection_design.md`:
- If `inputRoot` inaccessible, throw `DirectoryNotFoundException` or return empty list
- ViewModel displays "Input root not accessible" and disables selection controls
- Use `IsEnabled` binding on ComboBox based on data availability

**Tasks:**

1. Add property `bool IsInputRootValid` to `ShipSelectionViewModel` with `[ObservableProperty]` attribute:
   - Initialize to `false`
   - Set to `true` when folder scanning succeeds
   - Set to `false` on folder access errors

2. Update `SelectInputFolderCommand` implementation error handling:
   - Wrap `ScanInputFolder` call in try-catch
   - Catch `DirectoryNotFoundException` and `UnauthorizedAccessException`
   - On exception: set `IsInputRootValid = false`, clear `AvailableVessels`, set error message
   - On success: set `IsInputRootValid = true`

3. Add property `string? ErrorMessage` to `ShipSelectionViewModel` with `[ObservableProperty]` attribute:
   - Set to "Input root not accessible" on folder access errors
   - Set to null on successful operations

4. Update `ComboBox` in `ShipSelectionView.xaml`:
   - Add binding `IsEnabled="{Binding IsInputRootValid}"`
   - ComboBox will be disabled when input root is invalid

5. Add `TextBlock` for error message display in `ShipSelectionView.xaml`:
   - Place above ComboBox
   - Bind `Text="{Binding ErrorMessage}"`
   - Set `Foreground="Red"` and visibility based on message presence
   - Set `AutomationProperties.AutomationId="input-error"` and `AutomationProperties.Name="Input Root Error"`

6. Update `SourceDataScanner.ScanInputFolder` to throw appropriate exceptions:
   - Check if directory exists using `Directory.Exists(inputRoot)`
   - If not exists, throw `DirectoryNotFoundException` with message "Input root not accessible"
   - Catch and rethrow filesystem exceptions with context

7. Add unit test `SelectInputFolder_NonExistentPath_SetsErrorState`:
   - Mock `ISourceDataScanner` to throw `DirectoryNotFoundException`
   - Execute `SelectInputFolderCommand`
   - Assert `IsInputRootValid == false`
   - Assert `ErrorMessage == "Input root not accessible"`
   - Assert `AvailableVessels` is empty

8. Add integration test `ScanInputFolder_InvalidPath_ThrowsException`:
   - Call `ScanInputFolder` with non-existent path
   - Assert exception is thrown
   - Verify exception message contains helpful context

9. Enhance error messages with specific details:
   - Include the invalid path in error message for user clarity
   - Log full exception details for diagnostic purposes
   - Show user-friendly message in UI

---

## Code Examples

### Example: ISourceDataScanner Implementation

```csharp
// src/AISRouting.Infrastructure/IO/SourceDataScanner.cs
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AISRouting.Infrastructure.IO
{
    public class SourceDataScanner : ISourceDataScanner
    {
        private readonly IShipStaticDataLoader _staticLoader;
        private readonly ILogger<SourceDataScanner> _logger;

        public SourceDataScanner(IShipStaticDataLoader staticLoader, ILogger<SourceDataScanner> logger)
        {
            _staticLoader = staticLoader ?? throw new ArgumentNullException(nameof(staticLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ShipStaticData>> ScanInputFolder(
            string inputRoot,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(inputRoot))
            {
                _logger.LogError("Input root not accessible: {InputRoot}", inputRoot);
                throw new DirectoryNotFoundException($"Input root not accessible: {inputRoot}");
            }

            _logger.LogInformation("Scanning input folder: {InputRoot}", inputRoot);
            
            var vessels = new List<ShipStaticData>();
            var directories = Directory.GetDirectories(inputRoot);

            foreach (var directory in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var folderName = Path.GetFileName(directory);
                
                // Validate MMSI format (9-digit number)
                if (!long.TryParse(folderName, out var mmsi) || folderName.Length != 9)
                {
                    _logger.LogWarning("Skipping folder with invalid MMSI format: {FolderName}", folderName);
                    continue;
                }

                try
                {
                    var staticData = await _staticLoader.LoadStaticData(directory, folderName, cancellationToken);
                    var (minDate, maxDate) = ExtractMinMaxDatesFromFolder(directory);
                    
                    staticData.MinDate = minDate;
                    staticData.MaxDate = maxDate;
                    
                    vessels.Add(staticData);
                    _logger.LogInformation("Loaded vessel: {MMSI} ({Name})", staticData.MMSI, staticData.DisplayName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading vessel data from folder: {Folder}", directory);
                }
            }

            _logger.LogInformation("Scan complete. Found {Count} vessels", vessels.Count);
            return vessels;
        }

        private (DateTime min, DateTime max) ExtractMinMaxDatesFromFolder(string folderPath)
        {
            var csvFiles = Directory.GetFiles(folderPath, "*.csv");
            var dates = new List<DateTime>();

            foreach (var file in csvFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (DateTime.TryParseExact(
                    fileName,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                {
                    dates.Add(date);
                }
                else
                {
                    _logger.LogWarning("CSV file with invalid date format: {FileName}", fileName);
                }
            }

            if (!dates.Any())
            {
                _logger.LogWarning("No valid CSV files found in folder: {Folder}", folderPath);
                return (DateTime.MinValue, DateTime.MinValue);
            }

            return (dates.Min(), dates.Max());
        }
    }
}
```

### Example: ShipSelectionViewModel Core Logic

```csharp
// src/AISRouting.App.WPF/ViewModels/ShipSelectionViewModel.cs
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AISRouting.App.WPF.ViewModels
{
    public partial class ShipSelectionViewModel : ObservableObject
    {
        private readonly ISourceDataScanner _scanner;
        private readonly IFolderDialogService _folderDialog;
        private readonly ILogger<ShipSelectionViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<ShipStaticData> _availableVessels;

        [ObservableProperty]
        private ShipStaticData? _selectedVessel;

        [ObservableProperty]
        private TimeInterval _timeInterval;

        [ObservableProperty]
        private string _staticDataDisplay;

        [ObservableProperty]
        private string? _validationMessage;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isInputRootValid;

        [ObservableProperty]
        private string _inputFolderPath;

        public ShipSelectionViewModel(
            ISourceDataScanner scanner,
            IFolderDialogService folderDialog,
            ILogger<ShipSelectionViewModel> logger)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _folderDialog = folderDialog ?? throw new ArgumentNullException(nameof(folderDialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _availableVessels = new ObservableCollection<ShipStaticData>();
            _timeInterval = new TimeInterval();
            _staticDataDisplay = string.Empty;
            _inputFolderPath = string.Empty;
            _isInputRootValid = false;
        }

        [RelayCommand]
        private async Task SelectInputFolder()
        {
            var folderPath = _folderDialog.ShowFolderBrowser();
            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                _logger.LogInformation("Selected input folder: {FolderPath}", folderPath);
                
                var vessels = await _scanner.ScanInputFolder(folderPath);
                
                AvailableVessels.Clear();
                foreach (var vessel in vessels)
                {
                    AvailableVessels.Add(vessel);
                }

                InputFolderPath = folderPath;
                IsInputRootValid = true;
                ErrorMessage = null;
                
                _logger.LogInformation("Loaded {Count} vessels", AvailableVessels.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Input root not accessible");
                IsInputRootValid = false;
                ErrorMessage = "Input root not accessible";
                AvailableVessels.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning input folder");
                IsInputRootValid = false;
                ErrorMessage = $"Error scanning folder: {ex.Message}";
                AvailableVessels.Clear();
            }
        }

        partial void OnSelectedVesselChanged(ShipStaticData? value)
        {
            if (value == null)
            {
                StaticDataDisplay = string.Empty;
                return;
            }

            StaticDataDisplay = FormatStaticData(value);
            
            TimeInterval.Start = value.MinDate;
            TimeInterval.Stop = value.MaxDate.AddDays(1);
            
            ValidateTimeInterval();
            
            _logger.LogInformation("Selected vessel: {MMSI} ({Name})", value.MMSI, value.DisplayName);
        }

        private string FormatStaticData(ShipStaticData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MMSI: {data.MMSI}");
            sb.AppendLine($"Name: {data.Name ?? "N/A"}");
            sb.AppendLine($"Length: {(data.Length.HasValue ? $"{data.Length.Value:F1} m" : "N/A")}");
            sb.AppendLine($"Beam: {(data.Beam.HasValue ? $"{data.Beam.Value:F1} m" : "N/A")}");
            sb.AppendLine($"Draught: {(data.Draught.HasValue ? $"{data.Draught.Value:F1} m" : "N/A")}");
            
            if (data.MinDate != DateTime.MinValue && data.MaxDate != DateTime.MinValue)
            {
                sb.AppendLine($"Available Date Range: {data.MinDate:yyyy-MM-dd} to {data.MaxDate:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine("Available Date Range: N/A");
            }
            
            return sb.ToString();
        }

        private void ValidateTimeInterval()
        {
            if (!TimeInterval.IsValid)
            {
                ValidationMessage = "Invalid time range";
            }
            else
            {
                ValidationMessage = null;
            }
            
            CreateTrackCommand?.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanCreateTrack))]
        private void CreateTrack()
        {
            // Placeholder for future implementation
            _logger.LogInformation("Create Track command executed");
        }

        private bool CanCreateTrack()
        {
            return SelectedVessel != null && TimeInterval.IsValid;
        }
    }
}
```

### Example: ShipSelectionView.xaml

```xml
<!-- src/AISRouting.App.WPF/Views/ShipSelectionView.xaml -->
<UserControl x:Class="AISRouting.App.WPF.Views.ShipSelectionView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:converters="clr-namespace:AISRouting.App.WPF.Converters">
    
    <UserControl.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
        <converters:DateTimeToStringConverter x:Key="DateTimeToStringConverter"/>
    </UserControl.Resources>

    <StackPanel Margin="10">
        
        <!-- Error Message -->
        <TextBlock Text="{Binding ErrorMessage}"
                   Foreground="Red"
                   FontWeight="Bold"
                   Margin="0,0,0,10"
                   Visibility="{Binding ErrorMessage, Converter={StaticResource BoolToVisibilityConverter}}"
                   AutomationProperties.AutomationId="input-error"
                   AutomationProperties.Name="Input Root Error"/>

        <!-- Vessel Selection -->
        <GroupBox Header="Vessel Selection" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Vessel ComboBox -->
                <Grid Grid.Row="0" Margin="0,0,0,10">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <TextBlock Grid.Column="0" 
                               Text="Select Vessel:" 
                               VerticalAlignment="Center" 
                               Margin="0,0,10,0"/>
                    
                    <ComboBox Grid.Column="1"
                              ItemsSource="{Binding AvailableVessels}"
                              SelectedItem="{Binding SelectedVessel}"
                              DisplayMemberPath="DisplayName"
                              IsEnabled="{Binding IsInputRootValid}"
                              AutomationProperties.AutomationId="ship-combo"
                              AutomationProperties.Name="Select Vessel"/>
                </Grid>

                <!-- Static Data Display -->
                <GroupBox Grid.Row="1" Header="Ship Static Data">
                    <TextBox Text="{Binding StaticDataDisplay, Mode=OneWay}"
                             IsReadOnly="True"
                             TextWrapping="Wrap"
                             VerticalScrollBarVisibility="Auto"
                             Height="120"
                             FontFamily="Consolas"
                             AutomationProperties.AutomationId="ship-static"
                             AutomationProperties.Name="Ship Static Data"/>
                </GroupBox>
            </Grid>
        </GroupBox>

        <!-- Time Interval Selection -->
        <GroupBox Header="Time Interval Selection" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Start Time -->
                <TextBlock Grid.Row="0" Grid.Column="0" 
                           Text="Start Time:" 
                           VerticalAlignment="Center" 
                           Margin="0,0,10,0"/>
                <DatePicker Grid.Row="0" Grid.Column="1" 
                            SelectedDate="{Binding TimeInterval.Start, Mode=TwoWay}"
                            Margin="0,0,10,0"
                            AutomationProperties.AutomationId="start-picker"
                            AutomationProperties.Name="Start Date"/>
                <TextBlock Grid.Row="0" Grid.Column="2" 
                           Text="Time:" 
                           VerticalAlignment="Center" 
                           Margin="0,0,10,0"/>
                <TextBox Grid.Row="0" Grid.Column="3"
                         Text="{Binding TimeInterval.Start, StringFormat='HH:mm:ss', Mode=TwoWay}"
                         AutomationProperties.AutomationId="start-time-picker"
                         AutomationProperties.Name="Start Time"/>

                <!-- Stop Time -->
                <TextBlock Grid.Row="1" Grid.Column="0" 
                           Text="Stop Time:" 
                           VerticalAlignment="Center" 
                           Margin="0,10,10,0"/>
                <DatePicker Grid.Row="1" Grid.Column="1" 
                            SelectedDate="{Binding TimeInterval.Stop, Mode=TwoWay}"
                            Margin="0,10,10,0"
                            AutomationProperties.AutomationId="stop-picker"
                            AutomationProperties.Name="Stop Date"/>
                <TextBlock Grid.Row="1" Grid.Column="2" 
                           Text="Time:" 
                           VerticalAlignment="Center" 
                           Margin="0,10,10,0"/>
                <TextBox Grid.Row="1" Grid.Column="3"
                         Text="{Binding TimeInterval.Stop, StringFormat='HH:mm:ss', Mode=TwoWay}"
                         Margin="0,10,0,0"
                         AutomationProperties.AutomationId="stop-time-picker"
                         AutomationProperties.Name="Stop Time"/>

                <!-- Validation Message -->
                <TextBlock Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="4"
                           Text="{Binding ValidationMessage}"
                           Foreground="Red"
                           FontWeight="Bold"
                           Margin="0,10,0,0"
                           Visibility="{Binding ValidationMessage, Converter={StaticResource BoolToVisibilityConverter}}"
                           AutomationProperties.AutomationId="time-error"
                           AutomationProperties.Name="Time Range Validation Error"/>

                <!-- Create Track Button -->
                <Button Grid.Row="3" Grid.Column="3"
                        Content="Create Track"
                        HorizontalAlignment="Right"
                        Width="150"
                        Height="35"
                        Margin="0,15,0,0"
                        Command="{Binding CreateTrackCommand}"
                        AutomationProperties.AutomationId="create-track-button"
                        AutomationProperties.Name="Create Track"/>
            </Grid>
        </GroupBox>

    </StackPanel>
</UserControl>
```

## Success Criteria
- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.
- All BDD scenarios from `ship_selection.md` are fully covered by implementation.
- All UI elements have proper accessibility attributes for testing.
- Error handling provides clear user feedback for all edge cases.
- Time pickers support seconds resolution.
- Validation prevents invalid time ranges from proceeding to track creation.
- Fallback display names work correctly when static data is missing.
- Services are properly registered in DI container.
- All file I/O operations are async and support cancellation.
- Code follows MVVM patterns with clear separation of concerns.

## Technical Requirements

### Architecture Requirements
- Follow MVVM pattern with clear separation: Views (XAML), ViewModels (logic), Services (I/O)
- Use dependency injection for all services
- All interfaces defined in Core, implementations in Infrastructure
- ViewModels use `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`, `ObservableProperty`)

### Data Access Requirements
- Use `System.Text.Json` for JSON parsing with options: `PropertyNameCaseInsensitive`, `AllowTrailingCommas`, `ReadCommentHandling.Skip`
- All file I/O operations must be async with `CancellationToken` support
- Handle missing files gracefully with fallback values
- Log all file access operations with context (folder path, MMSI)

### UI Requirements
- All interactive elements must have `AutomationProperties.AutomationId` and `AutomationProperties.Name` for testing
- Use semantic labels and accessible controls
- Time pickers must support seconds resolution (HH:mm:ss format)
- Validation errors displayed in red with clear messages
- ComboBox disabled when input root is invalid

### Error Handling Requirements
- Catch `DirectoryNotFoundException` and `UnauthorizedAccessException` on folder access
- Log all exceptions with full context
- Display user-friendly error messages in UI
- Never crash on malformed data; use fallbacks
- Validate MMSI format (9-digit number) and skip invalid folders

### Testing Requirements
- Unit tests for ViewModels with mocked services
- Integration tests for Services with real file I/O using TestData folder
- All tests use NUnit framework
- Mock services using interfaces for isolation
- Test data includes valid and invalid scenarios

### Logging Requirements
- Use `ILogger<T>` for all logging
- Log levels: Info (folder selection, vessel count), Warning (malformed data), Error (exceptions)
- Include context: MMSI, folder paths, file names
- Log successful operations for diagnostics

### Performance Requirements
- Use async/await for all I/O operations to keep UI responsive
- Support cancellation for long-running operations
- Don't block UI thread during folder scanning
- Stream large files when possible (future enhancement)
