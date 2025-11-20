<!-- Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started
This feature explains initial setup: placing `route_waypoint_template.xml`, selecting input data root, and starting the UI.

## Positive Scenarios
### Scenario: Install and start AISRouting UI
Given the user has unpacked the AISRouting distribution to a working directory
And `route_waypoint_template.xml` is placed in the application root
When they start the desktop UI
Then the application should launch and show the main screen

### Scenario: Select input data root with vessel subfolders
Given the input root contains vessel subfolders each with CSV files
When the user opens the "Input Folder" selector and chooses the root
Then the UI should list vessel subfolder names in the combo box

## Negative Scenarios
### Scenario: Fail when input root empty
Given the selected input root contains no vessel subfolders
When the user opens the "Input Folder" selector
Then an error or empty list should be shown and a warning message displayed

## Edge & Permission Scenarios
### Scenario: Application requires `route_waypoint_template.xml` for export
Given `route_waypoint_template.xml` is missing
When the user prepares to export
Then a warning should be displayed that exports will fail until the template is added

### Cross-Feature Reference
- Ship selection and export features depend on selecting input root here.
