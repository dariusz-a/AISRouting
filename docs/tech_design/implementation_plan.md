# Implementation Plan: AIS Routing

This document outlines the iterative implementation plan for AIS Routing.

## Iterations

### ITERATION 1: FOUNDATION
#### Feature 1.1: Getting Started

- Scenario: Install the application from installer successfully
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Install the application from installer successfully"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Launch the application and view main window"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Verify AIS input folder structure and file details"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Installation fails due to insufficient permissions"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Application does not start due to missing runtime dependency"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Missing AIS input files shows clear guidance"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Input file with wrong encoding is skipped and reported"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "File with invalid filename format is ignored"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Input folder inaccessible due to permission or network error"
  status: not started
  tech_design_file: docs/tech_design/core_features/getting_started_design.md
  test_file: tests/getting_started.spec.ts
```

#### Feature 1.2: Input Data and CSV Format

- Scenario: Place AIS CSV files in correct folder structure
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "Place AIS CSV files in correct folder structure"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "CSV fields map correctly with example line"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "Validate encoding and line endings"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "CSV with wrong column count is rejected"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "Non-UTF-8 encoded file is flagged"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "Empty fields map to defaults"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
- bdd_spec_file: docs/spec_scenarios/input_data_and_csv_format.md
  scenario: "Decimal format uses dot as separator"
  status: not started
  tech_design_file: docs/tech_design/core_features/input_data_and_csv_format_design.md
  test_file: tests/input_data_and_csv_format.spec.ts
```

### ITERATION 2: SHIP SELECTION
#### Feature 2.1: Selecting a Ship and Time Range

- Scenario: Select a vessel by MMSI
```yaml
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Select a vessel by MMSI"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Choose valid start and end times"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Initiate route creation"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "End time before Start time is rejected"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Time outside available bounds is rejected"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Rapid consecutive selection updates"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
- bdd_spec_file: docs/spec_scenarios/selecting_ship_and_time_range.md
  scenario: "Missing ship list due to no input files"
  status: not started
  tech_design_file: docs/tech_design/core_features/selecting_ship_and_time_range_design.md
  test_file: tests/selecting_ship_and_time_range.spec.ts
```

### ITERATION 3: ROUTE GENERATION
#### Feature 3.1: Creating Routes and Output XML

- Scenario: Generate RouteTemplate from AIS track successfully
```yaml
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Generate RouteTemplate from AIS track successfully"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Save generated RouteTemplate as XML to default destination"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Prevent save when destination file exists and user declines overwrite"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Route generation fails for missing AIS records in range"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Save to location without write permission shows explicit error"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Generated route contains skipped records due to missing coordinates"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Preview/edit not available in this release"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Fail generation when AIS CSV missing required 'Latitude' column"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Show session expired error when generation started with expired session"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Show explicit error when AIS files cannot be read due to permission denial"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
- bdd_spec_file: docs/spec_scenarios/creating_routes.md
  scenario: "Show timeout when route generation exceeds SLO (30s)"
  status: not started
  tech_design_file: docs/tech_design/core_features/creating_routes_design.md
  test_file: tests/creating_routes.spec.ts
```

#### Feature 3.2: Generating Waypoint Mappings

- Scenario: Map AIS CSV record to WayPoint attributes with all fields present
```yaml
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Map AIS CSV record to WayPoint attributes with all fields present"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Compute MaxSpeed as maximum observed SOG in selected range"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Missing SOG maps Speed to 0 for that WayPoint"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Invalid numeric format in Latitude causes row to be skipped and parse error recorded"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Default XTE and MinSpeed applied when fields are not provided"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Empty CSV selection results in no WayPoints and visible user feedback"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
- bdd_spec_file: docs/spec_scenarios/generating_waypoint_mappings.md
  scenario: "Unauthenticated API call for waypoint generation is rejected with 401"
  status: not started
  tech_design_file: docs/tech_design/core_features/generating_waypoint_mappings_design.md
  test_file: tests/generating_waypoint_mappings.spec.ts
```

### DEPENDENCIES BETWEEN ITERATIONS   

- Foundation (Iteration 1) must be completed first: installation, input folder configuration, and CSV validation are prerequisites for all features.
- Input Data validation (Iteration 1) is required before Ship Selection (Iteration 2) because ship availability and time ranges are derived from validated input files.
- Ship Selection (Iteration 2) must be implemented before Route Generation (Iteration 3) since route generation requires a selected vessel and time range.
- Waypoint mapping logic (Feature 3.2) is a dependency for Creating Routes (Feature 3.1) because route templates embed mapped WayPoint elements.
- File I/O and permissions handling must be available before implementing save/export features in Route Generation.

### IMPLEMENTATION STRATEGY   

- Test-first implementation (BDD + E2E): implement features using BDD scenarios as tests. For each scenario, write the E2E test first (Playwright) derived from the Gherkin, run it to see failures, implement minimum code to satisfy tests, then refactor.
- Iteration policy: fully complete all Features and their Scenarios in an Iteration before progressing to the next Iteration. Within an Iteration, deliver features in a logical order (foundational UI and validation, then interactive selection flows, then export and persistence). Allocate time for refactoring and regression testing after feature completion.
- Dependency ordering: order work so that lower-level infrastructure and validation features (installation, input-file validation, encoding handling, and file I/O) are implemented before higher-level user flows (ship selection, route generation, and export). Authentication and user-management style features (if added) must be completed before any role/person management features that depend on them.
