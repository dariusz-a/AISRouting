# QA Session for User Manual Generation

## Q1: What is the primary purpose of the application?

**Answer:**

The primary purpose of the application is to create route for selected a ship from AIS data.

---

## Q2: Who are the primary users of the application and what user roles & permissions exist?

**Answer:**

Users of the application will be instructor of the K-Sim application. No extra permission needed.

---

## Q3: How do users access and authenticate to the application?

**Answer:**

The application is installed locally on the instructor's PC. No online authentication is required; users launch the application from the desktop/start menu. Local OS user account provides access control.

---

## Q4: What input data formats does the application accept and how should files be prepared?

**Answer:**

The application accepts AIS CSV files with the following characteristics:

- Location & naming:
  - Files are placed under: outputFolder/<MMSI>/<MMSI>-yyyy-MM-dd.csv
  - No header row (headerless)
  - Encoding: UTF-8
  - Line endings: CRLF on Windows

- Columns (order, headerless). Null/empty cells map to nullable types:
  1. Time: seconds (signed 64-bit) from T0
  2. Latitude: decimal (use invariant '.' as decimal separator)
  3. Longitude: decimal (use invariant '.')
  4. NavigationalStatusIndex: int (dictionary index)
  5. ROT: decimal or empty
  6. SOG: decimal or empty
  7. COG: decimal or empty
  8. Heading: int or empty
  9. Draught: decimal or empty
  10. DestinationIndex: int (dictionary index) or empty
  11. EtaSecondsUntil: signed 64-bit seconds or empty

- Notes:
  - Example single line (11 fields, headerless):
    0,55.884242,12.759092,0,,,,,,,
  - Use the invariant dot (.) for decimals. Empty fields (e.g., ,,) should be treated as null values.
  - Ensure MMSI directory and file naming match the specified pattern so the application locates files correctly.
  - Keep file sizes within expected processing limits on the instructor's PC (recommendations/limits can be documented later if needed).

---

## Q5: How does a user select a ship from the AIS data and initiate route creation?

**Answer:**

The user picks a vessel by MMSI. MMSI is selected from a ship list which contains information for each MMSI like length, width, ship type, etc. Before starting route creation the user needs to select a time range from which the track should be created.

---

## Q6: How should the time range selection work (format, UI controls, defaults, timezone, max span)?

**Answer:**

The time range UI should have a start time edit widget with verification limits derived from the available input data, and an end time edit widget with the same verification.

---

## Q7: What output format is produced when a route is created and where is the route saved?

**Answer:**

The output route file uses the XML format matching the repository example route_waypoint_template.xml. Key details:

- File format and structure:
  - Root element: <RouteTemplates>
  - One or more <RouteTemplate> elements. RouteTemplate has attributes such as Name and ColorR/ColorG/ColorB.
  - Each <RouteTemplate> contains multiple <WayPoint> elements.
  - WayPoint attributes used in the template include: Name, Lat, Lon, Alt, Speed, ETA, Delay, Mode, TrackMode, Heading, PortXTE, StbdXTE, MinSpeed, MaxSpeed.

- File naming and location:
  - Default save folder: outputFolder/<MMSI>/ (same MMSI directory as input files)
  - Suggested filename pattern: <MMSI>-route-yyyy-MM-dd.xml (example: 123456789-route-2025-11-19.xml)

- Encoding and line endings:
  - Encoding: UTF-8
  - Line endings: CRLF on Windows

- Metadata:
  - The RouteTemplate Name attribute can include source MMSI and the selected time range for traceability (e.g., "123456789 2025-11-01T00:00_2025-11-01T12:00").
  - Colors and other template-level metadata follow the template example.

- Overwrite behavior:
  - If a file with the same name exists, the application should prompt the user to overwrite or create a new file with an incremental suffix (this behavior can be configured later).

---

## Q8: How should AIS fields map to WayPoint attributes in the output (e.g., Speed, ETA, Heading, Alt)?

**Answer:**

Mapping rules from AIS CSV to WayPoint attributes:

- Name: use MMSI (string)
- Lat: Latitude (decimal)
- Lon: Longitude (decimal)
- Alt: no mapping, set to 0
- Speed: SOG (use as-is, units preserved)
- ETA: EtaSecondsUntil (use as provided)
- Delay: no mapping, set to 0
- Mode: determine by calling SetWaypointMode (implementation-defined)
- TrackMode: set to "Track"
- Heading: Heading (int) if present; otherwise 0 or omitted per template rules
- PortXTE: set to 20
- StbdXTE: set to 20
- MinSpeed: set to 0
- MaxSpeed: call GetMaxShipSpeed to obtain maximum ship speed for MaxSpeed

Handling of missing values and conversions:
- Empty CSV fields map to nulls; use defaults as specified (Alt=0, Delay=0, MinSpeed=0).
- Speed uses SOG directly; no unit conversion unless specified later.
- If Heading is missing, set Heading attribute to 0.

---

## Q9: How should SetWaypointMode and GetMaxShipSpeed behave and what modes/units should be used?

**Answer:**

SetWaypointMode is computed from input AIS data based on ROT/heading change; the precise definition will be provided later. GetMaxShipSpeed is computed from input AIS data based on the maximum observed SOG value for the vessel in the selected time range.

---

## Q10: How can users preview and edit the generated route before saving?

**Answer:**

It is not needed to precent it.

---

## Q11: When AIS data contains gaps or missing positions in the selected time range, should the application interpolate missing waypoints or skip them?

**Answer:**

This will be explained later.

---

## Q12: What unit should be used for Speed and MaxSpeed in the output (knots or m/s)?

**Answer:**

<!-- Please specify whether to use knots or meters per second for Speed and MaxSpeed in the generated WayPoint elements. -->

---

*Next step*: After you reply with the answer above, I will append your answer here and proceed with the next question (one question at a time) as specified in the workflow.
