# Overall Architecture and Technology Stack

This document covers the overall system architecture, technology stack decisions, and key architectural patterns for the AISRouting desktop application.

## System Overview

AISRouting is a Windows desktop application built with .NET 8 and WPF that processes AIS (Automatic Identification System) vessel position data, generates optimized route tracks, and exports them in XML format compatible with navigation systems.

## Technology Stack

### Framework and Runtime
- **Framework**: .NET 8 (net8.0)
- **Target Platform**: Windows 10/11
- **UI Framework**: WPF (Windows Presentation Foundation) with XAML
- **Language**: C# 12

### Core Libraries
- **MVVM Toolkit**: CommunityToolkit.Mvvm (Microsoft.Toolkit.Mvvm) for observable objects, commands, and messaging
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging with file-based sink (Serilog or Microsoft.Extensions.Logging.File)

### Data Processing
- **CSV Parsing**: CsvHelper (NuGet) for robust CSV parsing with type mapping
- **JSON Serialization**: System.Text.Json (built-in with .NET 8)
- **XML Generation**: System.Xml for route export

### Testing
- **Unit Testing**: NUnit with NUnit3TestAdapter
- **Test Platform**: Microsoft.NET.Test.Sdk
- **Mocking**: Moq or NSubstitute
- **Assertions**: FluentAssertions

### Optional Enhancement Packages
- **Folder Dialogs**: Ookii.Dialogs.Wpf for modern folder browser
- **Map Visualization**: Mapsui (future enhancement for track preview)

## System Architecture

### Layered Architecture

The application follows a three-layer architecture with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│    Presentation Layer (WPF)            │
│  - Views (XAML)                         │
│  - ViewModels (MVVM)                    │
│  - Commands & Data Binding              │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│    Business Logic Layer (Core)          │
│  - Domain Models                         │
│  - Service Interfaces                    │
│  - Business Rules & Validation           │
│  - Track Optimization Algorithms         │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│    Data Access Layer (Infrastructure)   │
│  - File I/O Operations                   │
│  - CSV/JSON Parsing                      │
│  - XML Export                            │
│  - Data Persistence                      │
└─────────────────────────────────────────┘
```

### Solution Structure

```
d:\repo\AISRouting
├── src
│   ├── AISRouting.sln
│   ├── AISRouting.App.WPF/           # Presentation layer
│   │   ├── App.xaml                   # Application entry point & DI setup
│   │   ├── MainWindow.xaml            # Main UI
│   │   ├── Views/                     # XAML view files
│   │   ├── ViewModels/                # MVVM ViewModels
│   │   └── Resources/                 # Styles, templates, assets
│   ├── AISRouting.Core/               # Business logic layer
│   │   ├── Models/                    # Domain entities
│   │   └── Services/                  # Business logic interfaces & implementations
│   ├── AISRouting.Infrastructure/     # Data access layer
│   │   ├── IO/                        # File system operations
│   │   └── Persistence/               # Data serialization
│   └── AISRouting.Tests/              # Test project
│       └── UnitTests/
├── docs/                              # Documentation
└── route_waypoint_template.xml        # XML template for export
```

## Key Architectural Patterns

### MVVM (Model-View-ViewModel)

**Views (XAML)**
- Pure UI layout and visual structure
- Minimal code-behind (limited to view-specific concerns like focus management)
- Data binding to ViewModels
- Example: MainWindow.xaml displays folder selection, vessel combo, and track results

**ViewModels**
- Expose properties for data binding (implement INotifyPropertyChanged via ObservableObject)
- Define commands for user actions (RelayCommand from CommunityToolkit.Mvvm)
- Orchestrate service calls
- Handle validation and error messages
- Example: MainViewModel coordinates folder selection, vessel selection, time range, track creation

**Models**
- Domain entities with business logic
- No UI dependencies
- Examples: ShipStaticData, ShipDataOut, RouteWaypoint, TimeInterval

### Dependency Injection

All services registered in `App.xaml.cs` using `IServiceCollection`:

```csharp
services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
services.AddSingleton<IShipStaticDataLoader, ShipStaticDataLoader>();
services.AddSingleton<IShipPositionLoader, ShipPositionLoader>();
services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
services.AddSingleton<IRouteExporter, RouteExporter>();
services.AddSingleton<IFolderDialogService, FolderDialogService>();
services.AddTransient<MainViewModel>();
```

ViewModels resolved via DI container or ViewModelLocator pattern.

### Service Layer Pattern

Business logic encapsulated in service interfaces:

- **ISourceDataScanner**: Discover vessels and date ranges from folder structure
- **IShipStaticDataLoader**: Load and parse vessel static data (JSON)
- **IShipPositionLoader**: Load and filter position data (CSV) by time interval
- **ITrackOptimizer**: Apply optimization algorithms to generate waypoints
- **IRouteExporter**: Serialize waypoints to XML format
- **IFolderDialogService**: Abstract folder selection dialogs for testability

## Data Flow Architecture

### Application Startup Flow

1. App.xaml.cs initializes DI container with all services
2. MainWindow created and MainViewModel injected
3. UI data bindings established
4. Application ready for user interaction

### Folder Selection Flow

```
User selects input folder
    ↓
