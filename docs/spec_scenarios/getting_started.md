<!-- Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started
This feature covers installation, launching the application, and preparing AIS input files for use with AISRouting.

## Background
Given a Windows instructor workstation
And the AISRouting installer or unpacked application files are available locally
And AIS input files are placed in the folder structure: outputFolder/<MMSI>/<MMSI>-yyyy-MM-dd.csv

## Positive Scenarios
### Scenario: Install the application from installer
  Given the instructor has the application installer available
  When they run the installer on the workstation
  Then the application executable and supporting folders should be present
  And the installer should offer to create a desktop shortcut

### Scenario: Launch the application and view main window
  Given the application is installed
  When the instructor launches the application from the desktop shortcut
  Then the AIS Routing main window should open
  And the navigation and ship list panel should be visible

### Scenario: Verify AIS input folder structure
  Given AIS CSV files exist for MMSI "123456789" in outputFolder/123456789/
  When the instructor checks the input folder
  Then files named "123456789-2025-01-01.csv" should be present
  And files should be UTF-8 encoded with CRLF line endings

## Negative Scenarios
### Scenario: Installation fails due to insufficient permissions
  Given the instructor runs the installer without administrative rights
  When the installer attempts to write to "C:\\Program Files\\AISRouting"
  Then an error message "Insufficient permission to install" should be displayed
  And the application should not be installed

### Scenario: Application does not start due to missing runtime
  Given the runtime dependency is not installed
  When the instructor launches the application
  Then a troubleshooting message about missing runtime should be shown
  And guidance to install required runtime should be provided

### Scenario: Missing AIS input files
  Given the input folder path is incorrect or empty
  When the instructor looks for MMSI folders
  Then a message "No AIS input files found in configured folder" should be displayed
  And the instructor should be instructed to verify the configured input folder path

## Edge & Permission Scenarios
### Scenario: Input files with wrong encoding are rejected
  Given a CSV file is encoded with a non-UTF-8 encoding
  When the application attempts to read the file
  Then a parse error should be reported indicating invalid encoding
  And the file should be skipped from processing

### Scenario: Non-instructor role handling TODO
  # TODO: Roles and permission matrix not defined in user manual. Add role-based install/launch scenarios when roles are specified.
  Given user "Guest" without instructor privileges is on the workstation
  When they attempt to run the installer or launch the instructor-only application
  Then a TODO: "Define expected behavior for non-instructor users" should be validated
