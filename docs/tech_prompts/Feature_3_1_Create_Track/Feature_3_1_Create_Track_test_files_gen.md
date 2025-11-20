# Test File Generation Prompt: Feature 3.1: Create Track

## Task:
Generate a new file named (`tests/create_track.spec.ts`) or update if it is existing, following the guidelines below.

The document is purely about test generation, not feature implementation. It's designed to create tests that will guide TDD implementation.

If the guidelines are ambiguous you MUST ask a single clear follow-up question and wait.

## Role:
When executing this prompt, you MUST assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, TypeScript, and robust test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.
Assume UI elements include proper accessibility attributes so tests can reliably select them with `getByRole`, `getByLabel`, and `getByTestId`.

**Selector Requirement Update:**
Follow the detailed selector requirements and testing framework specifications in the `docs/tech_design/overall_architecture.md` and the `docs/tech_design/testing/` directory. THESE FILES MUST ALWAYS BE USED AS A MANDATORY INPUT REQUIREMENT. Always check the application code for existing selectors before writing or updating tests. If you find test ids of the elements needed for the test, reuse them and do not create new.

## References
- BDD Scenarios: docs/spec_scenarios/create_track.md
- Test File: tests/create_track.spec.ts
- Technical Design Document: docs/tech_design/core_features/create_track_design.md
- Implementation Plan: docs/tech_design/implementation_plan.md
- Application Architecture: docs/tech_design/overall_architecture.md
- Application Organization: docs/tech_design/application_organization.md
- Testing docs and locator rules: docs/tech_design/testing/QA_testing.md, docs/tech_design/testing/QA_test_locators_instructions.md, docs/tech_design/testing/QA_playwright_authentication.md

## Required Content - Extracted BDD Scenarios (Full Text)

The following scenarios are included verbatim from `docs/spec_scenarios/create_track.md` and are the authoritative scenarios for Feature 3.1.

## Positive Scenarios

### Scenario Outline: Create track for selected ship and time range
Given the application has an input root "<input_root>" containing a vessel folder named "<mmsi>" and the simulator user "<user_id>" is logged in and the UI shows available CSV timestamps from "<first_ts>" to "<last_ts>".
When the user selects vessel "<mmsi>" and sets start "<start>" and stop "<end>" with second resolution and clicks "Create Track".
Then the system processes AIS CSV rows in the selected interval using default optimization parameters and an ordered list of track points is shown in the UI and the track reflects expected vessel continuity.

### Examples:
| input_root | mmsi | user_id | first_ts | last_ts | start | end |
| input_root_example | mmsi-1 | scenario-user | ts_first | ts_last | ts_first | ts_last |

### Scenario: Create track with noisy data and narrowed time window
Given the input root "C:\\data\\ais_root" contains noisy AIS CSV rows for vessel "205196000" and the simulator user "scenario-user" is logged in.
When the user selects vessel "205196000" and sets a narrow start/stop window within noisy interval and clicks "Create Track".
Then the generated track contains fewer spurious points due to the narrowed window and processing completes without errors and a completion status is displayed.

## Negative & Edge Scenarios

### Scenario: Reject track creation when no ship selected
Given the input root "C:\\data\\ais_root" is selected and the UI has no vessel selected and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then an inline error message with text "No ship selected" should be visible and track creation does not start.

### Scenario: Fail gracefully on malformed CSV rows
Given the selected time range includes CSV rows with missing latitude/longitude or required columns and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then the system skips malformed rows, processing continues for valid rows, and a warning banner with text "Some rows were ignored due to invalid format" is displayed.

### Scenario: Handle missing Heading or SOG values in records
Given CSV records in the selected range contain missing Heading or SOG values and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then missing Heading or SOG fields default to 0 for WayPoint mapping and points are generated where possible and a data-quality note is shown.

### Scenario: Prevent track creation when input root empty
Given the selected input root "C:\\empty\\root" contains no vessel subfolders and the simulator user "scenario-user" is logged in.
When the user opens the ship selection combo box and attempts to click "Create Track".
Then the ship combo shows an empty list and an inline warning "No vessels found in input root" is displayed and the Create Track action is disabled.

### Scenario: Create track unavailable for user without permission
Given a logged-in user "user-no-create" without create-track privileges and an available vessel "205196000".
When the user views the UI controls for creating a track.
Then the "Create Track" button is disabled and a tooltip with text "Insufficient privileges" is shown.

## Technical Design Summary (Inline Extract)

Feature overview:
- Generates optimized route (sequence of waypoints) from AIS CSV records for a selected vessel and user-defined time range.
- Handles folder scanning, vessel selection preconditions, second-resolution time selection, robust CSV parsing, optimization pipeline to remove spurious points, malformed row handling, permission gating, and user feedback.

