# Working Code Generation Prompt: Feature 1.1 Getting Started

## Task: 
Generate working code for Feature 1.1: Getting Started, following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using C# and WPF.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying MVVM architecture patterns with proper separation of concerns.
- Writing accessible, robust, and comprehensive tests following best practices.
- Ensuring all code aligns with project technical constraints, including layered architecture, dependency injection, and service-based design.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in WPF UI/UX best practices, accessibility, and robust desktop application engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.
- Ensuring all generated UI code uses semantic XAML elements and includes proper accessibility attributes (e.g., AutomationProperties.AutomationId, labels) on interactive elements so that tests can reliably select them with automation-first selectors.

## References
- BDD Scenarios: docs/spec_scenarios/getting_started.md
- Test File: tests/getting_started.spec.ts
- Feature Design Document: docs/tech_design/core_features/getting_started_design.md
- Application Architecture: docs/tech_design/overall_architecture.md
- Application Organization: docs/tech_design/application_organization.md
- Application Layout: docs/tech_design/application_layout.md
- Data Models: docs/tech_design/data_models.md

## Development Approach

This feature establishes the foundation for the AISRouting WPF desktop application, implementing the initial setup flow: application installation, startup, input folder selection, and vessel discovery.

**Architecture Patterns:**
- MVVM (Model-View-ViewModel) with CommunityToolkit.Mvvm
- Service Layer Pattern with dependency injection
- Async/await for all I/O operations
- Fail-fast validation with user-friendly error messages

**Key Services:**
- `ISourceDataScanner`: Scans input folder to discover MMSI vessel subfolders
- `IShipStaticDataLoader`: Loads and parses vessel static data from JSON files
- `IFolderDialogService`: Abstracts folder selection dialog for testability

**Data Flow:**
1. User clicks "Browse" → `IFolderDialogService` shows dialog
2. Selected path → `ISourceDataScanner.ScanInputFolderAsync(path)`
3. Scanner enumerates MMSI subfolders, loads static data, extracts date ranges
4. Results bound to UI via `ObservableCollection<ShipStaticData>`
5. Validation messages displayed for empty or invalid folders

## Implementation Plan

### Scenario 1: Install and start AISRouting UI

**BDD Scenario:**
```gherkin
Given the AISRouting distribution is unpacked at "<install_path>".
When the user executes the desktop application start action (double-click or run executable) from "<install_path>".
Then the application launches and the main screen is visible with the top-level navigation and the Input Folder selector control present.
```

**Technical Design Details:**

From the Feature Design Document:
- MainWindow (View) with MainViewModel
- Application entry point in App.xaml.cs with DI container setup
- Main UI presents Input Folder selector button with data-testid="select-input-folder"
- UI follows the layout structure from application_layout.md

File structure:
- `src/AISRouting.App.WPF/App.xaml` - Application entry point
- `src/AISRouting.App.WPF/App.xaml.cs` - DI container configuration
- `src/AISRouting.App.WPF/MainWindow.xaml` - Main window UI
- `src/AISRouting.App.WPF/MainWindow.xaml.cs` - Code-behind (minimal)
- `src/AISRouting.App.WPF/ViewModels/MainViewModel.cs` - Main orchestration ViewModel

**Tasks:**

1. Create the Visual Studio solution structure at `src/AISRouting.sln` with projects:
   - `AISRouting.App.WPF` (WPF Application, net8.0-windows)
   - `AISRouting.Core` (Class Library, net8.0)
   - `AISRouting.Infrastructure` (Class Library, net8.0)
   - `AISRouting.Tests` (NUnit Test Project, net8.0)

2. Add NuGet package references to `AISRouting.App.WPF.csproj`:
   - `CommunityToolkit.Mvvm` (latest stable)
   - `Microsoft.Extensions.DependencyInjection` (8.x)
   - `Microsoft.Extensions.Configuration` (8.x)
   - `Microsoft.Extensions.Logging` (8.x)
   - `Ookii.Dialogs.Wpf` (latest stable)

