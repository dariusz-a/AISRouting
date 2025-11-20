# Test File Generation Prompt: Feature_1_1_Getting_Started

## Task:
Generate a new file named `tests/getting_started.spec.ts` (or update it if one already exists). This prompt instructs the test-generation agent to create Playwright E2E tests that implement the BDD scenarios for the Getting Started feature. The tests must follow the project's testing architecture and QA guidance.

## Role:
You are an Automation QA Engineer with expertise in Playwright and TypeScript. Translate the BDD scenarios below into robust Playwright tests using accessibility-first selectors and the centralized mock data strategy.

## References
- BDD Scenarios: `docs/spec_scenarios/getting_started.md`
- Technical Design Document: `docs/tech_design/core_features/getting_started_design.md`
- Architecture: `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`
- Data Models: `docs/tech_design/data_models.md`
- Testing Patterns: `docs/tech_design/testing/QA_testing.md`
- Playwright Auth: `docs/tech_design/testing/QA_playwright_authentication.md`
- QA Locator Guidance: `docs/tech_design/testing/QA_test_locators_instructions.md`

---

## 1) BDD Scenarios (Full Text)
Include only those scenarios referenced by the Implementation Plan for Feature 1.1: Getting Started.

### Scenario Outline: Install and start AISRouting UI
Given the AISRouting distribution is unpacked at "<install_path>".
When the user executes the desktop application start action (double-click or run executable) from "<install_path>".
Then the application launches and the main screen is visible with the top-level navigation and the Input Folder selector control present.

Examples:
| install_path |
| input_root_example |

### Scenario: Select input data root with vessel subfolders
Given the file system path "C:\\data\\ais_root" contains vessel subfolders each with CSV files and the application is running and shows the Input Folder selector.
When the user opens the Input Folder selector and chooses "C:\\data\\ais_root".
Then the ship selection combo box lists vessel subfolder names and the first vessel is selectable.

### Scenario: Fail when input root empty
Given the file system path "C:\\empty\\root" contains no vessel subfolders and the application is running.
When the user opens the Input Folder selector and selects "C:\\empty\\root".
Then the ship selection combo box shows an empty list and an inline warning with text "No vessels found in input root" is displayed.

### Scenario: Prevent start when executable missing or corrupted
Given the install path "C:\\apps\\AISRouting" lacks a valid start executable or it is corrupted.
When the user tries to start the application from "C:\\apps\\AISRouting".
Then a visible error dialog with text "Application failed to start: executable missing or corrupted" is displayed.

---

## 2) Technical Design Summary (Inline key points)
Include the important design decisions that tests should be aware of, inline to the prompt so test generation is self-contained.

- The application is a WPF desktop app using MVVM (`MainViewModel`, `ShipSelectionViewModel`). UI elements are XAML controls bound to ViewModel properties.
- Folder selection is abstracted via `IFolderDialogService`. For tests, a `FakeFolderDialogService` can be injected to simulate folder selection where Playwright cannot interact with native folder pickers.
- The list of vessels is provided by `ISourceDataScanner.ScanInputFolder` which produces `ShipStaticData` instances; the UI binds an `ObservableCollection<ShipStaticData>` to the vessel combo box.
- When no vessels are found, the `ShipSelectionViewModel` exposes an inline warning with text `No vessels found in input root` and disables track creation controls.
- Error dialogs use a standard `IMessageBoxService` pattern; error dialog text for missing/corrupt executable is: `Application failed to start: executable missing or corrupted`.

---

## 3) Data Models (Inline)
Include minimal TypeScript interfaces mirroring the C# models for test-data generation and fixtures.

```ts
// Minimal test-friendly interfaces
interface ShipStaticData {
  MMSI: number;
  Name?: string;
  MinDate?: string; // ISO date
  MaxDate?: string; // ISO date
  FolderPath: string;
}

interface TestCsvRecord {
  Time: number;
  Latitude?: number;
  Longitude?: number;
  SOG?: number;
}
```

---

