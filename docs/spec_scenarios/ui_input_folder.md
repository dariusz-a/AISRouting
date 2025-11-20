<!-- Source: /docs/user_manual/ui_input_folder.md -->
# Feature: Input Folder Control

Short description: Select and validate the `input` root folder containing per-MMSI subfolders.

## Positive Scenarios
### Scenario: Select valid input folder and scan MMSI subfolders
  Given the user opens the `Select input folder` dialog
  And chooses a folder that contains numeric-named subfolders
  When the folder is selected
  Then the application should scan for MMSI subfolders and populate the ship table

## Negative Scenarios
### Scenario: Show error when no MMSI subfolders found
  Given the chosen folder has no numeric-named subfolders
  When the user selects the folder
  Then the error `No valid MMSI subfolders found in selected folder.` should be displayed

## Edge & Permission Scenarios
### Scenario: Remember last-used path between runs
  Given the user selected an input path in a previous run
  When the application is started again
  Then the last-used path should be prefilled in the input control
