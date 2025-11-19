<!-- Source: /docs/user_manual/input_data.md -->
# Feature: Input Data and CSV Format
This feature documents required AIS CSV file placement, naming, and field formats consumed by AISRouting.

## Positive Scenarios
### Scenario: Place AIS CSV files in correct folder structure
  Given AIS CSV files are prepared for MMSI "987654321"
  When the instructor places files under "outputFolder/987654321/"
  Then files named "987654321-2025-01-01.csv" should be discoverable by the application

### Scenario: CSV fields map correctly with example line
  Given a CSV line "0,55.884242,12.759092,0,,,,,,," is present
  When the application reads the line
  Then the values should be parsed into 11 fields in the expected order
  And empty fields should map to nullable values

### Scenario: Validate encoding and line endings
  Given a CSV file is present
  When the instructor verifies the file
  Then the file should be UTF-8 encoded and use CRLF line endings

## Negative Scenarios
### Scenario: CSV with wrong column count is rejected
  Given a CSV line with fewer or more than 11 fields exists
  When the application parses the line
  Then a parse error should be reported for that line
  And the offending line should be skipped

### Scenario: Non-UTF-8 encoded file is flagged
  Given a CSV file uses a non-UTF-8 encoding
  When the application attempts to read the file
  Then the file should be reported as invalid encoding
  And it should be skipped from processing

## Edge & Permission Scenarios
### Scenario: Empty fields map to defaults
  Given a CSV record has empty SOG and Heading fields
  When the application generates a WayPoint
  Then Speed should be set to 0 and Heading set to 0

### Scenario: Decimal format uses dot as separator
  Given latitude and longitude fields use "," as decimal separator
  When the application parses the fields
  Then a format error should be raised and the line skipped
