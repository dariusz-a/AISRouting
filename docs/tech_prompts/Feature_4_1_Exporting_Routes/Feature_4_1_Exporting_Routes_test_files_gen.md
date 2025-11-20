# Test File Generation Prompt: Feature 4.1: Exporting Routes

## Task:
Generate a new file named (`tests/export_route.spec.ts`) or update if it is existing, following the guidelines below.

The document is purely about test generation, not feature implementation. It's designed to create tests that will guide TDD (Test-Driven Development) implementation.

If the guidelines are ambiguous you MUST ask a single clear follow-up question and wait.

## Role:
When executing this prompt, you MUST assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, TypeScript, and robust test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.
Assume all generated UI code uses semantic HTML elements and includes proper accessibility attributes so that tests can reliably select them with `getByRole`, `getByLabel`, `getByTestId`, and other accessibility-first selectors.

**Selector Requirement Update:**
Follow the detailed selector requirements and testing framework specifications in the `docs/tech_design/overall_architecture.md` and the `docs/tech_design/testing/` directory. THESE FILES MUST ALWAYS BE USED AS A MANDATORY INPUT REQUIREMENT.
Always check the application code for existing selectors before writing or updating tests. If you find `data-testid` attributes for the elements needed, reuse them and do not create new ones.

## References
- BDD Scenarios: `docs/spec_scenarios/export_route.md`
- Test File: `tests/export_route.spec.ts`
- Technical Design Document: `docs/tech_design/core_features/exporting_routes_design.md`
- Implementation Plan: `docs/tech_design/implementation_plan.md`
- Application Architecture: `docs/tech_design/overall_architecture.md`
- Testing documentation: `docs/tech_design/testing/QA_testing.md`

## Required Content Extraction

#### 1. BDD Scenarios (Full Text)

Extracted from `docs/spec_scenarios/export_route.md` — ONLY scenarios for Feature 4.1 are included below.

---

# Feature: Exporting Routes
This feature describes exporting a generated track into an XML file with the standard route template structure.

## Positive Scenarios

### Scenario Outline: Export generated track to XML with valid output path
	Given a generated track exists for ship "<mmsi>" and the user "<user_id>" is logged in.
	When the user clicks the Export button, selects output folder "<output_path>" and confirms the export.
	Then a file named "<mmsi>-<start>-<end>.xml" should be created at "<output_path>" containing a `<RouteTemplates>` root with a single `<RouteTemplate Name="<mmsi>">` element containing an ordered list of `<WayPoint/>` elements.

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

### Scenario: Append numeric suffix on filename conflict
	Given a generated track exists and target filename "205196000-20250315T000000-20250316T000000.xml" already exists in "C:\\tmp\\exports" and the user "scenario-user" selects "Append numeric suffix" in the prompt.
	When the user confirms export.
	Then the application creates a new file such as "205196000-20250315T000000-20250316T000000 (1).xml" and no existing file is overwritten and a success message is shown.

### Scenario: Export WayPoint attribute mapping
	Given a generated track for ship "205196000" contains AIS records with sample values and the user "scenario-user" initiates export to "C:\\tmp\\exports".
	When the export completes and the XML is opened.
	Then each `<WayPoint>` element includes attributes mapped as: Name=MMSI, Lat=CSV latitude, Lon=CSV longitude, Alt=0, Speed=SOG, ETA=EtaSecondsUntil or 0, Delay=0, Mode=computed via SetWaypointMode (TODO), TrackMode="Track", Heading=Heading or 0, PortXTE=20, StbdXTE=20, MinSpeed=0, MaxSpeed=maximum SOG observed in range.

---

#### 2. Technical Design Summary (Inline)

Include the critical design points from `docs/tech_design/core_features/exporting_routes_design.md` relevant to tests:

- Purpose: Export `RouteWaypoint` list into XML using `IRouteExporter` and `XmlRouteWriter`.
- Key components to validate in tests: `ExportDialogView`/`ExportDialogViewModel` orchestration, `IRouteExporter` conflict behavior, `XmlRouteWriter` attribute mapping, and `FileSystemHelper` permission checks.
- Filename generation pattern to assert in tests: `{mmsi}-{start:yyyyMMddTHHmmss}-{stop:yyyyMMddTHHmmss}.xml`.
- Conflict resolution flows: Overwrite, AppendNumericSuffix, Cancel — tests must simulate both overwrite and suffix choices.
- Expected XML structure: `<RouteTemplates>` -> `<RouteTemplate Name="{MMSI}">` -> ordered `<WayPoint ... />` elements. Numeric formatting uses `InvariantCulture` and `F6` precision for lat/lon in writer.
- Selectors documented for UI interactions: `data-testid="export-button"`, `data-testid="export-output-folder"`, `data-testid="export-conflict-prompt"` with options `overwrite`, `suffix`, `cancel`.

