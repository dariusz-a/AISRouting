# Design Document v0.9 — Knowledge Accounting Platform
Version: 0.9 (pre–stress-test draft)
Linked ADR: docs/tech_design/architecture/ADR_001.md

## 1. Overview
- Purpose: Produce the first consolidated technical design aligned with ADR v1, OCR, and the C4 views, using the exact system vocabulary and targets already defined.
- Summary: The selected architecture is Candidate B — capability‑aligned micro frontends (MFEs) composed by a Shell with a typed data‑access SDK, backed by a Backend API as the system of record (SoT), and a Primary Database (RDBMS). Shell and MFEs are delivered via CDN/Edge Delivery; authentication uses an Identity Provider (OIDC); observability via an Observability/Telemetry Platform; rollout via a Feature Flag Service.

## 2. C4 Context View
C4-Context (text):

- Person
  - Administrator — manages roles/skills and governance (RBAC)

- System (under design)
  - Knowledge Accounting Platform — Shell + capability‑aligned MFEs; Backend API is SoT

- External Systems
  - Identity Provider (OIDC) — external IdP for authentication
  - CDN/Edge Delivery — delivers Shell and Capability MFEs
  - External CSV Source — provides CSV files for import
  - Observability/Telemetry Platform — logs, metrics, traces and SLO dashboards
  - Feature Flag Service — controls rollout and canary per MFE

- Relationships
  - Administrator → Knowledge Accounting Platform: Manages roles/skills and governance (RBAC)
  - Knowledge Accounting Platform → Identity Provider (OIDC): Authenticates via OIDC
  - Knowledge Accounting Platform → CDN/Edge Delivery: Serves Shell and MFEs via CDN
  - External CSV Source → Knowledge Accounting Platform: Provides CSV for import
  - Knowledge Accounting Platform → Observability/Telemetry Platform: Sends telemetry and SLO metrics
  - Knowledge Accounting Platform → Feature Flag Service: Evaluates feature flags for rollout

- QAs driving this view
  - FCP ≤ 1.8 s p95 (OCR); route change ≤ 250 ms p95; role CRUD ≤ 250 ms p95; Availability ≥ 99.95% via CDN + HA API; backend is SoT with cache revalidation p95 ≤ 1 s and staleness window 1–5 s (ADR/OCR).

Assumptions (≤3):
- Identity Provider vendor is unspecified (OIDC only). Observability/Telemetry Platform and Feature Flag Service are capabilities, not brand-specific.
- Exact tenant model is TBD; diagrams remain tenant‑agnostic.
- CSV source, schema, and encoding are external; validated at import boundary.

Open questions (≤3):
- Is role-name uniqueness global or per tenant, and how is tenant context represented (ADR Open Questions)?
- What is the authoritative skills/categories catalog and ID stability across imports (ADR/OCR)?
- CSV schema details (required/optional columns, delimiter/encoding) and upsert vs create-only semantics (ADR/OCR).

## 3. C4 Container View
Containers/services and responsibilities (text):

- Shell
  - Purpose: Composes capability‑aligned MFEs, handles routing, OIDC redirects, and uses the typed data‑access SDK to call the Backend API.
  - Data ownership: None (ephemeral UI/session state only; no durable writes).
  - Communication: Delivered via CDN/Edge Delivery; egress via HTTP REST/JSON to Backend API (GraphQL optional per ADR); OIDC auth redirects/token exchange with Identity Provider (OIDC); evaluates client‑side Feature Flags; emits web‑vitals/logs/traces to Observability/Telemetry Platform.

- Capability MFEs
  - Purpose: Roles, Skills, People, Projects, and Assessments UIs; honor RBAC and degrade gracefully per MFE when API is impaired.
  - Data ownership: None (view state only; optimistic updates with short‑lived caches).
  - Communication: Routed by Shell; assets via CDN/Edge; calls Backend API through Shell’s typed SDK; emits client telemetry.

- Backend API
  - Purpose: System of record and policy/RBAC enforcement; exposes domain APIs and import endpoints.
  - Data ownership: Core aggregates — Roles, Skills, People, Projects, Teams, Assessments; Import Jobs and staging artifacts.
  - Communication: Ingress via HTTP REST/JSON (GraphQL optional); validates JWT/claims from Identity Provider (OIDC); egress telemetry to Observability/Telemetry Platform; optional server‑side Feature Flag evaluations.

- Import/ETL Worker
  - Purpose: Validates and imports CSV from External CSV Source via the platform’s import endpoints; Anti‑Corruption Layer (ACL) maps external schemas to domain commands with idempotent upserts.
  - Data ownership: Import Jobs and staging (persisted under Backend API ownership in Primary Database).
  - Communication: Triggered by CSV upload; writes via Backend API commands; emits import metrics.

