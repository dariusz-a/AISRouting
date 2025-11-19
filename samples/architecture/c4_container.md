# C4-Container — Knowledge Accounting Platform

## 1) C4-Container (text form)

- Shell
  - Purpose: Composes capability‑aligned micro frontends (MFEs), handles routing, OIDC redirects, and uses the typed data‑access SDK to call the Backend API.
  - Owned data: None (ephemeral UI/session state only; no durable writes).
  - Key interfaces:
    - Ingress: Delivered via CDN/Edge Delivery (static assets; SPA bootstrap).
    - Egress: HTTP REST/JSON calls to Backend API (GraphQL optional per ADR); OIDC auth redirects/token exchange with Identity Provider (OIDC); evaluates client‑side Feature Flags; emits web‑vitals/logs/traces to Observability/Telemetry Platform.

- Capability MFEs
  - Purpose: Capability UIs for Roles, Skills, People, Projects, and Assessments; honor RBAC and degrade gracefully per MFE when API is impaired.
  - Owned data: None (view state only; optimistic updates backed by short‑lived caches).
  - Key interfaces:
    - Ingress: Routed and composed by Shell; assets delivered via CDN/Edge Delivery.
    - Egress: Uses Shell’s typed SDK over HTTP REST/JSON to Backend API; inherits OIDC session from Shell; emits client telemetry to Observability/Telemetry Platform.

- Backend API
  - Purpose: System of record and policy/RBAC enforcement; exposes domain APIs to support capability workflows and import flows.
  - Owned data: Core aggregates — Roles, Skills, People, Projects, Teams, Assessments; Import Jobs and staging artifacts.
  - Key interfaces:
    - Ingress: HTTP REST/JSON (GraphQL optional) for capability CRUD/queries; CSV import endpoints; validates JWT/claims from Identity Provider (OIDC).
    - Egress: Structured logs/metrics/traces to Observability/Telemetry Platform; optional server‑side Feature Flag evaluations.

- Import/ETL Worker
  - Purpose: Validates and imports CSV from External CSV Source via the platform’s import endpoints; provides an Anti‑Corruption Layer (ACL) to map external CSV schemas into domain aggregates and ensure idempotent upserts.
  - Owned data: Import Jobs, validation results, and staging (persisted in Primary Database under Backend API ownership; worker does not own standalone durable stores).
  - Key interfaces:
    - Ingress: Triggered by CSV upload requests (HTTP) received by Backend API; polls/receives work items.
    - Egress: Writes via Backend API commands (HTTP REST/JSON); publishes import outcome/metrics to Observability/Telemetry Platform.

### Data Stores

- Primary Database — RDBMS
  - Type: Relational database (RDBMS)
  - Primary owners: Backend API (single writer for domain aggregates and import artifacts)
  - Read accessors: Exposed to Shell/Capability MFEs only via Backend API; Import/ETL Worker writes through Backend API
  - Consistency mode: Strong consistency for writes; short‑lived client caches with revalidation p95 ≤ 1 s and staleness window 1–5 s (per ADR)

### Edges and Integration Notes

- Sync edges
  - Shell → Backend API: Capability CRUD/queries via HTTP REST/JSON (target p95 ≤ 250 ms)
  - Capability MFEs → Backend API: Calls via Shell’s typed SDK over HTTP REST/JSON (target p95 ≤ 250 ms)
  - Shell ↔ Identity Provider (OIDC): OIDC auth redirects/token exchange over HTTP/OIDC
  - Backend API → Identity Provider (OIDC): Validate JWT/claims over HTTP/JWT
  - Shell → Feature Flag Service: Evaluate feature flags over HTTP
  - Backend API → Feature Flag Service: Optional server‑side flag evaluation over HTTP
  - External CSV Source → Backend API: Upload CSV for import over HTTP (file upload)

- Async edges
  - Shell → Observability/Telemetry Platform: Web‑vitals/logs/traces (async)
  - Backend API → Observability/Telemetry Platform: Structured logs/metrics/traces and SLO metrics (async)
  - Import/ETL Worker → Observability/Telemetry Platform: Import job metrics/logs (async)

- Anti‑Corruption Layer
  - Scope: External CSV Source → platform import → domain aggregates
  - Responsibilities: Schema validation, field mapping, normalization, and idempotent upsert semantics; rejects poison data and preserves auditability of import decisions
  - Positioning: Implemented within Import/ETL Worker and Backend API’s import boundary; isolates domain model from external CSV schema variability