3. Add NuGet package references to `AISRouting.Infrastructure.csproj`:
   - `CsvHelper` (latest stable)
   - `System.Text.Json` (8.x, built-in)

4. Add NuGet package references to `AISRouting.Tests.csproj`:
   - `NUnit` (latest stable)
   - `NUnit3TestAdapter` (latest stable)
   - `Microsoft.NET.Test.Sdk` (latest stable)
   - `Moq` or `NSubstitute` (latest stable)
   - `FluentAssertions` (latest stable)

5. Configure project dependencies:
   - `AISRouting.App.WPF` references `AISRouting.Core` and `AISRouting.Infrastructure`
   - `AISRouting.Infrastructure` references `AISRouting.Core`
   - `AISRouting.Tests` references all three projects

6. Create `src/AISRouting.App.WPF/App.xaml` with:
   - Application resource dictionary setup
   - StartupUri pointing to MainWindow

7. Create `src/AISRouting.App.WPF/App.xaml.cs` with:
   - `OnStartup` method that configures `IServiceCollection`
   - Register all services (ISourceDataScanner, IShipStaticDataLoader, IFolderDialogService, etc.)
   - Register ViewModels as transient (MainViewModel, ShipSelectionViewModel)
   - Build `ServiceProvider` and resolve `MainWindow`
   - Show MainWindow

8. Create `src/AISRouting.App.WPF/MainWindow.xaml` with:
   - Window properties: Title="AISRouting", MinHeight="700", MinWidth="1000"
   - Grid layout with row definitions for Input Configuration, Vessel Selection, Time Interval, Track Results, Status Bar
   - GroupBox for "Input Configuration" section
   - TextBox for input folder path (IsReadOnly="True")
   - Button "Browse..." with data-testid="select-input-folder" bound to SelectInputFolderCommand

9. Create `src/AISRouting.App.WPF/MainWindow.xaml.cs` with minimal code-behind:
   - Constructor that accepts MainViewModel via DI
   - Set DataContext to injected ViewModel

10. Create `src/AISRouting.App.WPF/ViewModels/MainViewModel.cs` with:
    - Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
    - `[ObservableProperty]` for `InputFolderPath` (string)
    - `SelectInputFolderCommand` as `IAsyncRelayCommand` (to be implemented in next scenarios)
    - Constructor accepting required services via DI (ISourceDataScanner, IFolderDialogService)
    - Empty command implementation for now (will be filled in next scenario tasks)

11. Build and run the application to verify:
    - Application launches without errors
    - MainWindow is displayed
    - Input Configuration section with TextBox and Browse button is visible
    - No exceptions in DI container setup

12. Add test fixture for application startup in `src/AISRouting.Tests/IntegrationTests/ApplicationStartupTests.cs`:
    - Test that application can instantiate MainWindow via DI
    - Test that MainViewModel is correctly resolved with all dependencies

### Scenario 2: Select input data root with vessel subfolders

**BDD Scenario:**
```gherkin
Given the file system path "C:\\data\\ais_root" contains vessel subfolders each with CSV files and the application is running and shows the Input Folder selector.
When the user opens the Input Folder selector and chooses "C:\\data\\ais_root".
Then the ship selection combo box lists vessel subfolder names and the first vessel is selectable.
```

**Technical Design Details:**

From the Feature Design Document:
- `IFolderDialogService` abstracts Ookii.Dialogs.Wpf folder browser
- `ISourceDataScanner` scans input folder and enumerates MMSI subfolders
- `IShipStaticDataLoader` loads static JSON for each vessel
- `ShipSelectionViewModel` manages available vessels and selection state
- ShipSelectionView displays vessel ComboBox with data-testid="ship-combo"

Services and their responsibilities:
- `FolderDialogService`: Concrete implementation showing Ookii folder browser dialog
- `SourceDataScanner`: Enumerates directories, calls IShipStaticDataLoader, computes MinDate/MaxDate from CSV filenames
- `ShipStaticDataLoader`: Parses `<MMSI>.json` into `ShipStaticData` model

