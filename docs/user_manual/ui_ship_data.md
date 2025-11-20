# ShipStaticData Panel

## Overview
Specification for the `ShipStaticData` panel which shows the full static data record for the selected ship.

## Control Details
- **Content**: Display all fields from the `ShipStaticData` record without exclusion.
- **Format**: Label-value pairs; numeric values formatted using appropriate units (e.g., metres for length/width, MB for folder size).
- **Editable**: Read-only by default (QA workflow) unless explicit edit feature is later requested.
- **Copy/Export**: Allow copying individual fields to clipboard; provide `Export JSON` action to save the record as a JSON file.
- **Min/Max dates**: Display as `Available data: YYYY-MM-DD — YYYY-MM-DD` prominently near the top of the panel.
