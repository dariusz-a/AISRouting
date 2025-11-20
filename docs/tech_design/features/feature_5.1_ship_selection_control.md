# Feature 5.1: Ship Selection Control

Purpose: Provide a clear, accessible UI for selecting a ship (MMSI) from scanned input folders and surface the ship's static metadata so downstream controls (time pickers, process button) can be enabled.

Context:
- Depends on Feature 3.2: MMSI Folder Scanning and Feature 2.1/2.2 JSON & CSV parsing.
- Consumers: Time Interval Controls (Feature 6.1), ShipStaticData Panel (Feature 5.3), Process workflow (Feature 9.1).

User Stories:
- As a user, I want to see a list of discovered MMSI entries with metadata so I can pick a ship to generate a route for.
- As a user, I want rows without CSV data to be selectable but disabled, so I do not attempt processing on incomplete data.
- As a user, I want selecting a ship to populate the static data panel and enable dependent controls.

UI Controls & Layout:
- Ship Table (main control): columns:
  - `MMSI` (sortable)
  - `Name` (from ShipStaticData.name)
  - `Country` (from ShipStaticData.flag or similar)
  - `First Seen` (earliest timestamp from CSVs)
  - `Last Seen` (latest timestamp from CSVs)
  - `CSV Count` (number of CSV files found)
  - `Status` (e.g., `Ready`, `No CSVs`, `Malformed`)
- Selection model: single-row selection. Keyboard and mouse support.
- Disabled rows: visually dimmed and not selectable. Tooltip explaining why.

Data & State Model:
- Input: folder scan result provides for each MMSI:
  - `mmsi: string`
  - `staticData?: ShipStaticData`
  - `csvFiles: string[]`
  - `firstSeen?: ISODateString`
  - `lastSeen?: ISODateString`
  - `status: 'ready' | 'no-csv' | 'malformed'`
- Local UI state:
  - `selectedMmsi?: string`
  - `selectedShipStaticData?: ShipStaticData`
  - `isSelectionEnabled: boolean` (derived)

Behavior:
- On input-folder scan complete, populate Ship Table with discovered MMSIs and metadata.
- Sort default: `MMSI` ascending.
- Clicking an enabled row sets `selectedMmsi` and triggers loading of `selectedShipStaticData` (from pre-parsed JSON or lazy-load `.json`).
- Selecting a ship enables downstream controls: time pickers, `Process` button (when all other prerequisites met), and shows ShipStaticData panel.
- Attempting to select a disabled row shows a small inline tooltip: "No CSV files found for this MMSI." and does not change selection.
- Double-click (optional): open a read-only detail view of the ship's static JSON.

Validation & Edge Cases:
- If static JSON is missing or malformed, mark `status` as `malformed` and provide a recovery action ("View raw file", "Re-scan").
- If CSV timestamps are inconsistent (firstSeen > lastSeen), log and show `Malformed` status.
- Very large MMSI lists should be virtualized in the table for performance.

Accessibility:
- Table rows must be navigable via keyboard (arrow keys) and selectable with Enter/Space.
- Provide aria-labels for columns and a visible focus ring.

Telemetry & Logging:
- Log selection events with `mmsi` and `csvCount` to aid debugging.

Acceptance Criteria (mapped to tests):
- Display MMSI rows with metadata: table shows `MMSI`, `Name`, `First Seen`, `Last Seen`, `CSV Count`, and `Status`.
- Selecting a ship populates `ShipStaticData` panel and enables time pickers and process prerequisites.
- Ship row without CSV files is disabled and not selectable; tooltip explains reason.
- Sorting ship table by column works (unit test for MMSI ascending default).

Tests to Implement:
- Unit: `ShipTable` renders rows given a mocked scan result; disabled rows have `aria-disabled` and do not trigger selection events.
- Unit: selecting a row emits `selectedMmsi` and the parent loads `selectedShipStaticData`.
- Integration/E2E: after selecting an input folder, the Ship Table populates and selecting a ready ship enables downstream controls.

Implementation Tasks (high level):
1. UI component: `ShipTable` with props `items[]`, `onSelect(mmsi)`, `sortState`.
2. Hook/service: `useMmsiScanResults()` or equivalent to provide scanned data to UI.
3. Action handlers: `handleSelectMmsi(mmsi)` updates global state and triggers loading of `ShipStaticData`.
4. ShipStaticData panel: ensure read-only rendering and export action remains available.
5. Tests: add unit and E2E skeletons and fill with BDD scenarios.
6. Accessibility pass and performance (virtualization) if list length > 500.

Dependencies:
- Feature 3.2 MMSI Folder Scanning (data source)
- Feature 2.1/2.2 parsers (CSV/JSON)

Risks & Mitigations:
- Missing JSON files: show clear UI state and provide "Re-scan" action.
- Large lists: implement virtualization and server-side paging if later required.

Rollout Notes:
- This feature is UI only and can be developed behind a feature flag for integrated testing with folder scanning.
