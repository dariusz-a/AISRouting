# Selecting a Ship and Time Range

## Overview
This section explains how to pick a vessel from the available AIS data and how to specify the time range used to generate the route track.

## Prerequisites
- AIS input files present in outputFolder/<MMSI>/
- Application launched and main window visible

## Step-by-Step Instructions
### Selecting a ship
1. In the main window, open the Ship List panel.
2. Find the vessel by MMSI in the list. The list includes metadata for each MMSI (length, width, ship type, etc.).
3. Click the desired MMSI to select the vessel.
4. **Expected result**: The selected vessel is highlighted and the available dates/time ranges for that MMSI are displayed.

### Selecting time range
1. After selecting MMSI, choose the Start time using the Start time edit widget.
   - The widget validates values against the available data time bounds for the selected MMSI.
2. Choose the End time using the End time edit widget.
   - Validation ensures End time >= Start time and within available bounds.
3. **Expected result**: The selected time range is displayed and the number of track points to be used is shown.

### Initiate route creation
1. Click the "Create Route" button.
2. The application will parse the AIS data for the selected MMSI and time range and generate a RouteTemplate in memory.

## Tips and Best Practices
- If you are unsure about available times, first open the date picker to view min/max times detected from the input files.
- Use narrow time windows for quick previews; expand the range for full track extraction.

## Related Sections
- [Input Data and CSV Format](input_data.md)
- [Creating Routes and Output XML](creating_routes.md)
