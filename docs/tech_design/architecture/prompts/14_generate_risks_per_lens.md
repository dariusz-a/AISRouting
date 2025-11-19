# Risks per Lens & Mitigation Table 

## Role 

You are my architecture risk analyst.

## Task
Analyze the design through the four lenses below:

1. Failure Modes — outages, retries, partial failures.

2. Performance/Scalability — latency, load, concurrency, capacity.

3. Evolution/Change — schema or interface drift, versioning, deployment.

4. Policy/Compliance/Security — privacy, residency, authZ/N, auditability.

If your environment supports writing files to the repository, write the result to `docs/tech_design/architecture/risks_mitigation.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path.

## Inputs

- OCR sheet (Objectives, Constraints, Risks): `docs/tech_design/architecture/ocr.md`
- ADR v1 (context + decision + consequences): `docs/tech_design/architecture/ADR_001.md`
- DesignDoc v0.9 (to update Section 6): `docs/tech_design/architecture/design_doc_v1_0.md`
- Deployment topology (Section 5 source): `docs/tech_design/architecture/c4_deployment.md` (if present)
- C4 Container and Context (for vocabulary alignment):
   - `docs/tech_design/architecture/c4_container.md`
   - `docs/tech_design/architecture/c4_context.md`

## Behavior
- Parse the referenced files to extract objectives, constraints, risks, deployment/scaling, and service vocabulary.
- Reuse exact terminology from ADR/C4/DesignDoc (IdP/OIDC, Observability/Telemetry Platform, Backend API, Shell, MFEs, Primary Database, etc.).
- Produce exactly 12 rows: 3 per lens in the order Failure, Performance, Evolution, Policy. Do not include any extra prose outside the table.
- Each row must have concrete, testable entries:
   - Impact/Likelihood: High | Med | Low
   - Mitigation: specific mechanism or pattern (e.g., circuit breaker, idempotency keys, PDB/HPA)
   - Verification: chaos or test scenario (e.g., inject 500s, kill pod, exceed queue depth)
   - Metric/SLO: numeric or named metric (e.g., p95 latency < 300 ms, error_rate < 1%)
   - Owner: use roles from docs if available (e.g., Platform/SRE, Backend, Frontend). If unknown, write "Owner TBD".
- Write only the Markdown table to `docs/tech_design/architecture/risks_mitigation.md`. If writing is not possible, return the table and state the intended path.

## Output

For each lens, list the top 3 specific risks and complete the table below.
Always fill every column. Keep descriptions short and concrete.
Output as a single Markdown table in this exact format:

| Lens | Risk | Impact | Likelihood | Mitigation (Design) | Verification (Test) | Metric/SLO | Owner |
|------|------|---------|-------------|---------------------|---------------------|-------------|--------|
| Failure | [describe risk] | High/Med/Low | High/Med/Low | [mechanism or pattern] | [test or chaos check] | [numeric or qualitative metric] | [team/role] |
| Performance | ... | ... | ... | ... | ... | ... | ... |
| Evolution | ... | ... | ... | ... | ... | ... | ... |
| Policy | ... | ... | ... | ... | ... | ... | ... |

## Acceptance Checklist (internal; do not print these labels in the output)
- Exactly 12 rows: 3 per lens (Failure, Performance, Evolution, Policy)
- All columns filled with concrete, testable entries; no placeholders like "TBD" except Owner if unknown
- Vocabulary aligns with ADR/C4/DesignDoc; no net-new systems introduced
- Output is a single Markdown table with a header row and 12 data rows; no extra text
- File written to `docs/tech_design/architecture/risks_mitigation.md` if possible

## Derivation Guidance
- Failure modes: IdP outage/latency, DB failover/replication lag, import job retries/DLQ, telemetry backend unavailability.
- Performance/scalability: API p95/p99 latency targets vs HPA rules, CDN cache hit ratios, import backpressure/queue depth.
- Evolution/change: Contract drift between MFEs/SDK/API, schema migration risks, version pinning/semver and rollout.
- Policy/compliance/security: GDPR residency, RBAC/policy enforcement points, audit logs completeness, PII retention.
