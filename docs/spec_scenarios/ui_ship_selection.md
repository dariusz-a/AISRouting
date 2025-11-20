<!-- Source: /docs/user_manual/ui_ship_selection.md -->
# Feature: Ship Selection Control

Short description: Table control showing discovered MMSI folders and metadata; allows selecting one ship to process.

## Positive Scenarios
### Scenario: Display MMSI rows with metadata
  Given the `input` folder contains MMSI folder "205196000" with `2025-03-15.csv`
  When the folder is scanned
  Then the ship table should show a row with `MMSI` = "205196000" and `Interval [min, max]` covering `2025-03-15`

### Scenario: Selecting a ship populates ShipStaticData and enables controls
  Given the ship "205196000" exists in the table
  When the user selects that row
  Then the ShipStaticData panel should display static data and min/max dates
  And time-interval controls and `Process!` should be enabled when valid

## Negative Scenarios
### Scenario: Disable selection for MMSI with no CSV files
  Given an MMSI folder exists but contains no CSV files
  When the application populates the ship table
  Then that row should be disabled or shown greyed out

## Edge & Permission Scenarios
### Scenario: Sort the table by column
  Given multiple MMSI rows exist
  When the user sorts by `MMSI` column
  Then rows should be ordered ascending by MMSI by default
