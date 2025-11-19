<!-- Source: /docs/user_manual/creating_routes.md -->
# Feature: Creating Routes and Output XML
This feature covers generating a RouteTemplate from AIS track data, mapping fields, preview behavior, and saving XML output.

## Background
Given AIS CSV files are available and validated
And a vessel and a valid time range are selected

## Positive Scenarios
### Scenario: Generate RouteTemplate from AIS track
  Given the instructor has selected MMSI "135792468" and a valid time range
  When they click "Create Route"
  Then the application should read AIS CSV records for that MMSI and time span
  And construct a RouteTemplate with WayPoint elements using mapping rules
  And a success message should be shown

### Scenario: Save generated RouteTemplate as XML
  Given a RouteTemplate exists in memory for MMSI "135792468"
  When the instructor clicks "Save Route" and confirms file name
  Then an XML file named "135792468-route-2025-01-01.xml" should be written
  And the file should be UTF-8 encoded with CRLF line endings

## Negative Scenarios
### Scenario: Save fails due to existing file without overwrite
  Given a file "135792468-route-2025-01-01.xml" already exists at the destination
  When the instructor attempts to save and chooses not to overwrite
  Then the save operation should be cancelled and no file should be written

### Scenario: Route generation fails for missing AIS records
  Given no AIS records exist in the selected time range
  When the instructor clicks "Create Route"
  Then an error message "No AIS records found in selected range" should be displayed
  And no RouteTemplate should be created

## Edge & Permission Scenarios
### Scenario: Save to location without write permission
  Given the instructor selects a destination where they lack write permission
  When they attempt to save the XML file
  Then an error message "Insufficient permission to save file" should be displayed

### Scenario: Preview not available in this release
  Given the application does not provide an interactive preview UI in this release
  When the instructor requests to edit waypoints
  Then a message "Preview/edit not available" should be shown