IFolderDialogService shows dialog
    ↓
ISourceDataScanner enumerates MMSI subfolders
    ↓
For each folder:
    - IShipStaticDataLoader reads <MMSI>.json (if exists)
    - Extract min/max dates from CSV filenames
    ↓
ShipStaticData collection bound to vessel combo box
```

### Track Generation Flow

```
User selects vessel and time interval
    ↓
User clicks "Create Track"
    ↓
IShipPositionLoader.LoadPositions(mmsi, timeInterval)
    - Identifies CSV files in date range
    - Loads and filters position records
    ↓
ITrackOptimizer.OptimizeTrack(positions)
    - Applies deviation detection algorithms
    - Generates optimized waypoints
    ↓
Waypoints displayed in UI
    ↓
User clicks "Export"
    ↓
IRouteExporter.ExportToXml(waypoints, outputFolder, mmsi)
    - Generates filename: MMSI-StartDate-EndDate.xml
    - Handles file conflicts (overwrite/suffix/cancel)
    - Writes XML with RouteTemplate and WayPoint elements
```

## Key Architectural Decisions

### Decision 1: WPF with MVVM

**Rationale**: 
- Native Windows UI framework with mature tooling
- Strong separation of concerns via MVVM
- Excellent data binding capabilities
- Rich control library for desktop applications
- Good performance for data-heavy scenarios

**Alternatives Considered**: 
- WinForms: Less modern, weaker binding support
- Electron: Overkill for desktop-only app, larger footprint
- Blazor Hybrid: Less mature for desktop scenarios

### Decision 2: Pure .NET Stack (No External Services)

**Rationale**:
- Self-contained desktop application
- No network dependencies or external APIs
- All data processing local to user's machine
- Simplifies deployment and security

**Trade-offs**:
- Cannot offload heavy processing to server
- All data must fit in local storage
- Future cloud features require architecture changes

### Decision 3: File-Based Storage

**Rationale**:
- Input data already organized as files (CSV/JSON)
- No complex relational queries required
- Simple folder structure easy to understand
- Users retain full control of data
- No database setup or maintenance

**Future Enhancement**: 
- SQLite caching for indexing large datasets
- Faster repeated queries on multi-year data

### Decision 4: CsvHelper for CSV Parsing

**Rationale**:
- Robust handling of CSV edge cases (quotes, delimiters, encoding)
- Type mapping and nullable support
- Streaming mode for large files
- Well-maintained, widely adopted

**Alternative**: Custom parser for simpler scenarios

### Decision 5: System.Text.Json

**Rationale**:
- Built-in with .NET 8, no extra dependencies
- High performance
- Modern API with source generators
- Good support for nullable reference types

**Alternative**: Newtonsoft.Json if specific features needed

## Cross-Cutting Concerns

### Error Handling

- Service methods throw specific exceptions (FileNotFoundException, FormatException, InvalidOperationException)
- ViewModels catch exceptions and display user-friendly messages
- All exceptions logged with context (MMSI, filename, operation)
- Malformed CSV rows logged and skipped (not fatal)

### Logging

- Microsoft.Extensions.Logging throughout all layers
- Log levels:
  - **Info**: Folder selected, vessel count, track generation start/complete, export success
  - **Warning**: Malformed CSV rows skipped, missing JSON fields, default values applied
  - **Error**: File access failures, parsing exceptions, export failures
- File-based sink for diagnostics
- Include context: MMSI, filename, row number, timestamps

### Performance Considerations

- **Async/await**: All I/O operations async to keep UI responsive
- **Lazy loading**: Position data loaded only when needed (Create Track)
- **Streaming**: CSV parsed in streaming mode for large files
- **Cancellation**: CancellationToken support for long operations
- **Progress reporting**: IProgress<T> for folder scan and track generation
- **Multi-day optimization**: Load only CSV files within selected time interval

### Threading Model

- UI thread for WPF rendering and user interaction
- Background threads (via Task.Run or async I/O) for:
  - Folder scanning
  - File loading (CSV/JSON)
  - Track optimization algorithms
  - XML export
- ViewModel commands use async/await to prevent UI blocking
- Progress updates marshaled to UI thread via data binding

## Build and Deployment

### Build Process

```bash
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test

