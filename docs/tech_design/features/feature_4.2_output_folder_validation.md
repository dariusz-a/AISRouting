```markdown
# Feature 4.2: Output Folder Validation

## Summary

Ensure the selected output folder is writable, explicitly validated, and prevents the conversion workflow when invalid. This feature complements `Output Folder Control` by detailing validation logic, failure modes, and acceptance tests.

## Goals

- Provide deterministic validation for writability using a safe write test.
- Surface clear, actionable error messages for common failure causes.
- Prevent enabling processing or exporting when the output folder is not writable.
- Provide unit and integration tests for validation logic and persistence interactions.

## Dependencies

- Feature 4.1: Output Folder Control (UI + persistence)
- Iteration 2: File parsing (for end-to-end fail conditions)
- System: .NET file I/O APIs and cross-platform considerations

## User Stories

- As a user, I see an explicit validation result after choosing an output folder.
- As a user, the process button stays disabled if my chosen folder cannot be written to.
- As a user, I get guidance on how to fix common permission or disk issues.

## Behaviour & Validation Strategy

- Validation is triggered when:
  - The user selects a path via the folder picker.
  - The application restores a last-used path on startup.
  - The path is edited by advanced users (if editing is allowed).

- Validation steps (synchronous for local paths; background task for remote/slow):
  1. If the path does not exist, report `Path does not exist` and offer to create it.
  2. If path exists, attempt to create and delete a small temporary file in the folder to confirm write permission.
  3. Handle exceptions distinctly: `UnauthorizedAccessException` => `Permission denied`; `IOException` => `Disk error or network share unavailable`.
  4. Consider available free space check for large exports as an optional enhancement.

- On success: set `IsWritable=true`, clear validation messages, persist the path, and allow processing when other prerequisites met.
- On failure: set `IsWritable=false`, set `ValidationMessage` to a short actionable message, and keep process disabled.

## Failure Modes and UX

- Permission denied: show steps to run the app with necessary permissions or choose another folder.
- Read-only media (e.g., CD, mounted ISO): explain the device is read-only.
- Network share unreachable: suggest retrying or selecting a local folder.
- Path too long / invalid characters: show an explanatory message and suggested sanitized name.

## BDD Scenarios (Gherkin)

Scenario: Select writable output folder displays chosen path
  Given the application is running
  When the user selects a folder with write permission
  Then the output path is displayed
  And the validation indicator shows "Valid"
  And the process button may be enabled if other prerequisites are satisfied

Scenario: Non-writable output folder shows error
  Given the application is running
  When the user selects a folder without write permission
  Then the validation indicator shows "Not writable"
  And an inline error message explains the failure
  And the process button remains disabled

Scenario: Output folder not writable prevents enablement
  Given the user selected an unwritable folder
  When the user attempts to start the conversion
  Then the conversion is blocked and an error is shown

Scenario: Missing path is suggested to be created
  Given the user selects a non-existing path
  When validation runs
  Then the UI offers to create the folder or shows instructions to create it

## Acceptance Criteria

- Validation must use an actual write test (create & delete temp file) for local folders.
- Exceptions are mapped to user-friendly messages.
- Non-writable folders prevent process enablement and conversion.
- Tests cover success, permission denied, IO error, and missing-path creation suggestion.

## Testing Notes

- Unit tests should mock the file system layer to simulate exceptions without touching disk.
- Integration tests should exercise the create/delete temp file on a temporary directory.
- Add test cases under `tests/output_folder_validation.spec.ts`.

## Files / Code To Add

- Validation helper: `OutputFolderValidator` (static helper or service) with `ValidateAsync(string path)` returning a structured result `{isWritable, message, code}`.
- ViewModel integration: call validator and update `IsWritable` and `ValidationMessage`.
- Tests: `tests/output_folder_validation.spec.ts` (unit + integration stubs).

## Implementation Tasks

1. Add `OutputFolderValidator` service.
2. Wire validator into `OutputFolderViewModel` and call on folder selection and app start.
3. Add localized messages for the common error codes.
4. Write unit tests that mock file system exceptions.
5. Add an integration test that verifies create/delete temp file on a temp folder.

## Notes

- Keep validation lightweight; do not block UI for long network responses — use background task with cancellation.
- Persist only when validation succeeds, or persist a candidate with a flag indicating last-known validation status.

``` 
