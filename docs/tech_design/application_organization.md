# Application Organization and Project Structure

This document covers the project structure, component organization, code organization patterns, and module responsibilities for the AISRouting application.

## Solution Structure

```
d:\repo\AISRouting/
├── src/
│   ├── AISRouting.sln                          # Visual Studio solution file
│   │
│   ├── AISRouting.App.WPF/                     # Presentation Layer (WPF Application)
│   │   ├── AISRouting.App.WPF.csproj
│   │   ├── App.xaml                             # Application entry point
│   │   ├── App.xaml.cs                          # DI container setup
│   │   ├── MainWindow.xaml                      # Main UI window
│   │   ├── MainWindow.xaml.cs                   # Code-behind (minimal)
│   │   │
│   │   ├── Views/                               # XAML view files
│   │   │   ├── ShipSelectionView.xaml
│   │   │   ├── TimeIntervalView.xaml
│   │   │   ├── TrackResultsView.xaml
│   │   │   └── ExportDialogView.xaml
│   │   │
│   │   ├── ViewModels/                          # MVVM ViewModels
│   │   │   ├── MainViewModel.cs                 # Main window orchestration
│   │   │   ├── ShipSelectionViewModel.cs
│   │   │   ├── TimeIntervalViewModel.cs
│   │   │   ├── TrackResultsViewModel.cs
│   │   │   └── ViewModelBase.cs                 # Base class (if not using ObservableObject)
│   │   │
│   │   ├── Converters/                          # Value converters for data binding
│   │   │   ├── BoolToVisibilityConverter.cs
│   │   │   ├── DateTimeToStringConverter.cs
│   │   │   └── NullToDefaultConverter.cs
│   │   │
│   │   ├── Resources/                           # Styles, templates, assets
│   │   │   ├── Styles.xaml                      # Global styles
│   │   │   ├── Templates.xaml                   # Control templates
│   │   │   └── Icons/                           # Application icons
│   │   │
│   │   └── Services/                            # UI-specific services
│   │       ├── FolderDialogService.cs           # IFolderDialogService implementation
│   │       └── MessageBoxService.cs             # IMessageBoxService implementation
│   │
│   ├── AISRouting.Core/                         # Business Logic Layer (Domain)
│   │   ├── AISRouting.Core.csproj
│   │   │
│   │   ├── Models/                              # Domain entities
│   │   │   ├── ShipStaticData.cs                # Vessel metadata
│   │   │   ├── ShipDataOut.cs                   # Position report
│   │   │   ├── RouteWaypoint.cs                 # Optimized waypoint
│   │   │   ├── TimeInterval.cs                  # User-selected time range
│   │   │   └── OptimizationParameters.cs        # Threshold configuration
│   │   │
│   │   ├── Services/                            # Business logic interfaces and implementations
│   │   │   ├── Interfaces/
│   │   │   │   ├── ISourceDataScanner.cs
│   │   │   │   ├── IShipStaticDataLoader.cs
│   │   │   │   ├── IShipPositionLoader.cs
│   │   │   │   ├── ITrackOptimizer.cs
│   │   │   │   ├── IRouteExporter.cs
│   │   │   │   └── IFolderDialogService.cs
│   │   │   │
│   │   │   └── Implementations/
│   │   │       ├── TrackOptimizer.cs            # Core optimization algorithms
│   │   │       ├── DeviationDetector.cs         # Deviation analysis logic
│   │   │       └── GeoCalculator.cs             # Haversine, bearing calculations
│   │   │
│   │   └── Validators/                          # Domain validation logic
│   │       ├── ShipStaticDataValidator.cs
│   │       ├── TimeIntervalValidator.cs
│   │       └── PositionDataValidator.cs
│   │
│   ├── AISRouting.Infrastructure/               # Data Access Layer (I/O, Persistence)
│   │   ├── AISRouting.Infrastructure.csproj
│   │   │
│   │   ├── IO/                                  # File system operations
│   │   │   ├── SourceDataScanner.cs             # Folder enumeration, date extraction
│   │   │   ├── CsvReader.cs                     # CSV file loading (CsvHelper wrapper)
│   │   │   ├── JsonReader.cs                    # JSON deserialization (System.Text.Json)
│   │   │   └── FileSystemHelper.cs              # Common file operations
│   │   │
│   │   ├── Parsers/                             # Data parsing logic
│   │   │   ├── ShipStaticDataParser.cs          # Parse JSON to ShipStaticData
│   │   │   ├── PositionCsvParser.cs             # Parse CSV to ShipDataOut
│   │   │   └── CsvMappers.cs                    # CsvHelper type mappings
│   │   │
│   │   └── Persistence/                         # Data serialization
│   │       ├── RouteExporter.cs                 # XML export implementation
│   │       ├── XmlRouteWriter.cs                # XML serialization
│   │       └── TemplateLoader.cs                # Load route_waypoint_template.xml
│   │
│   └── AISRouting.Tests/                        # Test Project
│       ├── AISRouting.Tests.csproj
│       │
│       ├── UnitTests/                           # Unit tests (isolated, mocked)
│       │   ├── Core/
│       │   │   ├── TrackOptimizerTests.cs
│       │   │   ├── DeviationDetectorTests.cs
│       │   │   ├── GeoCalculatorTests.cs
│       │   │   └── ValidatorTests.cs
│       │   │
│       │   ├── Infrastructure/
│       │   │   ├── SourceDataScannerTests.cs
│       │   │   ├── CsvReaderTests.cs
│       │   │   ├── JsonReaderTests.cs
│       │   │   └── RouteExporterTests.cs
│       │   │
│       │   └── ViewModels/
│       │       ├── MainViewModelTests.cs
│       │       └── ShipSelectionViewModelTests.cs
│       │
│       ├── IntegrationTests/                    # End-to-end with real files
│       │   ├── EndToEndWorkflowTests.cs
│       │   ├── MultiDayTrackTests.cs
│       │   └── XmlExportValidationTests.cs
│       │
│       └── TestData/                            # Sample CSV, JSON for tests
│           ├── 205196000/
│           │   ├── 205196000.json
│           │   ├── 2024-01-01.csv
│           │   └── 2024-01-02.csv
│           └── route_waypoint_template.xml
│
├── docs/                                        # Documentation
│   ├── spec_scenarios/                          # BDD scenarios
│   ├── tech_design/                             # Technical design docs (this file)
│   ├── user_manual/                             # User guides
│   └── workflow/                                # Development workflow prompts
│
└── route_waypoint_template.xml                  # XML template for export (app root)
```

## Project Dependencies

```
AISRouting.App.WPF
    ↓ references
    ├── AISRouting.Core
    └── AISRouting.Infrastructure

AISRouting.Infrastructure
    ↓ references
    └── AISRouting.Core

AISRouting.Tests
    ↓ references
    ├── AISRouting.Core
    ├── AISRouting.Infrastructure
    └── AISRouting.App.WPF (for ViewModel tests)
```

**Dependency Rules:**
- Core has no dependencies on other projects (pure domain logic)
- Infrastructure depends only on Core
- App depends on Core and Infrastructure
- Tests can reference all projects

## Component Organization

### Presentation Layer (AISRouting.App.WPF)

**Responsibility:** User interface, data binding, command handling, DI setup

**Key Components:**

**App.xaml.cs** - Application Startup
```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register Core services
        services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
        services.AddSingleton<IGeoCalculator, GeoCalculator>();
        
        // Register Infrastructure services
        services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
        services.AddSingleton<IShipStaticDataLoader, ShipStaticDataLoader>();
        services.AddSingleton<IShipPositionLoader, ShipPositionLoader>();
        services.AddSingleton<IRouteExporter, RouteExporter>();
        
        // Register UI services
        services.AddSingleton<IFolderDialogService, FolderDialogService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        
        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddFile("Logs/aisrouting-{Date}.log");
        });
    }
}
```

**MainViewModel** - Orchestrates Main Window Logic
```csharp
public class MainViewModel : ObservableObject
{
    private readonly ISourceDataScanner _scanner;
    private readonly IShipPositionLoader _positionLoader;
    private readonly ITrackOptimizer _optimizer;
    private readonly IRouteExporter _exporter;
    private readonly IFolderDialogService _folderDialog;

    public ObservableCollection<ShipStaticData> AvailableVessels { get; }
    public ShipStaticData SelectedVessel { get; set; }
    public TimeInterval TimeInterval { get; set; }
    public ObservableCollection<RouteWaypoint> GeneratedWaypoints { get; }

    public IAsyncRelayCommand SelectInputFolderCommand { get; }
    public IAsyncRelayCommand CreateTrackCommand { get; }
    public IAsyncRelayCommand ExportRouteCommand { get; }

    // Command implementations use injected services
}
```

**View Organization:**
- **MainWindow.xaml**: Top-level container with regions
- **ShipSelectionView.xaml**: Vessel combo, static data display (user control)
- **TimeIntervalView.xaml**: Start/stop pickers (user control)
- **TrackResultsView.xaml**: Waypoint list display (user control)

### Business Logic Layer (AISRouting.Core)

**Responsibility:** Domain models, business rules, optimization algorithms, validation

**Key Components:**

**ITrackOptimizer** - Interface
```csharp
public interface ITrackOptimizer
{
    Task<IEnumerable<RouteWaypoint>> OptimizeTrack(
        IEnumerable<ShipDataOut> positions,
        OptimizationParameters parameters,
        CancellationToken cancellationToken = default);
}
```

**TrackOptimizer** - Implementation
```csharp
public class TrackOptimizer : ITrackOptimizer
{
    private readonly IDeviationDetector _deviationDetector;
    private readonly IGeoCalculator _geoCalculator;
    private readonly ILogger<TrackOptimizer> _logger;

    public async Task<IEnumerable<RouteWaypoint>> OptimizeTrack(...)
    {
        var waypoints = new List<RouteWaypoint>();
        ShipDataOut? previous = null;

        foreach (var current in positions)
        {
            if (ShouldRetainWaypoint(previous, current, parameters))
            {
                waypoints.Add(MapToWaypoint(current));
                previous = current;
            }
        }

        return waypoints;
    }

    private bool ShouldRetainWaypoint(ShipDataOut previous, ShipDataOut current, ...)
    {
        // Deviation detection logic:
        // - Heading change > threshold
        // - Distance > threshold
        // - SOG change > threshold
        // - ROT > threshold
    }
}
```

**GeoCalculator** - Geodesic Calculations
```csharp
public class GeoCalculator : IGeoCalculator
{
    public double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula implementation
    }

    public double Bearing(double lat1, double lon1, double lat2, double lon2)
    {
        // Initial bearing calculation
    }
}
```

### Data Access Layer (AISRouting.Infrastructure)

**Responsibility:** File I/O, CSV/JSON parsing, XML export, folder operations

**Key Components:**

**SourceDataScanner** - Folder Enumeration
```csharp
public class SourceDataScanner : ISourceDataScanner
{
    private readonly IShipStaticDataLoader _staticLoader;
    private readonly ILogger<SourceDataScanner> _logger;

    public async Task<IEnumerable<ShipStaticData>> ScanInputFolder(
        string inputFolder,
        CancellationToken cancellationToken = default)
    {
        var vessels = new List<ShipStaticData>();
        var mmsiDirs = Directory.GetDirectories(inputFolder);

        foreach (var dir in mmsiDirs)
        {
            var mmsi = Path.GetFileName(dir);
            var staticData = await _staticLoader.LoadStaticData(dir, mmsi);
            staticData.MinDate = ExtractMinDateFromCsvFiles(dir);
            staticData.MaxDate = ExtractMaxDateFromCsvFiles(dir);
            vessels.Add(staticData);
        }

        return vessels;
    }

    private DateTime ExtractMinDateFromCsvFiles(string folderPath)
    {
        // Parse CSV filenames (YYYY-MM-DD.csv) and return earliest date
    }
}
```

**PositionCsvParser** - CSV Parsing
```csharp
public class PositionCsvParser
{
    public async IAsyncEnumerable<ShipDataOut> ParseCsvFile(
        string csvPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, _config);
        csv.Context.RegisterClassMap<ShipDataOutMap>();

        await foreach (var record in csv.GetRecordsAsync<ShipDataOut>(cancellationToken))
        {
            yield return record;
        }
    }
}
```

