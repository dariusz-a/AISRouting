# Input Folder Control

## Overview
Specification for the `Select input folder` control used to choose the `input` folder containing source AIS data.

## Control Details
- **Label**: `Select input folder`
- **Type**: Windows folder picker dialog (standard system dialog)
- **Default**: Last-used path or blank on first run
- **Validation**: Dialog should verify selected folder contains at least one subfolder whose name looks like an MMSI (numeric); if none found, show an error.
- **Behavior after selection**:
  - Scan selected folder for MMSI subfolders.
  - Populate ship dropdown with discovered MMSI values.
  - Extract min and max available dates from CSV filenames and display in ShipStaticData panel.
- **Error messages**: `No valid MMSI subfolders found in selected folder.`
- **Persistence**: Remember last-used path between runs.
- **Tooltip/help**: `Select the root folder that contains per-MMSI subfolders with AIS data (one <MMSI>.json and YYYY-MM-DD.csv files).`
- **Drag-and-drop**: Not required; keep UI simple (no drag-and-drop).