Data Model:
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

**Tasks:**

1. Create `src/AISRouting.Core/Models/ShipStaticData.cs` with:
   - All properties from data model above
   - Computed property `DisplayName` that returns `Name ?? $"Vessel {MMSI}"`
   - Constructor accepting MMSI and FolderPath

2. Create `src/AISRouting.Core/Services/Interfaces/IFolderDialogService.cs` with:
   - Method signature: `string? ShowFolderBrowser(string? initialDirectory = null)`
   - Returns selected folder path or null if cancelled

3. Create `src/AISRouting.App.WPF/Services/FolderDialogService.cs` implementing `IFolderDialogService`:
   - Use `Ookii.Dialogs.Wpf.VistaFolderBrowserDialog`
   - Set `Description` = "Select AIS data input folder"
   - Return `dialog.SelectedPath` or null if `ShowDialog() != true`

4. Create `src/AISRouting.Core/Services/Interfaces/IShipStaticDataLoader.cs` with:
   - Method signature: `Task<ShipStaticData?> LoadStaticDataAsync(string folderPath, string mmsi, CancellationToken cancellationToken = default)`
   - Returns ShipStaticData or null if JSON file missing or malformed

5. Create `src/AISRouting.Infrastructure/Parsers/ShipStaticDataParser.cs` implementing `IShipStaticDataLoader`:
   - Inject `ILogger<ShipStaticDataParser>`
   - In `LoadStaticDataAsync`: construct path to `<mmsi>.json`
   - Use `System.Text.Json.JsonSerializer.DeserializeAsync` with case-insensitive options
   - Wrap in try/catch: log warning and return null on any exception
   - Set `FolderPath` property from input parameter
   - Return parsed `ShipStaticData` object

6. Create `src/AISRouting.Core/Services/Interfaces/ISourceDataScanner.cs` with:
   - Method signature: `Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(string inputFolder, CancellationToken cancellationToken = default)`
   - Returns list of discovered vessels with static data and date ranges

7. Create `src/AISRouting.Infrastructure/IO/SourceDataScanner.cs` implementing `ISourceDataScanner`:
   - Inject `IShipStaticDataLoader` and `ILogger<SourceDataScanner>`
   - In `ScanInputFolderAsync`:
     - Validate input folder exists with `Directory.Exists`, throw `DirectoryNotFoundException` if not
     - Use `Directory.EnumerateDirectories(inputFolder)` to iterate subfolders
     - For each subfolder: extract MMSI from `Path.GetFileName(dir)`
     - Call `_staticLoader.LoadStaticDataAsync(dir, mmsi, cancellationToken)`
     - If static data is null, create fallback `ShipStaticData` with MMSI and DisplayName from folder
     - Call helper method `ExtractMinMaxDatesFromCsvFiles(dir)` to set MinDate and MaxDate
     - Wrap per-folder operations in try/catch to skip invalid folders and continue
     - Return list of all successfully scanned vessels

8. Create private helper method in `SourceDataScanner`: `ExtractMinMaxDatesFromCsvFiles(string folderPath)`:
   - Use `Directory.EnumerateFiles(folderPath, "*.csv")`
   - Parse filenames matching pattern `YYYY-MM-DD.csv` using `DateTime.TryParseExact`
   - Return tuple `(DateTime minDate, DateTime maxDate)` or default dates if no valid CSV files found
   - Log warning if no CSV files with valid date format are found

