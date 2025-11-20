<!-- Source: /docs/user_manual/ui_output_folder.md -->
# Feature: Output Folder Control

Short description: Choose the destination folder for generated XML files and validate writability.

## Positive Scenarios
### Scenario: Select writable output folder
  Given the user opens the `Select output folder` dialog
  When they choose a writable folder
  Then the selected path should be displayed and used for file generation

## Negative Scenarios
### Scenario: Reject non-writable folder
  Given the user selects a folder without write permissions
  When the folder is selected
  Then an error `Selected folder is not writable. Choose a different folder.` should be displayed

## Edge & Permission Scenarios
### Scenario: Remember last-used output path
  Given a previous run selected an output path
  When the application starts
  Then the last-used output path should be remembered
