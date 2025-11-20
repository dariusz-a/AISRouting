<!-- Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started
This feature explains initial setup: placing `route_waypoint_template.xml`, selecting input data root, and starting the UI.
<!-- NOTE: The refinement guide requires `docs/workflow/inputs/mock_data.md` for concrete test values. That file is not present. Examples below are parameterized and include concrete example paths; replace with mock_data values when available. -->

## Positive Scenarios

### Scenario Outline: Install and start AISRouting UI
	Given the AISRouting distribution is unpacked at "<install_path>" and `route_waypoint_template.xml` is placed at "<install_path>\\route_waypoint_template.xml".
	When the user executes the desktop application start action (double-click or run executable) from "<install_path>".
	Then the application launches and the main screen is visible with the top-level navigation and the Input Folder selector control present.

### Examples:
	| install_path |
	| input_root_example |

### Scenario: Select input data root with vessel subfolders
	Given the file system path "C:\\data\\ais_root" contains vessel subfolders each with CSV files and the application is running and shows the Input Folder selector.
	When the user opens the Input Folder selector and chooses "C:\\data\\ais_root".
	Then the ship selection combo box lists vessel subfolder names and the first vessel is selectable.

## Negative & Edge Scenarios

### Scenario: Fail when input root empty
	Given the file system path "C:\\empty\\root" contains no vessel subfolders and the application is running.
	When the user opens the Input Folder selector and selects "C:\\empty\\root".
	Then the ship selection combo box shows an empty list and an inline warning with text "No vessels found in input root" is displayed.

### Scenario: Application warns when `route_waypoint_template.xml` missing
	Given the application root does not contain `route_waypoint_template.xml` and the application is running.
	When the user opens the Export dialog or prepares to export a generated track.
	Then a warning banner with text "route_waypoint_template.xml not found — exports will fail until template is added" is shown.

### Scenario: Prevent start when executable missing or corrupted
	Given the install path "C:\\apps\\AISRouting" lacks a valid start executable or it is corrupted.
	When the user tries to start the application from "C:\\apps\\AISRouting".
	Then a visible error dialog with text "Application failed to start: executable missing or corrupted" is displayed.

## Cross-Feature Reference
- Ship selection and export features depend on selecting a valid input root here.

<!-- TODO: Replace concrete example paths with identifiers from `docs/workflow/inputs/mock_data.md` when available. Use data-testids for UI controls when present. -->