9. Update `MainViewModel.cs`:
   - Add `[ObservableProperty]` for `ObservableCollection<ShipStaticData> AvailableVessels` initialized as empty collection
   - Add `[ObservableProperty]` for `ShipStaticData? SelectedVessel`
   - Add `[ObservableProperty]` for `bool IsScanning`
   - Implement `SelectInputFolderCommand` as `AsyncRelayCommand`:
     - Call `_folderDialogService.ShowFolderBrowser()`
     - If path is null/empty, return early
     - Set `IsScanning = true` and update `InputFolderPath` property
     - Call `await _scanner.ScanInputFolderAsync(path)`
     - Clear `AvailableVessels` and populate with scan results
     - Set `IsScanning = false` in finally block
     - Wrap in try/catch to handle `DirectoryNotFoundException` and display error

10. Update `MainWindow.xaml` to add Vessel Selection section:
    - Add GroupBox with Header="Vessel Selection"
    - Add Label "Select Vessel:"
    - Add ComboBox with data-testid="ship-combo" bound to:
      - `ItemsSource="{Binding AvailableVessels}"`
      - `SelectedItem="{Binding SelectedVessel}"`
      - `DisplayMemberPath="DisplayName"`
    - Add ProgressBar or TextBlock showing "Scanning..." when `IsScanning` is true

11. Update `App.xaml.cs` DI registration:
    - Register `IFolderDialogService` as singleton with `FolderDialogService` implementation
    - Register `IShipStaticDataLoader` as singleton with `ShipStaticDataParser` implementation
    - Register `ISourceDataScanner` as singleton with `SourceDataScanner` implementation

12. Create unit tests in `src/AISRouting.Tests/UnitTests/Infrastructure/SourceDataScannerTests.cs`:
    - Test scanning folder with valid MMSI subfolders returns expected vessels
    - Test scanning folder with missing static JSON uses folder name fallback
    - Test scanning folder with no CSV files sets default date range
    - Mock `IShipStaticDataLoader` for isolated testing

13. Create unit tests in `src/AISRouting.Tests/UnitTests/Infrastructure/ShipStaticDataParserTests.cs`:
    - Test parsing valid JSON returns correct ShipStaticData
    - Test parsing missing file returns null
    - Test parsing malformed JSON returns null and logs warning

14. Create integration test in `src/AISRouting.Tests/IntegrationTests/InputFolderScanningTests.cs`:
    - Create test fixture folder under `src/AISRouting.Tests/TestData/205196000/`
    - Add sample `205196000.json` file
    - Add sample CSV file `2025-03-15.csv`
    - Test end-to-end scanning returns vessel with correct MMSI, Name, MinDate, MaxDate

15. Build and run application to verify:
    - Clicking "Browse" opens folder dialog
    - Selecting valid folder with vessel subfolders populates ComboBox
    - Vessel names appear in ComboBox dropdown
    - Selecting a vessel updates SelectedVessel property

### Scenario 3: Fail when input root empty

**BDD Scenario:**
```gherkin
Given the file system path "C:\\empty\\root" contains no vessel subfolders and the application is running.
When the user opens the Input Folder selector and selects "C:\\empty\\root".
Then the ship selection combo box shows an empty list and an inline warning with text "No vessels found in input root" is displayed.
```

**Technical Design Details:**

From the Feature Design Document:
- Empty scan results trigger validation message in ViewModel
- UI displays inline warning TextBlock with data-testid="no-vessels-warning"
- Warning message property: `FolderErrorMessage` or `NoVesselsWarning`
- ComboBox remains empty and disabled

**Tasks:**

1. Update `MainViewModel.cs`:
   - Add `[ObservableProperty]` for `string? FolderErrorMessage`
   - In `SelectInputFolderCommand` implementation, after scan completes:
     - If `AvailableVessels.Count == 0`, set `FolderErrorMessage = "No vessels found in input root"`
     - Else set `FolderErrorMessage = null` to clear any previous warnings

2. Update `MainWindow.xaml` in Vessel Selection section:
   - Add TextBlock with data-testid="no-vessels-warning"
   - Bind `Text="{Binding FolderErrorMessage}"`
   - Bind `Visibility` to `FolderErrorMessage` using converter (visible when non-null/non-empty)
   - Style with warning color (e.g., Foreground="Orange" or "Red")

