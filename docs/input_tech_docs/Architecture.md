# Overall Architecture and Technology Stack (WPF / .NET)

This document describes the AISRouting Windows desktop application implemented with .NET and WPF using a native .NET technology stack and patterns.

## Goals
- Process pre-processed AIS "Source Data" organized in MMSI-based folder structure
- Read ship static data (JSON) and daily position reports (CSV) per vessel
- Enable user selection of vessel, input/output folders, and time interval
- Generate optimized route tracks based on deviation analysis (heading, ROT, position, speed)
- Export routes in XML format compatible with navigation systems
- Desktop Windows UI (WPF) with responsive MVVM architecture
- Pure .NET stack (NuGet packages allowed) and dotnet CLI-based build + test

## References
- AIS message formats: https://www.navcen.uscg.gov/ais-messages
- AIS sample data source: http://aisdata.ais.dk/
- Route waypoint XML template: `route_waypoint_template.xml` (project root)

## Target Platform
- OS: Windows 10 / 11
- Runtime: .NET 8 (or latest supported .NET) with WPF (Windows-only)
- Tooling: Visual Studio 2022/2023, VS Code optional, dotnet CLI

## Solution Layout
```
d:\repo\AISRouting
├── src
│   ├── AISRouting.sln
│   ├── AISRouting.App.WPF/           # WPF application (UI)
│   │   ├── AISRouting.App.WPF.csproj
│   │   ├── App.xaml
│   │   ├── MainWindow.xaml
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   └── Resources/
│   ├── AISRouting.Core/               # Domain model + interfaces + business logic
│   │   ├── AISRouting.Core.csproj
│   │   ├── Models/                    # Vessel, PositionReport, Route, Waypoint, Voyage
│   │   └── Services/                  # Routing/aggregation/validation
│   ├── AISRouting.Infrastructure/     # I/O, CSV parsing, persistence
│   │   ├── AISRouting.Infrastructure.csproj
│   │   ├── IO/                        # CsvReader, CsvMapper
│   │   └── Persistence/               # FileStorage, ProjectSerializer (JSON)
│   └── AISRouting.Tests/              # Unit and integration tests
│       ├── AISRouting.Tests.csproj
│       └── UnitTests/
└── docs/
```

## Technology Stack (dotnet ecosystem)
- Framework: .NET 8 (TFM: net8.0)
- UI: WPF (XAML) with MVVM
- MVVM Toolkit: Microsoft.Toolkit.Mvvm (CommunityToolkit.Mvvm) or Prism (both available as NuGet)
- Dependency Injection / Configuration / Logging: Microsoft.Extensions.* packages
- CSV parsing: CsvHelper (NuGet) or custom parser (pure .NET)
- Serialization: System.Text.Json
- Persistence: Local file storage (JSON for saved routes); optionally SQLite via Microsoft.Data.Sqlite / EF Core if relational persistence required
- Unit Testing: NUnit + Microsoft.NET.Test.Sdk
- Mocking: Moq (NuGet) or NSubstitute
- UI Automation / E2E (optional): WinAppDriver (Microsoft) or FlaUI (community .NET)
- Build & CI: dotnet CLI, GitHub Actions or Azure Pipelines

## Project-level Decisions
- Pattern: MVVM for separation of UI and business logic.
- Single solution with multiple projects (App, Core, Infrastructure, Tests).
- App project references Core and Infrastructure.
- Services registered via DI in App startup (IServiceCollection).
- ViewModels constructed by DI (either via ViewModelLocator pattern, factory or code-behind resolving from ServiceProvider).

## Folder / Component Responsibilities
- Views (XAML): UI layout only, minimal code-behind limited to view concerns (keyboard focus, navigation).
- ViewModels: Commands, observable properties (INotifyPropertyChanged), validation, invoking services.
- Models: Domain objects aligned with AIS data structure:
  - **ShipStaticData**: vessel metadata from `<MMSI>.json` (MMSI, name, dimensions, min/max date range)
  - **ShipDataOut**: position report from CSV (Time, Lat/Lon, NavigationalStatusIndex, ROT, SOG, COG, Heading, Draught, DestinationIndex, EtaSecondsUntil)
  - **RouteWaypoint**: optimized waypoint for XML export
  - **TimeInterval**: user-selected start/stop times (seconds resolution)
