# Data Models

This document covers the data structures, storage implementation, and data relationships for the AisToXmlRouteConvertor application.

## 1. Overview

The application uses strongly-typed C# records for immutable data representation. No database or persistent storage is used - all data is loaded from files, processed in memory, and exported to XML. This design keeps the application simple and stateless.

## 2. Core Domain Models

### 2.1 ShipStaticData

Represents vessel metadata loaded from `<MMSI>.json` files.

```csharp
/// <summary>
/// Static information about a vessel including identity and physical characteristics.
/// Loaded from {MMSI}.json in each vessel's subfolder.
/// </summary>
public sealed record ShipStaticData(
    long Mmsi,                    // Maritime Mobile Service Identity (unique vessel ID)
    string? Name,                 // Vessel name (e.g., "Alice's Cargo Ship")
    double? Length,               // Overall length in meters (e.g., 225.5)
    double? Beam,                 // Width/breadth in meters (e.g., 32.2)
    double? Draught,              // Current draught in meters (e.g., 12.8)
    string? CallSign,             // Radio call sign (e.g., "ONBZ")
    string? ImoNumber,            // International Maritime Organization number (e.g., "IMO1234567")
    DateTime? MinDateUtc,         // Earliest available AIS data timestamp
    DateTime? MaxDateUtc          // Latest available AIS data timestamp
);
```

**Example Data:**
```json
{
  "Mmsi": 205196000,
  "Name": "Alice's Container Ship",
  "Length": 285.0,
  "Beam": 40.0,
  "Draught": 14.5,
  "CallSign": "ONBZ",
  "ImoNumber": "IMO9234567",
  "MinDateUtc": "2025-03-10T00:00:00Z",
  "MaxDateUtc": "2025-03-20T23:59:59Z"
}
```

**Validation Rules:**
- `Mmsi` must be positive 9-digit number (100000000 - 999999999)
- Optional fields may be null if not present in source data
- `MinDateUtc` must be before `MaxDateUtc` when both present

**Usage Scenarios:**
- **Positive**: Display vessel name and dimensions in ship selection table
- **Negative**: Handle missing optional fields gracefully (display "N/A" or empty)
- **Edge**: MMSI folder exists but JSON is malformed - log error, skip vessel

### 2.2 ShipState

Represents a single AIS position report from CSV files.

```csharp
/// <summary>
/// A single AIS position report capturing vessel state at a specific timestamp.
/// Parsed from YYYY-MM-DD.csv files.
/// </summary>
public sealed record ShipState(
    DateTime TimestampUtc,              // Position report timestamp in UTC
    double Latitude,                    // Latitude in decimal degrees (-90 to 90)
    double Longitude,                   // Longitude in decimal degrees (-180 to 180)
    int? NavigationalStatusIndex,       // AIS navigational status code (0-15)
    double? RotDegPerMin,               // Rate of turn in degrees per minute
    double? SogKnots,                   // Speed over ground in knots
    double? CogDegrees,                 // Course over ground in degrees (0-360)
    int? Heading,                       // True heading in degrees (0-360)
    double? DraughtMeters,              // Current draught in meters
    int? DestinationIndex,              // Destination code (application-specific)
    long? EtaSecondsUntil               // Estimated time to destination in seconds
);
```

**Example CSV Data:**
```csv
TimestampUtc,Latitude,Longitude,NavigationalStatusIndex,RotDegPerMin,SogKnots,CogDegrees,Heading,DraughtMeters,DestinationIndex,EtaSecondsUntil
2025-03-15T08:30:00Z,51.2345,-3.4567,0,0.5,12.3,135.0,135,14.2,5,3600
2025-03-15T08:31:00Z,51.2350,-3.4560,0,0.3,12.4,135.5,136,14.2,5,3540
```

**Validation Rules:**
- `TimestampUtc` must be valid UTC datetime
- `Latitude` must be in range [-90, 90]
- `Longitude` must be in range [-180, 180]
- Optional numeric fields may be null (missing sensor data)
- Invalid rows logged and skipped, not blocking entire load

