# Overall Architecture

This document covers the overall architecture and technology stack for the AisToXmlRouteConvertor application.

## 1. System Overview

AisToXmlRouteConvertor is a cross-platform desktop application built with .NET 9 and Avalonia UI that converts AIS (Automatic Identification System) position data into XML route files compatible with marine navigation systems. The application follows a simple, lean architecture optimized for local desktop use without unnecessary complexity.

## 2. Technology Stack

### Core Framework
- **Runtime**: .NET 9 (Target Framework Moniker: `net9.0`)
- **UI Framework**: Avalonia 11+ (cross-platform XAML-based UI)
- **Architecture Pattern**: MVVM (Model-View-ViewModel)
- **MVVM Toolkit**: CommunityToolkit.Mvvm (lightweight, source-generator-based)

### Libraries and Components
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Microsoft.Extensions.Logging with Console provider (optional Serilog for file logging)
- **CSV Parsing**: CsvHelper for robust, streaming CSV parsing
- **JSON Processing**: System.Text.Json (built-in .NET)
- **XML Generation**: System.Xml.Linq (LINQ to XML)
- **Testing**: xUnit, FluentAssertions, Microsoft.NET.Test.Sdk

### Target Platforms
- **Windows**: 10/11 (x64, ARM64)
- **macOS**: 13+ (x64, ARM64)
- **Linux**: x64 distributions

## 3. Architectural Principles

### Simplicity-First Design
The application deliberately avoids over-engineering:
- **No multi-project solution complexity**: Single Avalonia application project
- **No dependency injection for helpers**: Static helper methods for straightforward operations
- **Synchronous operations**: All I/O and processing are synchronous (no async/await) to keep code simple
- **Minimal abstractions**: Direct implementation without unnecessary interfaces or layers

### Data Flow
```
Input Folder → Scan MMSI Folders → Load Static Data (JSON only) → 
[User Selects Ship & Time] → 
[Process! Button Clicked] → Load & Filter CSV Files → 
Optimize Track → Generate XML → Output Folder
```

## 4. System Architecture

### High-Level Components

#### 4.1 User Interface Layer (Avalonia)
- **MainWindow.axaml**: Single window interface with folder browsers, ship selection, time pickers, and process button
- **ViewModels**: MainViewModel manages all UI state and orchestrates data operations
- **Commands**: RelayCommand instances for user interactions (Browse, Scan, Load, Optimize, Export)

#### 4.2 Domain Models
Core data structures representing:
- **ShipStaticData**: Vessel metadata from JSON files (MMSI, name, dimensions, date range)
- **ShipState**: Individual AIS position reports (timestamp, lat/lon, speed, course, heading, etc.)
- **RouteWaypoint**: Optimized waypoints for XML export (sequence, position, speed, heading, ETA)
- **TimeInterval**: User-selected time range for filtering
- **TrackOptimizationParameters**: Thresholds for track optimization

#### 4.3 Services (Static Helpers)
All services implemented as static methods in `Helper` class:
- **GetAvailableMmsi**: Scans input folder for MMSI subfolders (checks for CSV file **existence only**, does not load content)
- **LoadShipStatic**: Reads ship metadata from JSON (called when user selects a ship)
- **LoadShipStates**: Parses CSV files and filters by time interval (**only called when Process! button is clicked**)
- **OptimizeTrack**: Applies optimization algorithm to reduce waypoints
- **ExportRoute**: Generates XML file in output folder

#### 4.4 Parsers
- **CSV Parser**: Reads daily AIS position files (`YYYY-MM-DD.csv`) - **Only invoked when Process! button is clicked**
- **JSON Parser**: Deserializes ship static data from `<MMSI>.json` - **Loaded immediately when ship is selected**
- **CSV File Scanner**: Identifies available CSV files by filename during folder scan (does not load content)
- Error handling: Malformed rows are logged and skipped, not blocking entire load

**Important**: CSV files can be gigabytes in size. To avoid performance issues, CSV content is **never loaded during the initial scan**. Only filenames are read to determine available date ranges. Actual parsing occurs only after the user clicks Process! with a selected time interval.

#### 4.5 Track Optimizer
Algorithm implementation:
- Always includes first and last positions
- Evaluates each position against last retained waypoint using configurable thresholds:
  - Heading change (degrees)
  - Distance (Haversine meters)
  - Speed over ground change (knots)
  - Rate of turn (degrees/second)
- Retains only significant navigation changes

#### 4.6 XML Export
Generates route XML conforming to navigation system format:
- Root element: `<RouteTemplate Name="{MMSI}">`
- Waypoint elements with attributes: Seq, Lat, Lon, Speed, Heading, ETA
- Timestamps in UTC format: `yyyyMMdd'T'HHmmss'Z'`

## 5. Key Architectural Decisions

### Decision 1: Single Project Structure
**Rationale**: For a small utility application, multi-project solutions add overhead without benefit. All components live in organized folders within a single project.

### Decision 2: Synchronous Operations
**Rationale**: Desktop application processing local files doesn't benefit from async complexity. Synchronous calls simplify debugging and state management while adequate for expected data volumes.

### Decision 3: No Dependency Injection for Helpers
**Rationale**: Static helper methods reduce boilerplate for simple operations. UI calls helpers directly rather than managing service lifetimes.

### Decision 4: Avalonia over WPF/WinForms
**Rationale**: Cross-platform requirement eliminates Windows-only frameworks. Avalonia provides modern XAML with MVVM support across all target platforms.

### Decision 5: CommunityToolkit.Mvvm over ReactiveUI
**Rationale**: Simpler, more familiar API with source generators for boilerplate reduction. ReactiveUI's reactive programming model adds complexity not needed for this application.

