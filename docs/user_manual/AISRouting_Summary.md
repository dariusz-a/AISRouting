# AISRouting User Manual Summary

## Application Overview
AISRouting is a desktop WPF application that ingests AIS (Automatic Identification System) CSV records, allows selecting a vessel and time range, creates an optimized track, and exports the route as an XML file with a standard route template structure. The application supports multiple vessels, handles static vessel data, and provides comprehensive data validation and error handling. Designed for Windows 10+ with .NET 8.0 Desktop Runtime, it processes vessel position data and generates industry-standard RTZ (Route Plan Exchange Format) XML exports.

## Manual Sections
### [Getting Started](getting_started.md)
- Comprehensive initial setup guide including installation, launch procedures, and input data configuration
- Explains required folder structure for AIS data (vessel subfolders with JSON and CSV files)
- Troubleshooting common setup issues (missing vessels, corrupted files, empty date ranges)
- Performance optimization tips and best practices for data organization
- First-time setup checklist and prerequisites verification
- Key workflows: install application, verify .NET runtime, select input root, validate vessel discovery, view static data

### [Ship Selection and Static Data](ship_selection.md)
- How to choose a ship, view static metadata, and set precise time ranges.
- Key workflows: select ship, set start/stop timestamps.

### [Create Track](create_track.md)
- Track generation process with default optimization parameters.
- Key workflows: generate track, review optimized points.

### [Exporting Routes](export_route.md)
- Export filename pattern, WayPoint mapping rules, output folder behavior, and conflict handling.
- Key workflows: export XML, respond to existing filename prompts.

### [Troubleshooting](troubleshooting.md)
- Common problems and resolutions for import, export, and missing data.

## Quick Reference

### Common Tasks
- **First Launch**: Extract application → Verify .NET 8.0 Runtime → Double-click AISRouting.App.WPF.exe
- **Select Data**: Browse to input root → Verify vessels appear in combo box → Select vessel
- **Check Static Data**: View MMSI, name, length, beam, draught, and available date range in static data panel
- **Handle Empty Results**: Verify folder structure → Check MMSI subfolder naming → Validate JSON files exist

### File Requirements
- **Input Root Structure**: Root folder → MMSI subfolders → JSON + CSV files per vessel
- **Static Data**: JSON file named [MMSI].json with vessel properties
- **Position Data**: CSV files named YYYY-MM-DD.csv
- **Export Format**: MMSINumber-StartDate-EndDate.xml (UTC, YYYYMMDDTHHMMSS)

### Technical Specifications
- **Platform**: Windows 10+ with .NET 8.0 Desktop Runtime
- **Minimum Requirements**: 4GB RAM, 500MB disk space
- **Optimization Parameters**: heading change 0.2°, min distance 5m, SOG threshold 0.2 knots
- **Output Behavior**: prompts user to Overwrite / Suffix / Cancel on conflicts
- **Data Validation**: JSON structure, CSV naming, coordinate validity, MMSI format

### Error Messages
- "No vessels found in input root" → Verify folder structure and MMSI subfolder naming
- "Application failed to start: executable missing or corrupted" → Re-extract distribution or verify .NET runtime
- "N/A" in static data fields → Check JSON file exists and contains valid data
- Empty date range → Verify CSV files exist with correct YYYY-MM-DD.csv naming

