# Getting Started

## Overview
This section explains how to install and start AISRouting, access the UI, and perform initial setup required before creating and exporting routes.

## Prerequisites
- A machine with Python 3.x for CLI utilities (optional). 
- Access to AIS CSV input folders containing vessel subfolders and CSV files (folder structure documented below).
- route_waypoint_template.xml available in the application directory for export metadata.

## Step-by-Step Instructions
### Install / Run
1. Unpack or clone the AISRouting distribution to a working directory.
2. Ensure the `route_waypoint_template.xml` file is placed in the application root (used for export metadata).
3. Start the desktop UI or run CLI tools from the repository as documented.

### Select Input Data Root
1. In the application, open the "Input Folder" selector.
2. Choose the root folder that contains vessel subfolders. The UI will list subfolders in a Combo box.
3. **Expected result**: vessel subfolders are displayed in the Combo box.

## Tips and Best Practices
- Keep CSV files organized by vessel in separate subfolders to ensure proper ship selection.
- Keep a copy of the `route_waypoint_template.xml` file available for consistent exports.

## Related Sections
- [Ship Selection and Static Data](ship_selection.md)
- [Exporting Routes](export_route.md)
