# Feature 1.1: Data Models and Core Domain

This document describes the core data models required by the AisToXmlRouteConvertor for Iteration 1. It defines record shapes, field types, validation rules, example data, and notes for tests and implementation.

Design goals
- Small, immutable-ish records where practical.
- Clear separation between static metadata (`ShipStaticData`) and dynamic AIS-derived positions (`ShipState`).
- Minimal, well-typed fields to make parsing and testing straightforward.

1. ShipStaticData
- Purpose: store static information about the ship that is read from `<MMSI>.json` files and displayed in UI.
- Primary consumer: ship selection UI, export metadata in XML filename/template.

Fields
- `mmsi`: string (numeric string, required) — Maritime Mobile Service Identity, 9 digits preferred.
- `name`: string | null — ship name, may be absent.
- `shipType`: string | null — free-text type (use mapping table during UI display if needed).
- `lengthMeters`: number | null — length overall in meters when available.
- `beamMeters`: number | null — beam in meters.
- `callsign`: string | null — radio callsign.
- `imo`: string | null — IMO number if present.
- `sourcePath`: string — absolute path to the JSON file that produced this record (for audit/debug).

Validation rules
- `mmsi` must be a numeric string of 7–9 digits; prefer strict 9-digit acceptance but allow 7–9 for legacy.
- Numeric fields must be positive if present.

Example
```
{
  "mmsi": "205196000",
  "name": "Vessel Example",
  "shipType": "Cargo",
  "lengthMeters": 120.5,
  "beamMeters": 18.2,
  "callsign": "ABC123",
  "imo": "IMO1234567",
  "sourcePath": "C:\\input\\205196000.json"
}
```

Test notes
- Validate parsing accepts missing optional fields.
- Validate rejection of non-numeric `mmsi`.

2. ShipState
- Purpose: represent a single AIS-derived position or observation parsed from CSV rows (or aggregated sources).
- Primary consumer: track optimization, time-range filtering, XML waypoint generation.

Fields
- `timestamp`: string (ISO-8601 UTC) — required. Use UTC for internal calculations.
- `lat`: number — required. Range: -90..90.
- `lon`: number — required. Range: -180..180.
- `sog`: number | null — speed over ground in knots.
- `cog`: number | null — course over ground in degrees (0–360).
- `heading`: number | null — vessel heading in degrees.
- `navStatus`: string | null — navigation status code or text.
- `mmsi`: string — owner ship identifier for cross-join with `ShipStaticData`.
- `sourceFile`: string — original CSV filename (for logging/troubleshooting).
- `rowNumber`: number | null — optional original CSV row number to help debugging.

Validation rules
- `timestamp` must be parsable to a valid UTC time and not null.
- `lat` and `lon` must be finite numbers within the ranges specified.

Example
```
{
  "timestamp": "2025-03-15T12:34:56Z",
  "lat": 54.12345,
  "lon": 18.54321,
  "sog": 12.5,
  "cog": 245.0,
  "heading": 240,
  "navStatus": "Under way using engine",
  "mmsi": "205196000",
  "sourceFile": "2025-03-15.csv",
  "rowNumber": 17
}
```

Test notes
- Accept CSV rows with optional `sog`, `cog`, or `heading` missing.
- Skip rows with unparsable timestamps and log them.

3. TimeInterval
- Purpose: represent the user-selected start/end pair for processing.

Fields
- `start`: string (ISO-8601 UTC) — required.
- `end`: string (ISO-8601 UTC) — required.

Validation rules
- `start` must be <= `end`.
- Intervals outside the available ShipState timestamps will be considered invalid in the UI; the backend filtering code should just return an empty sequence when no matching points.

Example
```
{
  "start": "2025-03-15T00:00:00Z",
  "end": "2025-03-15T23:59:59Z"
}
```

Test notes
- Ensure Start > End produces a validation error.

4. RouteWaypoint
- Purpose: canonical waypoint produced by track optimization and used to populate the XML route template.

Fields
- `sequence`: number — waypoint index starting at 1.
- `lat`: number — required.
- `lon`: number — required.
- `time`: string (ISO-8601 UTC) | null — optional ETA/Time for the waypoint when derivable.
- `description`: string | null — optional human note.

Validation rules
- `lat`/`lon` range checks as per `ShipState`.
- `sequence` must be positive and strictly increasing within a route.

Example
```
{
  "sequence": 1,
  "lat": 54.12345,
  "lon": 18.54321,
  "time": "2025-03-15T12:34:56Z",
  "description": "Start position"
}
```

Test notes
- Ensure first and last positions are always retained by optimizer.

5. TrackOptimizationParameters
- Purpose: parameters for the optimization algorithm controlling thresholds and behaviors.

Fields
- `maxDistanceMeters`: number | null — maximum allowed cross-track distance before a point is considered significant.
- `minDistanceMeters`: number | null — minimum distance to consider points distinct.
- `maxPoints`: number | null — cap on output waypoint count.
- `preserveEndpoints`: boolean — default true.

Defaults and notes
- Defaults should be chosen conservatively, e.g., `preserveEndpoints: true`, `maxPoints: null` (no cap), `minDistanceMeters: 5`, `maxDistanceMeters: 50`.

Test notes
- Unit tests should assert behavior with empty inputs, single-point inputs, and threshold-edge cases.

Implementation guidance
- Language-agnostic model; prefer small typed records or DTOs in the target runtime (.NET/Avalonia).
- Use ISO-8601 UTC strings for interchange; convert to native datetimes internally when needed.
- Validation functions should return clear, testable error objects rather than throwing exceptions for easy unit testing.

Open questions
- Should `mmsi` be stored as an integer type in internal models for faster comparisons, or kept as string to preserve leading zeros? Recommendation: store as string internally to avoid platform-specific formatting issues.
- Should `sourcePath` be relative to the input folder or absolute? Recommendation: store absolute path for debugging and tracing.

References
- See `docs/spec_scenarios/input_data_preparation.md` for CSV/JSON schema expectations.

Appendix: Code Examples

TypeScript DTOs (example)
```
export interface ShipStaticData {
  mmsi: string;
  name?: string | null;
  shipType?: string | null;
  lengthMeters?: number | null;
  beamMeters?: number | null;
  callsign?: string | null;
  imo?: string | null;
  sourcePath: string;
}

export interface ShipState {
  timestamp: string; // ISO-8601 UTC
  lat: number;
  lon: number;
  sog?: number | null;
  cog?: number | null;
  heading?: number | null;
  navStatus?: string | null;
  mmsi: string;
  sourceFile: string;
  rowNumber?: number | null;
}

export interface RouteWaypoint {
  sequence: number;
  lat: number;
  lon: number;
  time?: string | null; // ISO-8601 UTC
  description?: string | null;
}
```

C# DTOs (example)
```
public record ShipStaticData(
    string Mmsi,
    string? Name,
    string? ShipType,
    double? LengthMeters,
    double? BeamMeters,
    string? Callsign,
    string? Imo,
    string SourcePath
);

public record ShipState(
    string Timestamp,
    double Lat,
    double Lon,
    double? Sog,
    double? Cog,
    int? Heading,
    string? NavStatus,
    string Mmsi,
    string SourceFile,
    int? RowNumber
);

public record RouteWaypoint(
    int Sequence,
    double Lat,
    double Lon,
    string? Time,
    string? Description
);
```

Unit test notes
- Use table-driven tests for parsing CSV rows into `ShipState` and for validating `ShipStaticData` parsing.
- Add property-based tests around `TimeInterval` validation (start <= end).
- Add tests that assert all numerical ranges and required fields cause predictable validation results (error objects, not raw exceptions).

