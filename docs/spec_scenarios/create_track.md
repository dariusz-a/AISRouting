<!-- Source: /docs/user_manual/create_track.md -->
# Feature: Create Track
This feature covers generating an optimized track from AIS CSV records for a selected ship and time range.
<!-- NOTE: The refinement guide requires `docs/workflow/inputs/mock_data.md` for concrete test values. That file is not present in the repository.
	Examples below are parameterized; replace Example rows with concrete mock-data values when `mock_data.md` is available. -->

## Positive Scenarios

### Scenario Outline: Create track for selected ship and time range
	Given the application has an input root "<input_root>" containing a vessel folder named "<mmsi>" and the simulator user "<user_id>" is logged in and the UI shows available CSV timestamps from "<first_ts>" to "<last_ts>".
	When the user selects vessel "<mmsi>" and sets start "<start>" and stop "<end>" with second resolution and clicks "Create Track".
	Then the system processes AIS CSV rows in the selected interval using default optimization parameters and an ordered list of track points is shown in the UI and the track reflects expected vessel continuity.

### Examples:
	| input_root | mmsi | user_id | first_ts | last_ts | start | end |
	| input_root_example | mmsi-1 | scenario-user | ts_first | ts_last | ts_first | ts_last |

### Scenario: Create track with noisy data and narrowed time window
	Given the input root "C:\\data\\ais_root" contains noisy AIS CSV rows for vessel "205196000" and the simulator user "scenario-user" is logged in.
	When the user selects vessel "205196000" and sets a narrow start/stop window within noisy interval and clicks "Create Track".
	Then the generated track contains fewer spurious points due to the narrowed window and processing completes without errors and a completion status is displayed.

## Negative & Edge Scenarios

### Scenario: Reject track creation when no ship selected
	Given the input root "C:\\data\\ais_root" is selected and the UI has no vessel selected and the simulator user "scenario-user" is logged in.
	When the user clicks "Create Track".
	Then an inline error message with text "No ship selected" should be visible and track creation does not start.

### Scenario: Fail gracefully on malformed CSV rows
	Given the selected time range includes CSV rows with missing latitude/longitude or required columns and the simulator user "scenario-user" is logged in.
	When the user clicks "Create Track".
	Then the system skips malformed rows, processing continues for valid rows, and a warning banner with text "Some rows were ignored due to invalid format" is displayed.

### Scenario: Handle missing Heading or SOG values in records
	Given CSV records in the selected range contain missing Heading or SOG values and the simulator user "scenario-user" is logged in.
	When the user clicks "Create Track".
	Then missing Heading or SOG fields default to 0 for WayPoint mapping and points are generated where possible and a data-quality note is shown.

### Scenario: Prevent track creation when input root empty
	Given the selected input root "C:\\empty\\root" contains no vessel subfolders and the simulator user "scenario-user" is logged in.
	When the user opens the ship selection combo box and attempts to click "Create Track".
	Then the ship combo shows an empty list and an inline warning "No vessels found in input root" is displayed and the Create Track action is disabled.

### Scenario: Create track unavailable for user without permission
	Given a logged-in user "user-no-create" without create-track privileges and an available vessel "205196000".
	When the user views the UI controls for creating a track.
	Then the "Create Track" button is disabled and a tooltip with text "Insufficient privileges" is shown.

## Cross-Feature Reference
- This feature depends on `ship_selection` for vessel selection and `getting_started` for input root setup.

<!-- TODO: Replace Example values with concrete mock-data identifiers from `docs/workflow/inputs/mock_data.md` when that file is provided. Also replace UI control labels with data-testids if available. -->
