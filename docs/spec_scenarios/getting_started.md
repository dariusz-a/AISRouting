<!-- Refined per workflow/2.1_precise_bdd_spec_scenarios.md guide. Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started

Short description: Steps to launch the application, select folders, choose a ship and time interval, and generate a route.

## Positive Scenarios

### Scenario: Display main window controls on launch
  Given a signed-in desktop operator user with id "person-alice" and role id "role-operator" is on a Windows 11 machine with the application executable accessible at `C:\Apps\AisToXmlRouteConvertor.exe` and the user starts the application.
  When the main window initializes and loads UI components.
  Then a window with data-testid="main-window" is visible and shows controls data-testid="input-folder-picker" and data-testid="output-folder-picker" and a table data-testid="ship-table" and a panel data-testid="ship-static-panel" and time pickers data-testid="time-start" and data-testid="time-end" and a disabled button data-testid="process-btn" labeled "Process!".

### Scenario: Populate ship table after selecting folders
  Given the operator user "person-alice" with role id "role-operator" is viewing data-testid="main-window" and selects an input folder containing MMSI subfolders including "205196000" and selects a writable output folder path.
  When the user chooses the input folder via data-testid="input-folder-picker" and the output folder via data-testid="output-folder-picker".
  Then the application scans subdirectories and renders a table row in data-testid="ship-table" with column MMSI="205196000" and displays the chosen output path in a readonly field data-testid="output-path" and enables the ship row selection while keeping data-testid="process-btn" disabled until a ship row and valid times are set.

### Scenario: Generate route for selected ship within valid time interval
  Given the ship table in data-testid="ship-table" lists MMSI "205196000" with available data range start "2025-03-15 00:00" and end "2025-03-16 00:00" and the operator selects that row and chooses Start time "2025-03-15 06:00" and End time "2025-03-15 18:00" within range.
  When the user clicks the enabled button data-testid="process-btn" labeled "Process!".
  Then a modal message box data-testid="result-modal" appears showing text "Route generated: 205196000_2025-03-15_0600_1800.xml" and the file is written to the previously selected output folder with exact filename matching the message.

## Negative Scenarios

### Scenario: Prevent processing when no ship selected
  Given input and output folders are selected and data-testid="ship-table" is populated but no row has selected state data-selected="true".
  When the user attempts to activate data-testid="process-btn" by clicking it.
  Then data-testid="process-btn" remains disabled (HTML disabled attribute present) and no processing request is sent (no network call initiated) and an inline helper text data-testid="validation-inline" shows "Select a ship to enable processing".

### Scenario: Show validation error when Start time after End time
  Given a ship row for MMSI "205196000" is selected and Start time picker data-testid="time-start" is set to "2025-03-15 19:00" and End time picker data-testid="time-end" is set to "2025-03-15 08:00".
  When the application evaluates time inputs on blur of data-testid="time-end".
  Then an inline validation element data-testid="validation-inline" appears with text "Start time must be before End time" and data-testid="process-btn" remains disabled and aria-invalid="true" is set on both time inputs.

## Edge & Permission Scenarios

### Scenario: Graceful handling of transient backend processing failure
  Given a valid ship selection MMSI "205196000" and valid Start time "2025-03-15 06:00" and End time "2025-03-15 18:00" are set and data-testid="process-btn" is enabled and a transient backend error (HTTP 503) occurs during route generation.
  When the user clicks data-testid="process-btn" and the request is sent.
  Then an error banner data-testid="error-banner" appears with text starting "Processing failed:" followed by the backend status "503 Service Unavailable" and no partial output file is created (output directory does not contain a file matching pattern `205196000_2025-03-15_0600_1800*.xml.tmp`) and data-testid="process-btn" is re-enabled for retry.

### Scenario: Disable processing until both times selected
  Given a ship row for MMSI "205196000" is selected and only Start time data-testid="time-start" is set while End time data-testid="time-end" is empty.
  When the user focuses out of data-testid="time-start".
  Then data-testid="process-btn" stays disabled and an inline helper data-testid="validation-inline" shows "Select both Start and End time" and the End time input receives focus.

### Scenario: Output folder not writable prevents enablement
  Given the operator selects an input folder and an output folder path that is not writable (filesystem access denied) and ship table is populated.
  When the application attempts to validate the output folder after selection.
  Then an inline error data-testid="validation-inline" appears with text "Output folder is not writable" and data-testid="process-btn" remains disabled and the output folder field shows aria-invalid="true".

### Examples:
  | user_id      | role_id       | mmsi      |
  | person-alice | role-operator | 205196000 |

Note: The `mock_data.md` source file was not found; scenario entity values assume presence of these example IDs. Replace with actual entries from `docs/workflow/inputs/mock_data.md` when available.
