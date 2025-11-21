# Getting Started

## Overview
This section explains how to install and start AISRouting, access the desktop UI, and perform initial setup required before creating and exporting routes. AISRouting is a desktop application that helps you analyze vessel AIS (Automatic Identification System) data, create optimized tracks, and export routes in industry-standard formats.

## Prerequisites
- Windows operating system (Windows 10 or later recommended)
- .NET 8.0 Runtime (Desktop) installed
- Access to AIS CSV input folders containing vessel subfolders with CSV files
- Minimum 4GB RAM recommended
- At least 500MB free disk space for the application and temporary files

## Step-by-Step Instructions

### Install and Launch AISRouting

1. **Obtain the Application**
   - Unpack the AISRouting distribution ZIP file to a working directory of your choice
   - Alternatively, clone the AISRouting repository if you have source access
   - Recommended install location: `C:\Program Files\AISRouting` or `C:\Users\[YourUsername]\AISRouting`

2. **Verify Installation**
   - Navigate to the installation directory
   - Ensure the main executable file `AISRouting.App.WPF.exe` is present
   - Verify that required DLL files are in the same directory

3. **Start the Application**
   - Double-click `AISRouting.App.WPF.exe` to launch the desktop application
   - **Expected Result**: The main application window opens, displaying the Input Configuration section at the top
   - The main screen includes:
     - Input Folder selector with Browse button
     - Vessel Selection area (initially disabled)
     - Time Interval Selection controls (initially disabled)
     - Track Results area (initially empty)

4. **Troubleshooting Launch Issues**
   - If the application fails to start, verify:
     - The executable file is not corrupted (check file size is > 0 bytes)
     - .NET 8.0 Desktop Runtime is installed (download from microsoft.com/dotnet)
     - Windows Defender or antivirus is not blocking the application
     - You have read/execute permissions for the installation directory

### Understanding Input Data Structure

Before selecting your input folder, understand the required folder structure:
Input Root Folder (e.g., C:\data\ais_root)
??? 205196000\               (Vessel MMSI folder)
?   ??? 205196000.json       (Static vessel data)
?   ??? 2024-01-01.csv       (Daily position data)
?   ??? 2024-01-02.csv
?   ??? ...
??? 205196001\               (Another vessel)
?   ??? 205196001.json
?   ??? 2024-01-01.csv
?   ??? ...
??? ...
**Folder Requirements:**
- Root folder contains subfolders, each named with a vessel's MMSI number
- Each vessel subfolder contains:
  - A JSON file with static vessel data (name, dimensions, etc.)
  - Multiple CSV files with daily position records (one file per day)
- CSV files are named using the date format: `YYYY-MM-DD.csv`

### Select Input Data Root

1. **Open the Input Folder Selector**
   - In the main window, locate the "Input Folder" section at the top
   - Click the **[Browse]** button next to the Input Folder text field

2. **Choose Your Data Root**
   - A folder browser dialog will open
   - Navigate to your AIS data root folder (e.g., `C:\data\ais_root`)
   - Select the folder and click **OK**
   - **Expected Result**: 
     - The selected path appears in the Input Folder text field
     - The "Select Vessel" combo box becomes enabled
     - Vessel subfolders are automatically discovered and listed in the combo box

3. **Verify Vessel Discovery**
   - The vessel combo box should now display available vessels
   - Format: `[Vessel Name] (MMSI)`
   - Example: `Sea Explorer (205196000)`
   - If vessels are found, the first vessel is automatically selected

4. **View Static Vessel Data**
   - Once a vessel is selected, the "Ship Static Data" panel displays:
     - MMSI: Maritime Mobile Service Identity number
     - Name: Vessel name (if available in JSON)
     - Length: Vessel length in meters
     - Beam: Vessel width in meters
     - Draught: Vessel draft in meters
     - Available Date Range: First and last dates with CSV data
   - **Note**: Some fields may show "N/A" if data is missing or null in the JSON file

### Handle Common Setup Issues

**No Vessels Found**
- **Symptom**: The ship selection combo box is empty after selecting a folder
- **Cause**: The selected folder contains no vessel subfolders
- **Solution**: 
  - Verify you selected the correct root folder (not a vessel subfolder)
  - Ensure vessel subfolders exist and are named with MMSI numbers
  - Check that each vessel folder contains at least one .json file
  - An inline warning "No vessels found in input root" will be displayed

