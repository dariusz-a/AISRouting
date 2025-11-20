# Test File Generation Prompt: Feature 2.1: Ship Selection and Static Data

## Task:
Generate a new file named (`tests/ship_selection.spec.ts`) or update if it is existing, following the guidelines below.

The document is purely about test generation, not feature implementation. It's designed to create tests that will guide TDD implementation.

If the guidelines are ambiguous you MUST ask a single clear follow-up question and wait.

## Role:
When executing this prompt, you MUST assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, TypeScript, and robust test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.

Assume all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, and other accessibility-first selectors.

**Selector Requirement Update:**
Follow the detailed selector requirements and testing framework specifications in the `docs/tech_design/overall_architecture.md` and the `docs/tech_design/testing/` directory. THESE FILES MUST ALWAYS BE USED AS A MANDATORY INPUT REQUIREMENT. Read all the files in full and always follow the exact instructions in them.
**Always check the application code for existing selectors before writing or updating tests. If you find test ids of the elements needed for the test, reuse them and do not create new.**

## References
- BDD Scenarios: docs/spec_scenarios/ship_selection.md
- Test File: tests/ship_selection.spec.ts
- Technical Design Document: docs/tech_design/core_features/ship_selection_design.md
- Implementation Plan: docs/tech_design/implementation_plan.md
- Application Architecture `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`

## Required Content Extracted

#### 1. BDD Scenarios (Full Text)

Feature: Ship Selection and Static Data

Positive Scenarios

Scenario: Populate ship combo box from static files or folder names
Given the input root "input_root_example" contains vessel subfolders where folder "mmsi-1" has a static data file with Name="Sea Explorer" and folder "mmsi-2" has no static name.
When the application opens the ship selection combo box and the user selects "<input_root>".
Then the combo box lists "Sea Explorer" and "205196001" and the values are selectable.

Scenario: Display static data after ship selection
Given input root "input_root_example" contains vessel folder "mmsi-1" with static attributes including Name="Sea Explorer" and MMSI="mmsi-1".
When the user selects "Sea Explorer" in the ship combo box.
Then the static attributes are displayed in the large TextBox widget including Name and MMSI.

Scenario: Default start/stop time values set from file timestamps
Given vessel folder "mmsi-1" contains CSV files with earliest timestamp "ts_first" and latest "ts_last".
When the user selects vessel "205196000".
Then the StartValue defaults to "20250315T000000" and the StopValue defaults to "20250316T000000" plus 24 hours ("20250317T000000").

Negative & Edge Scenarios

Scenario: Show fallback when static name missing
Given a vessel folder "mmsi-2" lacks a static name in its static file and the application lists vessels from "input_root_example".
When the ship combo is shown.
Then the folder name "205196001" is used as the displayed ship name in the combo.

Scenario: Validate Min/Max date range before creation
Given vessel "mmsi-1" has CSV files with inconsistent timestamps causing Min > Max.
When the user inspects the Min/Max pickers.
Then a validation warning with text "Invalid time range" is displayed and the Create Track button is disabled until corrected.

Scenario: Use seconds resolution for time pickers
Given vessel "mmsi-1" is selected and the UI shows start/stop time pickers.
When the user opens the start time picker and sets seconds to "00".
Then the picker accepts seconds resolution and the selected timestamp shows seconds precision.

Scenario: Ship selection unavailable when input root missing
Given the input root path specified by "empty_root_example" is not accessible or does not exist.
When the user opens the ship selection combo box.
Then the combo box shows an error state with text "Input root not accessible" and selection is disabled.

#### 2. Technical Design Summary (Inline)

- Purpose: Provide UI and services for selecting a vessel, presenting static metadata, and selecting available time range. Follow MVVM, DI, and service-layer patterns.
- Key components: `ShipSelectionView`, `ShipSelectionViewModel`, `ISourceDataScanner`, `IShipStaticDataLoader`, `IFolderDialogService`, `TimeInterval` domain model.
- Data flow: `IFolderDialogService` → `ISourceDataScanner.ScanInputFolder(inputRoot)` → `IShipStaticDataLoader.LoadStaticData(folderPath, mmsi)` → `AvailableVessels` → `SelectedVessel` triggers `TimeInterval` defaults.
- Default behavior: On selection set Start = MinDate and Stop = MaxDate.AddDays(1). If Name missing, fallback to FolderName. If Input root inaccessible show "Input root not accessible".
- Playwright selectors recommended in design: `data-testid="ship-combo"`, `data-testid="ship-static"`, `data-testid="start-picker"`, `data-testid="stop-picker"`, `data-testid="time-error"`.

#### 3. Data Models (Inline)

TypeScript-style interface examples for tests and fixtures:

```ts
interface ShipStaticData {
  mmsi: number;
  name?: string | null;
  folderPath: string;
  minDate: string; // ISO timestamp or yyyy-MM-dd
  maxDate: string; // ISO timestamp or yyyy-MM-dd
  hasNoData?: boolean;
}

interface TimeInterval {
  start: string; // ISO timestamp
  stop: string;  // ISO timestamp
  isValid: boolean;
}
```

Mock fixture example values (to be placed in `src/mocks/mockData.ts` or `tests/fixtures/shipSelectionTestData.ts`):

- `mockShip_205196000`:
  - mmsi: 205196000
  - name: "Sea Explorer"
  - folderPath: "tests/fixtures/inputFolders/205196000"
  - minDate: "2025-03-15T00:00:00"
  - maxDate: "2025-03-16T00:00:00"