Architectural approach:
- MVVM (WPF) with services: `ISourceDataScanner`, `IShipPositionLoader`, `ITrackOptimizer`, `IRouteExporter`.
- Streaming CSV parsing with `PositionCsvParser` (CsvHelper), cancellable operations, and observable ViewModel state.
- Optimization pipeline: Deviation Detection → Collinearity Simplification → Douglas-Peucker → Optional Temporal Spacing.

Component responsibilities (summary):
- Views: `ShipSelectionView`, `TimeIntervalView`, `TrackResultsView` (test hooks: `AutomationId` attributes: `ship-combo`, `start-picker`, `stop-picker`, `create-track-button`, `track-results-list`).
- ViewModels expose `GeneratedWaypoints`, `DataQualityNotes`, and `OperationStatus` for assertions.
- PositionCsvParser provides counts of `skippedRows` and examples for diagnostics.

Error handling and UX:
- Malformed rows increment `skippedRows`; display: "Some rows were ignored due to invalid format" when > 0.
- Missing Heading/SOG default to 0 and show a data-quality note.
- Create action disabled when no vessel selected, invalid times, input root empty, or insufficient permissions.

Testing hooks and fixtures:
- Suggested test fixtures path: `tests/TestData/205196000/` and `src/mocks/mockData.ts` for Playwright fixtures and helpers. Reuse existing fixtures if present.

## Data Models (Inline)

Relevant TypeScript interface equivalents (for Playwright fixtures/tests):

```ts
export interface ShipStaticData {
  mmsi: string;
  displayName?: string;
  minDate?: string; // ISO
  maxDate?: string; // ISO
}

export interface ShipDataOut {
  timestamp: string; // ISO
  latitude?: number | null;
  longitude?: number | null;
  sog?: number | null; // speed over ground
  heading?: number | null;
}

export interface RouteWaypoint {
  name: string;
  lat: number;
  lon: number;
  speed: number;
  heading: number;
  eta?: number;
}
```

Mock fixture helper pattern (Playwright/TypeScript):

```ts
export function makeCsvRow(ts: string, lat?: number, lon?: number, sog?: number, heading?: number) {
  return `${ts},${lat ?? ''},${lon ?? ''},${sog ?? ''},${heading ?? ''}\n`;
}

// Reuse src/mocks/mockData.ts if exists; otherwise create fixtures under tests/fixtures/
```

## Test File Structure and Auth Setup

- File to generate or update: `tests/create_track.spec.ts` (Playwright TypeScript)
- Tests must use the authentication credentials from `docs/tech_design/testing/QA_playwright_authentication.md`:
  - Email: `alice.smith@company.com`
  - Password: `SecurePass123!`
- Use centralized fixtures per `docs/tech_design/testing/QA_testing.md` and helper patterns above.

## Critical Selector Strategy (Extracted Guidance with Examples)

Follow locator priority from `QA_test_locators_instructions.md`:
1) `getByRole` with accessible name where possible (e.g., `page.getByRole('button', { name: 'Create Track' })`).
2) `getByTestId` when ARIA/name unavailable or ambiguous. The codebase contains `data-testid="create-track"` in `app/index.html` and WPF views define `AutomationProperties.Name` / `AutomationId` attributes which map to test hooks. Prefer `getByTestId('create-track')` only if `getByRole` cannot express the selector reliably.
3) `getByText` only for stable non-localized text.

Examples to prefer in tests:
```ts
await page.getByRole('button', { name: 'Create Track' }).click();
await page.getByTestId('create-track').click(); // only if role/name ambiguous
const shipCombo = page.getByTestId('ship-combo');
await shipCombo.getByRole('option', { name: '205196000' }).click();
```

❌ Avoid fragile selectors like XPath, class names, nth-match, or DOM traversal.

## Test Patterns and Helper Functions (Complete Code Examples)

Provide reusable helper functions to be added to `tests/helpers/ui.ts` (example snippets included in the generated test file):

1) Safe element existence check

```ts
export async function elementExists(locator) {
  try { return await locator.isVisible(); } catch { return false; }
}
```

2) Select vessel helper

```ts
export async function selectVessel(page, mmsi) {
  const combo = page.getByTestId('ship-combo');
  await combo.click();
  await page.getByRole('option', { name: mmsi }).click();
}
```

3) Wait for track generation completion

```ts
export async function waitForTrackReady(page) {
  await page.getByTestId('track-results-list').waitFor({ state: 'visible', timeout: 20000 });
}
```

## Test Cases (Mapping each BDD Scenario to a `test()` block)

Implement the following tests in `tests/create_track.spec.ts`. Use `test.describe()` with the Feature line and a `test()` per Scenario. Use Playwright `fixtures` and `beforeEach` to perform login using the required credentials.

