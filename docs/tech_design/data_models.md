# Data Models and Storage Structure

This document covers the data structures, domain models, storage implementation, and data relationships in the AISRouting application.

## Domain Models

### ShipStaticData

Represents vessel metadata loaded from `<MMSI>.json` files.

**Properties:**
```csharp
public class ShipStaticData
{
    public long MMSI { get; set; }                    // 9-digit Maritime Mobile Service Identity
    public string? Name { get; set; }                 // Vessel name (nullable, fallback to folder name)
    public double? Length { get; set; }               // Length in meters
    public double? Beam { get; set; }                 // Beam (width) in meters
    public double? Draught { get; set; }              // Draught in meters
    public int? TypeCode { get; set; }                // AIS vessel type code
    public string? CallSign { get; set; }             // Radio call sign
    public long? IMO { get; set; }                    // IMO number
    public DateTime MinDate { get; set; }             // Earliest timestamp from CSV files
    public DateTime MaxDate { get; set; }             // Latest timestamp from CSV files
    public string FolderPath { get; set; }            // Full path to vessel folder
}
```

**Usage:**
- Populated during folder scanning by ISourceDataScanner and IShipStaticDataLoader
- Bound to vessel selection combo box
- Displayed in ship static data TextBox widget
- Used to determine available date range for time interval selection

**Validation Rules:**
- MMSI must be positive 9-digit number
- Name defaults to folder name if missing from JSON
- MinDate/MaxDate extracted from CSV filenames (format: YYYY-MM-DD.csv)
- All other fields nullable per AIS specification

### ShipDataOut

Represents a single position report from daily CSV files.

**Properties:**
```csharp
public class ShipDataOut
{
    public long Time { get; set; }                         // Seconds since T0 (date at 00:00:00 UTC)
    public double? Latitude { get; set; }                  // Decimal degrees
    public double? Longitude { get; set; }                 // Decimal degrees
    public int? NavigationalStatusIndex { get; set; }      // 0-15 per AIS specification
    public double? ROT { get; set; }                       // Rate of turn (degrees/minute)
    public double? SOG { get; set; }                       // Speed over ground (knots)
    public double? COG { get; set; }                       // Course over ground (degrees)
    public int? Heading { get; set; }                      // True heading (degrees, 0-359)
    public double? Draught { get; set; }                   // Current draught (meters)
    public int? DestinationIndex { get; set; }             // Lookup index for destination
    public long? EtaSecondsUntil { get; set; }             // Seconds until ETA from current time
}
```

**Usage:**
- Loaded by IShipPositionLoader from daily CSV files
- Filtered by time interval selection
- Input to ITrackOptimizer for waypoint generation
- All fields nullable to handle missing CSV data

**Derived Properties:**
```csharp
public DateTime AbsoluteTimestamp { get; }  // Computed: T0 + Time (seconds)
public bool IsValid { get; }                 // True if Latitude and Longitude are non-null
```

**Validation Rules:**
- Latitude: -90 to +90 degrees
- Longitude: -180 to +180 degrees
- SOG: non-negative
- Heading: 0-359 or null
- NavigationalStatusIndex: 0-15 or null

### RouteWaypoint

Represents an optimized waypoint for XML export.

**Properties:**
```csharp
public class RouteWaypoint
{
    public string Name { get; set; }              // MMSI as string
    public double Lat { get; set; }               // Latitude (decimal degrees)
    public double Lon { get; set; }               // Longitude (decimal degrees)
    public double Alt { get; set; }               // Altitude (always 0 for maritime)
    public double Speed { get; set; }             // SOG from CSV (knots)
    public long ETA { get; set; }                 // EtaSecondsUntil or 0
    public int Delay { get; set; }                // Always 0
    public string Mode { get; set; }              // Computed via SetWaypointMode
    public string TrackMode { get; set; }         // Always "Track"
    public int Heading { get; set; }              // True heading or 0
    public double PortXTE { get; set; }           // Always 20
    public double StbdXTE { get; set; }           // Always 20
    public double MinSpeed { get; set; }          // Always 0
    public double MaxSpeed { get; set; }          // Maximum SOG observed in route
}
```

**Mapping from ShipDataOut:**
- Name ← MMSI
- Lat ← Latitude
- Lon ← Longitude
- Alt ← 0 (fixed)
- Speed ← SOG (or 0 if null)
- ETA ← EtaSecondsUntil (or 0 if null)
- Delay ← 0 (fixed)
- Mode ← SetWaypointMode() implementation
- TrackMode ← "Track" (fixed)
- Heading ← Heading (or 0 if null)
- PortXTE ← 20 (fixed)
- StbdXTE ← 20 (fixed)
- MinSpeed ← 0 (fixed)
- MaxSpeed ← max(SOG) across all waypoints in route (ignoring zeros)

