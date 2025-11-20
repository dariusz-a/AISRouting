# Process Button

## Overview
Specification for the primary action button that triggers track generation.

## Control Details
- **Label**: `Process!`
- **Type**: Primary action button (large)
- **Behavior**:
  - No progress bar or live status updates (keep UI simple)
  - Button disabled until valid `Input folder`, `Output folder`, and ship/time-interval are selected
  - On press, perform processing synchronously and then show a blocking message box with success or failure message
- **Success message**: Example `Track generated successfully: <filename>.xml`
- **Failure message**: Example `Processing failed: <error details>`
- **Post-action behavior**: Do not auto-open the output folder. Only show the single generated filename in the message box.