1) Scenario Outline: Create track for selected ship and time range
- Convert to parametric Playwright tests using the Examples table. Steps:
  - Ensure mock input root contains vessel folder and CSVs for the timestamp range (reuse `src/mocks/mockData.ts` or `tests/fixtures/`).
  - Login using provided credentials.
  - Select input folder (if app supports folder dialog in test harness, else use mocking of `ISourceDataScanner`).
  - Select vessel by `mmsi` and set start/stop with second resolution using `start-picker`/`stop-picker` test hooks.
  - Click `Create Track` and assert `track-results-list` becomes visible and contains an ordered list of waypoints.
  - Assert number of waypoints > 0 and continuity (monotonic timestamps or plausible distance between consecutive points).

2) Create track with noisy data and narrowed time window
- Use fixture CSVs under `tests/TestData/205196000/` with noisy rows and assert final waypoint count is lower than raw rows and that status message shows completion. Also assert no uncaught errors and presence of completion status via `OperationStatus` or visible text.

3) Reject track creation when no ship selected
- With input root present but no selection, click `Create Track` and assert inline error "No ship selected" and that no processing starts (e.g., `track-results-list` is not visible).

4) Fail gracefully on malformed CSV rows
- Use a fixture CSV with malformed rows (missing lat/lon). Click `Create Track`, wait for completion, assert visible warning banner text "Some rows were ignored due to invalid format" and that valid waypoints are still produced.

5) Handle missing Heading or SOG values in records
- Provide CSVs with missing heading/sog fields; after `Create Track`, assert generated `RouteWaypoint` entries have `speed` and `heading` set to 0 where missing and that a data-quality note is visible.

6) Prevent track creation when input root empty
- Point to an empty fixture input root, open ship combo, assert empty list and inline warning "No vessels found in input root" and `Create Track` disabled.

7) Create track unavailable for user without permission
- Login as `user-no-create` (if available in fixtures) or mock permission service; assert `Create Track` button disabled and tooltip text "Insufficient privileges" shown on hover.

## Test Data Management

- Always check for existing fixtures:
  - `src/mocks/mockData.ts` (preferred)
  - `tests/fixtures/` for specific CSVs like `tests/TestData/205196000/*.csv`
- If compatible fixtures exist, reuse them. Do NOT create new fixtures if appropriate files already exist.
- If fixtures are missing, create minimal CSV fixtures under `tests/TestData/205196000/` and a small `src/mocks/mockData.ts` export describing `mockVessel205196000` and `mockInputRoot`.

Fixture suggestions (if new):

`tests/TestData/205196000/sample1.csv` - small set of valid position rows (ISO timestamps with seconds)
`tests/TestData/205196000/noisy.csv` - contains noisy positions and some malformed rows

Also provide `tests/fixtures/createTrackFixtures.ts` which imports `src/mocks/mockData.ts` and exports constants used by tests.

## Async Operations and Waiting

- Use `waitFor` on `track-results-list` and `OperationStatus` with a reasonable timeout (20s) and fall back to checking ViewModel state if possible.
- Use Playwright `expect(locator).toHaveText(...)` for status assertions and `locator.waitFor({ state: 'visible' })` for results.

## Test Isolation and Cleanup

- Each test must run in isolation: reset any mocked services and restore fixtures between tests.
- Use `test.afterEach()` to clean up any temporary files created under `tests/TestData/`.

## Success Criteria Validation

Before finalizing the generated `Feature_3_1_Create_Track_test_files_gen.md`, confirm each checklist item below is explicitly addressed in the generated test file content or in guidance included here:

1. Covers ONLY feature-related positive scenarios from the BDD file using robust, unambiguous selectors as defined in the project's architecture and testing documentation.
2. Covers ONLY feature-related negative/validation scenarios with realistic expectations about what's implemented.
3. Uses accessibility-first, specific selectors with explicit guidance on avoiding ambiguity, following the project's architecture and testing documentation.
4. Includes proper async handling as specified in the project's architecture documentation.
5. Maintains test isolation.
6. Follows established test patterns with comprehensive examples as defined in the project's testing documentation.
7. Guides TDD implementation.
8. Includes all Required Code Examples in alignment with the project's testing framework.
9. Provides practical implementation considerations.
10. Provides complete code examples for all recommended patterns.

Please validate each item above when you convert this prompt into actual tests. If anything is ambiguous (e.g., available fixtures, permission mock approach), ask one concise question before generating the test file itself.

## Implementation Note for the Test Generator

- When generating the actual `tests/create_track.spec.ts`, ensure the top of file includes imports for Playwright test helpers, centralized fixtures from `src/mocks/mockData.ts` if present, and helper functions defined above. Use `test.describe('Feature: Create Track', () => { ... })` and `test('Scenario: ...', async ({ page }) => { ... })` patterns.

---

End of Test File Generation Prompt for Feature 3.1: Create Track
