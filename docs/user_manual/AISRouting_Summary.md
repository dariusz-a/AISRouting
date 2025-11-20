# AISRouting User Manual Summary

## Application Overview
AISRouting ingests AIS CSV records, allows selecting a vessel and time range, creates an optimized track, and exports the route as an XML file with a standard route template structure.

## Manual Sections
### [Getting Started](getting_started.md)
- Explains initial setup, required files, and how to select input data root.
- Key workflows: install, select input root.

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
- Filename format for export: MMSINumber-StartDate-EndDate.xml (UTC, YYYYMMDDTHHMMSS)
- Default optimization parameters: heading change 0.2�, min distance 5m, SOG threshold 0.2 knots
- Output conflict behavior: prompts user to Overwrite / Suffix / Cancel

