<!-- Source: /docs/user_manual/output_specification.md -->
# Feature: Output Specification

Short description: Defines output file naming, location, and content structure for generated XML route files.

## Positive Scenarios
### Scenario: Generate XML with expected filename pattern
  Given processing runs for MMSI "205196000" from `2025-03-15T00:00:00` to `2025-03-15T12:00:00`
  When the application finishes processing
  Then a file named `205196000_20250315T000000_20250315T120000.xml` should exist in the selected output folder

### Scenario: XML content follows template
  Given the generated XML file exists
  When opened
  Then waypoints should include time, latitude and longitude per `route_waypoint_template.xml`

## Negative Scenarios
### Scenario: Fail when unable to write file
  Given the output folder becomes non-writable during processing
  When the application attempts to save the XML
  Then a failure message `Selected folder is not writable. Choose a different folder.` should be displayed

## Edge & Permission Scenarios
### Scenario: No automatic folder open after generation
  Given processing completed successfully
  When the application displays the success message
  Then the application should not auto-open the output folder