### TimeInterval

Represents user-selected time range for track generation.

**Properties:**
```csharp
public class TimeInterval
{
    public DateTime Start { get; set; }           // Start time (second resolution)
    public DateTime Stop { get; set; }            // Stop time (second resolution)
    
    public TimeSpan Duration => Stop - Start;
    public bool IsValid => Stop > Start;
}
```

**Usage:**
- Bound to Start/Stop time pickers in UI
- Defaults:
  - Start: timestamp from first CSV filename in vessel folder
  - Stop: timestamp from last CSV filename + 24 hours
- Validated before track creation (Start < Stop)
- Used by IShipPositionLoader to filter position records

## Storage Structure

### Input Data Organization

**Folder Structure:**
```
<InputFolder>/
  ├── <MMSI_1>/
  │   ├── <MMSI_1>.json          # Ship static data
  │   ├── 2024-01-01.csv         # Daily position reports
  │   ├── 2024-01-02.csv
  │   └── ...
  ├── <MMSI_2>/
  │   ├── <MMSI_2>.json
  │   ├── 2024-01-01.csv
  │   └── ...
```

**File Naming Conventions:**
- Vessel folders: Named by MMSI (9-digit number as string)
- Static data: `<MMSI>.json` (e.g., `205196000.json`)
- Daily CSV: `YYYY-MM-DD.csv` (e.g., `2024-01-15.csv`)

### Ship Static Data JSON Format

**Example: `205196000.json`**
```json
{
  "MMSI": 205196000,
  "Name": "Sea Explorer",
  "Length": 180.5,
  "Beam": 32.2,
  "Draught": 8.5,
  "TypeCode": 70,
  "CallSign": "5BZV",
  "IMO": 9876543
}
```

**Required Fields:**
- MMSI (must match folder name)

**Optional Fields:**
- All others nullable; missing values handled gracefully

### Daily CSV Format

**Header Row (required):**
```
Time,Latitude,Longitude,NavigationalStatusIndex,ROT,SOG,COG,Heading,Draught,DestinationIndex,EtaSecondsUntil
```

**Example Rows:**
```csv
Time,Latitude,Longitude,NavigationalStatusIndex,ROT,SOG,COG,Heading,Draught,DestinationIndex,EtaSecondsUntil
0,55.123456,12.345678,0,-0.5,12.3,45.2,45,8.5,1,3600
600,55.145678,12.367890,0,-0.3,12.5,46.1,46,8.5,1,3000
```

**Data Types:**
- Time: long (seconds since T0 = date at 00:00:00 UTC)
- Latitude, Longitude: double (decimal degrees)
- NavigationalStatusIndex: int (0-15)
- ROT: double (degrees/minute)
- SOG: double (knots)
- COG: double (degrees)
- Heading: int (degrees, 0-359)
- Draught: double (meters)
- DestinationIndex: int (lookup index)
- EtaSecondsUntil: long (seconds)

**Nullable Handling:**
- All fields except Time are nullable
- Empty cells or "null" text parsed as null
- Missing Heading/SOG default to 0 in waypoint mapping

### Output XML Format

**Filename Pattern:**
```
MMSINumber-StartDate-EndDate.xml
```

**Example:**
```
205196000-20250315T000000-20250316T000000.xml
```

**XML Structure:**
The application generates XML output following this structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RouteTemplates>
  <RouteTemplate Name="{MMSI}" ColorR="1" ColorG="124" ColorB="139">
    <WayPoint Name="{MMSI}" Lat="{Latitude}" Lon="{Longitude}" Alt="0" 
              Speed="{SOG}" ETA="{EtaSecondsUntil|0}" Delay="0" Mode="Cruise" 
              TrackMode="Track" Heading="{Heading|0}" PortXTE="20" StbdXTE="20" 
              MinSpeed="0" MaxSpeed="{MaxSOG}" />
    <!-- ... more waypoints ... -->
  </RouteTemplate>
</RouteTemplates>
```

**Default Template Attributes:**
- ColorR="1", ColorG="124", ColorB="139" (teal color)
- PortXTE="20", StbdXTE="20" (cross-track error tolerance)
- Alt="0" (altitude, always 0 for maritime)
- Delay="0" (no waypoint delay)
- MinSpeed="0" (no minimum speed constraint)
- Mode="Cruise" (default waypoint mode)
- TrackMode="Track" (track following mode)

## Data Relationships

### Entity Relationship Diagram

```
ShipStaticData (1) ----< (N) ShipDataOut
     |                         |
     |                         |
     ↓                         ↓