3. Create `src/AISRouting.App.WPF/Converters/StringToVisibilityConverter.cs`:
   - Implement `IValueConverter`
   - Convert: return `Visibility.Visible` if string is non-null and non-empty, else `Visibility.Collapsed`
   - ConvertBack: throw `NotImplementedException`

4. Update `MainWindow.xaml` resources:
   - Add converter resource: `<converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>`

5. Update ComboBox in MainWindow.xaml:
   - Bind `IsEnabled="{Binding AvailableVessels.Count, Converter={StaticResource CountToBooleanConverter}}"`
   - Create `CountToBooleanConverter` that returns true if count > 0

6. Create `src/AISRouting.App.WPF/Converters/CountToBooleanConverter.cs`:
   - Implement `IValueConverter`
   - Convert: return true if value > 0, else false
   - ConvertBack: throw `NotImplementedException`

7. Create unit test in `src/AISRouting.Tests/UnitTests/ViewModels/MainViewModelTests.cs`:
   - Mock `ISourceDataScanner` to return empty collection
   - Execute `SelectInputFolderCommand`
   - Assert `FolderErrorMessage == "No vessels found in input root"`
   - Assert `AvailableVessels.Count == 0`

8. Create integration test in `src/AISRouting.Tests/IntegrationTests/InputFolderScanningTests.cs`:
   - Create empty test directory with no subfolders
   - Scan using real `SourceDataScanner`
   - Assert result is empty collection

9. Build and run application to verify:
   - Selecting empty folder shows warning message
   - ComboBox is disabled and empty
   - Warning message disappears when valid folder is selected

### Scenario 4: Prevent start when executable missing or corrupted

**BDD Scenario:**
```gherkin
Given the install path "C:\\apps\\AISRouting" lacks a valid start executable or it is corrupted.
When the user tries to start the application from "C:\\apps\\AISRouting".
Then a visible error dialog with text "Application failed to start: executable missing or corrupted" is displayed.
```

**Technical Design Details:**

From the Feature Design Document:
- This is a system-level failure scenario handled by Windows/.NET runtime
- Application startup failures are caught in `App.xaml.cs` `OnStartup` method
- Display modal error dialog using `MessageBox.Show`
- Log critical error before showing dialog

**Tasks:**

1. Update `App.xaml.cs` `OnStartup` method:
   - Wrap entire method body in try/catch block
   - Catch `Exception` (general exception for any startup failure)
   - Log critical error with exception details
   - Show `MessageBox.Show` with error message: "Application failed to start: executable missing or corrupted\n\n{exception.Message}"
   - Use `MessageBoxButton.OK` and `MessageBoxImage.Error`
   - Call `Shutdown(1)` to exit with error code

2. Add global exception handler in `App.xaml.cs`:
   - Override `OnStartup` to also register `DispatcherUnhandledException` handler
   - In handler: log unhandled exception, show error dialog, mark as handled or shutdown

3. Add logging configuration in `App.xaml.cs`:
   - Configure `ILoggerFactory` in DI container
   - Use `Microsoft.Extensions.Logging` with console and file sinks
   - Register as singleton for use throughout application

4. Create helper method `ShowCriticalErrorDialog(string message)` in App.xaml.cs:
   - Encapsulates `MessageBox.Show` with consistent error styling
   - Returns void, always shows modal dialog

5. Create unit test in `src/AISRouting.Tests/UnitTests/ApplicationTests/StartupErrorHandlingTests.cs`:
   - Test that missing service registrations throw descriptive exceptions
   - Mock scenario where DI container fails to build
   - Assert appropriate error handling occurs (note: testing MessageBox is challenging, focus on exception handling logic)

6. Document startup error scenarios in `docs/user_manual/troubleshooting.md`:
   - "Application failed to start" error and common causes
   - Steps to verify installation integrity
   - How to check for corrupted files

