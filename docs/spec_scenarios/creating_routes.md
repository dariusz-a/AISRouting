<!-- Source: /docs/user_manual/creating_routes.md -->
# Feature: Creating Routes and Output XML
This feature covers generating a RouteTemplate from AIS track data, mapping fields, preview behavior, and saving XML output.

## Background
Given AIS CSV files are available and validated and a vessel and a valid time range are selected.

## Positive Scenarios

### Scenario: Generate RouteTemplate from AIS track successfully
  Given the instructor user is on the Create Route page with AIS CSV files for MMSI "135792468" present in folder `outputFolder/135792468/` and a valid time range 2025-01-01T00:00:00Z to 2025-01-01T12:00:00Z selected and the instructor has permission to generate routes.
  When the instructor clicks the "Create Route" button in the Ship Actions toolbar and confirms generation when prompted.
  Then the application reads AIS CSV records for MMSI "135792468" within the selected time span and constructs an in-memory RouteTemplate containing WayPoint elements where each waypoint is mapped using the rules: Name=>MMSI and Lat=>Latitude and Lon=>Longitude and Alt=>0 and Speed=>SOG and ETA=>EtaSecondsUntil and Delay=>0 and Mode=>SetWaypointMode result and TrackMode=>"Track" and Heading=>Heading or 0 and PortXTE=>20 and StbdXTE=>20 and MinSpeed=>0 and MaxSpeed=>GetMaxShipSpeed result, and a success banner with text "Route generated successfully" is visible.

### Scenario: Save generated RouteTemplate as XML to default destination
  Given a RouteTemplate exists in memory for MMSI "135792468" after generation and the Save Route dialog is open showing default path `outputFolder/135792468/135792468-route-2025-01-01.xml` and the instructor has write permission to that folder.
  When the instructor clicks the "Save Route" button and confirms the file name and destination in the Save dialog and selects "Save".
  Then an XML file named "135792468-route-2025-01-01.xml" is written to `outputFolder/135792468/` encoded in UTF-8 with CRLF line endings and the file contents have a top-level RouteTemplate element matching the route_waypoint_template.xml structure and an informational toast with text "Route saved to outputFolder/135792468/135792468-route-2025-01-01.xml" is visible and the file remains present after refreshing the file view.

## Negative & Edge Scenarios

### Scenario: Prevent save when destination file exists and user declines overwrite
  Given a RouteTemplate exists in memory for MMSI "135792468" and the destination `outputFolder/135792468/135792468-route-2025-01-01.xml` already exists and the Save Route dialog is open showing the existing file.
  When the instructor clicks "Save Route" and in the overwrite confirmation dialog selects "Do not overwrite".
  Then the save operation is cancelled, an inline alert with text "Save cancelled: file already exists" is visible in the Save dialog, and no file is modified on disk.

### Scenario: Route generation fails for missing AIS records in range
  Given the instructor user is on the Create Route page with AIS CSV files for MMSI "135792468" but no records exist within the selected time range 2025-01-02T00:00:00Z to 2025-01-02T01:00:00Z.
  When the instructor clicks the "Create Route" button.
  Then an error banner with exact text "No AIS records found in selected range" is visible and no RouteTemplate is created in memory.

### Scenario: Save to location without write permission shows explicit error
  Given a RouteTemplate exists in memory for MMSI "135792468" and the instructor opens the Save Route dialog and selects destination `C:\ProtectedFolder\` where the instructor lacks write permission.
  When the instructor clicks "Save" in the Save Route dialog.
  Then an error dialog with exact text "Insufficient permission to save file" is displayed with a link labeled "Choose another location" and the RouteTemplate remains unsaved in memory.

### Scenario: Generated route contains skipped records due to missing coordinates
  Given AIS CSV file for MMSI "135792468" contains 10 records within the selected range where 2 records are missing Latitude or Longitude and the instructor is on the Create Route page.
  When the instructor clicks "Create Route".
  Then the application creates a RouteTemplate with 8 WayPoint elements, a warning banner with exact text "2 records skipped: missing coordinates" is visible and the Diagnostics view lists the skipped record line numbers with status "Skipped - missing coordinates".

### Scenario: Preview/edit not available in this release
  Given the application build for this release does not include the interactive preview UI and a RouteTemplate exists in memory for MMSI "135792468".
  When the instructor clicks the "Preview/Edit Waypoints" control in the Route Actions panel.
  Then a modal with text "Preview/edit not available" is shown and an OK button closes the modal.

## Coverage Notes
- All scenarios reference MMSI "135792468" consistent with prior examples in this feature and use explicit file paths and timestamps so automated tests can prepare the filesystem state beforehand.
- Role: the actor is the instructor; write and generate permissions are explicitly stated where required.
- Each scenario uses exactly one Given, one When, and one Then and chains additional state or actions inline with "and" to remain unambiguous and testable.
- Negative cases cover missing data, existing-file overwrite flow, permission denial, and feature-not-implemented preview behavior.