#### 3. Data Models (Inline)

Include simplified TypeScript interfaces used by tests (mapped from C# models):

```ts
interface ShipDataOut {
  BaseDateTime: string; // ISO timestamp
  Lat?: number;
  Lon?: number;
  SOG?: number; // Speed over ground
  Heading?: number;
  EtaSecondsUntil?: number;
}

interface RouteWaypoint {
  Index: number;
  Time: string; // ISO timestamp
  Lat: number;
  Lon: number;
  Speed: number;
  Heading: number;
  Name: string; // MMSI
  Mode?: string;
  TrackMode?: string;
  ETA?: number;
  PortXTE?: number;
  StbdXTE?: number;
  MinSpeed?: number;
  MaxSpeed?: number;
}
```

Mock fixture guidance:
- Reuse existing fixture folder `tests/TestData/205196000/` if present.
- If using inline Playwright file-system checks, create temp folders via Node `fs` and clean them up in `test.afterEach()`.
 
Note: The repository contains `tests/TestData/205196000/` with sample CSVs (`noisy.csv`, `sample1.csv`) which can be reused by export tests to seed generated tracks. Prefer reusing these fixtures instead of creating new ones.

## Test File Structure with authentication setup

Follow authentication steps in `docs/tech_design/testing/QA_playwright_authentication.md`. Ensure tests login with provided automated test credentials before initiating export.

### Tests to Generate (one `test()` per BDD Scenario)

1) `Scenario Outline: Export generated track to XML with valid output path` → `test('Export generated track to XML creates expected file', async ({ page, tmpDir }) => { ... })`
  - Use fixture generated track (reuse `tests/TestData/205196000` when possible).
  - Steps: login, navigate to track view, ensure generated track present (or seed via API/mock), click `data-testid=export-button`, pick temp folder, confirm, then assert file exists with expected name and contains `<RouteTemplates>` root and one `<RouteTemplate Name="{mmsi}">` with `<WayPoint/>` children.

2) `Prompt on filename conflict and overwrite chosen` → `test('Overwrite existing export file when user chooses Overwrite', ... )`
  - Pre-create file at target path with expected filename.
  - Trigger export and respond to `data-testid=export-conflict-prompt` selecting `overwrite` option.
  - Assert file content updated and UI shows `Export successful` confirmation.

3) `Fail export when output path not writable` → `test('Fail export when output path not writable', ... )`
  - Create a read-only folder (or mock `FileSystemHelper.CanWriteToFolder` if integration not possible).
  - Trigger export; assert visible error banner `data-testid=export-error` with text `Cannot write to output path: C:\protected\exports` and assert no new file created.

4) `Append numeric suffix on filename conflict` → `test('Append numeric suffix on filename conflict', ... )`
  - Pre-create file with target name.
  - Trigger export and select `suffix` in `export-conflict-prompt`.
  - Assert a new file with ` (1)` appended is created and original file preserved.

5) `Export WayPoint attribute mapping` → `test('WayPoint attribute mapping in generated XML', ... )`
  - Use deterministic fixture with varying SOG/Heading/Eta to validate attribute values.
  - Export to temp folder, read XML, parse with an XML parser and assert attributes per mapping rules (Name, Lat, Lon, Alt=0, Speed, ETA, Delay=0, Mode present, TrackMode="Track", Heading, PortXTE=20, StbdXTE=20, MinSpeed=0, MaxSpeed computed).

## Test Data Management

- Reuse fixtures under `tests/TestData/205196000/` when possible.
- If no fixture exists for the scenario outline example values, create `tests/fixtures/exportRouteFixtures.ts` containing:

