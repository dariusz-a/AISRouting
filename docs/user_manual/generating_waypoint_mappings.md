# Generating Waypoint Mappings

## Overview
This section explains how AIS input fields are transformed into WayPoint attributes in the output XML and how missing values are handled.

## Prerequisites
- AIS CSV input prepared according to Input Data and CSV Format
- Selected vessel MMSI and time range

## Step-by-Step Instructions
### Field mappings
Apply the following mapping for each AIS record included in the selected time range:
- Name: set to MMSI (string)
- Lat: use CSV Latitude value
- Lon: use CSV Longitude value
- Alt: set to 0 (no mapping from AIS)
- Speed: use SOG from CSV (no unit conversion in this release)
- ETA: use EtaSecondsUntil from CSV if provided; otherwise set to 0
- Delay: set to 0
- Mode: computed via SetWaypointMode (defined later)
- TrackMode: set to "Track"
- Heading: use Heading from CSV or 0 if missing
- PortXTE: set to 20
- StbdXTE: set to 20
- MinSpeed: set to 0
- MaxSpeed: computed by GetMaxShipSpeed (maximum SOG observed in selected range)

### Missing values handling
1. Empty CSV fields map to default values above or nullable types where appropriate.
2. If SOG is missing for a record, Speed in the WayPoint will be set to 0.

## Tips and Best Practices
- If you later need different units (knots vs m/s), note the current implementation uses SOG as-is; convert prior to mapping if required.

## Related Sections
- [Creating Routes and Output XML](creating_routes.md)
- [Input Data and CSV Format](input_data.md)