## 6. Error Handling Strategy

### Input Validation
- Folder existence checks (`Directory.Exists`)
- MMSI subfolder validation (must contain at least one CSV file)
- Write permission verification for output folder
- Time interval validation (Start < End, within available data range)

### Processing Errors
- **CSV Parse Errors**: Log warning and skip row, continue processing
- **Missing Data**: Log info for missing optional fields, continue with nulls
- **File Access Errors**: Display error to user with specific details
- **Optimization Errors**: Capture and report in UI, no partial output

### User Feedback
- **Info**: Status messages for scan/load/optimize/export progress
- **Warnings**: Logged for skipped rows or missing optional data
- **Errors**: Modal dialogs with actionable messages (e.g., "Selected folder is not writable")

## 7. Performance Considerations

### Streaming CSV Parsing
Use CsvHelper's `GetRecords<T>()` for memory-efficient streaming parse of potentially large daily files.

### Date Range Filtering
Only load CSV files within the selected time interval (skip files outside range by filename pattern). CSV file content is **not loaded during folder scan** - only filenames are examined to determine available dates. Actual CSV parsing occurs only when the Process! button is clicked.

### Optimization Complexity
Track optimizer runs in O(n) time with single pass through chronological positions.

### Memory Management
No caching or persistence layer - all data held in memory during session only, released when window closes.

## 8. Logging Strategy

### Log Levels
- **Information**: 
  - Scan started/completed (vessel count)
  - Positions loaded (count, date range)
  - Waypoints produced (count, reduction percentage)
  - Export success (filename, path)
- **Warning**:
  - Skipped CSV row (line number, reason)
  - Missing optional fields
- **Error**:
  - File access denied (path, permission type)
  - Unhandled exceptions (full stack trace)

### Log Output
- Console logger for development
- Optional file logger (Serilog) for troubleshooting in production deployment

## 9. Extensibility Roadmap

### Phase 2 Enhancements (Not in Initial Scope)
- **Advanced Optimization**: Douglas-Peucker or Visvalingam-Whyatt algorithms
- **Batch Processing**: Multiple vessels, multiple time ranges
- **Map Preview**: Integrate Mapsui or Avalonia.Controls.Maps for visual route preview
- **Additional Formats**: GPX, KML, GeoJSON export options
- **UI Themes**: Dark mode, custom color schemes
- **Settings Persistence**: Remember last folders, optimization parameters

## 10. Non-Goals (Phase 1)

The following are explicitly out of scope for the initial release:
- Live AIS streaming or network integration
- Database persistence or caching
- Web or mobile interfaces
- Multi-user support or authentication
- Advanced map rendering with basemaps
- UI automation testing
- Plugin or extension system

## 11. Deployment Strategy

### Development Build
```bash
dotnet build src/AisToXmlRouteConvertor.csproj -c Debug
dotnet run --project src/AisToXmlRouteConvertor.csproj
```

### Release Build
```bash
dotnet build src/AisToXmlRouteConvertor.csproj -c Release
```

### Self-Contained Deployment
For distribution without requiring .NET runtime installation:
```bash
# Windows x64
dotnet publish src/AisToXmlRouteConvertor.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# macOS ARM64
dotnet publish src/AisToXmlRouteConvertor.csproj -c Release -r osx-arm64 --self-contained true

# Linux x64
dotnet publish src/AisToXmlRouteConvertor.csproj -c Release -r linux-x64 --self-contained true
```

### Packaging Options
- **Windows**: Installer via WiX Toolset or NSIS (future)
- **macOS**: .app bundle with code signing (future)
- **Linux**: .deb/.rpm packages or AppImage (future)

## 12. Build and Test Commands

### Restore Dependencies
```bash
dotnet restore src/AisToXmlRouteConvertor.sln
```

### Build Solution
```bash
dotnet build src/AisToXmlRouteConvertor.sln -c Release
```

### Run Application
```bash
dotnet run --project src/AisToXmlRouteConvertor/AisToXmlRouteConvertor.csproj
```

### Run Tests
```bash
dotnet test src/AisToXmlRouteConvertor.Tests/AisToXmlRouteConvertor.Tests.csproj
```

### Code Coverage (Optional)
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 13. Quality Attributes

### Usability
- Single-window interface minimizes navigation complexity
- Inline validation with immediate feedback
- Tooltips explain each control's purpose
- Clear error messages with actionable guidance

### Maintainability
- Simple folder structure, no deep nesting
- Static helpers easy to locate and modify
- Minimal dependencies reduce upgrade burden
- Inline comments for complex algorithms

### Portability
- .NET 9 provides consistent runtime across platforms
- Avalonia abstracts OS-specific UI differences
- No platform-specific APIs in core logic

### Testability
- Pure functions for optimization and calculations
- Static methods easily tested without mocking
- Sample test data included in repository

## 14. Development Tools

### Recommended IDEs
- **Visual Studio 2022** (17.8+): Full Avalonia designer support
- **JetBrains Rider**: Excellent .NET and Avalonia tooling
- **Visual Studio Code**: Lightweight, requires .NET SDK and extensions

### Required SDK
- **.NET 9 SDK**: [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)

### Useful Extensions/Plugins
- **Avalonia for Visual Studio**: XAML IntelliSense and previewer
- **AvaloniaRider**: Rider plugin for Avalonia development
- **GitHub Copilot**: AI-assisted coding (optional)

## 15. Summary

The AisToXmlRouteConvertor architecture prioritizes simplicity and directness over abstract patterns. A single-project Avalonia application with static helper methods and synchronous operations provides a maintainable foundation for converting AIS position data to navigation-compatible XML routes. The design supports cross-platform deployment while keeping complexity minimal, with clear pathways for future enhancements when needed.
