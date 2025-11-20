<!-- Refined per workflow/2.1_precise_bdd_spec_scenarios.md guidelines -->
# Feature: AisToXmlRouteConvertor User Manual Summary

High-level quick references for selecting input/output folders, choosing a ship (MMSI), defining a time interval, and generating the XML route. (Mock data file `docs/workflow/inputs/mock_data.md` is currently missing; scenarios exclude role/permission IDs pending its addition.)

### Scenario: Select a valid input folder populates ship list
  Given the application is open with no input folder selected and the ship table empty
  When the user opens the `Select input folder` dialog and chooses a folder containing at least one MMSI-named subfolder with CSV files and a matching `<MMSI>.json` and confirms selection
  Then the ship table populates with discovered MMSI rows including MMSI, Size (MB), Interval [min, max], Length (m), Width (m) and the last-used input path is persisted
  And the tooltip for the control displays text `Select the root folder that contains per-MMSI subfolders with AIS data (one <MMSI>.json and YYYY-MM-DD.csv files).`

### Scenario: Input folder with no MMSI subfolders shows error
  Given the application is open with no input folder selected
  When the user selects a folder that has zero subfolders whose names are numeric MMSI patterns and confirms selection
  Then an inline error message `No valid MMSI subfolders found in selected folder.` is shown and the ship table remains empty
  And the process button stays disabled

### Scenario: Select writable output folder displays chosen path
  Given a valid input folder is already selected with populated ship rows and no output folder chosen
  When the user opens the `Select output folder` dialog and chooses a writable folder and confirms
  Then the selected output path is displayed in the UI and stored as last-used
  And no error message is shown

### Scenario: Non-writable output folder shows error
  Given a valid input folder is selected and the ship table populated and no output folder chosen
  When the user selects a folder that is not writable and confirms
  Then an inline error message `Selected folder is not writable. Choose a different folder.` is shown and the output path field remains blank
  And the process button stays disabled

### Scenario: Ship selection enables time pickers and process prerequisites
  Given a valid input folder and output folder are selected and the ship table lists at least one MMSI unselected
  When the user selects a single MMSI row in the ship table
  Then the Start time and End time pickers display that ship's minimum and maximum available times and become editable and the `Process!` button is conditionally enabled subject to time validation
  And ShipStaticData panel shows length, width, and interval

### Scenario: Invalid time range disables process button
  Given a ship is selected with time pickers showing default min/max values and the `Process!` button enabled
  When the user sets the End time to a value earlier than the Start time
  Then an inline validation message `Start time must be before End time` is displayed and the `Process!` button becomes disabled
  And the previous valid values are retained in internal state until corrected

### Scenario: Time outside available range blocks processing
  Given a ship is selected with min/max interval displayed and the time pickers at default values
  When the user sets the Start time to a value earlier than the ship's minimum available time
  Then an inline validation message `Selected time is outside available data range` is displayed and the `Process!` button remains disabled
  And no XML generation is attempted

### Scenario: Process successfully generates XML file
  Given a valid input folder and writable output folder are selected and a ship is selected and Start time and End time form a valid within-range interval
  When the user presses the `Process!` button
  Then a blocking success message box with text starting `Track generated successfully: ` followed by a filename matching `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml` is shown and the button returns to enabled state after dismissal
  And the XML file exists in the output folder with waypoint structure per `route_waypoint_template.xml`

### Scenario: Process unavailable until all prerequisites selected
  Given only the input folder is selected with ship rows populated and no ship row chosen and no output folder selected and default time pickers hidden
  When the user views the `Process!` button state
  Then the `Process!` button is disabled and a tooltip or aria-description indicates missing selections (ship and output folder) and no processing occurs
  And enabling conditions update only after required selections

### Scenario: Processing failure displays error details
  Given a valid input folder, writable output folder, selected ship, and valid time interval are set and an internal processing error (e.g., malformed CSV parse) is triggered
  When the user presses the `Process!` button
  Then a blocking failure message box with text starting `Processing failed: ` followed by error details is shown and no XML file is created
  And the user can correct input or retry after dismissal

### Scenario: Input selection re-populates ship table and resets dependent controls
  Given a valid input folder, writable output folder, selected ship, and valid time interval are already set
  When the user reopens the `Select input folder` dialog and chooses a different valid folder with a different set of MMSI subfolders and confirms
  Then the ship table refreshes with the new MMSI list and any previously selected ship is cleared and time pickers revert to disabled until a new ship is selected
  And the `Process!` button becomes disabled until prerequisites are re-established

### Scenario: Ship row without CSV files is disabled
  Given a valid input folder is selected and ship table displays at least one MMSI row whose folder lacks CSV files
  When the user attempts to select that MMSI row
  Then the row is visibly greyed or selection is prevented and a tooltip indicates missing CSV data and time pickers remain disabled
  And the `Process!` button remains disabled

### Scenario: Sorting ship table by MMSI ascending default
  Given a valid input folder is selected populating multiple MMSI rows
  When the user first views the ship table without applying manual sorting
  Then rows are ordered by `MMSI` ascending and the sort indicator shows ascending on the `MMSI` column header
  And changing sort on another column updates ordering immediately

### Scenario: Output file naming pattern correctness
  Given a valid processing run just completed with success message showing generated filename
  When the user inspects the output folder contents
  Then the filename conforms exactly to `<MMSI>_<startYYYYMMDDTHHMMSS>_<endYYYYMMDDTHHMMSS>.xml` and matches the success message
  And no additional files are created

### Scenario: Success message does not auto-open output folder
  Given a successful processing run produced an XML file
  When the success message box is displayed
  Then the output folder is not auto-opened in the OS file explorer and only the single filename is shown in the message box
  And dismissing the box leaves the main UI state unchanged

