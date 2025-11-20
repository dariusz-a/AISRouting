<!-- Source: /docs/user_manual/application_overview.md -->
# Feature: AisToXmlRouteConvertor

Short description: Convert AIS route data files into XML route files compatible with navigation systems.

## Positive Scenarios
### Scenario: Convert imported AIS CSV to XML successfully
  Given the application is launched
  And the `input` folder contains MMSI subfolder "205196000" with valid CSV and JSON files
  And the `output` folder is writable
  When the user imports the AIS data file for MMSI "205196000"
  And they verify the data preview and click `Convert`
  Then an XML file should be created in the selected output folder
  And a confirmation message `Track generated successfully:` should be displayed with the filename

## Negative Scenarios
### Scenario: Fail conversion when output folder not writable
  Given the application is launched
  And the selected `output` folder is not writable
  When the user attempts to convert imported AIS data
  Then an error message `Selected folder is not writable. Choose a different folder.` should be displayed
  And no XML file should be created

### Scenario: Reject conversion when data preview shows invalid mapping
  Given the imported AIS data preview is missing required fields
  When the user clicks `Convert`
  Then conversion should be blocked and an error shown indicating missing or invalid fields

## Edge & Permission Scenarios
### Scenario: Handle processing failure with error message
  Given the application is launched
  And a transient processing error occurs during conversion
  When the user clicks `Convert`
  Then a failure message `Processing failed:` with error details should be shown
  And no partial XML file should remain in the output folder
