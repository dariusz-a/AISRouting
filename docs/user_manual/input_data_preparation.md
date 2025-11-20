# Input Data Preparation

## Overview
This section explains how the tool expects the input data to be organized and how to prepare the `input` and `output` folders.

## Prerequisites
- The `input` folder containing source data organized by MMSI subfolders
- An `output` folder where generated XML route files will be written

## Step-by-Step Instructions
### Folder Structure
1. Ensure the root `input` folder contains subfolders named by MMSI (e.g., `205196000/`).
2. Each MMSI folder must include a `<MMSI>.json` file with ship static data, and one or more `<YYYY-MM-DD>.csv` files containing AIS time-series data.
3. Create or choose an `output` folder where the application will write generated XML route files.

### CSV File Format
Each CSV row must match the `ShipDataOut` schema described in `docs/idea/idea.md`:
- `Time` (seconds since date-only T0)
- `Latitude` and `Longitude`
- `NavigationalStatusIndex`, `ROT`, `SOG`, `COG`, `Heading`, `Draught`, `DestinationIndex`, `EtaSecondsUntil`

### Preparing Data
1. Validate that CSV filenames use `YYYY-MM-DD.csv` format.
2. Confirm that the `<MMSI>.json` exists for each ship folder and contains required static fields.
3. (Optional) Run any preprocessing you normally use to fill missing fields or normalize timestamps — for this project you may assume the source data is already processed.

## Tips and Best Practices
- Keep input and output folders on fast local storage for quicker processing.
- Use consistent naming and versioning for test datasets.

## Related Sections
- [Application Overview](application_overview.md)
