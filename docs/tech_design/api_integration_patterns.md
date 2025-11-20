# API Integration Patterns

This document covers API design principles, integration patterns with external systems, data flow between services, error handling strategies, and performance considerations for the AisToXmlRouteConvertor application.

## 1. Overview

AisToXmlRouteConvertor is a standalone desktop application that operates entirely offline with no network communication or external API integration. This document addresses the internal "API" design patterns used within the application's architecture, focusing on the interfaces between UI, service layer, parsers, and file system operations. While there are no external REST APIs or web services, the document establishes patterns for potential future integration scenarios.

## 2. Internal API Design Principles

### 2.1 Static Helper API Pattern

**Philosophy**: Simplicity over abstraction for a small utility application.

**Design**:
- All service operations exposed as static methods in `Helper` class
- No dependency injection for core operations
- Synchronous execution model (no async/await complexity)
- Clear method signatures with explicit parameters and return types
- Comprehensive XML documentation for IntelliSense support

**Benefits**:
- Zero setup overhead (no DI container configuration)
- Easy to test (pure functions, no mocking required)
- Simple to understand and maintain
- No lifecycle management concerns

**Trade-offs**:
- Less flexible for future mocking needs (acceptable for current scope)
- Global state not possible (not needed for stateless operations)
- Cannot leverage DI features like scoped lifetimes (not applicable)

### 2.2 API Surface Design

**Helper Class Interface**:
```csharp
namespace AisToXmlRouteConvertor.Services;

/// <summary>
/// Lightweight static helper methods for core AIS processing operations.
/// All methods are synchronous and callers wait for results.
/// </summary>
public static class Helper
{
    /// <summary>
    /// Scans the root folder for available MMSI vessel subfolders.
    /// </summary>
    /// <param name="rootPath">Root folder containing MMSI subfolders</param>
    /// <returns>List of discovered MMSI identifiers</returns>
    /// <exception cref="DirectoryNotFoundException">Root path does not exist</exception>
    /// <exception cref="UnauthorizedAccessException">No read permission for folder</exception>
    public static IReadOnlyList<long> GetAvailableMmsi(string rootPath);

    /// <summary>
    /// Loads ship static data from vessel folder's JSON file.
    /// </summary>
    /// <param name="vesselFolder">Path to MMSI subfolder (e.g., "input/205196000")</param>
    /// <returns>Ship static data or null if file not found/malformed</returns>
    /// <exception cref="InvalidOperationException">JSON file too large or invalid format</exception>
    public static ShipStaticData? LoadShipStatic(string vesselFolder);

    /// <summary>
    /// Loads and filters AIS position states for a vessel within time interval.
    /// </summary>
    /// <param name="vesselFolder">Path to MMSI subfolder</param>
    /// <param name="interval">Time range for filtering positions</param>
    /// <returns>Chronologically sorted list of position states</returns>
    /// <exception cref="InvalidOperationException">CSV files too large or exceed record limit</exception>
    /// <exception cref="ApplicationException">General load failure (check inner exception)</exception>
    public static IReadOnlyList<ShipState> LoadShipStates(
        string vesselFolder, 
        TimeInterval interval);

    /// <summary>
    /// Optimizes AIS track by reducing positions to significant waypoints.
    /// </summary>
    /// <param name="states">Chronologically ordered position states</param>
    /// <param name="parameters">Optimization threshold parameters</param>
    /// <returns>Optimized list of route waypoints with sequential numbering</returns>
    public static IReadOnlyList<RouteWaypoint> OptimizeTrack(
        IReadOnlyList<ShipState> states, 
        TrackOptimizationParameters parameters);

    /// <summary>
    /// Exports route waypoints to XML file in navigation system format.
    /// </summary>
    /// <param name="waypoints">List of route waypoints to export</param>
    /// <param name="mmsi">Vessel MMSI identifier</param>
    /// <param name="interval">Time interval covered by route</param>
    /// <param name="outputFolder">Destination folder for XML file</param>
    /// <returns>Full path of created XML file</returns>
    /// <exception cref="UnauthorizedAccessException">Output folder not writable</exception>
    /// <exception cref="InvalidOperationException">Waypoint list empty or invalid</exception>
    public static string ExportRoute(
        IReadOnlyList<RouteWaypoint> waypoints, 
        long mmsi, 
        TimeInterval interval, 
        string outputFolder);
}
```

### 2.3 API Contract Principles

