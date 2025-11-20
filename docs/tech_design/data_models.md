# Data Models: Structures, storage implementation, relationships

This document covers all core data models used by AISRouting: domain entities, DTOs, persistence formats (CSV, JSON, XML), relationships, validation rules, and handling of happy path versus edge/error scenarios. It cross?references overall_architecture.md and api_integration_patterns.md.

## 1. Overview
Primary data flows originate from static vessel JSON + daily AIS position CSV files, transformed into in-memory domain models, optimized into RouteWaypoint sequence, then exported to XML. Models designed for immutability where practical and minimal formatting logic.

## 2. Domain Model Definitions

### ShipStaticData
Represents static vessel metadata.
Fields:
- long MMSI (required, 9 digits)
- string Name (optional; fallback to folder name if missing)
- double? LengthMeters
- double? BeamMeters
- double? DraughtMeters
- string? CallSign
- string? IMO
- int? TypeCode
- DateTime? MinDataDate (computed from CSV filenames)
- DateTime? MaxDataDate (computed)
Edge Cases:
- Missing JSON file ? ShipStaticData created with MMSI only.
- Malformed field (e.g., non-numeric length) ? Skip & log warning.

### ShipDataOut (Position Report)
Derived from CSV lines.
Fields:
- long TimeSeconds (seconds since file date midnight UTC)
- double Latitude
- double Longitude
- int NavigationalStatusIndex (0-15)
- double? ROT (degrees/min)
- double? SOG (knots)
- double? COG (degrees)
- int? Heading (degrees)
- double? Draught (meters)
- int? DestinationIndex
- long? EtaSecondsUntil
Derived / Computed:
- DateTime AbsoluteTimestamp = FileDate.Date + TimeSeconds offset.
Validation:
- Latitude ? [-90,90], Longitude ? [-180,180]; if invalid ? skip row.
Edge Cases:
- Missing numeric fields ? treat as null, do not skip entire row unless essential (lat/lon). Lat/Lon required.

### TimeInterval
Represents user-selected range.
Fields:
- DateTime StartUtc
- DateTime StopUtc
Rules:
- Start < Stop.
- Both within [MinDataDate, MaxDataDate] for selected vessel (UI enforced).
Edge Cases:
- Interval yields zero positions ? return empty list from loader; UI message.

### RouteWaypoint
Represents optimized waypoint for export.
Fields:
- string Name (MMSI or index)
- double Lat
- double Lon
- double Speed (knots; fallback 0)
- int Heading (fallback 0)
- long EtaSecondsUntil (0 if missing)
- double MaxSpeedObserved (provided externally for Min/Max mapping)
- double ROT (optional retention for debugging)
XML Mapping (see api_integration_patterns.md): attributes PortXTE=20, StbdXTE=20, MinSpeed=0, TrackMode="Track".
Edge Cases:
- Missing Speed/Heading ? default 0.
- Consecutive duplicates (same lat/lon) filtered by optimizer.

### Route
Container for final export.
Fields:
- long MMSI
- TimeInterval Interval
- IReadOnlyList<RouteWaypoint> Waypoints
- int InputPointCount
- int OutputWaypointCount
Statistics aid UI transparency.

## 3. Relationships
- ShipStaticData (1) — (Many) ShipDataOut via MMSI & dates.
- TimeInterval selects subset of ShipDataOut.
- TrackOptimizer maps filtered ShipDataOut ? many RouteWaypoint (subset selection).
- Route aggregates metadata + selected waypoints.

## 4. Persistence Structures
- Static JSON (<MMSI>.json) ? ShipStaticData.
- Daily CSV (YYYY-MM-DD.csv) ? enumerable ShipDataOut.
- XML Route Export (MMSI-Start-End.xml) ? serialized RouteWaypoint list inside <RouteTemplate>.

## 5. Mapping & Transformation
1. Load static JSON.
2. Enumerate CSV filenames to derive min/max dates for each vessel.
3. For selected interval, load CSVs spanning the days; parse rows; compute AbsoluteTimestamp.
4. Sort by AbsoluteTimestamp ascending.
5. Optimizer inspects sequential reports; retains subset as waypoints.
6. Compute MaxSpeedObserved across retained waypoints; assign to each XML attribute where needed.

## 6. Validation & Error Handling
- MMSI must be 9-digit numeric; if invalid folder name -> skip or warn.
- JSON parsing exceptions caught; partial fields applied.
- CSV row with invalid lat/lon skipped; count logged.
- TimeSeconds negative or > 86400 ? clamp or skip (log).
- Duplicate timestamps allowed but may be coalesced in optimizer.

## 7. Example Data
Static JSON example (Evergreen Star):
{
  "MMSI": 205196000,
  "Name": "Evergreen Star",
  "LengthMeters": 300.1,
  "BeamMeters": 48.2,
  "DraughtMeters": 14.5,
  "CallSign": "ONAB2",
  "IMO": "9301234",
  "TypeCode": 70
}

CSV snippet (2024-01-01.csv):
Time,Latitude,Longitude,NavigationalStatusIndex,ROT,SOG,COG,Heading,Draught,DestinationIndex,EtaSecondsUntil
0,51.2345,3.1234,0,0.0,12.4,182.3,180,14.5,12,3600
60,51.2346,3.1236,0,0.0,12.5,182.4,180,14.5,12,3540
...

RouteWaypoint after optimization:
Name=205196000, Lat=51.2345, Lon=3.1234, Speed=12.4, Heading=180

## 8. Edge Scenario Handling
Scenario: All CSV rows invalid lat/lon ? No ShipDataOut; UI informs user; track generation disabled.
Scenario: Partial JSON (only MMSI) ? Vessel still selectable with fallback name.
Scenario: CSV has huge ROT spikes (noise) ? May trigger waypoint retention; threshold tuning planned (see overall_architecture.md Iteration 5).
Scenario: TimeInterval crosses daylight saving boundary ? Use UTC; unaffected.

## 9. Performance Considerations
- ShipDataOut stored in List<ShipDataOut>; optimization iterates once O(n).
- Minimal allocations: compute bearing/ distance inline static helpers.
- Streaming CSV: yield rows; only materialize filtered set.

## 10. Extensibility
Future fields (e.g., AIS message types) added as nullable without breaking existing logic.
Alternative persistence (SQLite) can map ShipDataOut to table with MMSI + timestamp index for rapid range queries.

## 11. Cross-References
- overall_architecture.md (Algorithm thresholds)
- application_organization.md (Project folders)
- api_integration_patterns.md (XML attribute mapping)
- security_architecture.md (Validation rules for MMSI/path)
