# Application Organization

This document covers the project structure, component organization, and code organization for the AisToXmlRouteConvertor application.

## 1. Overview

AisToXmlRouteConvertor uses a single-project structure to minimize complexity while maintaining clear separation of concerns through logical folder organization. All components reside in one Avalonia application project with folders grouping related functionality.

## 2. Solution Structure

### 2.1 High-Level Layout

```
<RepositoryRoot>/
├── src/
│   ├── AisToXmlRouteConvertor/              # Main application project
│   │   └── AisToXmlRouteConvertor.csproj
│   └── AisToXmlRouteConvertor.Tests/        # Unit test project
│       └── AisToXmlRouteConvertor.Tests.csproj
│
├── docs/                                     # Documentation
│   ├── input_tech_docs/
│   ├── spec_scenarios/
│   ├── tech_design/                          # This document's location
│   ├── user_manual/
│   └── workflow/
│
├── sample_data/                              # Sample AIS data for testing
│   ├── 205196000/
│   │   ├── 205196000.json
│   │   └── 2025-03-15.csv
│   └── 123456000/
│       └── ...
│
├── .gitignore
├── README.md
└── LICENSE
```

### 2.2 Main Application Project Structure

```
src/AisToXmlRouteConvertor/
├── AisToXmlRouteConvertor.csproj            # Project file
├── App.axaml                                 # Application root XAML
├── App.axaml.cs                              # Application initialization
├── MainWindow.axaml                          # Main window XAML
├── MainWindow.axaml.cs                       # Main window code-behind
│
├── Models/                                   # Domain data models
│   ├── ShipStaticData.cs
│   ├── ShipState.cs
│   ├── TimeInterval.cs
│   ├── RouteWaypoint.cs
│   └── TrackOptimizationParameters.cs
│
├── ViewModels/                               # MVVM ViewModels
│   ├── MainViewModel.cs                      # Main window ViewModel
│   └── ViewModelBase.cs                      # Base class (optional)
│
├── Services/                                 # Static helper services
│   ├── Helper.cs                             # Main static helper methods
│   └── GeoMath.cs                            # Geographic calculations
│
├── Parsers/                                  # Data parsing
│   ├── CsvParser.cs                          # CSV file parsing
│   └── JsonParser.cs                         # JSON file parsing
│
├── Export/                                   # Output generation
│   └── XmlExporter.cs                        # XML route file export
│
├── Optimization/                             # Track optimization
│   └── TrackOptimizer.cs                     # Waypoint optimization algorithm
│
└── Assets/                                   # Application assets
    ├── Icons/
    │   └── app-icon.ico
    └── Styles/
        └── CustomStyles.axaml                # Additional styles (optional)
```

### 2.3 Test Project Structure

```
src/AisToXmlRouteConvertor.Tests/
├── AisToXmlRouteConvertor.Tests.csproj      # Test project file
│
├── UnitTests/
│   ├── GeoMathTests.cs                       # Geographic calculation tests
│   ├── TrackOptimizerTests.cs                # Optimization algorithm tests
│   ├── CsvParserTests.cs                     # CSV parsing tests
│   ├── JsonParserTests.cs                    # JSON parsing tests
│   └── XmlExporterTests.cs                   # XML export tests
│
├── IntegrationTests/
│   └── EndToEndTests.cs                      # Full workflow tests
│
├── TestData/                                 # Test fixtures
│   ├── valid_ship.json
│   ├── invalid_ship.json
│   ├── sample_positions.csv
│   └── malformed_positions.csv
│
└── Helpers/
    └── TestDataBuilder.cs                    # Test data factory methods
```

## 3. Component Organization

### 3.1 Models Folder

**Purpose**: Contains all domain data structures as immutable C# records.

**Files**:
- `ShipStaticData.cs`: Vessel metadata (MMSI, name, dimensions, date range)
- `ShipState.cs`: AIS position report (timestamp, lat/lon, speed, course, heading)
- `TimeInterval.cs`: User-selected time range (start and end UTC)
- `RouteWaypoint.cs`: Optimized navigation waypoint (sequence, position, speed, heading, ETA)
- `TrackOptimizationParameters.cs`: Optimization thresholds (heading change, distance, speed change, ROT)

