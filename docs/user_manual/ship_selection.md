# Ship Selection and Static Data

## Overview
This section explains how to choose a ship from the input data, view its static metadata, and set the available time range for route creation.

## Prerequisites
- Input root folder selected in Getting Started
- Each vessel provided as a subfolder containing CSV records
- Optional static data file in the vessel folder to provide a human-readable ship name

## Step-by-Step Instructions
### Choosing a Ship
1. Open the Ship selection combo box in the application.
2. The list populates from static files; if a static file does not provide a name, the folder name is used.
3. Select the desired vessel from the list.
4. **Expected result**: ship static data displays in the large TextBox widget.

### Viewing Static Data
1. After selecting a ship, static attributes (if present) are displayed in the TextBox widget.
2. If specific fields are missing, fall back to folder name or default values.

### Setting Time Range
1. The Min/Max date pickers show the earliest and latest timestamps available in the vessel's CSV files.
2. Use the start/stop pickers with second resolution to select exact timestamps. Defaults:
   - StartValue: timestamp from the first filename in the vessel folder
   - StopValue: timestamp from the last filename in the vessel folder + 24 hours
3. **Expected result**: the selected time range defines which AIS records are used to generate waypoints.

## Tips and Best Practices
- If CSV filenames include timestamps, enabling automatic defaults reduces selection errors.
- Validate the displayed Min/Max range before running track creation.

## Related Sections
- [Getting Started](getting_started.md)
- [Create Track](create_track.md)
