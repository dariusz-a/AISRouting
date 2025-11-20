# Ship Selection Control

## Overview
Specification for the ship selection table that displays discovered MMSI folders and related metadata.

## Control Details
- **Control Type**: Selectable table/grid
- **Columns**:
  - `MMSI` (string)
  - `Size (MB)` (numeric) — sum of file sizes inside MMSI folder, displayed in megabytes
  - `Interval [min, max]` (dates) — earliest and latest CSV filename dates available for that MMSI
  - `Length (m)` — from `<MMSI>.json` if available
  - `Width (m)` — from `<MMSI>.json` if available
- **Selection**: Whole-row selection; single-select only
- **Default selection behavior**: None selected by default; user must choose a row before generating a track
- **On selection**:
  - Populate `ShipStaticData` panel with static data and min/max dates
  - Enable time-interval controls and `Create Track` button
- **Validation**: Disable selection for rows where no CSV files exist (show greyed row or warning)
- **Sorting**: Allow sorting by any column (default sort by `MMSI` ascending)
- **Tooltip/help**: `Select a ship (MMSI) to view available dates and static data.`