- Services:
  - **ISourceDataScanner**: scan input folder structure, discover all vessels (MMSI folders), extract date ranges from CSV filenames
  - **IShipStaticDataLoader**: load and parse `<MMSI>.json` files
  - **IShipPositionLoader**: load daily CSV files for selected vessel and time interval, parse into ShipDataOut records
  - **ITrackOptimizer**: analyze position data, detect significant deviations (heading, ROT, position history, speed), generate optimized waypoints
  - **IRouteExporter**: serialize optimized route to XML format per `route_waypoint_template.xml`
  - **IFolderDialogService**: abstraction for folder browser dialogs (input/output)
- Infrastructure:
  - **SourceDataReader**: file system operations for MMSI folder enumeration and CSV/JSON file discovery
  - **CsvParser**: parse daily CSV files into ShipDataOut records (CsvHelper or custom)
  - **JsonParser**: deserialize ship static JSON files (System.Text.Json)
  - **XmlRouteWriter**: generate route XML per template specification

## Data Flow

### Application Startup / Folder Selection
1. User selects **input folder** (Source Data root) via folder selector
2. ISourceDataScanner enumerates vessel subfolders
3. Vessel subfolders displayed in combo box
4. For each vessel subfolder:
   - IShipStaticDataLoader reads static data file if present
   - If static file provides name, use it; otherwise use folder name
   - Scanner extracts min/max dates from CSV filenames
5. ShipStaticData collection populated and bound to vessel combo box

### Vessel Selection
1. User selects vessel from combo box
2. Ship static data displayed in large TextBox widget
   - Shows static attributes when available
   - Falls back to folder name or defaults for missing fields
3. Min/Max date pickers populated with earliest/latest timestamps from CSV files
4. Time interval defaults set:
   - **StartValue**: timestamp from first filename in vessel folder
   - **StopValue**: timestamp from last filename + 24 hours
5. Time interval controls enabled for user adjustment (second resolution)

### Track Generation (Create Track Button)
1. User configures:
   - Vessel (from combo box)
   - Time interval (start/stop with seconds resolution)
2. User clicks **Create Track**
3. ViewModel invokes:
   - IShipPositionLoader.LoadPositions(mmsi, timeInterval) → loads relevant CSV files in selected interval, filters by time
   - ITrackOptimizer.OptimizeTrack(positions) → applies default optimization parameters:
     - Heading change: 0.2 degrees
     - Distance: 5 meters
     - SOG threshold: 0.2 knots
     - ROT threshold: 0.2 deg/s
4. Generated track displayed as list of waypoints in UI
5. User reviews points for continuity and expected vessel behavior

### Route Export
1. User clicks **Export** button from generated track view
2. User selects output folder:
   - Application validates path is writable
   - Creates folder if it doesn't exist
   - Displays selected path in UI
   - Shows error if creation or write fails
3. Filename generated: `MMSINumber-StartDate-EndDate.xml` (UTC, YYYYMMDDTHHMMSS)
4. If file exists, prompt user: Overwrite / Append suffix / Cancel
5. IRouteExporter.ExportToXml(waypoints, outputFolder, mmsi) writes:
   - Single `<RouteTemplate Name="{MMSI}">` element
   - Ordered list of `<WayPoint .../>` elements with attribute mappings
   - Metadata from route_waypoint_template.xml applied
6. Success notification displayed to user

## Key UX Screens / Workflow

### Main Window (Single Window Application)
**Top Section - Folder Selection**
- **Input Folder** selector: select Source Data root folder containing vessel subfolders
  - Vessel subfolders are populated in combo box after selection
  - Expected folder structure: root contains vessel subfolders with CSV files