```ts
export const sampleShipDataOut: ShipDataOut[] = [
  { BaseDateTime: '2025-03-15T00:00:00Z', Lat: 59.123456, Lon: 10.123456, SOG: 10, Heading: 90, EtaSecondsUntil: 3600 },
  { BaseDateTime: '2025-03-15T00:05:00Z', Lat: 59.123556, Lon: 10.123556, SOG: 12, Heading: 95 }
];

export function createTempTrackFolder(tmpDir: string, mmsi = '205196000') {
  // helper stub — tests should create CSV or JSON fixtures consumed by the app
}
```

- Provide helper guidance in prompt for test authors to prefer existing fixture reuse and to create minimal new fixtures only when necessary.

## Critical Selector Strategy Updates with ❌/✅ examples

- ✅ Use `page.getByTestId('export-button')` for the export control when `data-testid` exists.
- ✅ Use `page.getByRole('dialog', { name: /export/i })` for the export dialog where accessible.
- ❌ Do not use `waitForSelector` for elements that expose `data-testid`. Prefer `getByTestId`.
- ❌ Avoid brittle text-based selectors for file names; instead assert on file content or structured attributes in XML.

## Test Patterns with complete code examples according to project architecture

Include example code snippets (Playwright + TS) demonstrating:
- Login helper usage per `QA_playwright_authentication.md`.
- Selecting export folder using a native folder picker simulation or mocking `IFolderDialogService` in test harness.
- Handling conflict prompt via `page.getByTestId('export-conflict-prompt').getByRole('button', { name: /overwrite/i })`.

## Locator Patterns specific to the feature

- `data-testid="export-button"` — Export action
- `data-testid="export-output-folder"` — Output folder input/button
- `data-testid="export-conflict-prompt"` — Conflict dialog container with options `overwrite`, `suffix`, `cancel`
- `data-testid="export-success"` — Success message area
- `data-testid="export-error"` — Error banner

## Common Actions with helper functions

- `async function loginAsScenarioUser(page) { /* use QA_playwright_authentication.md flow */ }`
- `async function ensureGeneratedTrackExists(mmsi, tmpDir) { /* seed app with fixture or use API */ }`
- `async function readXmlFile(path) { /* fs.readFile + parse via fast-xml-parser or xmldom */ }

## Test Implementation Guidelines with conditional logic

- Use `test.beforeEach()` to perform login and environment setup; `test.afterEach()` to clean up created files and folders.
- For permission error tests, prefer mocking `FileSystemHelper` if available in test harness; otherwise create OS-level read-only folder and ensure cleanup.

## Character Limit Testing Pattern addressing browser behavior

- Not applicable beyond filename length checks; include a single test ensuring long filenames produce a handled error or suffixing behavior if needed.

## Practical Validation Testing with realistic expectations

- Validate XML structure and a sample of attributes rather than asserting the entire file byte-for-byte (use `XmlAssert.AreEqualIgnoringWhitespace` equivalent in JS).

## Success Criteria for the generated test file

1. Covers ONLY feature-related positive scenarios from the BDD file using robust, unambiguous selectors.
2. Covers ONLY feature-related negative/validation scenarios with realistic expectations about what's implemented.
3. Uses accessibility-first, specific selectors with explicit guidance on avoiding ambiguity.
4. Includes proper async handling as specified in the project's architecture documentation.
5. Maintains test isolation.
6. Follows established test patterns with comprehensive examples as defined in the project's architecture and testing documentation.
7. Guides TDD implementation.
8. Includes all Required Code Examples in alignment with the project's testing framework.
9. Provides practical implementation considerations.
10. Provides complete code examples for all recommended patterns.

## Validation Checklist

- [ ] Confirmed tests reference `data-testid` selectors where present in codebase.
- [ ] Confirmed reuse of existing fixtures in `tests/TestData/205196000/` if compatible.
- [ ] Confirmed authentication flow usage per `QA_playwright_authentication.md`.
- [ ] Verified filename generation format in tech design and used same pattern in tests.

Validation notes:
- `data-testid` selectors are documented in the design doc but not present in compiled UI code; tests should check for them and fall back to accessible roles/labels if missing.
- Existing fixtures found at `tests/TestData/205196000/` will be reused.
- Authentication credentials: use `alice.smith@company.com` / `SecurePass123!` per `QA_playwright_authentication.md`.

Please ask one follow-up question if any of the following are required before generating test code:

- Do you want me to generate the actual Playwright test file `tests/export_route.spec.ts` now using the fixtures and helper stubs described above?
- Should I create a minimal fixture file `tests/fixtures/exportRouteFixtures.ts` in this change if existing TestData is insufficient?
