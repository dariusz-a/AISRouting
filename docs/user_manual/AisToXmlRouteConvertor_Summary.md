# AisToXmlRouteConvertor User Manual Summary

## Application Overview
AisToXmlRouteConvertor converts pre-processed AIS source data (per-MMSI folders with `<MMSI>.json` and `YYYY-MM-DD.csv`) into a single XML route file following `route_waypoint_template.xml`. This tool is intended for internal QA engineers; simplicity and deterministic behavior are primary goals.

## Manual Sections
### [Application Overview](application_overview.md)
- Brief description of the application and core workflow.

### [Input Data Preparation](input_data_preparation.md)
- How to organize `input` and `output` folders and expected CSV schema.

### [Ship Selection](ui_ship_selection.md)
- Ship selection table columns and behavior.

### [ShipStaticData Panel](ui_ship_data.md)
- Full display of `ShipStaticData` record and export options.

### [Time Picker](ui_time_picker.md)
- Date/time pickers with seconds resolution; defaults to available data range.

### [Thresholds](ui_thresholds.md)
- Adjustable thresholds and the filtering algorithm used during optimization.

### [Process Button](ui_process_button.md)
- The primary `Process!` button behavior and message box feedback.

### [Output Specification](output_specification.md)
- Single XML file output naming and location.

### [Getting Started](getting_started.md)
- Quick start steps for QA engineers.

### [Troubleshooting](troubleshooting.md)
- Common errors and corrective steps.

### [Best Practices](best_practices.md)
- Tips to keep testing reliable and reproducible.

## Quick Reference
- Common tasks:
  - Select input folder: `Select input folder` (Windows folder dialog)
  - Select output folder: `Select output folder` (Windows folder dialog)
  - Choose ship: Select row in ship table
  - Set interval: `Start time` and `End time` (date/time with seconds)
  - Generate: Click `Process!`