**Input Validation**:
- All public methods validate parameters before processing
- Throw `ArgumentException` or derived types for invalid arguments
- Null checks for reference types (leverage nullable reference types in C# 9+)

**Return Types**:
- Use immutable types (`IReadOnlyList<T>`, `record` types)
- Return null for "not found" scenarios (explicit nullable types)
- Return empty collections (not null) for "no results" scenarios

**Error Handling**:
- Throw specific exception types for different error categories
- Include actionable error messages
- Preserve inner exceptions for debugging
- Log detailed errors, surface user-friendly messages

**Documentation**:
- XML documentation on all public methods
- Include parameter descriptions, return value descriptions, exception types
- Provide usage examples in summary section

## 3. Data Flow Between Components

### 3.1 Component Interaction Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         UI Layer (Avalonia)                      │
│                                                                  │
│  MainWindow.axaml  ←→  MainViewModel (MVVM)                    │
│                         ├─ ObservableProperties                 │
│                         └─ RelayCommands                         │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      │ Calls static methods
                      ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Service Layer (Static)                        │
│                                                                  │
│  Helper.cs                                                       │
│  ├─ GetAvailableMmsi(rootPath)                                 │
│  ├─ LoadShipStatic(vesselFolder)                               │
│  ├─ LoadShipStates(vesselFolder, interval)                     │
│  ├─ OptimizeTrack(states, parameters)                          │
│  └─ ExportRoute(waypoints, mmsi, interval, outputFolder)       │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      │ Delegates to specialized components
                      ↓
┌─────────────────────────────────────────────────────────────────┐
│              Parser/Optimizer/Exporter Layer                     │
│                                                                  │
│  CsvParser.cs          JsonParser.cs        TrackOptimizer.cs   │
│  ├─ ParsePositions     ├─ ParseShipStatic  ├─ Optimize         │
│  └─ (streaming)        └─ (deserialize)    └─ (algorithm)      │
│                                                                  │
│  XmlExporter.cs        GeoMath.cs                               │
│  ├─ ExportToXml        ├─ HaversineDistance                    │
│  └─ (LINQ to XML)      └─ InitialBearing                       │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      │ File I/O operations
                      ↓
┌─────────────────────────────────────────────────────────────────┐
│                      File System Layer                           │
│                                                                  │
│  Input:  <RootFolder>/<MMSI>/                                  │
│          ├─ <MMSI>.json                                        │
│          └─ YYYY-MM-DD.csv                                     │
│                                                                  │
│  Output: <OutputFolder>/                                        │
│          └─ <MMSI>_<Start>_<End>.xml                           │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Data Flow Sequence

**Scenario: User processes AIS data for vessel "Alice's Container Ship" (MMSI 205196000)**

**Step 1: Folder Scan**
```
User Action: Clicks "Browse" and selects input folder
↓
UI: Calls MainViewModel.BrowseInputFolderCommand
↓
ViewModel: Sets InputFolder property → calls Helper.GetAvailableMmsi(inputFolder)
↓
Helper: Scans folder for numeric subfolders with CSV files
       Returns: [205196000, 123456000, 987654000]
↓
ViewModel: Sets AvailableMmsi property → triggers UI update
↓
UI: Populates ship table with MMSI rows
```

**Step 2: Ship Selection**
```
User Action: Selects MMSI 205196000 row in table
↓
UI: Sets MainViewModel.SelectedShip property
↓
ViewModel: Calls Helper.LoadShipStatic("input/205196000")
↓
Helper: Delegates to JsonParser.ParseShipStatic("input/205196000/205196000.json")
       Returns: ShipStaticData(Mmsi=205196000, Name="Alice's Container Ship", ...)
↓
ViewModel: Updates SelectedShip property → triggers UI update
↓
UI: Displays ship details, enables time pickers with min/max dates
```

**Step 3: Load Positions**
```
User Action: Sets time interval (2025-03-15 00:00 to 12:00) and clicks "Process!"
↓
UI: Triggers MainViewModel.ProcessCommand
↓
ViewModel: Calls Helper.LoadShipStates(
               "input/205196000",
               new TimeInterval(start: 2025-03-15T00:00:00Z, end: 2025-03-15T12:00:00Z))
↓
Helper: 1. Identifies CSV files in date range (2025-03-15.csv)
        2. Calls CsvParser.ParsePositions("input/205196000/2025-03-15.csv")
        3. Filters positions by timestamp within interval
        4. Returns: List<ShipState> (e.g., 1,234 positions)
↓
ViewModel: Stores positions in memory
```

**Step 4: Optimize Track**
```
ViewModel: Calls Helper.OptimizeTrack(positions, OptimizationParameters)
↓
Helper: Delegates to TrackOptimizer.Optimize(positions, parameters)
↓
TrackOptimizer: 1. Iterates through positions chronologically
                2. Evaluates each against last retained waypoint
                3. Applies thresholds (heading, distance, speed, ROT)
                4. Converts retained positions to RouteWaypoint records
                5. Returns: List<RouteWaypoint> (e.g., 87 waypoints)
↓
ViewModel: Stores waypoints in memory
```

**Step 5: Export XML**
```
ViewModel: Calls Helper.ExportRoute(
               waypoints,
               mmsi: 205196000,
               interval,
               "output")
↓
Helper: Delegates to XmlExporter.ExportToXml(waypoints, mmsi, interval, "output")
↓
XmlExporter: 1. Generates filename: "205196000_20250315T000000_20250315T120000.xml"
             2. Creates XML structure using LINQ to XML
             3. Writes file to output folder
             4. Returns: "output/205196000_20250315T000000_20250315T120000.xml"
↓
ViewModel: Displays success dialog with filename
↓
UI: Shows modal: "Track generated successfully: 205196000_20250315T000000_20250315T120000.xml"
```

### 3.3 Error Propagation Flow

**Scenario: CSV file is malformed**

```
Helper.LoadShipStates() calls CsvParser.ParsePositions()
↓
CsvParser encounters invalid row (latitude out of range)
↓
CsvParser logs warning: "Invalid position at line 142 in 2025-03-15.csv"
↓
CsvParser skips row, continues parsing
↓
CsvParser returns partial list (valid rows only)
↓
Helper returns partial list to ViewModel
↓
ViewModel displays info: "Loaded 1,233 positions (1 row skipped due to errors)"
```

**Scenario: Output folder not writable**

```
Helper.ExportRoute() checks folder writability
↓
Write test fails (permission denied)
↓
Helper throws UnauthorizedAccessException("Selected folder is not writable. Choose a different folder.")
↓
ViewModel catches exception in try/catch
↓
ViewModel displays error dialog: "Error: Selected folder is not writable. Choose a different folder."
↓
UI remains in ready state (user can select different folder)
```

## 4. Integration Patterns (Internal Components)

### 4.1 File System Integration Pattern

**Pattern**: Direct file system access with validation

**Current Implementation**:
```csharp
public static IReadOnlyList<long> GetAvailableMmsi(string rootPath)
{
    if (!Directory.Exists(rootPath))
        throw new DirectoryNotFoundException($"Input folder not found: {rootPath}");

    var mmsiList = new List<long>();
    foreach (var subfolder in Directory.GetDirectories(rootPath))
    {
        string folderName = Path.GetFileName(subfolder);
        if (long.TryParse(folderName, out long mmsi) && 
            mmsi >= 100000000 && mmsi <= 999999999)
        {
            // Check for at least one CSV file
            if (Directory.GetFiles(subfolder, "*.csv").Length > 0)
            {
                mmsiList.Add(mmsi);
            }
        }
    }

    return mmsiList;
}
```

### 4.2 CSV Parsing Integration Pattern

**Pattern**: Streaming iterator with error tolerance

**Benefits**:
- Memory-efficient (doesn't load entire file into memory)
- Error-resilient (skips malformed rows, continues processing)
- Progress reporting (optional, for large files)

### 4.3 Optimization Algorithm Integration Pattern

**Pattern**: Strategy pattern (configurable via parameters)

**Track Optimization**:
- Always includes first and last positions
- Evaluates intermediate positions against thresholds
- Returns optimized waypoint list

### 4.4 XML Export Integration Pattern

**Pattern**: Builder pattern with LINQ to XML

**Benefits**:
- Type-safe XML construction
- Automatic escaping of attribute values
- Readable, declarative syntax

## 5. Error Handling Strategies

### 5.1 Exception Hierarchy

**Three-Layer Approach**:

1. **Internal Layer**: Throw specific exceptions with full details
2. **Service Layer**: Catch, log, rethrow with context
3. **UI Layer**: Display user-friendly messages, offer recovery

### 5.2 Error Handling Layers

**Layer 1: Low-Level (Parsers/Utilities)**
- Catch specific exceptions
- Log detailed error
- Throw custom exception with user-friendly message

**Layer 2: Service (Helper Methods)**
- Add context
- Re-throw with enhanced message
- Provide recovery suggestions

**Layer 3: ViewModel**
- Display user-friendly error
- Reset UI state
- Never show stack traces to user

## 6. Performance Considerations

### 6.1 Memory Management

**Streaming CSV Parsing**:
- Process rows one at a time
- Release position list after optimization
- Maximum position limit: 1 million records

### 6.2 File I/O Optimization

**Date Range Filtering**:
- Only load CSV files within time interval
- Skip files outside date range by filename

### 6.3 Algorithm Performance

**Track Optimization Complexity**:
- Time: O(n) - single pass
- Space: O(m) - m waypoints retained

**Haversine Distance**:
- Minimal trigonometric operations
- Cached constants

### 6.4 UI Responsiveness

**Synchronous Operations Rationale**:
- Expected processing time < 5 seconds
- Blocking UI provides clear feedback
- Simplicity over async complexity

## 7. Future External API Integration (Phase 2+)

### 7.1 Potential Integration Scenarios

**Not in Current Scope**:

1. **AIS Data Streaming API**: Real-time AIS data from maritime tracking services
2. **Weather Data API**: Weather forecast integration for route planning
3. **Chart/Map Tile API**: Interactive map display
4. **Cloud Storage API**: Sync routes to cloud storage

### 7.2 REST API Integration Pattern (Future)

**Hypothetical Implementation**:
- HttpClient-based API clients
- Rate limiting and throttling
- Retry logic with exponential backoff
- Fallback to local files

## 8. Summary

The AisToXmlRouteConvertor internal API design prioritizes simplicity through static helper methods with clear, synchronous interfaces. Data flows linearly from UI through service layer to specialized parsers/optimizers/exporters, with comprehensive error handling at each layer. While the application currently has no external API integrations (operating entirely offline), the architecture establishes patterns for file I/O, data parsing, track optimization, and XML export that could accommodate future enhancements. Performance optimizations include streaming CSV parsing, date range filtering, and O(n) track optimization algorithms. Error handling uses a three-layer approach with user-friendly messaging and recovery guidance.
