<!-- Source: /docs/user_manual/ship_selection.md -->
# Feature: Ship Selection and Static Data
This feature covers selecting a ship from input data, viewing static metadata, and setting the available time range.
<!-- NOTE: The refinement guide requires `docs/workflow/inputs/mock_data.md` for concrete test values. That file is not present. Examples below are parameterized; replace Example rows with concrete mock-data values when `mock_data.md` is available. -->

## Positive Scenarios

### Scenario: Populate ship combo box from static files or folder names
	Given the input root "<input_root>" contains vessel subfolders where folder "205196000" has a static data file with Name="Sea Explorer" and folder "205196001" has no static name.
	When the application opens the ship selection combo box and the user selects "<input_root>".
	Then the combo box lists "Sea Explorer" and "205196001" and the values are selectable.

### Scenario: Display static data after ship selection
	Given input root "<input_root>" contains vessel folder "205196000" with static attributes including Name="Sea Explorer" and MMSI="205196000".
	When the user selects "Sea Explorer" in the ship combo box.
	Then the static attributes are displayed in the large TextBox widget including Name and MMSI.

### Scenario: Default start/stop time values set from file timestamps
	Given vessel folder "205196000" contains CSV files with earliest timestamp "20250315T000000" and latest "20250316T000000".
	When the user selects vessel "205196000".
	Then the StartValue defaults to "20250315T000000" and the StopValue defaults to "20250316T000000" plus 24 hours ("20250317T000000").

## Negative & Edge Scenarios

### Scenario: Show fallback when static name missing
	Given a vessel folder "205196001" lacks a static name in its static file and the application lists vessels from "<input_root>".
	When the ship combo is shown.
	Then the folder name "205196001" is used as the displayed ship name in the combo.

### Scenario: Validate Min/Max date range before creation
	Given vessel "205196000" has CSV files with inconsistent timestamps causing Min > Max.
	When the user inspects the Min/Max pickers.
	Then a validation warning with text "Invalid time range" is displayed and the Create Track button is disabled until corrected.

### Scenario: Use seconds resolution for time pickers
	Given vessel "205196000" is selected and the UI shows start/stop time pickers.
	When the user opens the start time picker and sets seconds to "00".
	Then the picker accepts seconds resolution and the selected timestamp shows seconds precision.

### Scenario: Ship selection unavailable when input root missing
	Given the input root path is not accessible or does not exist.
	When the user opens the ship selection combo box.
	Then the combo box shows an error state with text "Input root not accessible" and selection is disabled.

## Cross-Feature Reference
- Time range selection is used by `create_track` for determining which AIS records to process.

<!-- TODO: Replace placeholder `<input_root>` Example values with identifiers from `docs/workflow/inputs/mock_data.md` when available. Use data-testids for combo box and time pickers when present. -->
