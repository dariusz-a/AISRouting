<!-- Source: /docs/user_manual/troubleshooting.md -->
# Feature: Troubleshooting
This feature lists common problems and resolutions when preparing, creating, and exporting routes.

## Positive Scenarios
### Scenario: Detect missing CSV files and instruct user
Given the input root is selected
When no CSV files are detected in vessel subfolders
Then the UI should instruct the user to verify the input root and CSV placement

## Negative Scenarios
### Scenario: Export fails due to permission or path issues
Given the user selects an output folder that cannot be created or written to
When they attempt to export
Then an error message should indicate the filesystem problem and suggest alternative actions

### Scenario: Missing Heading or SOG values handled
Given CSV rows have missing Heading or SOG
When processing for track creation or export
Then WayPoint fields should default to 0 and processing should continue

## Edge & Permission Scenarios
### Scenario: Recommend narrowing time range for noisy data
Given noisy AIS data
When the user narrows the time window
Then track creation should produce improved results and fewer spurious points

### Cross-Feature Reference
- References `create_track` and `export_route` for remediation and expected behaviors.
