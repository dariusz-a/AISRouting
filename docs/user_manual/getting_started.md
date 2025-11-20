# Getting Started

## Overview
This guide helps QA engineers install and begin using AisToXmlRouteConvertor to convert AIS source data into XML route files.

## Prerequisites
- A Windows machine
- The application executable installed or built
- An `input` folder containing source data organized by MMSI (see `input_data_preparation.md`)
- An `output` folder for generated XML files

## Step-by-Step Instructions
### Launching the Application
1. Start the application executable.
2. The main window will display controls to select input and output folders, a ship selection table, the ShipStaticData panel, time pickers, and the `Process!` button.

### Select Input and Output
1. Click `Select input folder` and choose the `input` root folder using the standard Windows folder dialog. The application scans for MMSI subfolders and populates the ship table.
2. Click `Select output folder` and choose the destination folder for the generated XML file.

### Choose a Ship
1. In the ship selection table, select the row for the MMSI you want to process. The ShipStaticData panel will show full static data and available date range.

### Select Time Interval
1. Use the `Start time` and `End time` date/time pickers (with seconds) to choose the processing interval. Defaults are the ship's min and max available dates.
2. Ensure `Start time < End time` and both are within available data range.

### Generate Route
1. Click the large `Process!` button to start processing.
2. After processing the app will show a blocking message box with the single generated filename (or an error message if processing failed).

## Related Sections
- [Input Data Preparation](input_data_preparation.md)
- [Ship Selection](ui_ship_selection.md)
- [Output Specification](output_specification.md)