**Usage Scenarios:**
- **Positive**: Load all positions within user-selected time interval for vessel "Alice's Container Ship" on 2025-03-15
- **Negative**: CSV row has malformed latitude - log warning, skip row, continue parsing
- **Edge**: Multiple positions with identical timestamp - keep all (may represent duplicate transmissions)

### 2.3 TimeInterval

Represents the user-selected time range for filtering AIS data.

```csharp
/// <summary>
/// Time interval for filtering AIS position data.
/// User selects start and end times in the UI.
/// </summary>
public sealed record TimeInterval(
    DateTime StartUtc,    // Beginning of interval (inclusive)
    DateTime EndUtc       // End of interval (inclusive)
);
```

**Example Data:**
```csharp
var interval = new TimeInterval(
    StartUtc: new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc),
    EndUtc: new DateTime(2025, 3, 15, 12, 0, 0, DateTimeKind.Utc)
);
```

**Validation Rules:**
- `StartUtc < EndUtc` (start must be before end)
- Both timestamps must be within vessel's available data range (`MinDateUtc` to `MaxDateUtc`)

**Usage Scenarios:**
- **Positive**: User selects 8-hour interval within available data range
- **Negative**: User sets end time before start time - UI shows error "Start time must be before End time"
- **Edge**: User sets start time before vessel's `MinDateUtc` - UI shows error "Selected time is outside available data range"

### 2.4 RouteWaypoint

Represents an optimized navigation waypoint for XML export.

```csharp
/// <summary>
/// A single waypoint in the optimized route.
/// Generated by track optimization algorithm from ShipState records.
/// </summary>
public sealed record RouteWaypoint(
    int Sequence,             // Waypoint sequence number (1-based)
    double Latitude,          // Waypoint latitude in decimal degrees
    double Longitude,         // Waypoint longitude in decimal degrees
    double? SpeedKnots,       // Recommended speed in knots (from SOG)
    int? Heading,             // True heading in degrees (0-360)
    DateTime? EtaUtc          // Estimated time of arrival at waypoint
);
```

**Example Data:**
```csharp
var waypoint = new RouteWaypoint(
    Sequence: 1,
    Latitude: 51.2345,
    Longitude: -3.4567,
    SpeedKnots: 12.3,
    Heading: 135,
    EtaUtc: new DateTime(2025, 3, 15, 8, 30, 0, DateTimeKind.Utc)
);
```

**XML Representation:**
```xml
<WayPoint Seq="1" Lat="51.2345" Lon="-3.4567" Speed="12.3" Heading="135" ETA="20250315T083000Z" />
```

**Generation Rules:**
- `Sequence` starts at 1 and increments for each waypoint
- First and last positions always included
- Intermediate points included only if they meet optimization thresholds
- Optional fields omitted or set to 0 in XML if null

**Usage Scenarios:**
- **Positive**: Optimize 1000 AIS positions down to 50 significant waypoints representing course changes
- **Negative**: All positions identical (vessel stationary) - keep only first and last
- **Edge**: Single position in time range - create single waypoint

### 2.5 TrackOptimizationParameters

Configurable thresholds for the track optimization algorithm.

```csharp
/// <summary>
/// Parameters controlling which positions are retained as waypoints.
/// Higher thresholds produce fewer waypoints (more aggressive optimization).
/// </summary>
public sealed record TrackOptimizationParameters(
    double MinHeadingChangeDeg = 0.2,       // Minimum heading change in degrees
    double MinDistanceMeters = 5,           // Minimum distance between waypoints in meters
    double MinSogChangeKnots = 0.2,         // Minimum speed change in knots
    double RotThresholdDegPerSec = 0.2      // Rate of turn threshold in degrees/second
);
```

**Default Values:**
```csharp
var defaultParams = new TrackOptimizationParameters();
// MinHeadingChangeDeg: 0.2
// MinDistanceMeters: 5
// MinSogChangeKnots: 0.2
// RotThresholdDegPerSec: 0.2
```

