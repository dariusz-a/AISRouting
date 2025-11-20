<!-- Source: /docs/user_manual/ship_selection.md -->
# Feature: Ship Selection and Static Data
This feature covers selecting a ship from input data, viewing static metadata, and setting the available time range.

## Positive Scenarios
### Scenario: Populate ship combo box from static files
Given the input root folder contains vessel subfolders with optional static files
When the application opens the ship selection combo box
Then the list should populate using names from static files or fallback to folder names

### Scenario: Display static data after ship selection
Given a vessel with static data file exists
When the user selects the vessel in the combo box
Then the static attributes should display in the large TextBox widget

### Scenario: Default start/stop time values set from file timestamps
Given CSV filenames in the vessel folder include timestamps
When the vessel is selected
Then the StartValue should default to the first filename's timestamp
And the StopValue should default to the last filename's timestamp plus 24 hours

## Negative Scenarios
### Scenario: Show fallback when static name missing
Given a vessel folder lacks static name in static file
When listed in the combo box
Then the folder name should be used as the displayed ship name

### Scenario: Validate Min/Max date range before creation
Given the vessel's CSV files contain an invalid date range
When the user inspects the Min/Max pickers
Then a validation warning should be displayed preventing track creation until corrected

## Edge & Permission Scenarios
### Scenario: Use seconds resolution for time pickers
Given a vessel is selected
When the user sets start and stop times
Then the pickers should allow selection at second resolution

### Cross-Feature Reference
- Time range selection is used by `create_track` for determining which AIS records to process.
