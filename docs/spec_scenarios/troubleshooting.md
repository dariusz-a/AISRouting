<!-- Source: /docs/user_manual/troubleshooting.md -->
# Feature: Troubleshooting
This feature lists common problems and resolutions when preparing, creating, and exporting routes.
<!-- NOTE: The refinement guide requires `docs/workflow/inputs/mock_data.md` for concrete test values. That file is not present. Replace placeholders below with mock-data identifiers when available. -->

## Positive Scenarios

### Scenario: Detect missing CSV files and instruct user
	Given the input root "input_root_example" is selected and the application is running and the ship selection UI is visible.
	When no CSV files are detected in any vessel subfolders within "C:\\data\\ais_root".
	Then the UI displays an inline instruction with text "No CSV files detected in selected input root" and guidance to verify the input folder structure.

## Negative & Edge Scenarios

### Scenario: Export fails due to permission or path issues
	Given a generated track exists for ship "mmsi-1" and the user selects output folder "export_protected" which cannot be created or written to.
	When the user clicks the Export button and confirms the selected output folder.
	Then a visible error banner with text "Cannot write to output path: C:\\protected\\exports" is shown and export is aborted with no file created.

### Scenario: Missing Heading or SOG values handled
	Given the selected time range contains CSV rows missing Heading and/or SOG values and the user "scenario-user" initiates track creation or export.
	When the processing runs over those rows.
	Then waypoints generated for records with missing Heading or SOG default those fields to 0 and a warning banner with text "Missing Heading/SOG values defaulted to 0" is displayed.

### Scenario: Recommend narrowing time range for noisy data
	Given noisy AIS data is present in vessel folder "mmsi-1" and the simulator user is preparing to create a track.
	When the user narrows the time window and runs Create Track.
	Then the resulting track contains fewer spurious points and the UI shows a note "Narrowing the time window reduced noise in the generated track".

## Cross-Feature Reference
- References `create_track` and `export_route` for remediation and expected behaviors.

<!-- TODO: Replace concrete paths and MMSI values above with identifiers from `docs/workflow/inputs/mock_data.md` when that file is added, and update UI control selectors to use data-testids where available. -->