### Trade‑offs (3)

- Strong consistency (Backend = SoT) vs latency/availability: favors correctness and governance; mitigated with short‑lived caches and optimistic updates but introduces staleness windows (1–5 s).
- Capability‑aligned MFEs + typed SDK increase flexibility and team autonomy vs higher operational complexity, contract/versioning drift risk, and potential chatty network patterns.
- Deep observability (distributed tracing, SLOs, canary/flags) improves operability vs added runtime cost and tooling overhead; requires disciplined sampling and dashboards.

## 2) C4-PlantUML (Container)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

System_Boundary(sud, "Knowledge Accounting Platform") {
  Container(shell, "Shell", "Vue 3 SPA", "Routes MFEs, OIDC redirects, typed SDK")
  Container(mfes, "Capability MFEs", "SPA modules", "Roles, Skills, People, Projects, Assessments UIs")
  Container(api, "Backend API", "HTTP REST/JSON (GraphQL optional)", "System of record; RBAC/policy; domain APIs")
  Container(worker, "Import/ETL Worker", "Job/Worker", "CSV validation/import; ACL mapping to domain")
  ContainerDb(db, "Primary Database", "RDBMS", "Core aggregates: Roles, Skills, People, Projects, Teams, Assessments, Import Jobs")
}

System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(cdn, "CDN/Edge Delivery", "Delivers shell and MFEs")
System_Ext(csv, "External CSV Source", "Provides CSV files for import")
System_Ext(obs, "Observability/Telemetry Platform", "Logs/metrics/traces and SLO dashboards")
System_Ext(ff, "Feature Flag Service", "Feature toggles for rollout/canary")

Rel(cdn, shell, "Deliver static assets", "HTTP/HTTPS")
Rel(shell, mfes, "Compose capability routes", "in‑memory/module federation")
Rel(shell, api, "Capability CRUD/queries", "HTTP REST/JSON")
Rel(mfes, api, "Calls via Shell SDK", "HTTP REST/JSON")
Rel(shell, idp, "OIDC redirects/token exchange", "HTTP/OIDC")
Rel(api, idp, "Validate JWT/claims", "HTTP/JWT")
Rel(shell, ff, "Evaluate feature flags", "HTTP")
Rel(api, ff, "Server‑side flag evaluation (optional)", "HTTP")
Rel(csv, api, "Upload CSV for import", "HTTP file upload")
Rel(api, db, "Reads/Writes core aggregates", "SQL")

Rel(shell, obs, "Web‑vitals/logs/traces (async)")
Rel(api, obs, "Structured logs/metrics/traces (async)")
Rel(worker, obs, "Import job metrics/logs (async)")

@enduml
```

## 3) Hotspots to validate

- Coupling
  - Chatty UI→API patterns across MFEs; consider batching and HTTP/2 to meet p95 ≤ 250 ms
  - Shared CSV schema assumptions bleeding into domain; enforce ACL boundaries to avoid schema coupling
- Data ownership
  - Single‑writer rule: Backend API as sole writer to Primary Database; worker must write via API
  - Write hotspots on Roles/Skills/Projects during import; validate indexing and transaction scope
- Consistency modes
  - Short‑lived caches (1–5 s TTL) vs strong DB writes; ensure revalidation p95 ≤ 1 s and idempotent mutations
  - Reprocessing/backfill of failed imports; define idempotency keys and dead‑letter handling
- Failure modes
  - Timeouts/retries and circuit breaking on UI↔API and API↔IdP
  - Idempotency for import endpoints; poison CSV detection and quarantine
  - Observability ingestion backpressure; fallbacks that don’t block critical paths
- Security boundaries
  - OIDC authentication and server‑side RBAC enforcement points in Backend API
  - TLS in transit, encryption at rest for Primary Database; PII minimization and access controls

## 4) Assumptions/Unknowns

- No event stream is modeled; ADR does not mandate one. If domain events are later adopted, add a ContainerQueue and async subscriptions (TBD).
- Database engine/vendor is unspecified (RDBMS assumed); encryption/backup policies to be defined (TBD).
- Server‑side Feature Flag evaluation is optional; exact usage policy TBD.
- Tenant model, uniqueness rules, authoritative skills catalog, and CSV schema details remain open per ADR; import ACL will adapt once finalized (TBD).
