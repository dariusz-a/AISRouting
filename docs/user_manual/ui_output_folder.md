# Output Folder Control

## Overview
Specification for the `Select output folder` control used to choose where generated XML files will be saved.

## Control Details
- **Label**: `Select output folder`
- **Type**: Windows folder picker dialog
- **Default**: Last-used path or blank on first run
- **Validation**: Ensure folder is writable; if not, show an error and prompt to choose another folder.
- **Behavior after selection**:
  - Display selected path in the UI.
  - Use this path as destination for generated XML files.
- **Error messages**: `Selected folder is not writable. Choose a different folder.`
- **Persistence**: Remember last-used path between runs.
- **Tooltip/help**: `Select the folder where generated XML route files will be saved.`
- **Drag-and-drop**: Not required; keep UI simple.