- `mockShip_205196001` (no static name):
  - mmsi: 205196001
  - name: null
  - folderPath: "tests/fixtures/inputFolders/205196001"
  - minDate: "2025-03-15T00:00:00"
  - maxDate: "2025-03-15T00:00:00"

## Test File Structure with authentication setup

- Authentication: Follow `docs/tech_design/testing/QA_playwright_authentication.md` and use credentials `alice.smith@company.com` / `SecurePass123!` in tests that require login.
- Test file to create/update: `tests/ship_selection.spec.ts` (Playwright + TypeScript)
- Fixtures location: `tests/fixtures/inputFolders/` and `src/mocks/mockData.ts` per `QA_testing.md`.

## Tests Structure (Playwright)

- Use `test.describe()` with Feature line as outer block: `Feature: Ship Selection and Static Data`.
- Convert each Scenario into a `test()` block.
- Use `test.beforeEach()` to authenticate (when applicable) and to set up fixture folder structure.
- Use `test.afterEach()` to clean up created fixtures.

## Critical Selector Strategy Updates with ❌/✅ examples

- ✅ Prefer `page.getByRole('combobox', { name: /Ship selection/i })` or `page.getByTestId('ship-combo')` when available.
- ❌ Avoid selecting by DOM order or CSS classes.

Examples:

```ts
// Preferred (ARIA-first)
const shipCombo = page.getByTestId('ship-combo');
await shipCombo.getByRole('searchbox').fill('Sea');
await page.getByRole('option', { name: 'Sea Explorer' }).click();

// Fallback (data-testid)
await page.getByTestId('ship-combo').click();
await page.getByTestId('ship-combo').getByRole('option', { name: '205196001' }).click();
```

## Test Patterns with complete code examples according to project architecture and testing documentation

Provide full Playwright TypeScript examples for each scenario. Each test should:
- Reuse fixtures from `src/mocks/mockData.ts` if present; otherwise create `tests/fixtures/inputFolders/...` on-the-fly.
- Use helper functions for safe element interaction from `QA_testing.md` (e.g., `elementExistsSafely`).

Example test skeleton for "Display static data after ship selection":

```ts
import { test, expect } from '@playwright/test';
import { mockShip_205196000 } from '../../src/mocks/mockData';

test.describe('Feature: Ship Selection and Static Data', () => {
  test('Scenario: Display static data after ship selection', async ({ page }) => {
    // Arrange: ensure fixture folder exists (reuse existing if available)
    // Authenticate if UI requires login
    await page.goto('/');

    // Act: open ship combo, select 'Sea Explorer'
    const shipCombo = page.getByTestId('ship-combo');
    await shipCombo.click();
    await page.getByRole('option', { name: 'Sea Explorer' }).click();

    // Assert: static data textbox displays name and mmsi
    const staticBox = page.getByTestId('ship-static');
    await expect(staticBox).toContainText('Sea Explorer');
    await expect(staticBox).toContainText('205196000');
  });
});
```

## Locator Patterns specific to the feature

- Ship Combo: `data-testid="ship-combo"` or role `combobox` with accessible name "Ship selection".
- Static TextBox: `data-testid="ship-static"` or role `region` labelled "Ship static data".
- Start/Stop pickers: `data-testid="start-picker"` / `data-testid="stop-picker"` or labelled inputs.
- Time error message: `data-testid="time-error"` or `getByText('Invalid time range')`.

## Common Actions with helper functions

- `async function selectShip(page, shipNameOrFolder)` – opens combo, types/cliks and selects option; falls back to folder name when name missing.
- `async function ensureFixtureFolder(folderPath)` – creates files in `tests/fixtures/inputFolders/` if missing.

## Test Implementation Guidelines with conditional logic

- Always check for existing fixtures in `src/mocks/mockData.ts` or `tests/fixtures/` before creating new ones.
- Use `elementExistsSafely` before interacting with optional UI pieces.
- For date-range validation test, set pickers to values causing Min > Max and assert `time-error` visible and `Create Track` disabled using `page.getByRole('button', { name: 'Create Track' })`.

## Character Limit Testing Pattern addressing browser behavior

- Not applicable to primary controls here, but include pattern in prompt for future use.

## Practical Validation Testing with realistic expectations

- Do not assume server-side persistence — validate UI state and control enablement/disablement.

## Success Criteria for the generated test file

1. Covers ONLY feature-related positive scenarios from the BDD file using robust, unambiguous selectors as defined in project docs.
2. Covers ONLY feature-related negative/validation scenarios with realistic expectations about what's implemented.
3. Uses accessibility-first, specific selectors with explicit guidance on avoiding ambiguity.
4. Includes proper async handling using Playwright waiting and helper functions.
5. Maintains test isolation and cleans up fixtures after each test.
6. Follows established test patterns with comprehensive examples.
7. Guides TDD implementation and includes code examples for each scenario.
8. Includes all Required Code Examples in alignment with the project's testing framework.
9. Provides practical implementation considerations and fixture reuse guidance.
10. Provides complete code examples for all recommended patterns.

## Validation Step (Mandatory)

Confirm each success criterion is met for the generated test file. If any criterion is not met, revise the output before finalizing.

---

Notes and next steps:
- Before generating actual `tests/ship_selection.spec.ts` and fixture files, confirm whether you want me to create the `src/mocks/mockData.ts` and `tests/fixtures/inputFolders/...` fixtures now or have me only produce the test generation prompt file (this file).