**Middle Section - Vessel Selection**
- **Ship Selection Combo Box**: populated from vessel subfolders
  - Display: ship name from static file if available, otherwise folder name
  - Selection triggers display of static data
- **Ship Static Data TextBox** (large widget): displays selected vessel details
  - Shows static attributes when available
  - Falls back to folder name or defaults for missing fields
  - Displays Min/Max date range from CSV filenames

**Time Interval Selection**
- **Min/Max Date Pickers**: show earliest/latest timestamps from vessel's CSV files
- **Start Time Picker**: second resolution, defaults to timestamp from first filename
- **Stop Time Picker**: second resolution, defaults to timestamp from last filename + 24 hours
- Selected time range defines which AIS records generate waypoints
- Validation: ensure start < stop

**Bottom Section - Action**
- **Create Track** button: triggers track generation with default optimization parameters
- Generated track displays as list of points in UI
- **Export** button: exports route to XML
  - Output folder selector with path creation and validation
  - Displays selected output path in UI
  - Shows error if folder creation or write fails

**Track Results View**
- List of generated waypoints for review
- Continuity validation recommended before export

## Track Optimization Algorithms

### Significant Deviation Detection
Track optimization reduces redundant position reports by identifying significant deviations from previous course. A waypoint is retained if ANY of the following criteria indicate significant change:

**Default Optimization Parameters** (implemented in current release):
- **Minimum heading change**: 0.2 degrees
- **Minimum distance between points**: 5 meters
- **SOG change threshold**: 0.2 knots
- **ROT threshold**: 0.2 deg/s
- **Max allowed time gap**: not enforced in current release

**1. Heading Change**
- Threshold: > 0.2 degrees change from previous waypoint heading
- Detects course alterations

**2. Rate of Turn (ROT) Change**
- Threshold: ROT exceeds 0.2 deg/s
- Detects turning maneuvers

**3. Position History Deviation**
- Algorithm: Distance-based filtering
- Threshold: > 5 meters from previous waypoint
- Detects significant position changes

**4. Speed (SOG) Change**
- Threshold: > 0.2 knots change from previous waypoint speed
- Detects acceleration/deceleration events

### Implementation Strategy
- Process position reports chronologically
- Maintain sliding window (last 2-3 waypoints) for deviation calculation
- Retain first position, last position, and all positions meeting any significant deviation criteria
- Configurable thresholds via application settings or UI

### Supporting Algorithms
- **Haversine formula**: geodesic distance calculation between lat/lon points
- **Bearing calculation**: initial bearing between two geographic points (for heading validation)
- **Time interpolation**: convert Time (seconds from T0) to absolute timestamps

## Data Formats

### Input Formats

**Source Data Folder Structure**
```
<InputFolder>/
  ├── <MMSI1>/
  │   ├── <MMSI1>.json          # Ship static data
  │   ├── 2024-01-01.csv         # Daily position reports
  │   ├── 2024-01-02.csv
  │   └── ...
  ├── <MMSI2>/
  │   ├── <MMSI2>.json
  │   ├── 2024-01-01.csv
  │   └── ...
```

**Ship Static Data JSON** (`<MMSI>.json`)
- MMSI (long)
- Vessel name (string)
- Dimensions: length, beam, draught (optional)
- Type code (optional)
- Call sign (optional)
- IMO number (optional)

**Daily CSV Format** (`YYYY-MM-DD.csv`)
```
ShipDataOut fields (header row required):
- Time: long (seconds since T0 = date at 00:00:00 UTC)
- Latitude: double (decimal degrees)
- Longitude: double (decimal degrees)
- NavigationalStatusIndex: int (0-15 per AIS spec)
- ROT: double (rate of turn, degrees/minute)
- SOG: double (speed over ground, knots)
- COG: double (course over ground, degrees)
- Heading: int (true heading, degrees)
- Draught: double (meters)
- DestinationIndex: int (lookup index)
- EtaSecondsUntil: long (seconds until ETA from current time)
```

