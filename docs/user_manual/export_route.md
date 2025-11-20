# Exporting Routes

## Overview
This section describes how to export the generated route into an XML file that follows the route_waypoint_template.xml metadata. The export produces a single RouteTemplate containing all generated WayPoint elements.

## Prerequisites
- A generated track (Create Track) for a selected ship and time interval
- route_waypoint_template.xml present in the application directory

## Step-by-Step Instructions
### Export Steps
1. From the generated track view, click the "Export" button.
2. Choose an output folder. If the selected output path does not exist, the application will create it. The application validates that the path is writable and displays the selected path in the UI. If creation or write fails, an error is shown.
3. Filename pattern: MMSINumber-StartDate-EndDate.xml, where dates are UTC formatted as YYYYMMDDTHHMMSS. Example: 205196000-20250315T000000-20250316T000000.xml
4. If a file with the same name already exists, the application will prompt the user to choose: Overwrite, Append numeric suffix, or Cancel.
5. Confirm export. The XML written contains a single <RouteTemplate Name="{MMSI}"> element and an ordered list of <WayPoint .../> elements.

### WayPoint Attribute Mapping
For each AIS record included in the selected time range the following mapping applies:
- Name: set to MMSI (string)
- Lat: CSV latitude value
- Lon: CSV longitude value
- Alt: 0
- Speed: SOG from CSV (no unit conversion in this release)
- ETA: EtaSecondsUntil from CSV if provided; otherwise 0
- Delay: 0
- Mode: computed via SetWaypointMode (not yet defined)
- TrackMode: "Track"
- Heading: Heading from CSV or 0 if missing
- PortXTE: 20
- StbdXTE: 20
- MinSpeed: 0
- MaxSpeed: computed as the maximum SOG observed in the selected range (zeros ignored where possible)

## Tips and Best Practices
- Keep a consistent route_waypoint_template.xml in the application root to ensure color and template metadata are applied.
- Use the prompt decision when filename conflicts occur to avoid accidental overwrites.

## Related Sections
- [Create Track](create_track.md)
- [Getting Started](getting_started.md)
