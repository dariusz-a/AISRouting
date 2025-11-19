# Creating Routes and Output XML

## Overview
This section covers generating a route from AIS track data, mapping AIS fields to route waypoints, preview behavior, and saving the resulting XML route file compatible with the repository template.

## Prerequisites
- AIS CSV files available and validated
- A vessel selected and a valid time range selected

## Step-by-Step Instructions
### Generate route from AIS track
1. After selecting vessel and time range, click "Create Route".
2. Application reads AIS CSV records for the selected MMSI and time span and constructs a RouteTemplate with WayPoint elements.
3. Mapping rules are applied per the following table:
   - Name => MMSI
   - Lat => Latitude
   - Lon => Longitude
   - Alt => 0
   - Speed => SOG
   - ETA => EtaSecondsUntil
   - Delay => 0
   - Mode => computed by SetWaypointMode (implementation detail)
   - TrackMode => "Track"
   - Heading => Heading (or 0 if missing)
   - PortXTE => 20
   - StbdXTE => 20
   - MinSpeed => 0
   - MaxSpeed => computed by GetMaxShipSpeed (max observed SOG)
4. **Expected result**: The in-memory RouteTemplate mirrors the AIS track; user is shown a success message.

### Preview and edits
- The application does not provide an interactive preview/edit UI in this release. Generated RouteTemplates are available for saving.

### Save route as XML
1. Click "Save Route" and confirm the file name and location (default: outputFolder/<MMSI>/<MMSI>-route-yyyy-MM-dd.xml).
2. If the file exists, choose to overwrite or save with a different name.
3. **Expected result**: An XML file is written using UTF-8 encoding and CRLF line endings; file content follows the route_waypoint_template.xml structure.

## Tips and Best Practices
- Verify the generated file in a text editor before importing into other systems.
- Keep a copy of the original AIS input used to generate the route for traceability.

## Related Sections
- [Generating Waypoint Mappings](generating_waypoint_mappings.md)
- [Input Data and CSV Format](input_data.md)
