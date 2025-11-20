# Implementation Plan: AisToXmlRouteConvertor

This document outlines the iterative implementation plan for AisToXmlRouteConvertor.

## Iterations

### ITERATION 1: Foundation & Data Models

#### Feature 1.1: Data Models and Core Domain

- Scenario: Define ShipStaticData record
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Recognize valid MMSI folder structure"
  status: not started
  tech_design_file: docs/tech_design/data_models.md
  test_file: tests/data_models.spec.ts
```

- Scenario: Define ShipState record
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Accept CSV files with required schema"
  status: not started
  tech_design_file: docs/tech_design/data_models.md
  test_file: tests/data_models.spec.ts
```

- Scenario: Define TimeInterval record
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Show validation error when Start time after End time"
  status: not started
  tech_design_file: docs/tech_design/data_models.md
  test_file: tests/data_models.spec.ts
```

- Scenario: Define RouteWaypoint record
```yaml
- bdd_spec_file: docs/spec_scenarios/output_specification.md
  scenario: "Generate XML with expected filename pattern"
  status: not started
  tech_design_file: docs/tech_design/data_models.md
  test_file: tests/data_models.spec.ts
```

- Scenario: Define TrackOptimizationParameters record
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/data_models.md
  test_file: tests/data_models.spec.ts
```

#### Feature 1.2: Geographic Math Utilities

- Scenario: Calculate Haversine distance for same point returns zero
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/geo_math.spec.ts
```

- Scenario: Calculate Haversine distance for known points accurately
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/geo_math.spec.ts
```

- Scenario: Calculate initial bearing between two points
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/geo_math.spec.ts
```

### ITERATION 2: File Parsing & Data Loading

#### Feature 2.1: JSON Parser

- Scenario: Parse valid ship static JSON file
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Recognize valid MMSI folder structure"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/json_parser.spec.ts
```

- Scenario: Handle missing optional fields in JSON
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Missing <MMSI>.json file produces TODO note"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/json_parser.spec.ts
```

- Scenario: Return null for malformed JSON file
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Missing <MMSI>.json file produces TODO note"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/json_parser.spec.ts
```

#### Feature 2.2: CSV Parser

- Scenario: Parse valid CSV file with ShipState records
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Accept CSV files with required schema"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/csv_parser.spec.ts
```

- Scenario: Skip and log malformed CSV rows
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Accept CSV files with required schema"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/csv_parser.spec.ts
```

- Scenario: Handle missing optional columns gracefully
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Accept CSV files with required schema"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/csv_parser.spec.ts
```

- Scenario: Reject CSV with invalid filename format
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Reject CSV with invalid filename format"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/csv_parser.spec.ts
```

### ITERATION 3: Input Folder Selection & Scanning

#### Feature 3.1: Input Folder Control

- Scenario: Select valid input folder and scan MMSI subfolders
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_input_folder.md
  scenario: "Select valid input folder and scan MMSI subfolders"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_input_folder.spec.ts
```

- Scenario: Show error when no MMSI subfolders found
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_input_folder.md
  scenario: "Show error when no MMSI subfolders found"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_input_folder.spec.ts
```

- Scenario: Remember last-used path between runs
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_input_folder.md
  scenario: "Remember last-used path between runs"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_input_folder.spec.ts
```

#### Feature 3.2: MMSI Folder Scanning

- Scenario: Select a valid input folder populates ship list
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Select a valid input folder populates ship list"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/mmsi_folder_scanning.spec.ts
```

- Scenario: Input folder with no MMSI subfolders shows error
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Input folder with no MMSI subfolders shows error"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/mmsi_folder_scanning.spec.ts
```

- Scenario: Reject input when MMSI folders missing
```yaml
- bdd_spec_file: docs/spec_scenarios/input_data_preparation.md
  scenario: "Reject input when MMSI folders missing"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/mmsi_folder_scanning.spec.ts
```

### ITERATION 4: Output Folder Selection

#### Feature 4.1: Output Folder Control

- Scenario: Select writable output folder
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_output_folder.md
  scenario: "Select writable output folder"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_output_folder.spec.ts
```

- Scenario: Reject non-writable folder
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_output_folder.md
  scenario: "Reject non-writable folder"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_output_folder.spec.ts
```

- Scenario: Remember last-used output path
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_output_folder.md
  scenario: "Remember last-used output path"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_output_folder.spec.ts
```

#### Feature 4.2: Output Folder Validation

- Scenario: Select writable output folder displays chosen path
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Select writable output folder displays chosen path"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_folder_validation.spec.ts
```

- Scenario: Non-writable output folder shows error
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Non-writable output folder shows error"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_folder_validation.spec.ts
```

- Scenario: Output folder not writable prevents enablement
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Output folder not writable prevents enablement"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_folder_validation.spec.ts
```

- Scenario: Fail conversion when output folder not writable
```yaml
- bdd_spec_file: docs/spec_scenarios/application_overview.md
  scenario: "Fail conversion when output folder not writable"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_folder_validation.spec.ts
```

### ITERATION 5: Ship Selection & Display

#### Feature 5.1: Ship Selection Control

- Scenario: Display MMSI rows with metadata
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_selection.md
  scenario: "Display MMSI rows with metadata"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_selection.spec.ts
```

- Scenario: Selecting a ship populates ShipStaticData and enables controls
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_selection.md
  scenario: "Selecting a ship populates ShipStaticData and enables controls"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_selection.spec.ts
```

- Scenario: Disable selection for MMSI with no CSV files
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_selection.md
  scenario: "Disable selection for MMSI with no CSV files"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_selection.spec.ts
```

- Scenario: Sort the table by column
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_selection.md
  scenario: "Sort the table by column"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_selection.spec.ts
```

#### Feature 5.2: Ship Table Population

- Scenario: Ship selection enables time pickers and process prerequisites
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Ship selection enables time pickers and process prerequisites"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ship_table_population.spec.ts
```

- Scenario: Populate ship table after selecting folders
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Populate ship table after selecting folders"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ship_table_population.spec.ts
```

- Scenario: Ship row without CSV files is disabled
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Ship row without CSV files is disabled"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ship_table_population.spec.ts
```

- Scenario: Sorting ship table by MMSI ascending default
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Sorting ship table by MMSI ascending default"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ship_table_population.spec.ts
```

#### Feature 5.3: ShipStaticData Panel

- Scenario: Show full static data for selected ship
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_data.md
  scenario: "Show full static data for selected ship"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_data.spec.ts
```

- Scenario: Export JSON of static data
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_data.md
  scenario: "Export JSON of static data"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_data.spec.ts
```

- Scenario: Panel read-only and does not allow edits
```yaml
- bdd_spec_file: docs/spec_scenarios/ui_ship_data.md
  scenario: "Panel read-only and does not allow edits"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/ui_ship_data.spec.ts
```

### ITERATION 6: Time Interval Selection & Validation

#### Feature 6.1: Time Interval Controls

- Scenario: Time pickers enabled when ship selected
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Ship selection enables time pickers and process prerequisites"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/time_interval_controls.spec.ts
```

- Scenario: Invalid time range disables process button
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Invalid time range disables process button"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/time_interval_controls.spec.ts
```

- Scenario: Time outside available range blocks processing
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Time outside available range blocks processing"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/time_interval_controls.spec.ts
```

- Scenario: Show validation error when Start time after End time
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Show validation error when Start time after End time"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/time_interval_controls.spec.ts
```

- Scenario: Disable processing until both times selected
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Disable processing until both times selected"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/time_interval_controls.spec.ts
```

### ITERATION 7: Track Optimization

#### Feature 7.1: Track Optimization Algorithm

- Scenario: Optimize track by reducing positions to significant waypoints
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/track_optimizer.spec.ts
```

- Scenario: Always retain first and last positions
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/track_optimizer.spec.ts
```

- Scenario: Apply optimization thresholds correctly
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/track_optimizer.spec.ts
```

- Scenario: Handle empty input gracefully
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/track_optimizer.spec.ts
```

- Scenario: Handle single position input
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/track_optimizer.spec.ts
```

### ITERATION 8: XML Export

#### Feature 8.1: XML Route File Generation

- Scenario: Generate XML with expected filename pattern
```yaml
- bdd_spec_file: docs/spec_scenarios/output_specification.md
  scenario: "Generate XML with expected filename pattern"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_specification.spec.ts
```

- Scenario: XML content follows template
```yaml
- bdd_spec_file: docs/spec_scenarios/output_specification.md
  scenario: "XML content follows template"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_specification.spec.ts
```

- Scenario: Fail when unable to write file
```yaml
- bdd_spec_file: docs/spec_scenarios/output_specification.md
  scenario: "Fail when unable to write file"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_specification.spec.ts
```

- Scenario: No automatic folder open after generation
```yaml
- bdd_spec_file: docs/spec_scenarios/output_specification.md
  scenario: "No automatic folder open after generation"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/output_specification.spec.ts
```

#### Feature 8.2: XML Export Validation

- Scenario: Output file naming pattern correctness
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Output file naming pattern correctness"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/xml_export_validation.spec.ts
```

- Scenario: Success message does not auto-open output folder
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Success message does not auto-open output folder"
  status: not started
  tech_design_file: docs/tech_design/application_organization.md
  test_file: tests/xml_export_validation.spec.ts
