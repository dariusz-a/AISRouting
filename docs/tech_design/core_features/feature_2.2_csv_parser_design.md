# Feature Design:: Feature 2.2: CSV Parser

This document outlines the technical design for the Feature 2.2: CSV Parser feature

#### Feature Overview
- Purpose: Parse AIS CSV files placed in MMSI folders, validate rows against the ShipDataOut schema, produce in-memory ShipState records, and expose available date ranges for UI selection.
- Scenarios covered (from `docs/spec_scenarios/input_data_preparation.md`):
  - Recognize valid MMSI folder structure
  - Accept CSV files with required schema
  - Reject CSV with invalid filename format
  - Missing `<MMSI>.json` file produces TODO note
- Business value: Reliable ingestion of AIS position data is foundational for the conversion pipeline. Correct parsing ensures downstream modules (track optimizer, XML export) operate on clean, validated data and that the UI can present accurate available date ranges and error messages.
- High-level approach: Implement a small, well-tested parsing service in the application layer that uses streaming parsing for large files, strict schema validation with a tolerant mode for optional fields, and clear error reporting. Provide a synchronous facade API for UI interactions and an asynchronous worker for heavy parsing tasks.

#### Architectural Approach
- Patterns and principles:
  - Single Responsibility: Parsing logic lives in a `CsvParser` service; validation logic in `CsvSchemaValidator`; file scanning in `InputScanner`.
  - Fail-fast for schema-critical fields; tolerant handling for optional fields with warnings logged.
  - Streaming processing for memory efficiency (line-by-line parser) and ability to cancel long-running parses.
  - Clear boundaries: UI calls a lightweight controller/service which orchestrates scanning and parsing, returning structured results and diagnostics.
- Component hierarchy:
  - `InputScanner` — enumerates MMSI subfolders and candidate CSV files.
  - `CsvParser` — reads and parses CSV rows into `ShipState` records.
  - `CsvSchemaValidator` — validates parsed rows against the canonical `ShipDataOut` schema (defined in `docs/tech_design/data_models.md`).
  - `ParseResultRepository` (in-memory store) — stores parsed records, available date ranges and parsing diagnostics for the UI.
  - `TodoReporter` — flags missing `<MMSI>.json` files and surfaces TODO notes.
- Data flow and state management:
  1. UI selects input folder -> `InputScanner.Scan(folder)` returns MMSI folders and candidate CSV filenames.
  2. For a chosen MMSI, `ParserController.ParseCsvFiles(mmsiPath)` orchestrates parsing each CSV via `CsvParser.Parse(file)`.
 3. `CsvParser` streams rows, uses `CsvSchemaValidator` to validate, and pushes valid `ShipState` objects into `ParseResultRepository`.
 4. After parsing, `ParseResultRepository` computes available date ranges and exposes diagnostics (ignored files, malformed rows).
- Integration patterns:
  - `CsvParser` uses the project's logging subsystem for warnings and errors.
  - The validator references canonical type definitions from `docs/tech_design/data_models.md` and shared `types` module in `src/utils/types`.
  - For large files, parsing may be executed on a background worker thread and progress reported via events/observable.

#### File Structure
Follow `docs/tech_design/application_organization.md` naming and organization patterns.

```
src/
  services/
    input/
      InputScanner.ts            # Scans input folder and enumerates MMSI subfolders
      TodoReporter.ts            # Detects missing MMSI.json and creates TODO notes
    parser/
      CsvParser.ts               # Streaming CSV parser, orchestrates row processing
      CsvSchemaValidator.ts      # Validates row objects against ShipDataOut schema
      ParserController.ts        # High-level facade used by UI and commands
    store/
      ParseResultRepository.ts   # In-memory store for parsed ShipState records and diagnostics
  types/
    ship.ts                     # ShipStaticData, ShipState, RouteWaypoint types (shared)
  utils/
    csv.ts                      # small CSV helpers (safe parse, header normalization)
  tests/
    parser/
      csv_parser.spec.ts
      csv_schema_validator.spec.ts
      input_scanner.spec.ts

docs/
  tech_design/
    core_features/
      feature_2.2_csv_parser_design.md  # THIS DOCUMENT

```