- Data Store
  - Primary Database — RDBMS (single writer: Backend API). Strong consistency for writes; short‑lived client caches (staleness 1–5 s) with revalidation p95 ≤ 1 s.

Synchronous edges
- Shell → Backend API: Capability CRUD/queries via HTTP REST/JSON (target p95 ≤ 250 ms)
- Capability MFEs → Backend API: Calls via Shell’s typed SDK (HTTP REST/JSON)
- Shell ↔ Identity Provider (OIDC): OIDC redirects/token exchange; Backend API validates JWT/claims
- Shell → Feature Flag Service; Backend API → Feature Flag Service (optional)
- External CSV Source → Backend API: Upload CSV for import (HTTP file upload)

Asynchronous edges
- Shell → Observability/Telemetry Platform: Web‑vitals/logs/traces
- Backend API → Observability/Telemetry Platform: Structured logs/metrics/traces and SLO metrics
- Import/ETL Worker → Observability/Telemetry Platform: Import job metrics/logs

Hotspots to validate
- Coupling: Potential chatty UI→API patterns across MFEs; consider batching and HTTP/2 to keep p95 ≤ 250 ms.
- Consistency: Enforce single‑writer via Backend API; ensure idempotent mutations and cache revalidation p95 ≤ 1 s.
- Availability: Partial degradation per MFE when API impaired; circuit breaking/timeouts on UI↔API and API↔IdP.

## 4. C4 Component View — Critical Service: Backend API
Components (from c4_component_backend-api):
- HTTP API Adapter (Adapter): REST/JSON ingress + CSV upload; maps DTOs/commands to application services; stateless.
- OIDC Claims Mapper (Mapper/ACL): Validates JWTs, normalizes claims to Principal; prevents IdP details leaking into domain.
- Policy/Guard (RBAC) (Policy/Guard): Authorizes commands/queries before state changes and sensitive reads.
- ProjectManagementService (Application Service): Orchestrates project/role lifecycle; calls ProjectRepository and SkillMatcher.
- SkillMatcher (Domain Service): Matches people skills to role requirements; pure domain logic.
- ProjectRepository (Repository): Persistence for Projects/RoleDefinitions/RoleAssignments in Primary Database.
- ImportOrchestrator (Application Service): CSV import endpoints and job lifecycle; enforces idempotency keys.
- ImportJobRepository (Repository): Persistence for Import Jobs and staging artifacts with idempotent patterns.
- Observability Adapter (Adapter): Emits structured logs, metrics, and traces.

Key dependency directions
- Adapters/ACL → Application Services → Domain Services/Repositories → Primary Database (RDBMS)
- Cross‑cutting: Policy/Guard applied at Adapter and Service entry; Observability Adapter spans requests and DB interactions.

## 5. Deployment Topology
Deployment (text):
- Region: EU Region (GDPR) with Multi‑AZ (a/b); optional DR region (warm standby) TBD.
- Nodes/Clusters:
  - CDN/Edge Delivery: Global edge for Shell and Capability MFEs (static assets; immutable/versioned).
  - Kubernetes Cluster (prod):
    - Public segment: API Gateway/Ingress (Envoy/NGINX).
    - Private segment: Backend API (Deployment) and Import/ETL Worker (Job/CronJob).
  - Managed RDBMS (Primary Database): Multi‑AZ, private networking, TLS in transit, encryption at rest.
- Network boundaries: North‑south via CDN and API Gateway; east‑west private (mTLS if service mesh enabled, TBD).
- Autoscaling:
  - Backend API: HPA on CPU 70% and/or custom metric p95 latency > 250 ms for 3 min; min 3, max 12 replicas; PDBs.
  - Import/ETL Worker: Parallelism ≤ 2; backoffLimit=6; ingress rate limits and queue depth gates for import.
- Observability: Structured logs/metrics/traces (OTLP/HTTPS); web‑vitals from Shell/MFEs; synthetic checks; SLO dashboards (latency/availability/import success).
- Data replication & backup: Managed RDBMS with Multi‑AZ synchronous replication; daily full backups + PITR; quarterly restore drills (TBD); GDPR‑aligned residency.

## 6. SLOs & Capacity Assumptions
*(Derived from Objectives, Constraints, and Deployment design.)*

### 6.1 Purpose
This section translates non-functional constraints into measurable, AI-verifiable targets. These serve as the basis for the next session’s stress-tests and policy checks.

### 6.2 Input References
- OCR Sheet: `docs/tech_design/architecture/ocr.md`
- ADR v1: `docs/tech_design/architecture/ADR_001.md`
- Deployment Topology: `docs/tech_design/architecture/c4_deployment.md`

---