**Design Principles**:
- Immutable records (init-only properties)
- No business logic (pure data containers)
- Nullable types for optional fields
- XML documentation comments on all public members

**Example**:
```csharp
namespace AisToXmlRouteConvertor.Models;

/// <summary>
/// Static information about a vessel.
/// </summary>
public sealed record ShipStaticData(
    long Mmsi,
    string? Name,
    double? Length,
    double? Beam,
    double? Draught,
    string? CallSign,
    string? ImoNumber,
    DateTime? MinDateUtc,
    DateTime? MaxDateUtc
);
```

### 3.2 ViewModels Folder

**Purpose**: Contains MVVM ViewModels that manage UI state and orchestrate operations.

**Files**:
- `MainViewModel.cs`: Primary ViewModel for MainWindow
- `ViewModelBase.cs`: Optional base class providing `INotifyPropertyChanged` implementation (from CommunityToolkit.Mvvm)

**Design Principles**:
- Use `[ObservableProperty]` attribute from CommunityToolkit.Mvvm for bindable properties
- Use `[RelayCommand]` attribute for command methods
- No direct file I/O - delegate to Helper static methods
- Clear separation: ViewModel orchestrates, Helper executes
- Property change notifications for UI updates

**Example**:
```csharp
namespace AisToXmlRouteConvertor.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? inputFolder;

    [ObservableProperty]
    private List<long> availableMmsi = new();

    [ObservableProperty]
    private long? selectedMmsi;

    [RelayCommand]
    private async Task BrowseInputFolder()
    {
        // Open folder picker, set InputFolder property
        // Triggers property change notification automatically
    }

    [RelayCommand]
    private void ScanVessels()
    {
        if (InputFolder == null) return;
        AvailableMmsi = Helper.GetAvailableMmsi(InputFolder).ToList();
    }
}
```

### 3.3 Services Folder

**Purpose**: Contains static helper classes for core operations.

**Files**:
- `Helper.cs`: Main static methods for scanning, loading, optimizing, exporting
- `GeoMath.cs`: Geographic calculations (Haversine distance, bearing)

**Design Principles**:
- All methods static (no instance state)
- Synchronous operations (no async)
- Clear method signatures with XML documentation
- Return value types or throw specific exceptions
- No UI dependencies (console-testable)

**Helper.cs Methods**:
```csharp
namespace AisToXmlRouteConvertor.Services;

public static class Helper
{
    /// <summary>
    /// Scans root folder for MMSI subfolders containing AIS data.
    /// Checks for CSV file existence by filename only - does NOT load content.
    /// </summary>
    /// <param name="rootPath">Root folder containing MMSI subfolders</param>
    /// <returns>List of available MMSI numbers</returns>
    public static IReadOnlyList<long> GetAvailableMmsi(string rootPath);

    /// <summary>
    /// Loads ship static data from JSON file.
    /// </summary>
    public static ShipStaticData? LoadShipStatic(string vesselFolder);

    /// <summary>
    /// Loads and filters AIS positions for a vessel within time interval.
    /// This method actually loads CSV file content - only called when Process! button clicked.
    /// CSV files may be gigabytes in size, so this is deferred until necessary.
    /// </summary>
    public static IReadOnlyList<ShipState> LoadShipStates(string vesselFolder, TimeInterval interval);

    /// <summary>
    /// Optimizes track by reducing positions to significant waypoints.
    /// </summary>
    public static IReadOnlyList<RouteWaypoint> OptimizeTrack(
        IReadOnlyList<ShipState> states, 
        TrackOptimizationParameters parameters);

    /// <summary>
    /// Exports route waypoints to XML file.
    /// </summary>
    /// <returns>Full path of exported file</returns>
    public static string ExportRoute(
        IReadOnlyList<RouteWaypoint> waypoints, 
        long mmsi, 
        TimeInterval interval, 
        string outputFolder);
}
```

