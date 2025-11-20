# Feature 3.2 — MMSI Folder Scanning

Summary
- Provide a robust, cross-platform scanner that inspects a chosen input folder and enumerates MMSI subfolders and their contents so the UI can populate the ship list and enable subsequent processing.

Goals
- Detect MMSI subfolders (numeric folder names of 9 digits).
- Collect metadata per MMSI: presence of AIS CSV files, presence of `<MMSI>.json` static file, counts, first/last timestamps in CSV files (if parsable), and any validation warnings.
- Expose a clear scanning API usable by UI and background services.
- Provide progress, cancelation, and detailed diagnostics for failures.

Non-Goals
- Parsing CSV rows into domain records (left to CSV parser feature).
- Heavy validation of CSV file contents (only surface checks: readable, parseable header, timestamps present).

Assumptions
- Application runs on .NET 9 / Avalonia (cross-platform) and will call the scanner from UI code or from background tasks.
- Input directory is a local file system path (no remote mounts supported initially).
- MMSI folder name format is a 9-digit integer string (e.g., `205196000`).

Folder Rules
- Root input folder: user-selected path (UI). Scanner inspects immediate children only — each immediate child that matches the MMSI pattern is a candidate MMSI folder.
- Inside an MMSI folder:
  - CSV files: any `*.csv` files (expected AIS position logs). Filename pattern not strictly required during scan, but presence and readability are noted.
  - Static JSON: `<MMSI>.json` (optional). If present, validated as JSON and basic schema checks performed (object, has MMSI property).

API Design (C#)
- Public types:
  - `record MmsiScanItem(string Mmsi, bool HasCsv, bool HasJson, int CsvFileCount, DateTime? FirstTimestamp, DateTime? LastTimestamp, IEnumerable<string> CsvFileNames, IEnumerable<string> Warnings)`
  - `record MmsiScanResult(IEnumerable<MmsiScanItem> Items, IEnumerable<string> Errors)`
- Public methods:
  - `Task<MmsiScanResult> ScanInputFolderAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default)`
  - `bool IsValidMmsiFolderName(string name)` — small helper for validation and tests.

Behavior & Details
- Scanning steps:
  1. Validate `path` exists and is a directory.
  2. Enumerate immediate subdirectories.
  3. Filter subdirectories by `IsValidMmsiFolderName` (9-digit numeric string).
  4. For each candidate folder:
     - Enumerate `*.csv` files (non-recursive); count them and record filenames.
     - For each CSV file, attempt to read just enough lines (header + first and last data row) to infer timestamps. If the file is empty or unreadable, add a warning and continue.
     - Check for `<MMSI>.json` file; attempt to parse as JSON; if parse fails, add a warning.
  5. Aggregate items into `MmsiScanResult`.
- Performance: do not fully parse large CSVs; read up to first N and last N lines to extract first/last timestamps (seek from tail if file large). Limit per-file work to avoid UI freeze.
- Concurrency: scanning each MMSI folder may run in parallel (configurable). Use `Parallel.ForEachAsync` with a small concurrency cap (e.g., 4) to avoid I/O saturation.

UI Contract
- Events / progress model:
  - `ScanInputFolderAsync` accepts `IProgress<int>` reporting percent complete.
  - On completion, UI receives `MmsiScanResult` and uses it to populate the ship table (`MMSI`, `Name` from JSON if available, `FirstTimestamp`, `LastTimestamp`, `CSV count`, `Warnings`).
- The UI should display warnings per-row if present and allow the user to inspect individual debug details (file names, warnings).

Error Handling & Logging
- Errors returned in `MmsiScanResult.Errors` are fatal scan errors (e.g., root path missing). Per-folder and per-file problems should appear in `MmsiScanItem.Warnings`.
- All exceptions caught during scan are logged with context (folder, filename, error) but do not abort the whole scan unless the root path is invalid.

Acceptance Criteria / Tests (to be automated)
- Scenario: Select a valid input folder populates ship list
  - Given a temp folder with `205196000/205196000.csv` and `205196000/205196000.json`
  - When `ScanInputFolderAsync` runs
  - Then result contains an item with `Mmsi == "205196000"`, `HasCsv == true`, `HasJson == true`, `CsvFileCount == 1`.
- Scenario: Input folder with no MMSI subfolders shows error
  - Given a temp folder with no 9-digit child folders
  - When `ScanInputFolderAsync` runs
  - Then `Items` is empty and `Errors` contains a friendly message or UI shows "No MMSI folders found".
- Scenario: Reject input when MMSI folders missing
  - Covered above; also test that non-numeric folders are ignored.
- Additional tests:
  - CSV unreadable file yields item with `Warnings` containing parse issue.
  - Malformed JSON file yields `Warnings` for that MMSI.
  - Large CSVs do not require full parse; only first/last timestamps are extracted.

Implementation Tasks
- Implement `IsValidMmsiFolderName` + unit tests.
- Create `MmsiFolderScanner` service with `ScanInputFolderAsync`.
- Implement lightweight CSV tail reader utility to get last timestamp without streaming whole file.
- Wire scanner into UI input-folder selection flow and ensure table population.
- Add unit/integration tests in `tests/mmsi_folder_scanning.spec.ts` and C# unit tests where appropriate.

Open Questions
- Should scanner include recursive scan into nested subfolders (e.g., archive directories)? Default is NO — require explicit requirement to support recursion.
- CSV timestamp format variations: which columns to inspect for timestamp? (Coordinate with CSV parser feature.)

Cross-Platform & Security
- Use .NET APIs (`FileStream`, `Directory.EnumerateDirectories`, `File.OpenRead`) and avoid platform-specific calls.
- Sanitize and canonicalize paths to avoid path traversal risks.

Related Files & Tests
- Proposed new files:
  - `src/services/MmsiFolderScanner.cs`
  - `src/utils/CsvTailReader.cs`
  - `docs/tech_design/features/feature_3.2_mmsi_folder_scanning.md` (this file)
  - `tests/mmsi_folder_scanning.spec.ts`

Estimated Effort
- Prototype: 1 day
- Unit tests + integration: 1 day
- UI wiring + QA: 1 day

Done.