**Usage Scenarios:**
- **Positive**: Use default thresholds for typical vessel route optimization
- **Negative**: Set all thresholds to 0 - retains all positions (no optimization)
- **Edge**: Set very high thresholds - retains only first and last positions

## 3. Data Relationships

### 3.1 Relationship Diagram

```
┌─────────────────────┐
│  Input Folder       │
│  (File System)      │
└──────────┬──────────┘
           │
           ├─── MMSI Subfolder (205196000)
           │    │
           │    ├─── 205196000.json ────> ShipStaticData
           │    │
           │    └─── YYYY-MM-DD.csv ────> List<ShipState>
           │
           └─── MMSI Subfolder (123456000)
                ...

┌─────────────────────┐
│  User Input (UI)    │
└──────────┬──────────┘
           │
           ├─── Selected MMSI ────────────┐
           │                              │
           └─── TimeInterval              │
                                          │
                                          ▼
                              ┌──────────────────────┐
                              │  ShipState List      │
                              │  (filtered by time)  │
                              └──────────┬───────────┘
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │  TrackOptimization   │
                              │  Parameters          │
                              └──────────┬───────────┘
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │  RouteWaypoint List  │
                              └──────────┬───────────┘
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │  Output XML File     │
                              │  (File System)       │
                              └─────────────────────┘
```

### 3.2 Data Flow Narrative

**Step 1: Folder Scan**
- User selects input folder containing MMSI subfolders
- Application scans for numeric folder names (9-digit MMSI patterns)
- For each MMSI folder, checks for `<MMSI>.json` and at least one `.csv` file
- Returns list of available MMSI numbers

**Step 2: Static Data Load**
- User selects an MMSI from the list (e.g., 205196000 - "Alice's Container Ship")
- Application reads `205196000.json` and deserializes into `ShipStaticData`
- Displays vessel name, dimensions, and available date range in UI

**Step 3: Position Data Load**
- User specifies `TimeInterval` (e.g., 2025-03-15 00:00 to 12:00)
- Application identifies relevant CSV files by filename date pattern
- Parses each CSV file, deserializes rows into `ShipState` records
- Filters positions where `TimestampUtc` falls within `TimeInterval`
- Returns chronologically sorted list of `ShipState` records

**Step 4: Track Optimization**
- Application applies optimization algorithm using `TrackOptimizationParameters`
- Iterates through `ShipState` list, evaluating each position against last retained waypoint
- Positions meeting any threshold criterion are converted to `RouteWaypoint` records
- First and last positions always included
- Returns list of `RouteWaypoint` records with sequential numbering

**Step 5: XML Export**
- Application generates XML structure with `<RouteTemplate Name="205196000">`
- Each `RouteWaypoint` becomes a `<WayPoint>` element with attributes
- Timestamps formatted as `yyyyMMdd'T'HHmmss'Z'`
- File saved as `205196000_20250315T000000_20250315T120000.xml` in output folder

## 4. Storage Implementation

### 4.1 File System Structure

```
<InputRootFolder>/
├── 205196000/                          # MMSI subfolder (Alice's Container Ship)
│   ├── 205196000.json                  # Ship static data
│   ├── 2025-03-10.csv                  # Daily AIS positions
│   ├── 2025-03-11.csv
│   ├── ...
│   └── 2025-03-20.csv
│
├── 123456000/                          # Another vessel (Bob's Tanker)
│   ├── 123456000.json
│   ├── 2025-03-12.csv
│   └── ...
│
└── 987654000/                          # Another vessel (Carol's Ferry)
    ├── 987654000.json
    └── ...

<OutputFolder>/
├── 205196000_20250315T000000_20250315T120000.xml
├── 123456000_20250313T060000_20250313T180000.xml
└── ...
```

### 4.2 JSON File Format (ShipStaticData)

**File**: `<MMSI>.json`

**Schema**:
```json
{
  "Mmsi": <long>,
  "Name": <string|null>,
  "Length": <double|null>,
  "Beam": <double|null>,
  "Draught": <double|null>,
  "CallSign": <string|null>,
  "ImoNumber": <string|null>,
  "MinDateUtc": <ISO8601 datetime|null>,
  "MaxDateUtc": <ISO8601 datetime|null>
}
```