7. Add application icon and manifest for professional appearance:
   - Create `app.ico` icon file in `src/AISRouting.App.WPF/Resources/`
   - Update `.csproj` to include `<ApplicationIcon>Resources\app.ico</ApplicationIcon>`
   - Add application manifest with requested execution level

8. Create smoke test in `src/AISRouting.Tests/IntegrationTests/ApplicationSmokeTests.cs`:
   - Test that application can be instantiated without exceptions
   - Test that all required services are registered in DI container
   - Test that MainWindow can be created via DI

9. Build and run application to verify:
   - Application starts successfully in normal conditions
   - Error handling code is in place (simulate by temporarily breaking DI config)
   - Error dialog appears with appropriate message

## Code Examples

### Example: ISourceDataScanner Interface

```csharp
namespace AISRouting.Core.Services.Interfaces
{
    public interface ISourceDataScanner
    {
        /// <summary>
        /// Scans the input folder for vessel subfolders and returns vessel metadata.
        /// </summary>
        /// <param name="inputFolder">Path to the root input folder containing MMSI subfolders.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Collection of discovered vessels with static data and date ranges.</returns>
        Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(
            string inputFolder, 
            CancellationToken cancellationToken = default);
    }
}
```

### Example: SourceDataScanner Implementation

```csharp
namespace AISRouting.Infrastructure.IO
{
    public class SourceDataScanner : ISourceDataScanner
    {
        private readonly IShipStaticDataLoader _staticLoader;
        private readonly ILogger<SourceDataScanner> _logger;

        public SourceDataScanner(
            IShipStaticDataLoader staticLoader,
            ILogger<SourceDataScanner> logger)
        {
            _staticLoader = staticLoader ?? throw new ArgumentNullException(nameof(staticLoader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ShipStaticData>> ScanInputFolderAsync(
            string inputFolder, 
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(inputFolder))
            {
                _logger.LogError("Input folder not found: {InputFolder}", inputFolder);
                throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");
            }

            var results = new List<ShipStaticData>();
            
            foreach (var dir in Directory.EnumerateDirectories(inputFolder))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mmsiString = Path.GetFileName(dir);
                
                try
                {
                    // Attempt to load static data
                    var staticData = await _staticLoader.LoadStaticDataAsync(dir, mmsiString, cancellationToken);
                    
                    // Fallback if static data file missing
                    if (staticData == null)
                    {
                        staticData = new ShipStaticData
                        {
                            MMSI = long.TryParse(mmsiString, out var mmsi) ? mmsi : 0,
                            FolderPath = dir
                        };
                    }

                    // Extract date range from CSV files
                    var (minDate, maxDate) = ExtractMinMaxDatesFromCsvFiles(dir);
                    staticData.MinDate = minDate;
                    staticData.MaxDate = maxDate;

                    results.Add(staticData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping folder {Dir} during input scan", dir);
                }
            }

            _logger.LogInformation("Scanned {Count} vessels from {InputFolder}", results.Count, inputFolder);
            return results;
        }

        private (DateTime minDate, DateTime maxDate) ExtractMinMaxDatesFromCsvFiles(string folderPath)
        {
            var dates = new List<DateTime>();

            foreach (var csvFile in Directory.EnumerateFiles(folderPath, "*.csv"))
            {
                var filename = Path.GetFileNameWithoutExtension(csvFile);
                
                if (DateTime.TryParseExact(filename, "yyyy-MM-dd", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, 
                    out var date))
                {
                    dates.Add(date);
                }
            }

            if (dates.Count == 0)
            {
                _logger.LogWarning("No valid CSV date files found in {FolderPath}", folderPath);
                return (DateTime.MinValue, DateTime.MaxValue);
            }

            return (dates.Min(), dates.Max());
        }
    }
}
```

### Example: MainViewModel SelectInputFolderCommand

