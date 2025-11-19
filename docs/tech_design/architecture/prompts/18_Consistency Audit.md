# Consistency Audit

## Role
You are my architecture documentation auditor.

## Inputs
- ADR v1 (target to update): `docs/tech_design/architecture/ADR_001.md`
- DesignDoc v0.9: `docs/tech_design/architecture/design_doc_v1_0.md`
- Trade-off matrix: `docs/tech_design/architecture/alt_arch_matrix.md`
- Risk & Mitigation Table: `docs/tech_design/architecture/risks_mitigation.md` (if present)
- Risk → Mitigation → SLO links: `docs/tech_design/architecture/risk_mitigation_slo_links.md` (if present)

If your environment supports writing files to the repository, write the audit result to `docs/tech_design/architecture/adr_consistency_audit.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path.

## Task

Audit for inconsistencies among: requirements/constraints ↔ ADR ↔ C4/deployment/sequence ↔ risks ↔ SLOs.

1. List specific mismatches (cite section NAMES).
2. Propose minimal diffs to fix them (one-liners).
3. Flag any missing non-functional coverage (security, privacy, compliance, resilience).

 ## Output

Output a Marckdown table: 
Issue | Evidence | Proposed Fix | Impact if unfixed.

## Behavior
- Read the inputs and compare terminology and numbers across:
	- ADR v1 sections (Context, Decision, Consequences, Alternatives)
	- DesignDoc ection NAMES
	- Trade-off matrix: confirm Decision aligns with cited winning criteria
	- Risks/SLO links: confirm Consequences reference risks and SLOs
- Identify and prioritize inconsistencies in this order: (1) numeric/SLO conflicts, (2) data ownership and single-writer violations, (3) naming/anchor mismatches, (4) missing failure paths, (5) ambiguous network/ingress boundaries, (6) gaps in non-functionals (security/privacy/compliance/resilience).
- For each issue, cite evidence precisely with file and anchor (e.g., `design_doc_v1_0.md#slo-table`, `ADR_001.md § Consequences`).
- Propose minimal, surgical one-line diffs (e.g., “ADR Consequences: change 'p95 ≤ 150 ms' → 'p95 ≤ 250 ms' to match SLO table”). Do not output full patches.
- Produce up to 10 issues total. If more, include the top 10 by impact.
- Write only the Markdown table to `docs/tech_design/architecture/adr_consistency_audit.md`. No extra prose.

## Acceptance Checklist (internal; do not print these labels in the output)
- Table includes 1–10 rows, prioritized by impact; columns exactly: Issue | Evidence | Proposed Fix | Impact if unfixed
- Evidence cells reference concrete files/sections
- Proposed fixes are one-liners, minimal diffs; no broad rewrites
- Includes at least one row for missing non-functional coverage if any gaps are found
- Output file path `docs/tech_design/architecture/adr_consistency_audit.md` used when writing is possible

## Derivation Guidance
- Numeric conflicts: compare latency/availability targets in ADR vs `design_doc_v1_0.md#slo-table`.
- Ownership conflicts: enforce Backend API as SoT per ADR/Container; ensure Import/ETL Worker writes via API, not directly.
- Naming alignment: IdP/OIDC, Observability/Telemetry Platform, Feature Flag Service, Primary Database—ensure identical naming across ADR and C4 sections.
- Networking: ensure API Gateway/Ingress presence in Deployment is reflected in Container or called out as ingress.
- Risks/SLO linkage: Consequences should mention risks in `#risk-table` and SLOs in `#slo-table`; add “Evidence Links” if absent.