TimeInterval  ---------->  ITrackOptimizer
                              |
                              ↓
                         RouteWaypoint (N)
                              |
                              ↓
                         XML Export
```

**Relationships:**
- One ShipStaticData per vessel (MMSI)
- Many ShipDataOut records per vessel (time series)
- TimeInterval filters ShipDataOut records
- ITrackOptimizer converts filtered ShipDataOut → RouteWaypoint list
- IRouteExporter serializes RouteWaypoint list → XML file

### Data Flow Example: Alice Creates Track for Vessel 205196000

**Step 1: Folder Selection**
- Alice selects input folder: `C:\AISData\`
- ISourceDataScanner discovers subfolder: `205196000\`
- IShipStaticDataLoader reads: `205196000\205196000.json`
- Result: ShipStaticData instance created
  - MMSI: 205196000
  - Name: "Sea Explorer" (from JSON)
  - MinDate: 2024-01-01 (from first CSV filename)
  - MaxDate: 2024-01-31 (from last CSV filename)

**Step 2: Time Interval Selection**
- Alice selects vessel "Sea Explorer" from combo
- Time pickers default:
  - Start: 2024-01-01 00:00:00
  - Stop: 2024-02-01 00:00:00 (last + 24h)
- Alice adjusts to: 2024-01-15 06:00:00 → 2024-01-15 18:00:00

**Step 3: Track Creation**
- IShipPositionLoader loads CSV files:
  - `205196000\2024-01-15.csv`
- Filters ShipDataOut records by Time:
  - Time >= 21600 (06:00:00 in seconds from T0)
  - Time <= 64800 (18:00:00 in seconds from T0)
- Result: ~720 position reports (assuming 1 report per minute)

**Step 4: Track Optimization**
- ITrackOptimizer processes 720 ShipDataOut records
- Applies deviation detection:
  - Heading change > 0.2°
  - Distance > 5m
  - SOG change > 0.2 knots
  - ROT > 0.2 deg/s
- Result: ~50 RouteWaypoint instances (significant deviations only)

**Step 5: Export**
- Alice selects output folder: `C:\Routes\`
- IRouteExporter generates filename: `205196000-20240115T060000-20240115T180000.xml`
- Serializes 50 RouteWaypoint instances to XML
- File created with single RouteTemplate and 50 WayPoint elements

## Storage Implementation Details

### CSV Parsing (IShipPositionLoader)

**CsvHelper Configuration:**
```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    MissingFieldFound = null,  // Ignore missing fields
    BadDataFound = context => _logger.LogWarning("Bad CSV data at row {Row}", context.Row)
};
```

**Type Mapping:**
```csharp
public class ShipDataOutMap : ClassMap<ShipDataOut>
{
    public ShipDataOutMap()
    {
        Map(m => m.Time).Name("Time");
        Map(m => m.Latitude).Name("Latitude").Optional();
        Map(m => m.Longitude).Name("Longitude").Optional();
        // ... all fields marked optional except Time
    }
}
```

**Multi-Day Loading:**
- Determine date range from TimeInterval
- List all CSV files with filenames in range
- Load each file sequentially
- Filter by Time field (seconds since T0 for that date)
- Aggregate into single collection

### JSON Deserialization (IShipStaticDataLoader)

**System.Text.Json Options:**
```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};
```

**Error Handling:**
- File not found: Use folder name as vessel name, all fields null
- Malformed JSON: Log warning, use folder name, continue
- Missing fields: Nullable properties handle gracefully

### XML Export (IRouteExporter)

**XmlWriter Settings:**
```csharp
var settings = new XmlWriterSettings
{
    Indent = true,
    IndentChars = "  ",
    Encoding = Encoding.UTF8,
    OmitXmlDeclaration = false
};
```

**Template Structure:**
- Use embedded default template structure
- Apply standard color attributes (ColorR=1, ColorG=124, ColorB=139)
- Generate `<RouteTemplate>` element with MMSI as Name
- Wrap in `<RouteTemplates>` root element

**File Conflict Resolution:**
- Check if file exists
- If exists, prompt user: Overwrite / Append suffix / Cancel
- Append suffix: `filename (1).xml`, `filename (2).xml`, etc.

## Data Validation and Error Handling

### Input Validation

**Folder Structure:**
- Input folder must exist and be readable
- Must contain at least one MMSI subfolder
- Each MMSI subfolder should contain CSV files
- Log warning if JSON missing; use folder name

**CSV Validation:**
- Header row must match expected columns
- Time field required (non-nullable)
- Skip rows with invalid Latitude/Longitude
- Log malformed rows with row number

**JSON Validation:**
- MMSI field must match folder name
- All other fields optional

**Time Interval:**
- Start < Stop (enforced in UI)
- Range must overlap available CSV files (warning if no data)

### Missing Data Handling

**Scenario: Missing Static Data**
- Name: Use folder name (MMSI as string)
- Example: Folder `205196000` → Name = "205196000"

**Scenario: Missing Heading/SOG in CSV**
- Waypoint Heading: Default to 0
- Waypoint Speed: Default to 0
- Log warning: "Missing Heading/SOG values defaulted to 0"

**Scenario: No CSV Files in Time Interval**
- Show error: "No position data found for selected time interval"
- Suggest adjusting time range

**Scenario: Malformed CSV Rows**
- Skip row, log warning with row number
- Continue processing remaining rows
- Display warning: "Some rows were ignored due to invalid format"

### Edge Cases

**Scenario: Single Position Report**
- Track optimizer retains single waypoint
- MaxSpeed = SOG of that single report

**Scenario: All Positions Identical**
- Track optimizer retains first and last
- Zero distance, zero heading change

**Scenario: Noisy Data**
- Many waypoints retained (all exceed thresholds)
- User advised to narrow time window

**Scenario: Export Folder Creation Fails**
- Show error: "Cannot write to output path: {path}"
- Suggest alternative folder or permissions check

## Performance Considerations

### Large Dataset Handling

**Problem: Multi-year CSV data for single vessel**
- Solution: Load only CSV files within selected TimeInterval
- Don't scan entire date range unless needed

**Problem: Large daily CSV files (100K+ rows)**
- Solution: Use CsvHelper streaming mode
- Process records one at a time, filter by Time
- Don't load entire file into memory

**Problem: Many vessels (1000+ MMSI folders)**
- Solution: Lazy load static data on demand
- Don't parse all JSON files at startup
- Load only when vessel selected in combo

### Memory Optimization

**Streaming CSV Parsing:**
```csharp
using var reader = new StreamReader(csvPath);
using var csv = new CsvReader(reader, config);
await foreach (var record in csv.GetRecordsAsync<ShipDataOut>())
{
    if (record.Time >= startSeconds && record.Time <= stopSeconds)
        yield return record;
}
```

**Chunked Processing:**
- Process waypoints in batches of 1000
- Release memory for intermediate results

### Caching Strategy (Future Enhancement)

**SQLite Index:**
- Cache parsed CSV metadata (file, date range, row count)
- Query index before loading files
- Update index when new CSV files detected

## Example Data Scenarios

### Happy Path: Complete Data

**Input:**
- Vessel: 205196000 with complete JSON
- Time: 2024-01-15 06:00:00 → 18:00:00
- CSV: 2024-01-15.csv with 720 valid rows
- All fields populated

**Output:**
- 50 optimized waypoints
- XML file: `205196000-20240115T060000-20240115T180000.xml`

### Edge Case: Missing Static Name

**Input:**
- Vessel: 205196001 with no JSON file
- Folder name: `205196001`

**Handling:**
- Name defaulted to "205196001"
- Display in combo: "205196001"
- Static data TextBox shows: "Name: 205196001 (from folder)"

### Error Case: No CSV Files

**Input:**
- Vessel: 205196000
- Time: 2025-01-15 (future date, no CSV exists)

**Handling:**
- IShipPositionLoader returns empty collection
- Error message: "No position data found for selected time interval"
- Create Track button disabled or shows error

### Edge Case: Export Filename Conflict

**Input:**
- Export file already exists: `205196000-20240115T060000-20240115T180000.xml`
- User chooses "Append suffix"

**Handling:**
- Check `205196000-20240115T060000-20240115T180000 (1).xml` → exists
- Check `205196000-20240115T060000-20240115T180000 (2).xml` → available
- Create file with suffix (2)
- Success message: "Route exported to ...\\(2).xml"

## References

- AIS CSV Specification: Daily position reports with Time, Lat, Lon, SOG, COG, etc.
- route_waypoint_template.xml: Defines XML structure for export
- CsvHelper Documentation: https://joshclose.github.io/CsvHelper/
- System.Text.Json Documentation: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview
