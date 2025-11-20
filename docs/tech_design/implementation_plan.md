# Implementation Plan: AISRouting

This document outlines the iterative implementation plan for AISRouting.

## Iterations

### ITERATION 1: Foundation and Infrastructure

#### Feature 1.1: Getting Started

- Scenario: Install and start AISRouting UI
```yaml
bdd_spec_file: docs/spec_scenarios/getting_started.md
scenario: "Install and start AISRouting UI"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/getting_started.spec.ts
```

- Scenario: Select input data root with vessel subfolders
```yaml
bdd_spec_file: docs/spec_scenarios/getting_started.md
scenario: "Select input data root with vessel subfolders"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/getting_started.spec.ts
```

- Scenario: Fail when input root empty
```yaml
bdd_spec_file: docs/spec_scenarios/getting_started.md
scenario: "Fail when input root empty"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/getting_started.spec.ts
```

- Scenario: Application warns when `route_waypoint_template.xml` missing
```yaml
bdd_spec_file: docs/spec_scenarios/getting_started.md
scenario: "Application warns when `route_waypoint_template.xml` missing"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/getting_started.spec.ts
```

- Scenario: Prevent start when executable missing or corrupted
```yaml
bdd_spec_file: docs/spec_scenarios/getting_started.md
scenario: "Prevent start when executable missing or corrupted"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/getting_started.spec.ts
```

### ITERATION 2: Ship Selection and Static Data

#### Feature 2.1: Ship Selection and Static Data

- Scenario: Populate ship combo box from static files or folder names
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Populate ship combo box from static files or folder names"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Display static data after ship selection
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Display static data after ship selection"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Default start/stop time values set from file timestamps
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Default start/stop time values set from file timestamps"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Show fallback when static name missing
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Show fallback when static name missing"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Validate Min/Max date range before creation
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Validate Min/Max date range before creation"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Use seconds resolution for time pickers
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Use seconds resolution for time pickers"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

- Scenario: Ship selection unavailable when input root missing
```yaml
bdd_spec_file: docs/spec_scenarios/ship_selection.md
scenario: "Ship selection unavailable when input root missing"
status: not started
tech_design_file: docs/tech_design/data_models.md
test_file: tests/ship_selection.spec.ts
```

### ITERATION 3: Track Generation

#### Feature 3.1: Create Track

- Scenario: Create track for selected ship and time range
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Create track for selected ship and time range"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Create track with noisy data and narrowed time window
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Create track with noisy data and narrowed time window"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Reject track creation when no ship selected
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Reject track creation when no ship selected"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Fail gracefully on malformed CSV rows
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Fail gracefully on malformed CSV rows"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Handle missing Heading or SOG values in records
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Handle missing Heading or SOG values in records"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Prevent track creation when input root empty
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Prevent track creation when input root empty"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

- Scenario: Create track unavailable for user without permission
```yaml
bdd_spec_file: docs/spec_scenarios/create_track.md
scenario: "Create track unavailable for user without permission"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/create_track.spec.ts
```

### ITERATION 4: Route Export

#### Feature 4.1: Exporting Routes

- Scenario: Export generated track to XML with valid output path
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Export generated track to XML with valid output path"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

- Scenario: Prompt on filename conflict and overwrite chosen
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Prompt on filename conflict and overwrite chosen"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

- Scenario: Fail export when output path not writable
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Fail export when output path not writable"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

- Scenario: Handle missing template file
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Handle missing template file"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

- Scenario: Append numeric suffix on filename conflict
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Append numeric suffix on filename conflict"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

- Scenario: Export WayPoint attribute mapping
```yaml
bdd_spec_file: docs/spec_scenarios/export_route.md
scenario: "Export WayPoint attribute mapping"
status: not started
tech_design_file: docs/tech_design/api_integration_patterns.md
test_file: tests/export_route.spec.ts
```

### ITERATION 5: End-to-End Integration and Error Handling

#### Feature 5.1: AISRouting User Manual Summary

- Scenario: End-to-end create and export flow
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "End-to-end create and export flow"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Fail export when template file missing
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Fail export when template file missing"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Fail export when output path not writable
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Fail export when output path not writable"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Prevent export when user lacks permission
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Prevent export when user lacks permission"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Block export when template file is missing
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Block export when template file is missing"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Prevent export for user without export privileges
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Prevent export for user without export privileges"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

- Scenario: Show error when export fails due to write timeout
```yaml
bdd_spec_file: docs/spec_scenarios/aisrouting_summary.md
scenario: "Show error when export fails due to write timeout"
status: not started
tech_design_file: docs/tech_design/overall_architecture.md
test_file: tests/aisrouting_summary.spec.ts
```

#### Feature 5.2: Troubleshooting

- Scenario: Detect missing CSV files and instruct user
```yaml
bdd_spec_file: docs/spec_scenarios/troubleshooting.md
scenario: "Detect missing CSV files and instruct user"
status: not started
tech_design_file: docs/tech_design/core_features/troubleshooting_design.md
test_file: tests/troubleshooting.spec.ts
```

- Scenario: Export fails due to permission or path issues
```yaml
bdd_spec_file: docs/spec_scenarios/troubleshooting.md
scenario: "Export fails due to permission or path issues"
status: not started
tech_design_file: docs/tech_design/core_features/troubleshooting_design.md
test_file: tests/troubleshooting.spec.ts
```

- Scenario: Missing Heading or SOG values handled
```yaml
bdd_spec_file: docs/spec_scenarios/troubleshooting.md
scenario: "Missing Heading or SOG values handled"
status: not started
tech_design_file: docs/tech_design/core_features/troubleshooting_design.md
test_file: tests/troubleshooting.spec.ts
```

- Scenario: Recommend narrowing time range for noisy data
```yaml
bdd_spec_file: docs/spec_scenarios/troubleshooting.md
scenario: "Recommend narrowing time range for noisy data"
status: not started
tech_design_file: docs/tech_design/core_features/troubleshooting_design.md
test_file: tests/troubleshooting.spec.ts
```

## DEPENDENCIES BETWEEN ITERATIONS

- **Iteration 1 (Foundation)** must be completed first as it establishes the core application structure, DI container setup, and input folder selection capability
- **Iteration 2 (Ship Selection)** depends on Iteration 1 for input root selection and folder scanning infrastructure
- **Iteration 3 (Track Generation)** depends on Iteration 2 for vessel and time range selection capabilities
- **Iteration 4 (Route Export)** depends on Iteration 3 as it requires a generated track to export
- **Iteration 5 (Integration)** depends on all previous iterations as it validates end-to-end workflows and error handling across the complete feature set

## IMPLEMENTATION STRATEGY

- **Test-First Development**: All scenarios will be implemented using BDD principles:
  - Write E2E test cases in Playwright based on Gherkin scenarios before implementation
  - Implement features to satisfy test cases
  - Run tests continuously to validate functionality
  
- **Iterative Workflow**:
  - Complete all scenarios within an Iteration before moving to the next
  - Each Iteration produces a potentially shippable increment
  - Refactor code within iterations to maintain quality and prevent technical debt
  
- **Dependency Ordering**:
  - Foundation features (application startup, folder selection) are implemented first
  - Data loading and visualization features follow once infrastructure is established
  - Business logic (track optimization) builds on data loading capabilities
  - Export functionality caps the workflow, depending on all previous features
  
- **Error Handling**:
  - Negative scenarios are tested alongside positive scenarios within each iteration
  - Input validation is implemented at service boundaries
  - User-facing error messages are clear and actionable
  
- **Security and Validation**:
  - Path validation and sanitization applied to all file system operations
  - CSV parsing includes malformed data handling and DoS prevention
  - MMSI validation enforces AIS specification compliance
