<!-- Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started

Short description: Steps to launch the application, select folders, choose a ship and time interval, and generate a route.

## Positive Scenarios
### Scenario: Launch application and view main controls
  Given the application executable is present on a Windows machine
  When the user starts the application
  Then the main window should display controls to select input and output folders, a ship selection table, ShipStaticData panel, time pickers, and the `Process!` button

### Scenario: Select input and output folders and populate ship table
  Given the `input` folder contains MMSI subfolders
  And the `output` folder is writable
  When the user selects the `input` and `output` folders
  Then the application should scan for MMSI subfolders and populate the ship table
  And the selected output path should be displayed

### Scenario: Choose a ship and generate route
  Given the ship table contains a row for MMSI "205196000"
  And the user selects that row
  And valid `Start time` and `End time` are chosen within available range
  When the user clicks `Process!`
  Then the application should process the data and show a message box with the generated filename

## Negative Scenarios
### Scenario: Prevent processing without ship selection
  Given the `input` and `output` folders are selected
  And no ship is selected in the ship table
  When the user attempts to click `Process!`
  Then the `Process!` button should remain disabled

### Scenario: Reject invalid time interval
  Given a ship with available data from `2025-03-15` to `2025-03-16`
  When the user sets `Start time` after `End time`
  Then an inline validation message `Start time must be before End time` should be shown

## Edge & Permission Scenarios
### Scenario: Handle processing failure gracefully
  Given valid selections are made
  And a transient backend error occurs during processing
  When the user clicks `Process!`
  Then the application should show `Processing failed:` with details and not create a partial file
