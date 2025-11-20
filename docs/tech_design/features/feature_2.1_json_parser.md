```markdown
# Feature 2.1: JSON Parser

This document describes the design for the JSON Parser responsible for reading `<MMSI>.json` files that provide `ShipStaticData` used throughout the application.

Design goals
- Robust, small, and testable parser functions.
- Fail-soft on malformed input: return `null` (caller-friendly) and log an error instead of throwing where recoverable.
- Preserve source path information for audit and debugging.
- Keep parsing sync and deterministic; introduce async/file I/O separation at call sites.

Responsibilities
- Read and parse a single JSON file into a `ShipStaticData` record.
- Validate required fields (`mmsi`) and perform lightweight normalization (trim strings, coerce numeric fields when safe).
- Return a structured error or `null` for files that cannot be parsed or do not meet minimum validity.

Input and expected file layout
- File name pattern: `<MMSI>.json` (e.g., `205196000.json`).
- JSON object with optional and required properties. Minimal acceptable payload must include `mmsi` (string or numeric-looking value).

Recommended JSON schema (informal)
```json
{
  "mmsi": "205196000",
  "name": "Vessel Example",
  "shipType": "Cargo",
  "lengthMeters": 120.5,
  "beamMeters": 18.2,
  "callsign": "ABC123",
  "imo": "IMO1234567"
}
```

Parsing behaviour and rules
- Read file content as UTF-8 text.
- Use a tolerant JSON deserializer that disallows trailing commas by default but accepts numbers-as-strings for common numeric fields.
- Trim string fields and treat empty strings as `null` for optional fields.
- Convert numeric-looking strings to numbers for `lengthMeters` and `beamMeters` when safe (finite, non-negative).
- Validate `mmsi` presence: accept numeric string or number; store as string in canonical form (no leading/trailing whitespace).
- If top-level is not an object, treat as malformed and return `null`.

Error handling policy
- Malformed JSON or non-object root: log error and return `null`.
- Missing required field (`mmsi`): log warning and return `null`.
- Invalid numeric ranges: set invalid numeric optional fields to `null` and log debug-level note (do not fail parsing unless `mmsi` is missing).
- Parsing exceptions (I/O, encoding): bubble up file-system errors to caller if they represent unexpected I/O failure; however, JSON format errors should be caught and normalized to `null` return.

Return contract
- On success: return a `ShipStaticData` object matching the DTO in `feature_1.1_data_models.md` and include `sourcePath` field set to the absolute path of the file parsed.
- On recoverable problems (malformed JSON / missing required fields): return `null`.
- On unrecoverable I/O errors (permission denied, file unreadable): let the caller handle the exception so it can decide retry/abort.

Mapping to internal model
- Always map `mmsi` to a string. Example: `205196000` or `"205196000"` => `"205196000"`.
- Optional fields not present or empty => `null`.

Examples
- Valid file => returns object:
```
{
  "mmsi": "205196000",
  "name": "Vessel Example",
  "shipType": "Cargo",
  "lengthMeters": 120.5,
  "beamMeters": 18.2,
  "callsign": "ABC123",
  "imo": "IMO1234567",
  "sourcePath": "C:\\input\\205196000.json"
}
```

- Malformed JSON => return `null` and log: `Malformed JSON: <path>`
- Missing `mmsi` => return `null` and log: `Missing required mmsi in: <path>`

Test notes (BDD + unit)
- Unit tests should cover:
  - Parsing a valid JSON file returns correct `ShipStaticData` with `sourcePath` set.
  - Missing optional fields are accepted and set to `null`.
  - Malformed JSON returns `null`.
  - Non-object JSON roots (arrays, numbers) return `null`.
  - Numeric strings are converted when safe.
  - I/O errors propagate (simulate by throwing from file read).

- BDD scenarios (from implementation plan):
  - Parse valid ship static JSON file
  - Handle missing optional fields in JSON
  - Return null for malformed JSON file

Implementation guidance (language-specific)
- .NET / C# (.NET 9 recommended):
  - Use `System.Text.Json` with `JsonSerializerOptions` set to `PropertyNameCaseInsensitive = true`.
  - Read file content with `File.ReadAllText(path)`; handle `IOException` separately from `JsonException`.
  - Implement a small mapper that validates fields and returns either `ShipStaticData` or `null`.

- TypeScript / Node.js:
  - Use `fs.readFileSync(path, 'utf8')` for synchronous tests; in production use async `fs.promises.readFile`.
  - Use `JSON.parse` wrapped in try/catch; prefer `zod` or `ajv` for schema validation in complex cases.

Performance and concurrency
- Files parsed individually per MMSI; parsing is cheap and I/O bound. For large input sets, parallelize file reads with a bounded concurrency limit to avoid filesystem saturation.

Logging and observability
- Log at these levels:
  - ERROR: unexpected I/O failures when reading the file.
  - WARN: missing required `mmsi` or non-object root.
  - DEBUG: optional field coercions or numeric conversion failures.

Open questions
- Should we accept JSON schemas with nested `static` wrappers (e.g., `{ "static": { ... } }`)? Recommendation: no — keep parser simple; add an adapter layer if required by real inputs.

References
- `docs/tech_design/features/feature_1.1_data_models.md` for `ShipStaticData` DTO.

---
```
