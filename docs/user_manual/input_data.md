# Input Data and CSV Format

## Overview
This section describes the required input AIS CSV files the application consumes, file naming conventions, and field formats. It provides examples and notes for preparing data suitable for route creation.

## Prerequisites
- AIS CSV files exported or available in the specified folder structure
- Files encoded in UTF-8

## Step-by-Step Instructions
### File placement and naming
1. Place AIS CSV files in the application input folder using the pattern:
   outputFolder/<MMSI>/<MMSI>-yyyy-MM-dd.csv
2. Ensure each file is placed in a folder that matches the MMSI value.

### CSV format and columns
The CSV files have no header row and must include columns in this order:
1. Time: seconds (signed 64-bit) from T0
2. Latitude: decimal (use '.' decimal separator)
3. Longitude: decimal (use '.' decimal separator)
4. NavigationalStatusIndex: int (dictionary index)
5. ROT: decimal or empty
6. SOG: decimal or empty
7. COG: decimal or empty
8. Heading: int or empty
9. Draught: decimal or empty
10. DestinationIndex: int (dictionary index) or empty
11. EtaSecondsUntil: signed 64-bit seconds or empty

Example line (11 fields):
0,55.884242,12.759092,0,,,,,,,

### Handling nulls and decimals
1. Empty fields (e.g., ,,) map to nullable values.
2. Use invariant '.' as decimal separator.

## Tips and Best Practices
- Test with small sample files before processing large datasets.
- Validate CSVs with a simple script or spreadsheet to verify column counts and numeric formats.

## Related Sections
- [Getting Started](getting_started.md)
- [Selecting a Ship and Time Range](selecting_ship_and_time_range.md)
