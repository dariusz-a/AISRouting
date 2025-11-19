<!-- Source: /docs/user_manual/getting_started.md -->
# Feature: Getting Started

## Background
Given a Windows instructor workstation and the AISRouting installer or unpacked application files are available locally and the configured input folder contains the path `outputFolder/123456789/` with files following the pattern `<MMSI>-yyyy-MM-dd.csv`.

## Positive Scenarios

### Scenario: Install the application from installer successfully
  Given the instructor user is signed into the workstation with administrative privileges and the installer executable is present in the Downloads folder and the installer is signed by the vendor.
  When the instructor runs the installer and accepts the default install path `C:\Program Files\AISRouting` and clicks the install action in the installer UI.
  Then the installer shows a progress dialog and on completion a dialog with exact text "Installation complete" is visible and the installer offers to create a desktop shortcut.
  And the file `C:\Program Files\AISRouting\AISRouting.exe` exists on disk and the installation persists after a system reboot.

### Scenario: Launch the application and view main window
  Given the application is installed at `C:\Program Files\AISRouting` and a desktop shortcut named "AIS Routing" exists on the instructor's desktop.
  When the instructor launches the application by double-clicking the "AIS Routing" desktop shortcut.
  Then the AIS Routing main window opens with window title containing "AIS Routing" and the left navigation panel with label "Navigation" and the ship list panel with data-testid="ship-list" is visible and focused.
  And the ship list shows MMSI "123456789" when the configured input folder contains `123456789-2025-01-01.csv` and the application restores the same view after closing and re-opening.

### Scenario: Verify AIS input folder structure and file details
  Given the instructor has the input folder set to `outputFolder/` containing subfolder `123456789` and the file `123456789-2025-01-01.csv` encoded in UTF-8 with CRLF line endings.
  When the instructor opens Settings > Input Folder, selects the configured folder path `outputFolder/` and clicks "Save".
  Then the application lists the discovered file `123456789-2025-01-01.csv` in the Ship Data panel with details showing Encoding: "UTF-8" and Line endings: "CRLF" and the file name matches the expected pattern `<MMSI>-yyyy-MM-dd.csv`.
  And the setting persists after application restart and the discovered file is selectable in the Select Ship dialog.

## Negative & Edge Scenarios

### Scenario: Installation fails due to insufficient permissions
  Given the instructor user is signed into the workstation without administrative privileges and the installer is launched from the Downloads folder.
  When the installer attempts to write to `C:\Program Files\AISRouting` and perform system-level registration steps.
  Then an error dialog with exact text "Insufficient permission to install" is displayed and the installer aborts.
  And the application executable is not created and the installer offers to retry with elevated privileges.

### Scenario: Application does not start due to missing runtime dependency
  Given the target machine does not have the required runtime dependency installed (for example .NET Desktop Runtime) and the application is installed.
  When the instructor launches the application from the desktop shortcut.
  Then a troubleshooting message with exact text "Required runtime not found. Please install the required runtime to run AIS Routing." is displayed and a link labeled "Install runtime" is present.
  And the application does not proceed to the main window until the runtime is installed.

### Scenario: Missing AIS input files shows clear guidance
  Given the configured input folder is empty or the configured path is incorrect (no `outputFolder/123456789/` subfolders exist).
  When the instructor opens the Ship List panel or navigates to Settings > Input Folder.
  Then an inline banner with exact text "No AIS input files found in configured folder" is visible with a suggestion link labeled "Verify input folder" and the ship list remains empty.
  And the banner persists until a valid file is detected and the configured path is not saved as valid.

### Scenario: Input file with wrong encoding is skipped and reported
  Given the input folder `outputFolder/123456789/` contains a CSV file `123456789-2025-01-01.csv` encoded in a non-UTF-8 encoding (for example ISO-8859-1).
  When the application scans and attempts to read the file during startup processing.
  Then the file is skipped from processing and an error banner with exact text "Invalid file encoding: expected UTF-8 for file 123456789-2025-01-01.csv" is shown and the file is listed in the Diagnostics view with status "Skipped - invalid encoding".
  And the rest of valid files (if any) continue to be processed.

### Scenario: File with invalid filename format is ignored
  Given the input folder `outputFolder/123456789/` contains a file named `12345678-2025-01-01.csv` that does not match the required `<MMSI>-yyyy-MM-dd.csv` pattern.
  When the application scans the folder for AIS CSV files.
  Then the file is shown in the Diagnostics view with exact error text "Invalid filename format: expected <MMSI>-yyyy-MM-dd.csv" and the file is excluded from processing.
  And the Diagnostics view provides the offending filename for manual correction.

### Scenario: Input folder inaccessible due to permission or network error
  Given the configured input folder is located on a network share or protected folder and the instructor's user account has read access denied.
  When the application attempts to enumerate files in the configured input folder.
  Then an error dialog with exact text "Unable to access input folder: access denied" is displayed and a troubleshooting link labeled "Check folder permissions" is visible.
  And the application remains responsive and allows the instructor to change the configured input folder.

## Coverage Notes
- All scenarios use the MMSI value "123456789" which is referenced in the user manual examples; parameterize values using a Scenario Outline only when additional MMSI values from project mock data are available.
- Roles and permissions are explicit: the "instructor" role must have administrative rights for installation scenarios; application runtime and folder access require local system prerequisites.
- Each scenario specifies UI context (control names, settings path, file paths) and immediate visible outcomes (dialogs, banners, Diagnostics view entries) to enable deterministic automated tests.