```csharp
namespace AISRouting.App.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISourceDataScanner _scanner;
        private readonly IFolderDialogService _folderDialog;
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private string? _inputFolderPath;

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private string? _folderErrorMessage;

        [ObservableProperty]
        private ShipStaticData? _selectedVessel;

        public ObservableCollection<ShipStaticData> AvailableVessels { get; } = new();

        public IAsyncRelayCommand SelectInputFolderCommand { get; }

        public MainViewModel(
            ISourceDataScanner scanner,
            IFolderDialogService folderDialog,
            ILogger<MainViewModel> logger)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _folderDialog = folderDialog ?? throw new ArgumentNullException(nameof(folderDialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SelectInputFolderCommand = new AsyncRelayCommand(SelectInputFolderAsync);
        }

        private async Task SelectInputFolderAsync()
        {
            var folder = _folderDialog.ShowFolderBrowser(InputFolderPath);
            if (string.IsNullOrEmpty(folder))
            {
                _logger.LogDebug("Folder selection cancelled by user");
                return;
            }

            try
            {
                IsScanning = true;
                FolderErrorMessage = null;
                InputFolderPath = folder;

                var vessels = await _scanner.ScanInputFolderAsync(folder);
                
                AvailableVessels.Clear();
                foreach (var vessel in vessels)
                {
                    AvailableVessels.Add(vessel);
                }

                if (AvailableVessels.Count == 0)
                {
                    FolderErrorMessage = "No vessels found in input root";
                    _logger.LogWarning("No vessels found in input folder: {Folder}", folder);
                }
                else
                {
                    _logger.LogInformation("Loaded {Count} vessels from input folder", AvailableVessels.Count);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                FolderErrorMessage = "Input root not accessible";
                _logger.LogError(ex, "Failed to access input folder: {Folder}", folder);
            }
            catch (Exception ex)
            {
                FolderErrorMessage = "Error scanning input folder";
                _logger.LogError(ex, "Unexpected error scanning input folder: {Folder}", folder);
            }
            finally
            {
                IsScanning = false;
            }
        }
    }
}
```

### Example: ShipStaticData Model

```csharp
namespace AISRouting.Core.Models
{
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
        public string FolderPath { get; set; } = string.Empty;

        public string DisplayName => Name ?? $"Vessel {MMSI}";
    }
}
```

### Example: MainWindow.xaml Vessel Selection Section

```xml
<!-- Vessel Selection Panel -->
<GroupBox Grid.Row="1" Header="Vessel Selection" Margin="0,0,0,10">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Vessel ComboBox -->
        <Grid Grid.Row="0" Margin="0,0,0,10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <TextBlock Grid.Column="0" Text="Select Vessel:" 
                       VerticalAlignment="Center" Margin="0,0,10,0" />
            <ComboBox Grid.Column="1" 
                      AutomationProperties.AutomationId="ship-combo"
                      ItemsSource="{Binding AvailableVessels}"
                      SelectedItem="{Binding SelectedVessel}"
                      DisplayMemberPath="DisplayName"
                      IsEnabled="{Binding AvailableVessels.Count, Converter={StaticResource CountToBooleanConverter}}" />
        </Grid>

        <!-- Warning Message -->
        <TextBlock Grid.Row="1"
                   AutomationProperties.AutomationId="no-vessels-warning"
                   Text="{Binding FolderErrorMessage}"
                   Visibility="{Binding FolderErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
                   Foreground="Orange"
                   FontWeight="SemiBold"
                   Margin="0,5,0,10" />

        <!-- Scanning Progress -->
        <StackPanel Grid.Row="2" 
                    Orientation="Horizontal"
                    Visibility="{Binding IsScanning, Converter={StaticResource BoolToVisibilityConverter}}">
            <TextBlock Text="Scanning for vessels..." Margin="0,0,10,0" />
            <ProgressBar IsIndeterminate="True" Width="150" Height="20" />
        </StackPanel>
    </Grid>
</GroupBox>
```

### Example: App.xaml.cs DI Configuration