**Example**:
```json
{
  "Mmsi": 205196000,
  "Name": "Alice's Container Ship",
  "Length": 285.0,
  "Beam": 40.0,
  "Draught": 14.5,
  "CallSign": "ONBZ",
  "ImoNumber": "IMO9234567",
  "MinDateUtc": "2025-03-10T00:00:00Z",
  "MaxDateUtc": "2025-03-20T23:59:59Z"
}
```

### 4.3 CSV File Format (ShipState)

**File**: `YYYY-MM-DD.csv` (e.g., `2025-03-15.csv`)

**Schema**:
```
TimestampUtc,Latitude,Longitude,NavigationalStatusIndex,RotDegPerMin,SogKnots,CogDegrees,Heading,DraughtMeters,DestinationIndex,EtaSecondsUntil
```

**Example**:
```csv
TimestampUtc,Latitude,Longitude,NavigationalStatusIndex,RotDegPerMin,SogKnots,CogDegrees,Heading,DraughtMeters,DestinationIndex,EtaSecondsUntil
2025-03-15T00:00:00Z,51.5074,-0.1278,0,0.0,0.0,0.0,0,14.5,0,0
2025-03-15T00:01:00Z,51.5075,-0.1277,0,0.1,1.2,45.0,45,14.5,1,300
2025-03-15T00:02:00Z,51.5076,-0.1275,0,0.2,2.5,47.0,47,14.5,1,240
```

### 4.4 XML Output Format (RouteWaypoint)

**File**: `<MMSI>_<StartTimestamp>_<EndTimestamp>.xml`

**Example**: `205196000_20250315T000000_20250315T120000.xml`

**Schema**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RouteTemplate Name="{MMSI}">
  <WayPoint Seq="{int}" Lat="{double}" Lon="{double}" Speed="{double|0}" Heading="{int|0}" ETA="{yyyyMMddTHHmmssZ|omit}" />
  ...
</RouteTemplate>
```

**Example**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RouteTemplate Name="205196000">
  <WayPoint Seq="1" Lat="51.5074" Lon="-0.1278" Speed="0" Heading="0" ETA="20250315T000000Z" />
  <WayPoint Seq="2" Lat="51.5123" Lon="-0.1156" Speed="12.5" Heading="45" ETA="20250315T003000Z" />
  <WayPoint Seq="3" Lat="51.5245" Lon="-0.0923" Speed="13.2" Heading="48" ETA="20250315T010000Z" />
</RouteTemplate>
```

## 5. Data Validation Rules

### 5.1 Input Validation

**ShipStaticData (JSON)**
- `Mmsi`: Required, 9-digit positive integer
- Optional fields: Accept null or missing, display "N/A" in UI
- Date range: `MinDateUtc < MaxDateUtc` if both present

**ShipState (CSV)**
- `TimestampUtc`: Required, valid UTC datetime
- `Latitude`: Required, range [-90, 90]
- `Longitude`: Required, range [-180, 180]
- Optional fields: Accept null or empty, skip if unparseable
- Invalid row: Log warning, skip, continue parsing

**TimeInterval (User Input)**
- `StartUtc < EndUtc`: Display error "Start time must be before End time"
- Within vessel range: `StartUtc >= MinDateUtc && EndUtc <= MaxDateUtc`, else error "Selected time is outside available data range"

### 5.2 Processing Validation

**Track Optimization**
- Empty input: Return empty list
- Single position: Return single waypoint
- All identical positions: Return first position only as waypoint

**XML Export**
- Empty waypoint list: Show error "No waypoints to export"
- Output folder not writable: Show error "Selected folder is not writable. Choose a different folder."
- File exists: Overwrite with confirmation (future enhancement)

## 6. Data Type Mapping

### 6.1 CSV to ShipState