Purpose of files:
- `InputScanner.ts`: Collects candidate CSV files using filename regex `^\d{4}-\d{2}-\d{2}\.csv$` and enumerates MMSI subfolders.
- `TodoReporter.ts`: Checks for `<MMSI>.json` presence; if missing, returns a `TodoNote` object which UI displays.
- `CsvParser.ts`: Streams CSV rows using a robust parser (built-in or small 3rd-party); normalizes headers; maps columns to `ShipState` fields; emits progress and diagnostics.
- `CsvSchemaValidator.ts`: Validates required fields (timestamp, latitude, longitude) and optional fields; returns typed validation results.
- `ParseResultRepository.ts`: Stores parsed `ShipState[]`, aggregates min/max timestamps, and exposes `getAvailableDateRange()`.

#### Component Architecture
1) InputScanner
  - Purpose: Discover valid MMSI subfolders and CSV files matching the expected pattern.
  - Design patterns: Simple service with synchronous enumeration API. Uses regex-based filters and returns results with metadata (file path, last modified).
  - Interaction: Called by UI startup and by ParserController prior to parsing. Emits warnings for ignored files.

2) CsvParser
  - Purpose: Convert CSV rows into `ShipState` objects.
  - Design patterns: Streaming parser with pluggable row handler. Uses an internal pipeline: read file stream -> normalize headers -> parse row -> map to domain -> validate -> emit/store.
  - State management: Stateless per file; incremental writes to `ParseResultRepository`.
  - Accessibility & UX: Provides progress events and error summaries for UI. Parsing can be cancelled.

3) CsvSchemaValidator
  - Purpose: Ensure parsed rows conform to domain expectations.
  - Design patterns: Pure function set that returns `ValidationResult` objects. Allows two modes: `strict` (reject rows with missing required fields) and `tolerant` (skip non-critical fields and attach warnings).

4) ParserController
  - Purpose: High-level orchestration for UI. Exposes simple methods:
    - `scanInputFolder(path): ScanSummary`
    - `parseMmsiFolder(mmsiPath, options): ParseReport`
  - Responsibilities: coordinate `InputScanner`, `CsvParser`, `TodoReporter`, and `ParseResultRepository`.

5) ParseResultRepository
  - Purpose: In-memory index of parsed records keyed by MMSI and date. Exposes `getAvailableDateRange(mmsi)` and `getRecords(mmsi, range)`.

#### Data Integration Strategy
- Data Entities:
  - `ShipState` (time-stamped position) — canonical type in `src/types/ship.ts`.
  - `ShipStaticData` — loaded separately from `<MMSI>.json`.
  - `ParseDiagnostics` — structure capturing file-level and row-level issues.
- Data flow:
  - InputScanner -> CsvParser -> CsvSchemaValidator -> ParseResultRepository -> UI
- Error handling and edge cases:
  - Invalid filename format: file ignored and logged; `ParseDiagnostics` contains ignore reason.
  - Malformed CSV row: row-level validation failure appended to `ParseDiagnostics`; depending on mode, row is skipped or parse fails for file.
  - Missing `<MMSI>.json`: `TodoReporter` adds a TODO note to show in the UI; parsing of CSVs proceeds but static fields are null and UI warns about missing metadata.
  - Empty files: treated as no-op; captured in diagnostics.
  - Duplicate timestamps: handled deterministically by keeping first occurrence and logging duplicates in diagnostics.

#### Implementation Examples
Below are illustrative TypeScript-like examples showing key parts of the implementation. They are intentionally compact and annotated with architectural comments.

1) Types: `src/types/ship.ts`

```ts
export interface ShipState {
  timestamp: string; // ISO 8601
  lat: number;
  lon: number;
  sog?: number; // speed over ground
  cog?: number; // course over ground
  heading?: number;
}

export interface ParseDiagnostics {
  file: string;
  ignored?: boolean;
  warnings: string[];
  errors: Array<{row: number; message: string}>;
}
```

2) CsvParser core loop (conceptual): `src/services/parser/CsvParser.ts`

```ts
// Open a file stream and parse line-by-line
async function parseCsvFile(filePath, validator, repository, progressCallback, cancelToken) {
  const stream = fs.createReadStream(filePath, {encoding: 'utf8'});
  let rowNumber = 0;
  for await (const line of readLines(stream)) {
    rowNumber++;
    if (cancelToken.cancelled) break;
    const obj = mapLineToObject(line, headerMap);
    const validation = validator.validateRow(obj);
    if (!validation.ok) {
      repository.appendDiagnostic({file: filePath, errors: [{row: rowNumber, message: validation.message}]});
      continue; // skip malformed row
    }
    const shipState = mapToShipState(obj);
    repository.addRecord(mmsi, shipState);
    progressCallback?.(rowNumber);
  }
}
```

