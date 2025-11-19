# C4 Consistency Audit

This audit reviews the current architecture artifacts for internal consistency and alignment with constraints from OCR, ADR-001, and the C4 views (Context, Container, Deployment).

## 1) Top 5 inconsistencies

1) Current vs target posture is mixed across docs (client-only vs API-backed)
   - Impact: Delivery scope and SLOs are interpreted differently by readers and tests; creates confusion about what must exist now vs later.
   - Suggested fix: Introduce explicit state labels and a small "Now vs Target" table referenced by all docs.
     - Add a one-pager (docs/tech_design/architecture/state_posture.md) that defines:
       - Prototype (Now): client-only Manage Roles, static hosting, availability ≥ 99.9% (OCR), no backend dependency.
       - Target Platform: Backend API as SoT, MFEs, availability ≥ 99.95% (ADR), CSV import via API.
     - In each C4 view and ADR, add a one-line banner: "This view models the Target Platform (future)."

2) Numeric SLOs conflict across OCR, ADR, and Deployment
   - Evidence: OCR UI actions p95 ≤ 200 ms; ADR CRUD p95 ≤ 250 ms; Deployment API p95 ≤ 300 ms; Availability 99.9% (OCR) vs 99.95% (ADR).
   - Impact: Inconsistent alerting thresholds and misaligned performance budgets; risk of oscillating tuning and false positives.
   - Suggested fix: Standardize SLOs with precise scoping and inherit them everywhere.
     - Adopt the following single source:
       - Shell/MFEs: FCP ≤ 1.8 s p95; route change ≤ 250 ms p95; UI action responsiveness (client-only paths) ≤ 200 ms p95.
       - Backend API: CRUD endpoints ≤ 250 ms p95 (steady state); import control-plane ≤ 400 ms p95.
       - Availability: Prototype (static) ≥ 99.9%; Target Platform (CDN+HA API) ≥ 99.95%.
     - Update ADR (Consequences) and Deployment to match these numbers; keep OCR as prototype-specific but link to the registry.

3) Ingress/Gateway is present in Deployment but absent in Container view
   - Impact: Ambiguous ingress path and unclear placement of cross-cutting concerns (TLS termination, rate limits, JWT passthrough, timeouts, retries, 429 policy).
   - Suggested fix: Add an "API Gateway/Ingress" adapter to the Container view or annotate the Shell→Backend API edge with responsibilities.
     - Responsibilities: TLS termination, request/response size limits, per-route timeouts, retry policy for idempotent GET, 429 + Retry-After, AV scan for CSV (optional).
     - Note that OIDC token validation remains in the Backend API; gateway passes Authorization header through.

4) Import/ETL Worker write path is missing from the Container diagram
   - Evidence: Text states the Worker writes via Backend API; the Container PlantUML lacks Rel(worker, api), while Deployment shows Rel(worker, api).
   - Impact: Suggests possible direct DB writes; undermines the single-writer rule and policy enforcement clarity.
   - Suggested fix: Add Rel(worker, api, "Import commands", "HTTP REST/JSON") to the Container PlantUML and add a note "Worker never writes to DB directly".

5) Resilience policies (timeouts, retries, idempotency) are stated as needs but lack concrete thresholds
   - Impact: Hard to implement reliable behaviors; risk of thundering herds, duplicate effects on retry, and poor UX during partial failures.
   - Suggested fix: Publish a short "Resilience Profile" referenced by the SDK, API, and Gateway.
     - UI→API via SDK: default timeout 1,000 ms; GET retries: 2 with jitter/backoff (100/300 ms); POST/PUT require idempotency keys to allow safe retry, else no automatic retry.
     - API→IdP (JWKs/claims): cache JWKs for 24 h with refresh; timeout 500 ms; retry 2x with jitter.
     - Import endpoints: require idempotency-key header; dedupe by key + checksum; DLQ/quarantine for poison CSV; backoff on 429/5xx with exponential retry up to 6 attempts.

## 2) Refactor suggestions (max 3)

- Elevate API Gateway/Ingress as a first-class adapter in the Container view
  - Rationale: Makes ingress responsibilities explicit and reduces ambiguity around auth, limits, and timeouts.
  - Expected outcome: Clear operational boundaries; easier mapping from Container→Deployment; fewer production surprises.

- Consolidate SLOs into an "SLO Registry" and link from OCR, ADR, and C4 views
  - Rationale: Removes conflicting numbers and provides one truth for alerts and performance budgets.
  - Expected outcome: Consistent tuning and reporting; reduced toil from threshold drift.

- Harden the typed data-access SDK with built-in resilience and error taxonomy
  - Rationale: Centralizes timeouts/retries/idempotency enforcement; prevents chatty or unsafe patterns per MFE.
  - Expected outcome: Better p95 under load, safer retries, and consistent UX during partial failures.

## 3) Items to push to ADR consequences or risk log (max 3)

- Eventual consistency windows (1–5 s) between caches and Backend SoT
  - Why it matters: Users may briefly see stale reads after mutations; needs expectation-setting and fast revalidation.
  - Where to record: ADR-001 Consequences (consistency trade-off) and Risk log with UX mitigation (short TTL + revalidation banner on mutations).

- Optional service mesh introduces ops complexity
  - Why it matters: If enabled, it alters retry/mTLS behavior and requires policy alignment with gateway and SDK.
  - Where to record: Risk log; note decision criteria and fallback if mesh is not adopted.

- CSV import backpressure and schema drift
  - Why it matters: Large/bad files can saturate DB or produce repeated failures; external schema changes can break ACL mapping.
  - Where to record: Risk log; mitigation: staged validation, rate limits, idempotency keys, DLQ/quarantine, schema contract tests.

## 4) Assumptions/Unknowns

- IdP, Feature Flag Service, and Observability vendors are TBD; assume OIDC-compliant IdP, flag SDK with local cache, and OTLP/HTTPS exporter.
- Tenant model and uniqueness scope remain open; assume global uniqueness for now with future tenant scoping via claims.
- Database engine and backup/restore runbook specifics are TBD; assume managed RDBMS with PITR and quarterly restore drills.
- Service mesh adoption is optional; if not adopted, rely on gateway-based retries and TLS-only east–west.
- CSV schema (columns/encodings/upsert semantics) not finalized; assume strict validation with dry-run preview and atomic commit.

---

Notes on vocabulary alignment: This audit reuses terms from ADR-001 and C4 views — "Knowledge Accounting Platform", "Backend API", "Capability MFEs", "API Gateway/Ingress", "Primary Database (RDBMS)", "Identity Provider (OIDC)", "Observability/Telemetry Platform", and "Feature Flag Service" — and does not introduce net-new systems.
