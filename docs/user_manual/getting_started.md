# Getting Started

## Overview
This section explains how to install, launch and access the AIS Routing application (AISRouting). It covers the initial setup, where to place AIS input files and how to open the application on the instructor workstation.

## Prerequisites
- Windows PC used by the K-Sim instructor
- Application installer or unpacked application files available locally
- "Vessel AIS data" input files placed in the expected folder structure: outputFolder/<MMSI>/<MMSI>-yyyy-MM-dd.csv
- "Vessel static data" input files placed in the expected folder structure: outputFolder/<MMSI>/<MMSI>.json
- Familiarity with opening local applications on Windows

## Step-by-Step Instructions
### Install the application
1. Run the provided installer or unzip the application package to a local folder (e.g., C:\Program Files\AISRouting or a chosen folder).
2. Verify that the application executable and supporting folders are present.
3. Create a desktop shortcut or pin to Start for quick launch.

### Launch the application
1. Double-click the desktop shortcut or start the app from the Start menu.
2. **Expected result**: The AIS Routing main window opens showing the application navigation and the ship list panel.
3. **Troubleshooting**: If the app does not start, ensure required runtime dependencies (if any) are installed and that the user has local permission to run executables.

### Prepare AIS files (quick check)
1. Verify the input folder structure: outputFolder/<MMSI>/ and files named <MMSI>-yyyy-MM-dd.csv.
2. Confirm files are UTF-8 encoded and use CRLF line endings on Windows.
3. If files are not found, check the configured input folder path in the application settings.

## Tips and Best Practices
- Use a consistent folder naming convention for MMSI directories so the application can locate data automatically.
- Keep working sets of AIS files dedicated to the current exercise to avoid confusion.
- Back up original input files before editing.

## Related Sections
- [Input Data and CSV Format](input_data.md)
- [Selecting a Ship and Time Range](selecting_ship_and_time_range.md)
- [Creating Routes and Output XML](creating_routes.md)
