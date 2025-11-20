# Troubleshooting

## Overview
Common issues QA engineers may encounter when using AisToXmlRouteConvertor and how to resolve them.

## Issues and Solutions
- `No valid MMSI subfolders found in selected folder.`
  - Ensure the selected `input` folder contains subfolders named by MMSI (numeric) each containing a `<MMSI>.json` and CSV files.

- `Selected folder is not writable. Choose a different folder.`
  - Verify filesystem permissions for the selected `output` folder.

- `Start time must be before End time` or `Selected time is outside available data range`.
  - Select times within the ship's available interval displayed in the ShipStaticData panel.

- `Processing failed: <error details>`
  - Confirm that the CSV files in the input folder follow the expected `ShipDataOut` schema and filenames are in `YYYY-MM-DD.csv` format.
  - Check that the selected time interval contains data.

## Diagnosing Data Problems
- Use small, known-good test datasets (version-controlled) to reproduce problems.
- Verify the `<MMSI>.json` file exists and CSV files contain rows with `Time`, `Latitude`, and `Longitude`.
