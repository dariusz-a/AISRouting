# Security Architecture: Authentication, authorization, data access controls, security testing

This document covers the security approach for AISRouting: identity assumptions, authorization boundaries, data validation, secure file handling, logging hygiene, and security testing. Cross?references overall_architecture.md and application_organization.md. Includes happy path and edge/error scenarios with realistic examples (Alice, Bob).

## 1. Scope & Assumptions
AISRouting is a local Windows desktop application processing AIS source data from local or network file shares. No multi-user concurrent server; thus classic web auth flows are out-of-scope for MVP. Security focus centers on:
- Safe file system access (path validation, permission checks).
- Data integrity validation (MMSI format, lat/lon ranges).
- Minimizing sensitive leak through logs.
- Optional future integration with external APIs (if added) requiring auth tokens.

## 2. Authentication Strategy (Current)
- User authentication delegated to Windows account running the process.
- Optional enhancement: Windows Principal capture for audit logs.
- Future extension: Add Azure AD device token retrieval if remote APIs integrated.

## 3. Authorization Model
Because all operations occur locally, authorization is coarse:
- Read Input Folder: Allowed if user has read permissions; verify via Directory.Exists + ACL checks.
- Write Output Folder: Allowed if user can create directory & write file; attempt pre-flight create of temp file.
- No role-based restriction in MVP (single persona). Future: RBAC around threshold tuning or export operations.

## 4. Data Access Controls
- Path Normalization: Resolve full path with Path.GetFullPath and ensure it starts with selected root to prevent traversal (e.g., ../../../). Reject if outside.
- MMSI Validation: 9-digit numeric (Regex ^\d{9}$); skip folder if mismatched.
- File Enumeration: Use search pattern *.csv; ignore hidden/system files.
- CSV Row Validation: Latitude [-90,90], Longitude [-180,180]; skip invalid rows.
- JSON Parsing: Reject embedded scripts (not expected); treat unexpected properties as benign.

## 5. Input Validation Matrix
| Input | Validation | Failure Handling |
|-------|------------|------------------|
| Input folder path | Directory.Exists + accessibility | Error message + disable scan |
| Output folder path | Attempt create if missing | Display error, abort export |
| MMSI folder name | Regex numeric | Skip folder; log warning |
| CSV filename | Date parse (YYYY-MM-DD) | Skip file; log warning |
| Time interval | Start < Stop within vessel bounds | Show validation message; disable Create Track |

## 6. Secure Logging Practices
- Do not log full file contents or large CSV rows.
- Log counts (e.g., 12 malformed rows skipped in 2024-01-01.csv).
- Include context: MMSI, filename, row index for diagnostics.
- Avoid PII; vessel names and MMSI considered non-sensitive maritime identifiers.
- Sanitize paths before logging (replace user home if needed).

## 7. Error Handling & User Feedback
Happy Path: Alice selects accessible input folder, generates route, exports successfully.
Edge Cases:
- Bob selects output folder on read-only share ? ExportException; UI shows “Cannot write to selected output folder.”
- Malformed JSON (truncated) ? Warning logged, partial static data displayed.
- Excessive malformed rows (>10%) ? After parsing, show advisory message: “High number of invalid AIS rows; data quality may degrade optimization.”

## 8. Future External API Security (Placeholder)
If integrating external AIS enrichment APIs:
- Token storage: Use Windows DPAPI ProtectedData for caching tokens locally.
- HTTPS enforced; reject plain HTTP endpoints.
- Rate limiting handshake logged.
- API error mapping: 401 triggers token refresh, 429 instructs user to retry later.

## 9. Compliance Considerations
- Maritime AIS data often public but treat local datasets with confidentiality respect.
- Provide optional purge function for temporary processing caches (future).
- Ensure exported XML does not embed user environment paths (only route data).

## 10. Security Testing Approach
Unit Tests:
- MMSI validation rejects non-numeric.
- Path traversal attempt (..\..\other) detection.
- CSV validation skipping invalid lat/lon.
Integration Tests:
- Read-only output path simulation (use ACL or mock) triggers failure gracefully.
- Large malformed row percentage triggers advisory message.
Manual / Exploratory:
- Attempt cancellation mid-load ensures no partial insecure writes.

## 11. Threat Model Highlights
Assets: AIS source data, optimized routes.
Threats:
- Data corruption (malformed CSV) ? Mitigated by robust parsing and skip strategy.
- Path traversal (crafted folder names) ? Path normalization + root containment.
- Unauthorized data export to restricted location ? Windows ACL denies; app surfaces error.
Residual Risks:
- User intentionally modifies CSV to mislead optimizer; mitigated by statistical anomalies logging.

## 12. Secure Development Practices
- Prefer safe parsing (TryParse) vs direct conversions.
- Avoid dynamic code execution entirely.
- Keep dependencies updated (CsvHelper, MVVM Toolkit). Monitor CVEs.
- Strict null checks around essential fields (lat/lon).

## 13. Cross-References
- overall_architecture.md (Iterations & logging)
- application_organization.md (Project/service boundaries)
- data_models.md (Validation rules)
- api_integration_patterns.md (XML export constraints)

## 14. Future Enhancements
- RBAC for batch processing / threshold adjustments.
- Audit log (append-only) of exports with timestamp + MMSI.
- Integrity hash of exported XML (SHA256) for tamper detection.