3) InputScanner example (filename filtering)

```ts
function findCandidateCsvFiles(mmsiFolderPath) {
  const files = readdirSync(mmsiFolderPath);
  const csvRegex = /^\d{4}-\d{2}-\d{2}\.csv$/; // enforces YYYY-MM-DD.csv
  return files.filter(f => csvRegex.test(f)).map(f => path.join(mmsiFolderPath, f));
}
```

4) CsvSchemaValidator example (simplified)

```ts
function validateRow(obj) {
  if (!obj.timestamp) return {ok:false, message:'missing timestamp'};
  const ts = Date.parse(obj.timestamp);
  if (Number.isNaN(ts)) return {ok:false, message:'invalid timestamp'};
  if (obj.lat == null || obj.lon == null) return {ok:false, message:'missing coordinates'};
  // optional numeric coercion
  return {ok:true};
}
```

Testing hooks and selectors:
- Each service exposes observable events and returns `ParseReport` objects. Tests use these hooks to validate progress and end states.

#### Testing Strategy and Quality Assurance
- Test-first approach: write BDD-style tests that mirror the scenarios in `docs/spec_scenarios/input_data_preparation.md`.
- Unit tests:
  - `csv_schema_validator.spec.ts`: cover malformed rows, missing fields, optional fields, numeric coercion.
  - `csv_parser.spec.ts`: parse small sample files, assert records stored, diagnostics captured, progress reported.
  - `input_scanner.spec.ts`: assert filename filtering and MMSI discovery, including negative cases.
- Integration tests (in `tests/parser/integration`):
  - End-to-end parse of sample MMSI folder with `205196000.json` and `2025-03-15.csv` fixture — assert available date range computed, records accessible.
  - Missing `<MMSI>.json` fixture — assert `TodoReporter` outputs expected TODO note and parsing still proceeds.
  - Invalid filename `march15.csv` fixture — assert file ignored and diagnostic contains ignore reason.
- E2E (UI) tests:
  - Use the application's test harness to select an input folder containing fixtures and assert ship table population and available date range UI elements.

Testing hooks & mock data requirements:
- Centralized mock data approach: tests import fixtures from `tests/fixtures/` which contains example CSVs and JSONs.
- Helper functions: `tests/utils/mockDataHelpers.ts` provides functions to generate CSV contents and timestamps in ranges.
- Fixtures (examples):
  - `tests/fixtures/205196000/2025-03-15.csv`
  - `tests/fixtures/205196000/205196000.json`
  - `tests/fixtures/205196000/malformed_row.csv`
- Test data exposure: `ParseResultRepository` exposes a debug API `dumpForTest()` that returns structured data suitable for assertions.

#### Mock Data Requirements
- Follow centralized mock data approach from `docs/tech_design/testing/QA_testing.md`:
  - Store canonical fixtures in `tests/fixtures` and reference them in tests by path.
  - Provide `createSampleCsv(rows)` helper in `tests/utils/mockDataHelpers.ts`.
  - Provide `minimalShipStaticJson()` for generating minimal `ShipStaticData` when the `<MMSI>.json` is missing.

Example fixture generator (conceptual):

```ts
export function createSampleCsv(date, positions) {
  const header = 'timestamp,lat,lon,sog,cog,heading';
  return [header, ...positions.map(p=>`${p.timestamp},${p.lat},${p.lon},${p.sog||''},${p.cog||''},${p.heading||''}`)].join('\n');
}
```

#### Operational Concerns
- Performance: streaming parser avoids large memory usage; repository indexes by day to keep lookups fast.
- Concurrency: parsing per-MMSI can be parallelized; repository uses per-MMSI locks to avoid races.
- Observability: structured logs for ignored files, parsing errors, and summary metrics (rows parsed, rows skipped).

#### Acceptance Criteria (mapped to BDD scenarios)
- Recognize valid MMSI folder structure: `InputScanner.Scan()` returns MMSI folder list containing `205196000` when provided fixture.
- Accept CSV files with required schema: sample CSV parsed, records available, and available date range computed.
- Reject CSV with invalid filename format: non-matching filename ignored and diagnostic recorded.
- Missing `<MMSI>.json` file produces TODO note: `TodoReporter` returns TODO note and UI displays it.

---
Generated on: 2025-11-20