**RouteExporter** - XML Export
```csharp
public class RouteExporter : IRouteExporter
{
    private readonly ILogger<RouteExporter> _logger;

    public async Task ExportToXml(
        IEnumerable<RouteWaypoint> waypoints,
        string outputFolder,
        string mmsi,
        DateTime start,
        DateTime stop)
    {
        var filename = GenerateFilename(mmsi, start, stop);
        var fullPath = Path.Combine(outputFolder, filename);

        if (File.Exists(fullPath))
        {
            // Handle conflict: prompt user for overwrite/suffix/cancel
        }

        await WriteXml(waypoints, fullPath);
    }

    private string GenerateFilename(string mmsi, DateTime start, DateTime stop)
    {
        return $"{mmsi}-{start:yyyyMMddTHHmmss}-{stop:yyyyMMddTHHmmss}.xml";
    }
}
```

## Code Organization Patterns

### MVVM Pattern Implementation

**View (XAML):**
```xml
<Window x:Class="AISRouting.App.WPF.MainWindow"
        DataContext="{Binding Source={StaticResource Locator}, Path=MainViewModel}">
    <Grid>
        <Button Command="{Binding SelectInputFolderCommand}" Content="Select Input Folder"/>
        <ComboBox ItemsSource="{Binding AvailableVessels}"
                  SelectedItem="{Binding SelectedVessel}"/>
        <Button Command="{Binding CreateTrackCommand}" Content="Create Track"/>
    </Grid>
</Window>
```

**ViewModel:**
```csharp
public class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ShipStaticData> _availableVessels;

    [ObservableProperty]
    private ShipStaticData _selectedVessel;

    [RelayCommand]
    private async Task SelectInputFolder()
    {
        var folder = _folderDialog.ShowFolderBrowser();
        if (!string.IsNullOrEmpty(folder))
        {
            AvailableVessels = new ObservableCollection<ShipStaticData>(
                await _scanner.ScanInputFolder(folder));
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateTrack))]
    private async Task CreateTrack()
    {
        var positions = await _positionLoader.LoadPositions(
            SelectedVessel.MMSI, TimeInterval);
        var waypoints = await _optimizer.OptimizeTrack(positions, _parameters);
        GeneratedWaypoints = new ObservableCollection<RouteWaypoint>(waypoints);
    }

    private bool CanCreateTrack() => SelectedVessel != null && TimeInterval.IsValid;
}
```

### Service Layer Pattern

**Interface Definition (Core):**
```csharp
namespace AISRouting.Core.Services.Interfaces
{
    public interface IShipPositionLoader
    {
        Task<IEnumerable<ShipDataOut>> LoadPositions(
            long mmsi,
            TimeInterval interval,
            CancellationToken cancellationToken = default);
    }
}
```

**Implementation (Infrastructure):**
```csharp
namespace AISRouting.Infrastructure.IO
{
    public class ShipPositionLoader : IShipPositionLoader
    {
        private readonly IPositionCsvParser _csvParser;
        private readonly ILogger<ShipPositionLoader> _logger;

        public async Task<IEnumerable<ShipDataOut>> LoadPositions(...)
        {
            var csvFiles = IdentifyCsvFilesInRange(mmsi, interval);
            var positions = new List<ShipDataOut>();

            foreach (var file in csvFiles)
            {
                await foreach (var record in _csvParser.ParseCsvFile(file, cancellationToken))
                {
                    if (IsInTimeRange(record, interval))
                        positions.Add(record);
                }
            }

            return positions;
        }
    }
}
```

### Dependency Injection Pattern

**Registration (App.xaml.cs):**
```csharp
services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
services.AddTransient<MainViewModel>();
```

**Consumption (ViewModel Constructor):**
```csharp
public MainViewModel(
    ISourceDataScanner scanner,
    IShipPositionLoader positionLoader,
    ITrackOptimizer optimizer,
    IRouteExporter exporter,
    IFolderDialogService folderDialog,
    ILogger<MainViewModel> logger)
{
    _scanner = scanner;
    _positionLoader = positionLoader;
    _optimizer = optimizer;
    _exporter = exporter;
    _folderDialog = folderDialog;
    _logger = logger;
}
```