| CSV Column | C# Property | Type | Conversion |
|------------|-------------|------|------------|
| TimestampUtc | TimestampUtc | DateTime | `DateTime.Parse(..., DateTimeStyles.AssumeUniversal)` |
| Latitude | Latitude | double | `double.Parse(...)` |
| Longitude | Longitude | double | `double.Parse(...)` |
| NavigationalStatusIndex | NavigationalStatusIndex | int? | `int.TryParse(...)` |
| RotDegPerMin | RotDegPerMin | double? | `double.TryParse(...)` |
| SogKnots | SogKnots | double? | `double.TryParse(...)` |
| CogDegrees | CogDegrees | double? | `double.TryParse(...)` |
| Heading | Heading | int? | `int.TryParse(...)` |
| DraughtMeters | DraughtMeters | double? | `double.TryParse(...)` |
| DestinationIndex | DestinationIndex | int? | `int.TryParse(...)` |
| EtaSecondsUntil | EtaSecondsUntil | long? | `long.TryParse(...)` |

### 6.2 ShipState to RouteWaypoint

| ShipState Property | RouteWaypoint Property | Transformation |
|--------------------|------------------------|----------------|
| (sequence counter) | Sequence | Incremental 1-based index |
| Latitude | Latitude | Direct copy |
| Longitude | Longitude | Direct copy |
| SogKnots | SpeedKnots | Direct copy (nullable) |
| Heading | Heading | Direct copy (nullable) |
| TimestampUtc + EtaSecondsUntil | EtaUtc | `TimestampUtc.AddSeconds(EtaSecondsUntil)` if not null |

### 6.3 RouteWaypoint to XML Attributes

| RouteWaypoint Property | XML Attribute | Format |
|------------------------|---------------|--------|
| Sequence | Seq | Integer |
| Latitude | Lat | Double (full precision) |
| Longitude | Lon | Double (full precision) |
| SpeedKnots | Speed | Double or "0" if null |
| Heading | Heading | Integer or "0" if null |
| EtaUtc | ETA | `yyyyMMddTHHmmssZ` or omitted if null |

## 7. Edge Cases and Error Scenarios

### 7.1 Missing or Malformed Data

| Scenario | Handling |
|----------|----------|
| MMSI folder missing JSON | Skip vessel, log warning "Missing static data for MMSI {mmsi}" |
| MMSI folder has no CSV files | Mark vessel as disabled in UI, show tooltip "No AIS data available" |
| CSV row with invalid latitude/longitude | Skip row, log warning "Invalid position at line {line}" |
| JSON with missing optional fields | Load with nulls, display "N/A" in UI |
| CSV with extra unknown columns | Ignore extra columns, parse known fields |
| CSV with missing required columns | Skip entire file, log error "Missing required columns in {filename}" |

### 7.2 Boundary Conditions

| Scenario | Handling |
|----------|----------|
| Time interval with no matching positions | Show info "No positions found in selected time range" |
| Time interval with single position | Create single waypoint |
| Optimization reduces to only first/last | Valid result, inform user "{original} positions optimized to 2 waypoints" |
| Vessel at anchor (all identical positions) | Keep only first position as single waypoint |
| Positions with identical timestamps | Keep all, may represent duplicate AIS transmissions |
| Antimeridian crossing (longitude wraps) | Haversine distance calculation handles correctly |
| Polar regions (high latitude) | Haversine distance accurate for all latitudes |

### 7.3 Permission and I/O Errors

| Scenario | Handling |
|----------|----------|
| Input folder not accessible | Show error "Cannot access selected folder: {reason}" |
| Output folder not writable | Show error "Selected folder is not writable. Choose a different folder." |
| Disk full during export | Show error "Failed to write XML file: Insufficient disk space" |
| File locked by another process | Show error "Cannot write {filename}: File is in use" |

## 8. Summary

The AisToXmlRouteConvertor data model uses simple, immutable C# records for type safety and clarity. All data flows from file system inputs through in-memory processing to XML file outputs without persistence. The design handles realistic vessel names like "Alice's Container Ship" and includes comprehensive validation for both positive usage paths and negative error scenarios. Optional fields accommodate incomplete AIS data, while strict validation of required fields ensures data integrity throughout the conversion pipeline.
