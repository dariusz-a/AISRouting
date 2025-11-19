<!-- Source: /docs/user_manual/selecting_ship_and_time_range.md -->
# Feature: Selecting a Ship and Time Range
This feature covers selecting a vessel by MMSI, choosing start/end times, and initiating route creation.

## Background
Given the application is launched and the Ship List panel is visible
And AIS input files exist for MMSI "246813579"

## Positive Scenarios
### Scenario: Select a vessel by MMSI
  Given the ship list includes MMSI "246813579" with metadata
  When the instructor clicks the MMSI "246813579"
  Then the vessel should be highlighted
  And available dates/time ranges detected from input files should be displayed

### Scenario: Choose valid start and end times
  Given the detected available bounds are from "2025-01-01T00:00:00Z" to "2025-01-01T12:00:00Z"
  When the instructor sets Start time "2025-01-01T01:00:00Z" and End time "2025-01-01T02:00:00Z"
  Then the time range should be accepted and the number of track points displayed

### Scenario: Initiate route creation
  Given a vessel and valid time range are selected
  When the instructor clicks "Create Route"
  Then a RouteTemplate should be generated in memory from AIS records in the range
  And a success message should be displayed

## Negative Scenarios
### Scenario: End time before Start time is rejected
  Given Start time "2025-01-01T05:00:00Z" is set
  When the instructor sets End time "2025-01-01T04:00:00Z"
  Then a validation error "End time must be after Start time" should be shown
  And route creation should be prevented

### Scenario: Time outside available bounds is rejected
  Given available data ranges do not include "2024-12-31T23:00:00Z"
  When the instructor sets a Start time outside bounds
  Then a validation error "Selected time is outside available data range" should be shown

## Edge & Permission Scenarios
### Scenario: Rapid consecutive selection updates
  Given the instructor rapidly changes Start and End times
  When the system validates each update
  Then the final accepted time range should be the last valid values

### Scenario: Missing ship list due to no input files
  Given no MMSI folders exist in the input folder
  When opening the Ship List panel
  Then a message "No ships available" should be displayed
  And instructions to add AIS input files should be shown