**GeoMath.cs Methods**:
```csharp
namespace AisToXmlRouteConvertor.Services;

public static class GeoMath
{
    /// <summary>
    /// Calculates distance between two points using Haversine formula.
    /// </summary>
    /// <returns>Distance in meters</returns>
    public static double HaversineDistance(
        double lat1, double lon1, 
        double lat2, double lon2);

    /// <summary>
    /// Calculates initial bearing from point 1 to point 2.
    /// </summary>
    /// <returns>Bearing in degrees (0-360)</returns>
    public static double InitialBearing(
        double lat1, double lon1, 
        double lat2, double lon2);
}
```

### 3.4 Parsers Folder

**Purpose**: File format parsing implementations.

**Files**:
- `CsvParser.cs`: Parses daily AIS CSV files into `ShipState` records
- `JsonParser.cs`: Deserializes ship static JSON into `ShipStaticData`

**Design Principles**:
- Use `CsvHelper` library for robust CSV parsing
- Use `System.Text.Json` for JSON deserialization
- Graceful error handling (log and skip malformed rows, don't crash)
- Streaming parse for memory efficiency
- Return strongly-typed collections

**CsvParser.cs**:
```csharp
namespace AisToXmlRouteConvertor.Parsers;

using CsvHelper;
using CsvHelper.Configuration;

public static class CsvParser
{
    /// <summary>
    /// Parses CSV file into ShipState records.
    /// Skips and logs malformed rows.
    /// </summary>
    public static IReadOnlyList<ShipState> ParsePositions(string csvFilePath);

    // Internal: CSV to ShipState mapping configuration
    private sealed class ShipStateMap : ClassMap<ShipState>
    {
        public ShipStateMap()
        {
            Map(m => m.TimestampUtc).Name("TimestampUtc");
            Map(m => m.Latitude).Name("Latitude");
            Map(m => m.Longitude).Name("Longitude");
            // ... additional mappings
        }
    }
}
```

**JsonParser.cs**:
```csharp
namespace AisToXmlRouteConvertor.Parsers;

using System.Text.Json;

public static class JsonParser
{
    /// <summary>
    /// Deserializes ship static JSON file.
    /// Returns null if file not found or malformed.
    /// </summary>
    public static ShipStaticData? ParseShipStatic(string jsonFilePath);
}
```

### 3.5 Export Folder

**Purpose**: Output file generation.

**Files**:
- `XmlExporter.cs`: Generates XML route files from waypoints

**Design Principles**:
- Use `System.Xml.Linq` for structured XML generation
- Proper XML declaration and encoding
- Handle null optional fields (omit or use defaults)
- Generate filename following pattern: `<MMSI>_<StartTimestamp>_<EndTimestamp>.xml`
- Create output folder if missing

**XmlExporter.cs**:
```csharp
namespace AisToXmlRouteConvertor.Export;

using System.Xml.Linq;

public static class XmlExporter
{
    /// <summary>
    /// Exports waypoints to XML file.
    /// </summary>
    /// <returns>Full path of created file</returns>
    public static string ExportToXml(
        IReadOnlyList<RouteWaypoint> waypoints,
        long mmsi,
        TimeInterval interval,
        string outputFolder);

    // Internal: Formats timestamp as yyyyMMddTHHmmssZ
    private static string FormatTimestamp(DateTime utc);

    // Internal: Creates waypoint XML element
    private static XElement CreateWaypointElement(RouteWaypoint waypoint);
}
```

### 3.6 Optimization Folder

**Purpose**: Track optimization algorithms.

**Files**:
- `TrackOptimizer.cs`: Reduces AIS positions to essential waypoints

**Design Principles**:
- Single-pass O(n) algorithm
- Always retain first and last positions
- Evaluate each position against last retained waypoint
- Include position if any threshold exceeded
- Clear, well-commented algorithm implementation

**TrackOptimizer.cs**:
```csharp
namespace AisToXmlRouteConvertor.Optimization;

public static class TrackOptimizer
{
    /// <summary>
    /// Optimizes track by retaining only significant positions.
    /// First and last positions always included.
    /// </summary>
    public static IReadOnlyList<RouteWaypoint> Optimize(
        IReadOnlyList<ShipState> states,
        TrackOptimizationParameters parameters);

    // Internal: Evaluates if position meets any threshold
    private static bool MeetsThreshold(
        ShipState current,
        ShipState last,
        TrackOptimizationParameters parameters);

    // Internal: Converts ShipState to RouteWaypoint
    private static RouteWaypoint ToWaypoint(ShipState state, int sequence);
}
```

### 3.7 Assets Folder

**Purpose**: Application resources (icons, styles).

**Files**:
- `Icons/app-icon.ico`: Application icon for window and taskbar
- `Styles/CustomStyles.axaml`: Optional custom styling (if needed)

**Design Principles**:
- Platform-appropriate icon formats
- Consistent visual styling
- Minimal custom styling (use Avalonia defaults)

## 4. Code Organization Principles

### 4.1 Namespace Structure

All namespaces follow the project folder structure:

```csharp
// Models
namespace AisToXmlRouteConvertor.Models;

// ViewModels
namespace AisToXmlRouteConvertor.ViewModels;

// Services
namespace AisToXmlRouteConvertor.Services;

// Parsers
namespace AisToXmlRouteConvertor.Parsers;

// Export
namespace AisToXmlRouteConvertor.Export;

// Optimization
namespace AisToXmlRouteConvertor.Optimization;
```

### 4.2 File Naming Conventions

- **Models**: `<ModelName>.cs` (e.g., `ShipStaticData.cs`)
- **ViewModels**: `<Feature>ViewModel.cs` (e.g., `MainViewModel.cs`)
- **Static Classes**: `<Purpose>.cs` (e.g., `Helper.cs`, `GeoMath.cs`)
- **Tests**: `<ClassUnderTest>Tests.cs` (e.g., `GeoMathTests.cs`)

### 4.3 Class Organization

Within each class file:

1. **Using directives** (sorted, system namespaces first)
2. **Namespace declaration** (file-scoped)
3. **XML documentation comment** (class summary)
4. **Class declaration** (public/internal/private, sealed when applicable)
5. **Fields** (private, readonly when possible)
6. **Constructors**
7. **Properties** (public first, then internal/private)
8. **Public methods**
9. **Internal methods**
10. **Private methods**

**Example**:
```csharp
using System;
using System.Collections.Generic;
using AisToXmlRouteConvertor.Models;

namespace AisToXmlRouteConvertor.Services;

/// <summary>
/// Geographic calculation utilities for AIS position processing.
/// </summary>
public static class GeoMath
{
    private const double EarthRadiusMeters = 6371000.0;

    /// <summary>
    /// Calculates distance between two points using Haversine formula.
    /// </summary>
    public static double HaversineDistance(
        double lat1, double lon1, 
        double lat2, double lon2)
    {
        // Implementation
    }

    /// <summary>
    /// Calculates initial bearing from point 1 to point 2.
    /// </summary>
    public static double InitialBearing(
        double lat1, double lon1, 
        double lat2, double lon2)
    {
        // Implementation
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
}
```

### 4.4 Dependency Flow

```
UI Layer (MainWindow.axaml)
    ↓
ViewModel Layer (MainViewModel.cs)
    ↓
Service Layer (Helper.cs)
    ↓
Domain/Parser/Optimizer Layer (CsvParser, TrackOptimizer, XmlExporter)
    ↓
Model Layer (ShipState, RouteWaypoint, etc.)
    ↓
Utility Layer (GeoMath.cs)
```

**Rules**:
- Higher layers depend on lower layers, never reverse
- Models have no dependencies on other layers
- Utilities depend only on models
- ViewModels never directly reference Parsers/Export (go through Helper)

### 4.5 Test Organization

**Unit Tests**:
- One test class per production class (e.g., `GeoMathTests.cs` for `GeoMath.cs`)
- Test methods named: `MethodName_Scenario_ExpectedOutcome`
- Arrange-Act-Assert pattern
- Use FluentAssertions for readable assertions

**Example**:
```csharp
namespace AisToXmlRouteConvertor.Tests.UnitTests;

using FluentAssertions;
using Xunit;

public class GeoMathTests
{
    [Fact]
    public void HaversineDistance_SamePoint_ReturnsZero()
    {
        // Arrange
        double lat = 51.5074;
        double lon = -0.1278;

        // Act
        double distance = GeoMath.HaversineDistance(lat, lon, lat, lon);

        // Assert
        distance.Should().Be(0.0);
    }

    [Fact]
    public void HaversineDistance_KnownDistance_ReturnsAccurateResult()
    {
        // Arrange: London to Paris (approx 344 km)
        double lat1 = 51.5074, lon1 = -0.1278;
        double lat2 = 48.8566, lon2 = 2.3522;

        // Act
        double distance = GeoMath.HaversineDistance(lat1, lon1, lat2, lon2);

        // Assert
        distance.Should().BeApproximately(344000, 1000); // Within 1km tolerance
    }
}
```

**Integration Tests**:
- Test complete workflows end-to-end
- Use realistic sample data
- Verify file outputs
- Clean up test artifacts in disposal

**Example**:
```csharp
namespace AisToXmlRouteConvertor.Tests.IntegrationTests;

public class EndToEndTests : IDisposable
{
    private readonly string _testOutputFolder;

    public EndToEndTests()
    {
        _testOutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testOutputFolder);
    }

    [Fact]
    public void ConvertAisToXml_ValidData_ProducesValidXmlFile()
    {
        // Arrange: Use sample data from TestData folder
        string vesselFolder = "TestData/205196000";
        var interval = new TimeInterval(
            new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 15, 12, 0, 0, DateTimeKind.Utc));

        // Act: Execute full pipeline
        var states = Helper.LoadShipStates(vesselFolder, interval);
        var waypoints = Helper.OptimizeTrack(states, new TrackOptimizationParameters());
        string outputPath = Helper.ExportRoute(waypoints, 205196000, interval, _testOutputFolder);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var xml = XDocument.Load(outputPath);
        xml.Root.Should().NotBeNull();
        xml.Root.Name.LocalName.Should().Be("RouteTemplate");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputFolder))
            Directory.Delete(_testOutputFolder, recursive: true);
    }
}
```

## 5. Project File Configuration

### 5.1 Main Project (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
    <PackageReference Include="CsvHelper" Version="30.0.1" />
  </ItemGroup>
</Project>
```

### 5.2 Test Project (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AisToXmlRouteConvertor\AisToXmlRouteConvertor.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="TestData\**\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

## 6. Build and Development Workflow

### 6.1 Initial Setup

```bash
# Clone repository
git clone <repository-url>
cd AISRouting

# Restore NuGet packages
dotnet restore src/AisToXmlRouteConvertor.sln

# Build solution
dotnet build src/AisToXmlRouteConvertor.sln -c Debug
```

### 6.2 Development Iteration

```bash
# Run application
dotnet run --project src/AisToXmlRouteConvertor/AisToXmlRouteConvertor.csproj

# Run tests with watch mode
dotnet watch test --project src/AisToXmlRouteConvertor.Tests/AisToXmlRouteConvertor.Tests.csproj

# Format code
dotnet format src/AisToXmlRouteConvertor.sln
```

### 6.3 Release Build

```bash
# Build release configuration
dotnet build src/AisToXmlRouteConvertor.sln -c Release

# Run tests
dotnet test src/AisToXmlRouteConvertor.Tests/AisToXmlRouteConvertor.Tests.csproj -c Release

# Publish self-contained for Windows
dotnet publish src/AisToXmlRouteConvertor/AisToXmlRouteConvertor.csproj `
  -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## 7. Code Quality Standards

### 7.1 StyleCop/Analyzer Rules

Enable .NET analyzers in project:

```xml
<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
</PropertyGroup>
```

### 7.2 .editorconfig

Create `.editorconfig` at solution root:

```ini
root = true

[*.cs]
# Indentation
indent_style = space
indent_size = 4

# New line preferences
end_of_line = crlf
insert_final_newline = true

# Nullable reference types
dotnet_diagnostic.CS8600.severity = error
dotnet_diagnostic.CS8602.severity = error

# Code style
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:suggestion
```

### 7.3 XML Documentation

All public types and members must have XML documentation:

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="paramName">What this parameter represents</param>
/// <returns>What the method returns</returns>
/// <exception cref="ExceptionType">When this exception is thrown</exception>
public static ReturnType MethodName(ParamType paramName)
{
    // Implementation
}
```

## 8. Adding New Features

### 8.1 Feature Addition Checklist

When adding a new feature (e.g., "Export to GPX format"):

1. **Model Changes** (if needed): Add new models to `Models/` folder
2. **Service Logic**: Add static method to `Helper.cs` or create new static class in `Services/`
3. **UI Update**: Add control to `MainWindow.axaml`, bind to ViewModel
4. **ViewModel Update**: Add properties/commands to `MainViewModel.cs`
5. **Tests**: Create unit tests in `UnitTests/`, integration test in `IntegrationTests/`
6. **Documentation**: Update user manual and technical docs

### 8.2 Example: Adding GPX Export

**Step 1**: Create model (optional, reuse RouteWaypoint)

**Step 2**: Add service
```csharp
// In Services/ folder
namespace AisToXmlRouteConvertor.Services;

public static class GpxExporter
{
    public static string ExportToGpx(
        IReadOnlyList<RouteWaypoint> waypoints,
        long mmsi,
        TimeInterval interval,
        string outputFolder)
    {
        // Implementation
    }
}
```

**Step 3**: Update Helper
```csharp
public static class Helper
{
    // Add new method
    public static string ExportRouteGpx(
        IReadOnlyList<RouteWaypoint> waypoints,
        long mmsi,
        TimeInterval interval,
        string outputFolder)
    {
        return GpxExporter.ExportToGpx(waypoints, mmsi, interval, outputFolder);
    }
}
```

**Step 4**: Update ViewModel
```csharp
public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private void ExportGpx()
    {
        if (OptimizedWaypoints == null || SelectedMmsi == null) return;
        
        string path = Helper.ExportRouteGpx(
            OptimizedWaypoints, 
            SelectedMmsi.Value, 
            TimeInterval, 
            OutputFolder);
        
        StatusMessage = $"GPX exported: {Path.GetFileName(path)}";
    }
}
```

**Step 5**: Update UI
```xml
<Button Content="Export GPX" Command="{Binding ExportGpxCommand}" />
```

**Step 6**: Add tests
```csharp
public class GpxExporterTests
{
    [Fact]
    public void ExportToGpx_ValidWaypoints_CreatesGpxFile()
    {
        // Test implementation
    }
}
```

## 9. Troubleshooting Common Issues

### 9.1 Build Errors

**Issue**: "Could not find Avalonia package"
**Solution**: Ensure NuGet sources configured, run `dotnet restore`

**Issue**: "CS8600 nullable reference type warning"
**Solution**: Add null checks or use nullable syntax (`string?`)

### 9.2 Runtime Errors

**Issue**: "FileNotFoundException" when loading CSV
**Solution**: Verify file path, check folder permissions

**Issue**: "CsvHelperException" during parsing
**Solution**: Check CSV format matches expected schema, add error logging

### 9.3 Test Failures

**Issue**: Integration test fails with file access error
**Solution**: Ensure test output folder cleanup in `Dispose()`, use temp paths

**Issue**: Unit test flaky on different machines
**Solution**: Avoid hardcoded paths, use relative paths or embedded resources

## 10. Summary

The AisToXmlRouteConvertor application organization prioritizes simplicity through a single-project structure with clear folder-based separation of concerns. Static helper methods eliminate dependency injection complexity, while MVVM with CommunityToolkit.Mvvm provides clean UI separation. The organized folder structure (Models, ViewModels, Services, Parsers, Export, Optimization) makes navigation intuitive and supports straightforward feature additions. Test organization mirrors production code structure, ensuring comprehensive coverage with minimal overhead.