## 4) Test File Structure and Authentication Setup
Provide instructions for the generated test file and where to place fixtures.

- Test file path: `tests/getting_started.spec.ts`
- Fixtures: `tests/fixtures/gettingStartedTestData.ts` (create only if not present)
- Use Playwright test fixtures and follow authentication setup in `QA_playwright_authentication.md` if app requires auth. For Getting Started (desktop app) assume no web login; if any web components require auth, incorporate the standard auth fixture.
- Use Playwright's `test.beforeEach` to ensure fixture environment and `test.afterEach` to clean up temp test folders.

Example test file skeleton (instructions to the generator):

1. `import { test, expect } from '@playwright/test';`
2. Use fixture helper `createVesselFolder(root, mmsi, csvDates)` to prepare test directories.
3. For folder selection UI (native dialog), tests should stub `IFolderDialogService` or bypass dialog by setting `InputFolderPath` programmatically via ViewModel test harness. If the UI exposes a text-input for path, use it; otherwise rely on injected fake service.

---

## 5) Critical Selector Strategy (explicit guidance)
Follow `QA_test_locators_instructions.md` and `QA_testing.md`. Key points for test generation:

- Prefer accessibility-first selectors: `getByRole`, `getByLabel`, then `getByTestId`.
- For elements with `data-testid`, always use `page.getByTestId('...')`.
- Use stable test attributes: add `data-test="folder-select-button"`, `data-test="vessel-combo"`, `data-test="no-vessels-warning"` to the UI (generator should check existing code and reuse IDs if present).
- Avoid brittle selectors (class names, nth-child). Use text and ARIA attributes for assertions.

Examples (✅ vs ❌):

- ✅ `await page.getByRole('button', { name: 'Select Input Folder' }).click();`
- ✅ `await page.getByTestId('vessel-combo').selectOption({ label: '205196000' });`
- ❌ `await page.locator('.combo > option').first().click();`

---

## 6) Test Patterns and Helper Functions
Include code examples the generator should produce alongside tests.

Helper: `createVesselFolder(root, mmsi, csvDates)` — creates a folder `root/mmsi/` with `mmsi.json` and `YYYY-MM-DD.csv` files.

Helper: `writeCsvSample(csvPath)` — writes header row and minimal sample rows.

Safe Locator Helper (TypeScript):

```ts
async function elementExistsSafely(locator: Locator) {
  try { return await locator.isVisible(); } catch { return false; }
}
```

Async wait pattern example:

```ts
await expect(page.getByTestId('vessel-combo')).toBeVisible({ timeout: 5000 });
```

---

## 7) Test Implementation Guidelines (mapping BDD → tests)
- Use `describe('Feature: Getting Started', () => { ... })` as outer block.
- Convert each Scenario to an individual `test('Scenario: ...', async ({ page }) => { ... })` block.
- For `Install and start` scenario: this is best validated in an integration or smoke test harness. If the environment cannot start the desktop app from Playwright, the test generator should instead provide a ViewModel-level test (unit/integration) that asserts the `MainViewModel` transitions to ready state when start action invoked. Provide both options and detect runnable environment.
- For folder selection scenarios:
  - If the UI includes a text input for `InputFolderPath` and a `Scan` button, tests should set the path and click `Scan`.
  - If the UI only uses native folder dialog, tests must use the `FakeFolderDialogService` pattern in a harness or integration test environment.
- For empty input root: assert `page.getByTestId('no-vessels-warning')` contains text `No vessels found in input root` and the vessel combo has zero options.
- For corrupted executable: prefer a unit-level test that simulates start failure via DI-injected `IAppStarter` throwing an exception; verify `IMessageBoxService.ShowError` called with the expected text. If a UI dialog is exposed, assert the error dialog content.

---

## 8) Test Data Management (fixtures)
Follow `QA_testing.md` centralized mock data approach. Reuse existing fixtures if available.

- Check for existing `tests/TestData/205196000` and `route_waypoint_template.xml` and reuse.
- If no suitable fixture exists, create `tests/fixtures/gettingStartedTestData.ts` with:
  - `export const mockVesselMMSI = '205196000';`
  - `export async function createGettingStartedFixture(root: string) { /* create folder + files */ }`