# Publish self-contained
dotnet publish src/AISRouting.App.WPF -c Release -r win-x64 --self-contained
```

### CI/CD

- GitHub Actions or Azure Pipelines
- Windows-latest runner
- Steps: restore → build → test → publish
- Artifacts: published executable and dependencies

### Deployment

- Self-contained executable (includes .NET runtime)
- Copy published files to target machine
- Include `route_waypoint_template.xml` in application root
- No installation required

## Testing Strategy

### Unit Tests
- Services tested in isolation with mocked dependencies
- ViewModel logic tested with mocked services
- Optimization algorithms tested with known input/output pairs
- CSV/JSON parsing tested with sample files

### Integration Tests
- End-to-end flows with test data folders
- Multi-day track generation spanning CSV files
- XML export and validation

### UI Tests (Optional)
- WinAppDriver or FlaUI for smoke tests
- Happy path workflow automation

## Security Considerations

### Input Validation
- Folder paths validated (exists, readable, correct structure)
- MMSI format validated (9-digit number per AIS spec)
- CSV/JSON parsing handles malformed data gracefully
- All nullable fields in ShipDataOut handled
- Date format validation for CSV filenames

### File System Security
- Output folder creation with permission checks
- Write validation before export
- Error messages indicate filesystem issues
- No sensitive data stored or transmitted

### Future Enhancements
- User role-based export permissions (if multi-user scenarios)
- Data encryption for saved routes (if required)

## Extension Points

### Future Capabilities
- **Map visualization**: Integrate Mapsui for track preview
- **Batch processing**: Process multiple vessels with single configuration
- **Threshold tuning**: UI controls for optimization parameters
- **Alternative algorithms**: Douglas-Peucker, Visvalingam-Whyatt
- **Export formats**: GPX, KML, GeoJSON
- **Statistics dashboard**: Optimization metrics, track length
- **Destination lookup**: Resolve DestinationIndex to port names
- **NavigationalStatus decoding**: Human-readable status display

## References

- AIS Message Formats: https://www.navcen.uscg.gov/ais-messages
- AIS Sample Data: http://aisdata.ais.dk/
- WPF Documentation: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/
- CommunityToolkit.Mvvm: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
