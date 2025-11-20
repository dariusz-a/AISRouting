<!-- Source: /docs/user_manual/export_route.md -->
# Feature: Exporting Routes
This feature describes exporting a generated track into an XML file using the `route_waypoint_template.xml`.

## Positive Scenarios
### Scenario: Export generated track to XML with valid output path
Given a generated track exists for ship "205196000"
And `route_waypoint_template.xml` is present in the application directory
And the user selects an output folder that is writable
When they click "Export" and confirm
Then a file named matching pattern "205196000-20250315T000000-20250316T000000.xml" should be created in the chosen folder
And the file should contain a single `<RouteTemplate Name="205196000">` element with ordered `<WayPoint/>` elements

### Scenario: Prompt on filename conflict and overwrite chosen
Given an export filename already exists in the output folder
When the user chooses "Overwrite" after prompt
Then the existing file should be replaced with the new XML
And a confirmation message should be shown

## Negative Scenarios
### Scenario: Fail export when output path not writable
Given a generated track exists
And the selected output path is not writable
When the user attempts to export
Then an error message indicating the filesystem problem should be displayed
And no file should be created

### Scenario: Handle missing template file
Given a generated track exists
And `route_waypoint_template.xml` is missing from the application directory
When the user attempts to export
Then an error message "route_waypoint_template.xml not found" should be displayed
And export should be blocked

## Edge & Permission Scenarios
### Scenario: Append numeric suffix on filename conflict
Given an export filename already exists
When the user chooses "Append numeric suffix"
Then the application should create a new file with a numeric suffix appended to the filename
And no existing file should be overwritten

### Scenario: Export WayPoint attribute mapping
Given a generated track contains AIS records with values
When the XML is written
Then each `<WayPoint>` element should include attributes mapped as:
- Name: MMSI
- Lat: CSV latitude
- Lon: CSV longitude
- Alt: 0
- Speed: SOG
- ETA: EtaSecondsUntil or 0
- Delay: 0
- Mode: computed via SetWaypointMode (TODO: SetWaypointMode rules defined later)
- TrackMode: "Track"
- Heading: Heading or 0
- PortXTE: 20
- StbdXTE: 20
- MinSpeed: 0
- MaxSpeed: maximum SOG observed in range (zeros ignored where possible)

### Cross-Feature Reference
- Depends on `create_track` for generated track input.
