# Time Picker Control

## Overview
Specification for the Time Picker used to select the processing interval for track generation.

## Control Details
- **Type**: Date+time picker with seconds resolution (date selector + time fields including seconds)
- **Labels**:
  - `Start time`
  - `End time`
- **Default values**:
  - `Start time` = ship's available minimum date/time
  - `End time` = ship's available maximum date/time
- **Validation**:
  - `Start time < End time` required
  - Both times must lie within the ship's available [min, max] interval
  - Show clear validation messages inline (e.g., `Start time must be before End time`, `Selected time is outside available data range`)
- **Typing**: Allow typing exact seconds and provide incremental controls (up/down) for time fields
- **Behavior**: Changing the times updates the preview and enables/disables the `Create Track` button based on validation
