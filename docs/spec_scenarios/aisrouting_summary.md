<!-- Source: /docs/user_manual/AISRouting_Summary.md -->
# Feature: AISRouting User Manual Summary
A short summary linking core flows: select input root, pick ship/time, create track, export XML.
<!-- NOTE: `docs/workflow/inputs/mock_data.md` referenced by the refinement guide is not present in the repository.
	Examples below are parameterized; replace Example rows with concrete mock-data values when `mock_data.md` is available. -->

## Positive Scenarios

### Scenario Outline: End-to-end create and export flow
	Given the application has an input root containing a vessel folder named "<mmsi>" and `route_waypoint_template.xml` is present and a generated track exists for "<mmsi>" covering start "<start>" and end "<end>".
	When the simulator user "<user_id>" uses the UI export flow by clicking the Export button and confirming the output folder "<output_path>".
	Then a file named "<mmsi>-<start>-<end>.xml" should be created at "<output_path>" and the file should contain a single `<RouteTemplate Name="<mmsi>">` element with an ordered list of `<WayPoint/>` elements where each waypoint includes mapped attributes (Name, Lat, Lon, Alt=0, Speed, ETA or 0, Delay=0, Mode, TrackMode="Track", Heading or 0, PortXTE=20, StbdXTE=20, MinSpeed=0, MaxSpeed).

### Examples:
 	| mmsi | start | end | user_id | output_path |
 	| mmsi-1 | ts_first | ts_last | scenario-user | export_tmp |

## Negative & Edge Scenarios

### Scenario: Fail export when template file missing
	Given the input root contains vessel folder "205196000" and a generated track exists and `route_waypoint_template.xml` is absent.
	When the user clicks the Export button and confirms an output folder.
	Then a visible error banner with text "route_waypoint_template.xml not found" should be shown and no file should be created.

### Scenario: Fail export when output path not writable
	Given a generated track exists for "205196000" and the user selects output folder "C:\\protected\\exports" which the application cannot create or write to.
	When the user attempts to export via the Export button.
	Then a visible error banner with text "Cannot write to output path: C:\\protected\\exports" should be shown and export should be aborted.

### Scenario: Prevent export when user lacks permission
	Given a logged-in user with id "user-no-export" who lacks export privileges and a generated track exists for "205196000".
	When the user attempts to initiate export by clicking the Export button.
	Then the Export button should be disabled and a tooltip or inline message "Insufficient privileges" should be visible and no file should be created.

## Cross-Feature Reference
- See `getting_started`, `ship_selection`, `create_track`, and `export_route` for detailed per-feature scenarios and preconditions.

<!-- TODO: Replace Example rows above with concrete values from `docs/workflow/inputs/mock_data.md` when that file is added or available. Also map `user_id` to mock-data user identifiers and replace control labels with data-testids where present. -->

## Critical Flow: End-to-end create and export flow — NHP Scenarios

Reference: the happy path is defined above in "Scenario Outline: End-to-end create and export flow"; do not duplicate that scenario here.

@critical @nhp @validation
Scenario Outline: Block export when template file is missing
	Given the application has an input root containing vessel folder "<mmsi>" and a generated track exists for "<mmsi>" and `route_waypoint_template.xml` is absent and the user "<user_id>" is logged in.
	When the user clicks the Export button and confirms the output folder "<output_path>".
	Then a visible error banner with text "route_waypoint_template.xml not found" is displayed and no file is created at "<output_path>".

Examples:
	| mmsi | user_id | output_path |
	| mmsi-1 | scenario-user | export_tmp |

@critical @nhp @auth
Scenario Outline: Prevent export for user without export privileges
	Given a generated track exists for "<mmsi>" and the logged-in user "<user_id>" lacks export privileges.
	When the user attempts to click the Export button.
	Then the Export button is disabled and a tooltip with text "Insufficient privileges" is visible and no export is initiated.

Examples:
	| mmsi | user_id |
	| mmsi-1 | user-no-export |

@critical @nhp @timeout
Scenario Outline: Show error when export fails due to write timeout
	Given a generated track exists for "<mmsi>" and `route_waypoint_template.xml` is present and the user "<user_id>" is logged in and the storage backend will return a write timeout for writes to "<output_path>".
	When the user initiates export and confirms the output folder "<output_path>".
	Then a visible error banner with text "Unable to save export: write timeout" is shown and no partial file remains in "<output_path>".

Examples:
	| mmsi | user_id | output_path |
	| mmsi-1 | scenario-user | export_tmp |

<!-- TODO: Replace the above Example rows with canonical identifiers from `docs/workflow/inputs/mock_data.md` when that file is provided. -->
