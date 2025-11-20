<!-- Source: /docs/user_manual/input_data_preparation.md -->
# Feature: Input Data Preparation

Short description: Requirements for folder structure and CSV/JSON formats expected by the application.

## Positive Scenarios
### Scenario: Recognize valid MMSI folder structure
  Given an `input` root folder exists
  And it contains a subfolder named "205196000" with `205196000.json` and `2025-03-15.csv`
  When the user selects the `input` folder
  Then the application should detect the MMSI subfolder and include it in the ship table

### Scenario: Accept CSV files with required schema
  Given a CSV file `2025-03-15.csv` in MMSI folder with rows matching `ShipDataOut` schema
  When the application reads the CSV
  Then records should be accepted and used for available date range calculations

## Negative Scenarios
### Scenario: Reject input when MMSI folders missing
  Given the selected `input` folder contains no numeric-named subfolders
  When the user selects that folder
  Then the error `No valid MMSI subfolders found in selected folder.` should be displayed

### Scenario: Reject CSV with invalid filename format
  Given a file `march15.csv` exists in the MMSI folder
  When the application scans filenames
  Then the file should be ignored and not included in available dates

## Edge & Permission Scenarios
### Scenario: Missing `<MMSI>.json` file produces TODO note
  Given the MMSI folder lacks `205196000.json`
  When the application scans the folder
  Then the situation should be flagged in the UI and a TODO note added to documentation for required static fields