```csharp
namespace AISRouting.App.WPF
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                ShowCriticalErrorDialog($"Application failed to start: executable missing or corrupted\n\n{ex.Message}");
                Shutdown(1);
            }

            // Register global exception handler
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Core services
            services.AddSingleton<IGeoCalculator, GeoCalculator>();
            services.AddSingleton<ITrackOptimizer, TrackOptimizer>();

            // Infrastructure services
            services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
            services.AddSingleton<IShipStaticDataLoader, ShipStaticDataParser>();

            // UI services
            services.AddSingleton<IFolderDialogService, FolderDialogService>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = _serviceProvider?.GetService<ILogger<App>>();
            logger?.LogCritical(e.Exception, "Unhandled exception occurred");

            ShowCriticalErrorDialog($"An unexpected error occurred:\n\n{e.Exception.Message}");
            e.Handled = true;
            Shutdown(1);
        }

        private void ShowCriticalErrorDialog(string message)
        {
            MessageBox.Show(
                message,
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
```

## Success Criteria
- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.
- Application launches successfully and displays main window with input folder selector.
- Folder selection dialog opens and allows user to select a directory.
- Valid input folders with vessel subfolders populate the ship combo box correctly.
- Empty input folders display appropriate warning message.
- Application handles startup errors gracefully with error dialog.
- All UI controls have proper data-testid or AutomationProperties.AutomationId for testing.
- Unit tests pass for all service components (SourceDataScanner, ShipStaticDataParser).
- Integration tests pass for end-to-end folder scanning scenarios.
- Code follows MVVM pattern with proper separation of concerns.
- All services are properly registered in DI container and resolve without errors.

## Technical Requirements

### Architecture and Design Patterns
- Follow MVVM architecture strictly: Views bind to ViewModels, ViewModels call Services
- Use CommunityToolkit.Mvvm for ObservableObject and RelayCommand implementations
- All business logic resides in Core/Infrastructure services, not in ViewModels
- ViewModels are responsible for orchestration and UI state management only

### Dependency Injection
- All services registered in App.xaml.cs using Microsoft.Extensions.DependencyInjection
- Services registered as singletons (stateless) or transient (ViewModels)
- Constructor injection for all dependencies
- No service locator pattern or manual instantiation

### Asynchronous Programming
- All I/O operations use async/await pattern
- Use CancellationToken for long-running operations
- Commands use IAsyncRelayCommand from CommunityToolkit.Mvvm
- Proper exception handling in async methods

### Error Handling and Logging
- Use Microsoft.Extensions.Logging throughout
- Log levels: Debug (verbose), Information (key events), Warning (recoverable issues), Error (failures), Critical (startup failures)
- Try/catch blocks around all I/O operations
- User-friendly error messages displayed in UI
- Technical details logged for debugging

### Testing Strategy
- Unit tests for all services with mocked dependencies
- Integration tests for file-based operations using test fixtures
- Test coverage for positive and negative scenarios
- Use FluentAssertions for readable assertions
- Mock file system operations where appropriate

### UI and Accessibility
- All interactive controls have labels
- Use AutomationProperties.AutomationId for test automation
- Provide visual feedback for async operations (progress bars, spinners)
- Disable controls appropriately based on state
- Clear, concise error messages with actionable guidance

### Code Quality
- Follow C# naming conventions (PascalCase for public members, _camelCase for private fields)
- XML documentation comments on public APIs
- Keep methods small and focused (single responsibility)
- Avoid code duplication through helper methods
- Use nullable reference types appropriately

### File Organization
- One class per file, filename matches class name
- Follow namespace structure matching folder structure
- Group related files in appropriate folders (Models, Services, ViewModels, Views, etc.)
- Place interfaces in Services/Interfaces folder

### Data Validation
- Validate all user inputs before processing
- Display validation errors inline near controls
- Prevent invalid operations through CanExecute on commands
- Log validation failures for debugging

### Performance Considerations
- Use streaming operations for large file enumerations
- Avoid blocking UI thread with long-running operations
- Implement cancellation for operations that can take significant time
- Lazy load data only when needed