### 6.3 Latency & Availability
| Service / API | Target | Metric | Verification Hook |
|----------------|---------|---------|--------------------|
| Shell/Web-Vitals (CDN delivered) | FCP p95 ≤ 1.8 s; TTI p95 ≤ 2.5 s | `web_vitals_fcp_seconds`, `web_vitals_tti_seconds` | synthetic page probe + web‑vitals beacons |
| Capability MFEs (route change) | p95 ≤ 250 ms | `spa_route_change_duration_ms` | RUM beacon + trace spans (OTEL) |
| Backend API (capability CRUD/queries) | p95 ≤ 250 ms; p99 ≤ 400 ms | `http_server_request_duration_seconds` | Prometheus query + synthetic API checks |
| Backend API (import control‑plane) | p95 ≤ 400 ms | `http_server_request_duration_seconds` (route label) | synthetic import API workflow |
| Import/ETL Worker (job end‑to‑end) | 5,000 rows p95 ≤ 10 s | `import_job_duration_seconds` | job trace + metric assertion |
| CDN/Edge Delivery (assets) | TTFB p95 ≤ 100 ms | `cdn_ttfb_seconds` | multi‑region synthetic probe |

Availability target: 99.95% monthly (≤ 21.6 min downtime). For client‑only prototype scope (static hosting), 99.9% monthly (≤ 43.2 min).

---

### 6.4 Throughput & Capacity
- Baseline API load: 150 RPS (operational range 50–200 RPS) derived from Deployment view; maintain targets above with no SLO breach.
- Peak load: 500 RPS sustained for 5 minutes with ≤ 20% latency degradation at p95 and no error‑budget burn beyond allowance.
- Expected concurrency: 500–2,000 active users with CDN offloading static assets; CRUD mix ≤ 2 TPS per active 100 users assumed.
- Import throughput: CSV 5,000 rows completes ≤ 10 s p95 (≥ ~500 rows/s effective), with worker parallelism ≤ 2 to protect Primary Database.

Assumptions used where ADR/OCR omit exact numbers: concurrency distribution is even; network RTT ~50–100 ms; HTTP/2 and request batching are enabled to meet p95 ≤ 250 ms.

---

### 6.5 Scaling Triggers
| Metric | Threshold | Action | Max Replicas |
|---------|------------|---------|--------------|
| CPU usage (Backend API) | > 70% for 3 min | HPA adds 1 replica | 12 |
| p95 latency (Backend API) | > 250 ms for 3 min | HPA scale‑out by 1; alert if > 2 consecutive windows | 12 |
| Ingress 429 rate | > 1% for 5 min | Increase replicas by 1 and tighten rate limits on hot routes | 12 |
| Import job queue depth | > 2 jobs for 2 min | Increase worker parallelism up to 2; throttle new uploads | n/a |
| DB connection pool saturation | > 80% for 5 min | Increase API replicas or pool; cap per‑replica pool to avoid thundering herd | 12 |

Safeguards: min replicas = 3; scale‑in cooldown 10 min; max surge 25% during rollouts. Return 429 + Retry‑After on overload.

---

### 6.6 Resource & Data Limits
- Primary Database: initial size budget ≤ 10 GB; replication lag ≤ 2 s (Multi‑AZ sync); PITR enabled.
- Retention policy: Import job artifacts/history retained 30 days; telemetry metrics 30 days (prod) / 14 days (non‑prod).
- Recovery targets: RPO ≤ 5 min; RTO ≤ 30 min for Primary Database failover/restore.
- API payload limits: CSV upload ≤ 10 MB/file; HTTP body ≤ 5 MB/req; enforce server‑side validation.
- Client storage: Local Storage working set ≤ 5 MB per origin; warn at 80% quota.
- Per‑pod resources (initial): Backend API request 250m CPU / 512 MiB; limit 1 vCPU / 1 GiB; revisit after profiling.

---

### 6.7 Observability Hooks
Metrics
- Backend API: `http_server_requests_total`, `http_server_request_duration_seconds{le=...}`, `http_requests_in_flight`, `process_cpu_seconds_total`, `container_memory_working_set_bytes`, `db_pool_in_use_connections`, `slo_breach_total` (custom)
- Import/ETL Worker: `import_job_duration_seconds`, `import_job_success_total`, `import_queue_depth`
- Shell/MFEs (RUM): `web_vitals_fcp_seconds`, `web_vitals_tti_seconds`, `spa_route_change_duration_ms`, `frontend_error_rate`
- CDN/Edge: `cdn_ttfb_seconds`, `cdn_cache_hit_ratio`

Traces
- OpenTelemetry spans from Shell → Backend API → Primary Database with attributes: `http.route`, `db.statement`, `slo.target`, `slo.measured`.

Logs
- Structured logs with fields: `service`, `route`, `duration_ms`, `status`, `tenant_id` (if applicable), and explicit markers `SLO_BREACH=true` when thresholds are exceeded.