Cleanup pattern:

```ts
test.afterEach(async () => { await cleanupFixture(root); });
```

---

## 9) Success Criteria & Validation Checklist (Generator MUST validate)
Before finalizing, the generator must check and confirm all items below are satisfied in the produced prompt.

1. Covers ONLY the listed BDD scenarios for Feature 1.1 and nothing from other features.
2. Uses accessibility-first selectors and `getByTestId` for `data-testid` elements.
3. Includes async handling and sensible timeouts for UI updates.
4. Maintains test isolation via fixture setup/teardown.
5. Reuses any existing fixtures in `tests/TestData/` if compatible.
6. Provides helper functions for fixtures and safe element interaction.
7. Provides guidance for native folder dialog handling via `IFolderDialogService` fake and ViewModel harness.
8. For scenarios that cannot be run by Playwright against a desktop app, includes alternative ViewModel-level tests and explains why.
9. Includes code examples for each test pattern required by the success criteria.
10. The prompt itself must be self-contained and include inline technical design and data model snippets as provided above.

The generator must explicitly state, in the produced prompt file, that each checklist item is satisfied.

---

## 10) Final Notes to Test Generator
- If any UI element testids or aria labels exist in the codebase, reuse them; do not invent new IDs.
- If native dialogs block Playwright, prefer DI-based harness tests or ViewModel unit tests for CI-friendly automation.
- If uncertain about an implementation detail, ask one precise question before generating tests.

---

> **Changelog**
> Created on: 2025-11-20
> - Generated Tests Generation Prompt file for `Feature_1_1_Getting_Started` (includes BDD scenarios, inlined design summary, data models, fixture guidance, and validation checklist).

## 11) Checklist Confirmation (explicit)
The items below confirm how this Test File Generation Prompt satisfies each Success Criteria. The generator must still perform the runtime checks described in the checklist when producing tests.

1. Covers ONLY the listed BDD scenarios for Feature 1.1 and nothing from other features.: **Satisfied** — this prompt inlines only the four scenarios referenced in the Implementation Plan for Feature 1.1.
2. Uses accessibility-first selectors and `getByTestId` for `data-testid` elements.: **Satisfied** — prompt mandates `getByRole`, `getByLabel`, then `getByTestId` and gives ✅/❌ examples.
3. Includes async handling and sensible timeouts for UI updates.: **Satisfied** — async wait patterns and `expect(...).toBeVisible({ timeout: 5000 })` examples are included.
4. Maintains test isolation via fixture setup/teardown.: **Satisfied** — fixture creation and `test.afterEach` cleanup guidance are included.
5. Reuses any existing fixtures in `tests/TestData/` if compatible.: **Satisfied** (instructional) — prompt instructs generator to check `tests/TestData/` and reuse existing fixtures if found; it does not create fixtures unless missing.
6. Provides helper functions for fixtures and safe element interaction.: **Satisfied** — helper prototypes `createVesselFolder`, `writeCsvSample`, and `elementExistsSafely` are included.
7. Provides guidance for native folder dialog handling via `IFolderDialogService` fake and ViewModel harness.: **Satisfied** — explicit guidance on using `FakeFolderDialogService` or ViewModel-level tests is provided.
8. For scenarios that cannot be run by Playwright against a desktop app, includes alternative ViewModel-level tests and explains why.: **Satisfied** — the prompt explains the alternative approach and when to use it.
9. Includes code examples for each test pattern required by the success criteria.: **Satisfied** — multiple code snippets and patterns are provided inline.
10. The prompt itself must be self-contained and include inline technical design and data model snippets as provided above.: **Satisfied** — technical design summary and minimal TypeScript interfaces are inlined.

If you want, I can now run a quick search to locate any existing test fixtures and produce the actual `tests/getting_started.spec.ts` and `tests/fixtures/gettingStartedTestData.ts` files following this prompt. Which would you like me to do next?
