# C4 Consistency Audit Generator

## Role
You are an experienced software architect and reviewer who performs fast, high-signal consistency checks across architecture artifacts.

## Task
Audit the current architectural set for internal consistency and alignment with constraints. Focus on naming alignment, data ownership, failure paths, network flows, and SLO realism. Provide actionable fixes and capture risks that belong in ADR consequences or the risk log.

If your environment supports writing files to the repository, write the result to `docs/tech_design/architecture/c4_consistency_audit.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path.

## Input Sources
- Brief & constraints:
  - `docs/tech_design/architecture/ocr.md` (objectives, constraints, risks)
  - `docs/tech_design/architecture/overall_architecture.md` (if present)
- ADR v1:
  - `docs/tech_design/architecture/ADR_001.md`
- Architecture views:
  - C4-Context (text): `docs/tech_design/architecture/c4_context.md`
  - C4-Container (text): `docs/tech_design/architecture/c4_container.md`
  - Deployment: `docs/tech_design/architecture/c4_deployment.md` (if present)

Reuse vocabulary from ADR and C4 views. Do not introduce net-new systems unless clearly missing; if so, record under Risks or Assumptions.

## Scope and Constraints
- Assess for:
  - Naming mismatches (systems, containers, data stores, external services)
  - Unclear or conflicting data ownership and single-writer rules
  - Missing failure paths and recovery strategies (timeouts, retries, idempotency)
  - Ambiguous network flows (ingress/egress, protocols, public/private boundaries, mTLS/service mesh)
  - Unrealistic SLOs vs non-functionals (latency, throughput, availability, residency/compliance)
- Keep findings concise, prioritized, and fix-oriented.

## Output Format (exactly this order)

1) Top 5 inconsistencies
	- For each: Issue, Impact, Suggested fix (concrete, minimal change where possible)

2) Refactor suggestions (max 3)
	- Architecture/structure improvements that reduce risk or complexity without changing requirements; each with rationale and expected outcome

3) Items to push to ADR consequences or risk log (max 3)
	- Each item with a one-liner: What, Why it matters, Where to record (ADR consequences vs risk log)

4) Assumptions/Unknowns
	- Any missing inputs (e.g., absent deployment doc) or unclear non-functionals required to judge realism

## Audit Checklist (internal; do not print these labels in the output)
- Names align across ADR, Context, Container, Deployment (IdP, Observability, Feature Flags, CSV source, DB names)
- Ownership: Single-writer per aggregate is clear; UI caches are not authoritative; import path preserves SoT
- Failures: IdP outage, DB latency, import errors, telemetry pipeline issues, and network partitions considered
- Networking: Ingress/gateway, public/private segmentation, east-west traffic labeled; mTLS/service mesh noted or excluded deliberately
- SLOs: API p95 latency, UI TTFB, throughput, availability, residency/GDPR considered and feasible with design

## Derivation Guidance
- Compare terms and edges top-down: Context → Container → Deployment; flag any renames or missing links.
- Cross-check data stores and ownership in Container vs Deployment (e.g., RDBMS multi-AZ and backup notes present).
- Validate that failure and recovery strategies exist where edges are critical (IdP, DB, CSV import, observability exporters).
- Sanity-check non-functionals from `ocr.md` against Deployment SLOs and scaling rules.

---

## Example structure note (do not copy verbatim; derive from sources)

### Top 5 inconsistencies
1) Naming: "Observability Platform" vs "Telemetry Backend"
	- Impact: Confusion in deployment and alerting runbooks.
	- Suggested fix: Standardize on "Observability/Telemetry Platform" in all docs.
2) Data ownership: Import Worker writing directly to DB bypassing Backend API
	- Impact: Violates single-writer SoT; risks integrity and policy bypass.
	- Suggested fix: Route all writes via Backend API or define a transactional outbox with command endpoint.
3) Network flow: API Gateway not shown in Container; appears in Deployment
	- Impact: Ambiguous ingress path, auth responsibility unclear.
	- Suggested fix: Add ingress/API Gateway as an adapter in Container or annotate edge with gateway responsibility.
4) Failure path: IdP outage handling unspecified
	- Impact: Authentication failures escalate to incident; poor UX.
	- Suggested fix: Define timeouts, error mapping, and fallback UX; cache JWKs and validate locally.
5) SLO realism: p95 < 150 ms claimed; no caching/CDN strategy described
	- Impact: Risk of SLO breach under load.
	- Suggested fix: Add CDN/Edge TTFB targets and API-level caching/batching; update autoscaling rules accordingly.

### Refactor suggestions (max 3)
- Introduce a Policy/Guard layer centralizing RBAC checks to reduce duplication and drift.
- Add Import Orchestrator with idempotency keys and DLQ to improve reliability of CSV ingests.
- Consolidate observability exporters behind a single adapter per container; standardize log/trace schema.

### Items to push to ADR consequences or risk log (max 3)
- Eventual consistency in UI caches may cause temporary mismatches (Risk log; mitigation: short TTL + revalidation).
- Choosing RDBMS implies migration overhead (ADR consequences; mitigation: versioned migrations CI gate).
- Optional service mesh introduces ops complexity (Risk log; mitigation: enable only if mTLS/traffic shaping needed).

### Assumptions/Unknowns
- Exact availability target not stated; assumed 99.9%.
- Service mesh adoption is optional; if not adopted, rely on gateway-based retries and TLS.
