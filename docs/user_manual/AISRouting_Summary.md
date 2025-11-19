# AISRouting User Manual Summary

## Application Overview
AISRouting is a local application for K-Sim instructors that creates navigational routes from AIS track data. Users select a vessel (by MMSI), choose a time range, and generate a RouteTemplate XML file compatible with Kongsberg route waypoint templates.

## Manual Sections
### [Getting Started](getting_started.md)
- Explains installation and launching the application on an instructor's PC.
- Includes quick checks for correct AIS input folder structure.

### [Input Data and CSV Format](input_data.md)
- Describes required input CSV format, column order, encoding, and naming conventions.
- Example lines and null handling are included.

### [Selecting a Ship and Time Range](selecting_ship_and_time_range.md)
- Shows how to pick an MMSI from the ship list and select start/end times within available bounds.
- Explains how to initiate route creation.

### [Creating Routes and Output XML](creating_routes.md)
- Details generating RouteTemplate and WayPoint elements from AIS data.
- Describes saving XML output, filename pattern, encoding, and overwrite behavior.

### [Generating Waypoint Mappings](generating_waypoint_mappings.md)
- Documents exact mapping rules from AIS CSV fields to WayPoint attributes and default handling of missing values.

## Quick Reference
- Input folder: outputFolder/<MMSI>/
- Input filename: <MMSI>-yyyy-MM-dd.csv (no header, UTF-8, CRLF)
- Output filename: <MMSI>-route-yyyy-MM-dd.xml (UTF-8, CRLF)
- WayPoint mapping highlights: Name=MMSI, Lat=Latitude, Lon=Longitude, Speed=SOG, ETA=EtaSecondsUntil