Synthetic Probes
- Multi‑region API probes for CRUD and import flows; Web‑Vitals beacon sampling at 1–5% of sessions.

---

### 6.8 Verification Methods
AI‑ or pipeline‑driven stress tests can validate each constraint:
1. Generate synthetic workload to 5× baseline (to 500 RPS) for 5 minutes while measuring `http_server_request_duration_seconds` and error rates; verify autoscaling occurs within 2 minutes and p95 remains ≤ 300 ms during peak, returning to ≤ 250 ms post‑scale.
2. Trigger import of a 5,000‑row CSV and assert `import_job_duration_seconds` p95 ≤ 10 s and `import_job_success_total` increments; ensure worker parallelism ≤ 2 and DB pool not saturated.
3. Run multi‑region page‑load probes to validate FCP p95 ≤ 1.8 s and TTI p95 ≤ 2.5 s; inspect web‑vitals beacons for corroboration.
4. Inject latency to API pods to breach p95 > 250 ms; confirm HPA scales out and `SLO_BREACH=true` log markers appear with corresponding alert but remain within monthly error budget for 99.95% availability.
5. Simulate quota pressure on Local Storage and verify warning telemetry is emitted and UI degrades gracefully without data loss.

---

### 6.9 Summary Table
| Category | Target | Verification Metric | Expected Behavior |
|-----------|---------|--------------------|-------------------|
| Latency | Backend API p95 ≤ 250 ms; Shell FCP p95 ≤ 1.8 s | Prometheus `http_server_request_duration_seconds`; RUM web‑vitals | No latency alerts; autoscale on breach |
| Availability | 99.95% platform; 99.9% static‑only | Uptime probes; SLO burn‑rate | ≤ 21.6 min (platform) / ≤ 43.2 min (static) downtime per month |
| Throughput | 150 RPS baseline; 500 RPS peak (5 min) | RPS counters; error rate | Stable latency within budget; no sustained 5xx |
| Scaling | CPU>70% or p95>250 ms → +1 replica | HPA events; pod count | Scale within 2 min; min=3, max=12 |
| Import | 5,000 rows ≤ 10 s p95 | `import_job_duration_seconds` | Success rate ≥ 99%; no DB saturation |

Notes on vocabulary alignment: Terms such as Shell, Capability MFEs, Backend API, Import/ETL Worker, Primary Database, Identity Provider (OIDC), Observability/Telemetry Platform, and CDN/Edge Delivery follow ADR/C4 documentation.

## 7. Risks & Mitigations
- Local storage limits/quota eviction at scale (~5–10 MB per origin) (R)
  - Mitigation: Compact schema; purge fixtures in prod; warn near‑quota; document backend path when scaling.
- Cross‑tab consistency gaps and duplicate/conflicting assignments (R)
  - Mitigation: Idempotent writes; storage events reconciliation; provenance indicators; disable already‑assigned in UI.
- Contract/version drift across MFEs/SDK/API (N)
  - Mitigation: Typed SDK + contract tests; semantic versioning; canary per MFE; backward‑compatible API changes.
- Traffic spikes and network latency/timeouts impacting p95 ≤ 250 ms (N)
  - Mitigation: CDN for assets; batching and HTTP/2; HPA on latency; client timeouts/retries and circuit breakers.
- Import workload saturates Primary Database (N)
  - Mitigation: Limit worker parallelism; staged validation; idempotent upserts; queue depth alarms; quarantine poison data.

## 8. Assumptions & Open Questions
Assumptions
- Role name uniqueness is case‑insensitive and trimmed (OCR).
- Only non‑PII metadata stored by default; PII needs consent and ≤ 30‑day retention if introduced (OCR).
- No event stream is mandated by ADR; synchronous persistence only for now (C4 Container/Component notes).

Open Questions
- Tenant model and uniqueness scope (global vs per tenant) (ADR/OCR).
- Authoritative skills/categories catalog and ID stability across imports (ADR/OCR).
- CSV schema details; delimiter/encoding; upsert vs create‑only; export needs to mitigate local storage risks (ADR/OCR).
- Cloud vendor, exact regions, managed DB engine, and service mesh adoption (Deployment TBDs).

## 9. Linked Artifacts
- ADR v1: docs/tech_design/architecture/ADR_001.md
- OCR sheet: docs/tech_design/architecture/ocr.md
- Trade‑off matrix: docs/tech_design/architecture/alt_arch_matrix.md
- C4 Context: docs/tech_design/architecture/c4_context.md
- C4 Container: docs/tech_design/architecture/c4_container.md
- C4 Deployment: docs/tech_design/architecture/c4_deployment.md
- Component (Backend API): docs/tech_design/architecture/c4_component_backend-api.md