### Output Format

**Export Filename Pattern**
- Format: `MMSINumber-StartDate-EndDate.xml`
- Dates in UTC formatted as `YYYYMMDDTHHMMSS`
- Example: `205196000-20250315T000000-20250316T000000.xml`

**File Conflict Handling**
- If file exists, prompt user to choose:
  - Overwrite existing file
  - Append numeric suffix to filename
  - Cancel export operation

**Route XML Structure** (per `route_waypoint_template.xml`)
- Single `<RouteTemplate Name="{MMSI}">` root element
- Ordered list of `<WayPoint .../>` elements

**WayPoint Attribute Mapping** (from AIS CSV records):
- **Name**: MMSI (string)
- **Lat**: CSV latitude value (decimal degrees)
- **Lon**: CSV longitude value (decimal degrees)
- **Alt**: 0 (fixed)
- **Speed**: SOG from CSV (no unit conversion in current release)
- **ETA**: EtaSecondsUntil from CSV if provided; otherwise 0
- **Delay**: 0 (fixed)
- **Mode**: computed via SetWaypointMode (implementation-defined)
- **TrackMode**: "Track" (fixed)
- **Heading**: Heading from CSV or 0 if missing
- **PortXTE**: 20 (fixed)
- **StbdXTE**: 20 (fixed)
- **MinSpeed**: 0 (fixed)
- **MaxSpeed**: maximum SOG observed in selected range (zeros ignored)

**Output Folder Behavior**
- If output path does not exist, application creates it
- Path validation ensures writability
- Selected path displayed in UI
- Error shown if creation or write fails

**Missing Data Handling**
- Missing Heading or SOG values default to 0 in WayPoint fields
- Consider preprocessing CSV to fill missing values for better results

## Testing Strategy

### Unit Tests (NUnit)
**Data Loading**
- SourceDataScanner: correct MMSI folder enumeration, date range extraction from filenames
- ShipStaticDataLoader: JSON deserialization, handling missing/malformed fields
- ShipPositionLoader: CSV parsing, time filtering, multi-day file aggregation
- Time conversion: seconds-from-T0 to absolute DateTime, handling date boundaries

**Track Optimization**
- Deviation detection algorithms: heading change, ROT threshold, position deviation, speed change
- Edge cases: single position, all positions identical, missing fields (nullable handling)
- Waypoint retention logic: first/last always retained, intermediate points per criteria
- Known test tracks with expected waypoint outputs

**XML Export**
- RouteExporter: XML structure matches template, proper escaping, encoding
- Round-trip validation: generate XML, validate against schema/template

**ViewModel Logic**
- Command enable/disable states (folder selected, vessel selected, valid time interval)
- Time interval validation (start < stop, within date range)
- Error handling and user notification

### Integration Tests
- End-to-end: point to test data folder, select vessel, generate route, verify XML output
- Multi-day track: time interval spanning multiple CSV files
- Large dataset: performance test with thousands of position reports

### UI Tests (Optional)
- WinAppDriver or FlaUI: folder selection, vessel dropdown population, create track button interaction
- Smoke tests for happy path workflow

## Build & Run
- dotnet CLI:
  - Build solution: dotnet build
  - Run app (from App project): dotnet run --project src\AISRouting.App.WPF
  - Test: dotnet test
- CI: GitHub Actions using windows-latest runner with dotnet/setup-dotnet, run build & tests.

## Security & Performance Considerations

### Input Validation
- Validate folder paths: exist, readable, expected structure (MMSI folders, JSON + CSV files)
- CSV/JSON parsing: handle malformed data gracefully, log errors, skip invalid rows
- MMSI format: validate as 9-digit number (AIS spec)
- Date format: validate CSV filename format (YYYY-MM-DD.csv)
- Nullable fields: all ShipDataOut fields are nullable per spec, handle missing data

