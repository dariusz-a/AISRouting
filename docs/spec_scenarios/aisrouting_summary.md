<!-- Source: /docs/user_manual/AISRouting_Summary.md -->
# Feature: AISRouting User Manual Summary
A short summary linking core flows: select input root, pick ship/time, create track, export XML.

## Positive Scenarios
### Scenario: End-to-end create and export flow
Given the user selects input root and a vessel "205196000"
And selects a valid time range
When they create a track and export it
Then an XML file following the template should be produced with mapped WayPoint elements

## Negative Scenarios
### Scenario: Fail end-to-end when template missing
Given `route_waypoint_template.xml` is absent
When the user attempts export after creating a track
Then export should fail with a clear message about the missing template

## Cross-Feature Reference
- See `getting_started`, `ship_selection`, `create_track`, and `export_route` for detailed scenarios.
