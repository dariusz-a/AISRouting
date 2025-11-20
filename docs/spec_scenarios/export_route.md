<!-- Source: /docs/user_manual/export_route.md -->
# Feature: Exporting Routes
This feature describes exporting a generated track into an XML file using the `route_waypoint_template.xml`.
<!-- NOTE: The refinement guide requires `docs/workflow/inputs/mock_data.md` for concrete test values. That file is not present. Examples below are parameterized; replace Example rows with concrete mock-data values when `mock_data.md` is available. -->

## Positive Scenarios

### Scenario Outline: Export generated track to XML with valid output path
	Given a generated track exists for ship "<mmsi>" and `route_waypoint_template.xml` is present in the application directory and the user "<user_id>" is logged in.
	When the user clicks the Export button, selects output folder "<output_path>" and confirms the export.
	Then a file named "<mmsi>-<start>-<end>.xml" should be created at "<output_path>" containing a single `<RouteTemplate Name="<mmsi>">` element with an ordered list of `<WayPoint/>` elements.

### Examples:
	| mmsi | start | end | user_id | output_path |
	| mmsi-1 | ts_first | ts_last | scenario-user | export_tmp |

### Scenario: Prompt on filename conflict and overwrite chosen
	Given a generated track exists for ship "205196000" and an export file named "205196000-20250315T000000-20250316T000000.xml" already exists in "C:\\tmp\\exports" and the user "scenario-user" is logged in.
	When the user initiates export and chooses the "Overwrite" option in the conflict prompt.
	Then the existing file is replaced with the new XML and a confirmation message with text "Export successful" is shown.

## Negative & Edge Scenarios

### Scenario: Fail export when output path not writable
	Given a generated track exists for ship "205196000" and the user "scenario-user" selects an output folder "C:\\protected\\exports" which is not writable.
	When the user confirms export.
	Then a visible error banner with text "Cannot write to output path: C:\\protected\\exports" is displayed and no file is created.

### Scenario: Handle missing template file
	Given a generated track exists for ship "205196000" and `route_waypoint_template.xml` is missing from the application directory and the user "scenario-user" is logged in.
	When the user attempts to export.
	Then a visible error banner with text "route_waypoint_template.xml not found" is displayed and export is blocked.

### Scenario: Append numeric suffix on filename conflict
	Given a generated track exists and target filename "205196000-20250315T000000-20250316T000000.xml" already exists in "C:\\tmp\\exports" and the user "scenario-user" selects "Append numeric suffix" in the prompt.
	When the user confirms export.
	Then the application creates a new file such as "205196000-20250315T000000-20250316T000000 (1).xml" and no existing file is overwritten and a success message is shown.

### Scenario: Export WayPoint attribute mapping
	Given a generated track for ship "205196000" contains AIS records with sample values and the user "scenario-user" initiates export to "C:\\tmp\\exports".
	When the export completes and the XML is opened.
	Then each `<WayPoint>` element includes attributes mapped as: Name=MMSI, Lat=CSV latitude, Lon=CSV longitude, Alt=0, Speed=SOG, ETA=EtaSecondsUntil or 0, Delay=0, Mode=computed via SetWaypointMode (TODO), TrackMode="Track", Heading=Heading or 0, PortXTE=20, StbdXTE=20, MinSpeed=0, MaxSpeed=maximum SOG observed in range.

## Cross-Feature Reference
- Depends on `create_track` for a generated track input and `getting_started` for presence of `route_waypoint_template.xml`.

<!-- TODO: Replace Example rows above with concrete values from `docs/workflow/inputs/mock_data.md` when that file is added. Replace control labels with data-testids if available. -->