### Performance
- **Lazy loading**: don't load position data until vessel selected and Create Track clicked
- **Streaming CSV parsing**: use CsvHelper streaming mode for large daily files (avoid full load into memory)
- **Multi-day aggregation**: load only CSV files within selected time interval, not entire date range
- **Cancellation support**: all long-running operations (folder scan, CSV load, optimization) support CancellationToken
- **Progress reporting**: IProgress<T> for folder scan and track generation
- **Async/await**: all I/O and processing operations async to keep UI responsive

### Logging & Diagnostics
- Microsoft.Extensions.Logging throughout (file-based sink recommended)
- Log levels:
  - Info: folder selected, vessel count, track generation start/complete
  - Warning: malformed CSV rows skipped, missing JSON fields
  - Error: file access failures, parsing exceptions
- Include context: MMSI, filename, row number for diagnostics

## Troubleshooting Common Issues

### No CSV Files Detected
- **Cause**: Input root folder or vessel subfolders missing CSV files
- **Resolution**: 
  - Verify input root folder structure
  - Ensure vessel subfolders contain CSV files
  - Verify filenames or contents include numeric epoch timestamps in first column

### Export Fails Due to Permission or Path
- **Cause**: Output path not writable or insufficient permissions
- **Resolution**:
  - Verify selected output path is writable
  - If folder creation fails, choose different folder
  - Run application with sufficient permissions
  - Error message indicates underlying filesystem problem

### Missing Heading or SOG Values
- **Cause**: CSV rows missing required fields
- **Resolution**:
  - WayPoint fields default to 0 for missing values
  - Consider preprocessing CSV to fill missing values
  - Review data quality before export

### Poor Track Quality
- **Cause**: Noisy AIS data or wide time window
- **Resolution**:
  - Narrow time window to improve results
  - Run track creation on smaller ranges
  - Inspect intermediate data
  - Keep backups of generated exports

## Extensions / Future Work
- **Map visualization**: integrate Mapsui or similar .NET mapping control for track preview
- **Batch processing**: process multiple vessels in sequence with single configuration
- **Deviation threshold tuning**: UI controls for heading/ROT/speed/position thresholds
- **Alternative algorithms**: Douglas-Peucker, Visvalingam-Whyatt for comparison
- **Export formats**: GPX, KML, GeoJSON in addition to XML
- **Statistics dashboard**: optimization metrics, track length, time savings
- **SQLite caching**: index large datasets for faster repeated queries
- **Destination lookup**: resolve DestinationIndex to port names via lookup table
- **NavigationalStatus decoding**: display human-readable status (underway, at anchor, etc.)

## NuGet Package Recommendations

**Required**
- **CommunityToolkit.Mvvm**: MVVM framework (ObservableObject, RelayCommand, etc.)
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **Microsoft.Extensions.Logging**: logging abstraction
- **Microsoft.Extensions.Logging.File** (or Serilog.Extensions.Logging.File): file-based logging

**Recommended**
- **CsvHelper**: robust CSV parsing with type mapping
- **System.Text.Json**: built-in, included with .NET 8

**Testing**
- **NUnit**: test framework
- **NUnit3TestAdapter**: VS test integration
- **Microsoft.NET.Test.Sdk**: test platform
- **Moq** or **NSubstitute**: mocking framework
- **FluentAssertions**: readable assertions

**Optional**
- **Ookii.Dialogs.Wpf**: modern folder browser dialog (better than WinForms FolderBrowserDialog)
- **Mapsui**: .NET mapping library for track visualization

## Next Steps
This architecture document establishes the foundation for the AISRouting WPF application. The following artifacts should be created next:

1. **Technical Design Document**: detailed class diagrams, service interfaces, data models (ShipStaticData, ShipDataOut records)
2. **XML Route Template Specification**: document structure of route_waypoint_template.xml for IRouteExporter implementation
3. **Sample Data Files**: example `<MMSI>.json` and `YYYY-MM-DD.csv` for testing and reference
4. **DI Configuration**: App.xaml.cs service registration, ViewModel factory pattern
5. **Implementation Plan**: task breakdown, development phases, testing milestones