**Missing Static Data**
- **Symptom**: Static data panel shows "N/A" for many fields
- **Cause**: The vessel's JSON file is missing, malformed, or contains null values
- **Solution**:
  - Verify the JSON file exists (e.g., `205196000\205196000.json`)
  - Open the JSON file in a text editor and verify it's valid JSON
  - Ensure required fields (MMSI, name, length, beam, draught) contain values
  - The application will continue to work with partial data

**Empty Date Range**
- **Symptom**: Available Date Range shows "N/A"
- **Cause**: No valid CSV files found in the vessel folder
- **Solution**:
  - Verify CSV files exist in the vessel folder
  - Ensure CSV files are named correctly: `YYYY-MM-DD.csv`
  - Check that CSV files contain valid position data
  - You cannot create tracks without valid date ranges

**Application Won't Start**
- **Symptom**: Error dialog "Application failed to start: executable missing or corrupted"
- **Cause**: Installation is incomplete or files are damaged
- **Solution**:
  - Re-extract the distribution ZIP file
  - Verify all DLL files are present in the installation directory
  - Check Windows Event Viewer for detailed error information
  - Try running as Administrator (right-click exe > Run as Administrator)

## Tips and Best Practices

### Data Organization
- **Consistent Structure**: Maintain a consistent folder structure for all vessels
- **Regular Backups**: Keep backups of your AIS data in a separate location
- **Data Validation**: Before importing, verify CSV files have correct date formats in filenames
- **Vessel Naming**: Use descriptive names in JSON files to easily identify vessels

### Performance Optimization
- **Local Storage**: Store input data on a local drive (not network drives) for better performance
- **SSD Recommended**: Use solid-state drives for faster data loading
- **File Size**: Very large CSV files (>100MB) may take longer to load; consider splitting by date
- **Memory**: Close other applications if processing large datasets (multiple months of data)

### Security and Access
- **Read Permissions**: Ensure the application has read access to the input folder
- **Write Permissions**: Export operations require write access to the output folder
- **Network Paths**: UNC paths (\\server\share) are supported but may be slower
- **Temporary Files**: The application creates temporary files during processing; ensure adequate disk space

### First-Time Setup Checklist
1. ? Verify .NET 8.0 Desktop Runtime is installed
2. ? Extract application to a permanent location (not in Downloads or Temp)
3. ? Prepare your AIS data in the correct folder structure
4. ? Test with a small dataset first (1-2 vessels, 1 week of data)
5. ? Verify vessel static data appears correctly
6. ? Check that date ranges match your CSV files

## What's Next?

After successfully completing the getting started steps, you can:

1. **Select Specific Time Intervals**: Choose date and time ranges for analysis (see [Create Track](create_track.md))
2. **Create Optimized Tracks**: Generate waypoint sequences from position data (see [Create Track](create_track.md))
3. **Export Routes**: Save tracks in XML format for use in other systems (see [Exporting Routes](export_route.md))

## Related Sections
- [Ship Selection and Static Data](ship_selection.md) - Detailed information about vessel data management
- [Create Track](create_track.md) - Learn how to generate optimized tracks from AIS data
- [Exporting Routes](export_route.md) - Export tracks in industry-standard XML format
- [Troubleshooting](troubleshooting.md) - Solutions to common problems and error messages

## Technical Notes

### Supported File Formats
- **Static Data**: JSON format with flexible property names (case-insensitive)
- **Position Data**: CSV format with specific column ordering
- **Export Format**: RTZ (Route Plan Exchange Format) XML

### Data Validation
The application performs automatic validation of:
- JSON structure and required fields
- CSV file naming conventions
- Date/time formats in filenames
- Coordinate validity (latitude: -90 to 90, longitude: -180 to 180)
- MMSI format (9-digit number)

### Embedded Resources
AISRouting includes:
- XML template structure for consistent exports
- Validation schemas for route data
- Default configuration settings

For advanced configuration options and troubleshooting, see the [Troubleshooting](troubleshooting.md) section.
