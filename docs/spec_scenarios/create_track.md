<!-- Source: /docs/user_manual/create_track.md -->
# Feature: Create Track
This feature covers generating an optimized track from AIS CSV records for a selected ship and time range.

## Positive Scenarios
### Scenario: Create track for selected ship and time range
Given the simulator user "Alice" is logged in
And the input root folder contains a vessel subfolder for MMSI "205196000"
And the ship "205196000" is selected
And a start and stop time with second resolution are chosen within available CSV timestamps
When they click "Create Track"
Then the system processes AIS CSV rows in the selected interval using default optimization parameters
And an ordered list of track points should appear in the UI
And the track should reflect expected vessel continuity

### Scenario: Create track with noisy data narrowed time window
Given the input root contains noisy AIS data for vessel "205196000"
And the simulator user "Bob" selects a narrow time window
When they click "Create Track"
Then the generated track should contain fewer spurious points due to the narrowed window
And processing should complete without errors

## Negative Scenarios
### Scenario: Reject track creation when no ship selected
Given the input root folder is selected
And no ship is selected
When the user clicks "Create Track"
Then an error message "No ship selected" should be displayed
And track creation should not start

### Scenario: Fail gracefully on malformed CSV rows
Given the selected time range includes CSV rows with missing required columns
When the user clicks "Create Track"
Then the system should skip malformed rows and continue processing
And the UI should show a warning "Some rows were ignored due to invalid format"

## Edge & Permission Scenarios
### Scenario: Create track with default optimization parameters
Given the simulator user "Alice" is logged in
And default optimization parameters are in place
When they run "Create Track"
Then the algorithm should apply: Minimum heading change 0.2 degrees, Minimum distance 5 meters, SOG change threshold 0.2 knots, ROT threshold 0.2 deg/s

### Scenario: Handle missing Heading or SOG values in records
Given CSV records in the selected range contain missing Heading or SOG
When track creation runs
Then missing Heading or SOG fields should default to 0 for WayPoint mapping
And points should still be generated where possible

### Cross-Feature Reference
- This feature depends on `ship_selection` and `getting_started` for ship choice and input root selection.