## Module Responsibilities

### AISRouting.App.WPF Module

**Owns:**
- XAML views and code-behind
- ViewModels (presentation logic)
- UI-specific services (dialogs, message boxes)
- Application startup and DI configuration
- Value converters for data binding
- Styles and resources

**Does NOT Own:**
- Business logic or algorithms
- File I/O operations
- Data validation rules
- CSV/JSON parsing

### AISRouting.Core Module

**Owns:**
- Domain models (ShipStaticData, ShipDataOut, RouteWaypoint)
- Service interfaces (contracts)
- Business logic implementations (TrackOptimizer, GeoCalculator)
- Validation rules
- Algorithm implementations

**Does NOT Own:**
- UI concerns (XAML, data binding)
- File system access
- External library dependencies (CsvHelper, System.Xml)

### AISRouting.Infrastructure Module

**Owns:**
- File I/O operations
- CSV/JSON parsing
- XML serialization
- Folder scanning
- Data persistence

**Does NOT Own:**
- UI elements
- Business rules
- Optimization algorithms

### AISRouting.Tests Module

**Owns:**
- Unit tests for all layers
- Integration tests (end-to-end)
- Test data files
- Mocks and test doubles

## Naming Conventions

**Namespaces:**
- `AISRouting.App.WPF.Views`
- `AISRouting.App.WPF.ViewModels`
- `AISRouting.Core.Models`
- `AISRouting.Core.Services.Interfaces`
- `AISRouting.Core.Services.Implementations`
- `AISRouting.Infrastructure.IO`
- `AISRouting.Infrastructure.Parsers`

**Classes:**
- ViewModels: `MainViewModel`, `ShipSelectionViewModel`
- Services: `TrackOptimizer`, `SourceDataScanner`
- Models: `ShipStaticData`, `RouteWaypoint`
- Interfaces: `ITrackOptimizer`, `IRouteExporter`

**Files:**
- One class per file
- File name matches class name: `TrackOptimizer.cs`
- Test files: `TrackOptimizerTests.cs`

## Build Configuration

**Project Files (csproj):**

**AISRouting.Core.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**AISRouting.Infrastructure.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\AISRouting.Core\AISRouting.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="CsvHelper" Version="30.0.1" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**AISRouting.App.WPF.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\AISRouting.Core\AISRouting.Core.csproj" />
    <ProjectReference Include="..\AISRouting.Infrastructure\AISRouting.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.File" Version="8.0.0" />
    <PackageReference Include="Ookii.Dialogs.Wpf" Version="5.0.1" />
  </ItemGroup>
</Project>
```

## Extension Points

### Adding New Features

**Example: Add Map Visualization**

1. Add NuGet package to App.WPF: `Mapsui.Wpf`
2. Create `MapView.xaml` in Views folder
3. Create `MapViewModel.cs` in ViewModels folder
4. Register MapViewModel in DI container
5. Add map display region to MainWindow.xaml

**Example: Add New Export Format (GPX)**

1. Create `IGpxExporter` interface in Core.Services.Interfaces
2. Implement `GpxExporter` in Infrastructure.Persistence
3. Register in DI container
4. Add export command to ViewModel
5. No changes needed to Core domain logic

### Testing Strategy

**Unit Tests:**
- Mock all dependencies
- Test single class in isolation
- Fast execution (<1ms per test)

**Integration Tests:**
- Use test data files
- Test complete workflows
- Validate XML output

**Example Unit Test:**
```csharp
[Test]
public async Task TrackOptimizer_ReturnsOptimizedWaypoints()
{
    // Arrange
    var mockDetector = new Mock<IDeviationDetector>();
    var optimizer = new TrackOptimizer(mockDetector.Object, ...);
    var positions = CreateTestPositions();

    // Act
    var waypoints = await optimizer.OptimizeTrack(positions, ...);

    // Assert
    waypoints.Should().HaveCountLessThan(positions.Count());
}
```

## References

- WPF MVVM Pattern: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview
- CommunityToolkit.Mvvm: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- Dependency Injection in .NET: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