```

### ITERATION 9: End-to-End Processing

#### Feature 9.1: Process Button & Workflow

- Scenario: Process successfully generates XML file
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process successfully generates XML file"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Process unavailable until all prerequisites selected
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Process unavailable until all prerequisites selected"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Processing failure displays error details
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Processing failure displays error details"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Input selection re-populates ship table and resets dependent controls
```yaml
- bdd_spec_file: docs/spec_scenarios/ais_to_xml_route_convertor_summary.md
  scenario: "Input selection re-populates ship table and resets dependent controls"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Prevent processing when no ship selected
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Prevent processing when no ship selected"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Generate route for selected ship within valid time interval
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Generate route for selected ship within valid time interval"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

- Scenario: Graceful handling of transient backend processing failure
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Graceful handling of transient backend processing failure"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/process_workflow.spec.ts
```

#### Feature 9.2: Complete AIS to XML Conversion

- Scenario: Convert imported AIS CSV to XML successfully
```yaml
- bdd_spec_file: docs/spec_scenarios/application_overview.md
  scenario: "Convert imported AIS CSV to XML successfully"
  status: not started
  tech_design_file: docs/tech_design/overall_architecture.md
  test_file: tests/application_overview.spec.ts
```

- Scenario: Reject conversion when data preview shows invalid mapping
```yaml
- bdd_spec_file: docs/spec_scenarios/application_overview.md
  scenario: "Reject conversion when data preview shows invalid mapping"
  status: not started
  tech_design_file: docs/tech_design/overall_architecture.md
  test_file: tests/application_overview.spec.ts
```

- Scenario: Handle processing failure with error message
```yaml
- bdd_spec_file: docs/spec_scenarios/application_overview.md
  scenario: "Handle processing failure with error message"
  status: not started
  tech_design_file: docs/tech_design/overall_architecture.md
  test_file: tests/application_overview.spec.ts
```

### ITERATION 10: Application Initialization & UI Setup

#### Feature 10.1: Main Window Initialization

- Scenario: Display main window controls on launch
```yaml
- bdd_spec_file: docs/spec_scenarios/getting_started.md
  scenario: "Display main window controls on launch"
  status: not started
  tech_design_file: docs/tech_design/application_layout.md
  test_file: tests/getting_started.spec.ts
```

### DEPENDENCIES BETWEEN ITERATIONS

- **Iteration 1** (Foundation & Data Models) must be completed first as all other iterations depend on the core data structures
- **Iteration 2** (File Parsing & Data Loading) depends on Iteration 1 for data models
- **Iteration 3** (Input Folder Selection) depends on Iteration 2 for JSON/CSV parsing capabilities
- **Iteration 4** (Output Folder Selection) can be developed in parallel with Iteration 3
- **Iteration 5** (Ship Selection & Display) depends on Iteration 3 for MMSI folder scanning
- **Iteration 6** (Time Interval Selection) depends on Iteration 5 for ship selection state
- **Iteration 7** (Track Optimization) depends on Iteration 1 for data models and Iteration 2 for parsing
- **Iteration 8** (XML Export) depends on Iteration 7 for optimized waypoints
- **Iteration 9** (End-to-End Processing) depends on all previous iterations as it integrates the complete workflow
- **Iteration 10** (Application Initialization) depends on Iterations 3, 4, 5, 6 for UI components

### IMPLEMENTATION STRATEGY

- **Test-First Development (BDD + E2E)**: Each scenario must have corresponding tests written before implementation code. Tests are based on the BDD specifications in Gherkin format and should initially fail against stubs/mocks.

- **Iterative Workflow**: Complete all scenarios within an Iteration before moving to the next. Each Iteration represents a logical domain (data models, parsing, UI controls, processing). After completing an Iteration, refactor and ensure all tests pass before proceeding.

- **Dependency Ordering**: The plan sequences Iterations to respect technical dependencies. Foundation (data models, utilities) comes first, followed by file I/O capabilities, then UI controls in workflow order (input → selection → time → processing), and finally integration and end-to-end scenarios.

- **Feature Completeness**: All scenarios within a Feature must be implemented together. Features are not split across Iterations unless explicitly noted. This ensures each Feature area is fully functional before moving on.

- **Status Tracking**: Each scenario tracks status (not started / in-progress / completed / deferred). Update status as work progresses to maintain visibility of implementation progress.

- **Cross-Platform Considerations**: The application targets Windows, macOS, and Linux. All implementations should use cross-platform .NET 9 and Avalonia APIs, avoiding platform-specific code unless absolutely necessary.

- **Simplicity Principle**: Follow the architecture principle of "simplicity-first design" - use synchronous operations, static helper methods where appropriate, and avoid over-engineering with unnecessary abstractions.
