# Troubleshooting

## Overview
Common issues when preparing, creating, and exporting routes and their resolutions.

## Prerequisites
- Review Getting Started and Ship Selection sections

## Problems and Solutions
### No CSV files detected
1. Verify the input root folder and that vessel subfolders contain CSV files.
2. Ensure filenames or contents include numeric epoch timestamps in the first column.

### Export fails due to permission or path
1. Verify the selected output path is writable.
2. If the application cannot create the folder, choose a different folder or run with sufficient permissions.
3. Error message will indicate the underlying filesystem problem.

### Missing Heading or SOG values
- If Heading or SOG fields are missing in rows, WayPoint fields default to 0. Consider preprocessing CSV to fill missing values.

## Tips and Best Practices
- Keep a backup of generated exports to avoid data loss when overwriting files.
- If data quality is poor, run track creation on narrow ranges and inspect intermediate data.

## Related Sections
- [Exporting Routes](export_route.md)
