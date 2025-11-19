## C4-Component (text form)

- Container: Backend API
- Components
  1) HTTP API Adapter (Adapter)
     - Responsibility: Ingress for REST/JSON and CSV upload; validates and parses requests; maps DTOs/commands to application services; shapes responses.
     - Interfaces/Ports: Ingress HTTP (REST/JSON; file upload for CSV); egress in-process calls to Application Services and Policy/Guard.
     - Data touched/owned: None (stateless; request-scoped only).
     - Boundary enforcement: Keeps transport/protocol concerns out of application/domain layers; maps external contracts to internal commands/queries.

  2) OIDC Claims Mapper (Mapper/ACL)
     - Responsibility: Validates JWTs, extracts/normalizes claims to an internal Principal; caches JWKs per library guidance.
     - Interfaces/Ports: Uses IdP/OIDC library and JWKs over HTTP (as needed); provides Principal to Policy/Guard and Application Services.
     - Data touched/owned: None durable (short-lived key/metadata caches per library).
     - Boundary enforcement: Prevents IdP-specific claim shapes from leaking into domain; centralizes authn mapping.

  3) Policy/Guard (RBAC) (Policy/Guard)
     - Responsibility: Authorizes commands/queries based on roles/policies; evaluated before any state-changing operation and for sensitive reads.
     - Interfaces/Ports: Invoked by HTTP API Adapter and Application Services; reads role assignments via repositories as needed.
     - Data touched/owned: Reads role/assignment data; no durable ownership.
     - Boundary enforcement: Centralizes authorization logic; ensures domain mutations respect RBAC and organizational policies.

  4) ProjectManagementService (Application Service)
     - Responsibility: Orchestrates project/role lifecycle (create/update projects, define roles, assign people); coordinates validation and persistence.
     - Interfaces/Ports: Called by HTTP API Adapter; calls ProjectRepository and Domain Services (e.g., SkillMatcher); emits observability events.
     - Data touched/owned: Coordinates transactional updates to Project and RoleAssignment aggregates in Primary Database.
     - Boundary enforcement: Enforces aggregate invariants and single-writer rules; isolates domain from persistence and transport.

  5) SkillMatcher (Domain Service)
     - Responsibility: Matches people skills against role requirements to support staffing/readiness.
     - Interfaces/Ports: Pure domain function invoked by Application Services; no I/O.
     - Data touched/owned: None (stateless computation over domain entities/values).
     - Boundary enforcement: Encapsulates algorithmic logic separate from orchestration and persistence; testable in isolation.

  6) ProjectRepository (Repository)
     - Responsibility: Persistence for the Project aggregate (and nested role definitions) in the Primary Database.
     - Interfaces/Ports: SQL via data access layer; returns domain aggregates; participates in application-level transactions.
     - Data touched/owned: Projects, RoleDefinitions, and RoleAssignments tables; strong consistency on writes; optimistic reads allowed if request permits.
     - Boundary enforcement: Encapsulates schema and SQL; no persistence details leak into services or domain.

  7) ImportOrchestrator (Application Service)
     - Responsibility: Handles CSV import endpoints; persists Import Jobs/staging artifacts; exposes job lifecycle operations for the worker; ensures idempotency.
     - Interfaces/Ports: Called by HTTP API Adapter; calls ImportJobRepository; may call Policy/Guard for scope; emits observability signals.
     - Data touched/owned: Creates/updates ImportJob and staging records transactionally; enforces idempotency keys per job/source.
     - Boundary enforcement: Provides an anti-corruption boundary between external CSV payloads and domain upsert commands (worker writes back via API).

  8) ImportJobRepository (Repository)
     - Responsibility: Persistence for Import Jobs and staging artifacts; supports idempotent create/update and worker coordination endpoints.
     - Interfaces/Ports: SQL via data access layer; provides query patterns for job status, claims/ack, and retries with backoff.
     - Data touched/owned: ImportJobs, ImportStaging artifacts; strong transactional semantics to avoid duplicates; idempotency keys indexed.
     - Boundary enforcement: Hides storage layout from orchestrator; enforces write patterns and idempotency at the repository boundary.

  9) Observability Adapter (Adapter)
     - Responsibility: Emits structured logs, metrics, and traces for requests, use-case steps, and DB interactions.
     - Interfaces/Ports: Telemetry SDK/exporters to Observability/Telemetry Platform (async where applicable).
     - Data touched/owned: None durable.
     - Boundary enforcement: Keeps telemetry concerns out of core logic; consistent correlation/trace context propagation.

## C4-PlantUML (Component)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

Container_Boundary(api, "Backend API") {
  Component(api_adapter, "HTTP API Adapter", "Adapter", "REST/JSON ingress + CSV upload")
  Component(oidc_mapper, "OIDC Claims Mapper", "Mapper/ACL", "JWT -> Principal")
  Component(policy, "Policy/Guard (RBAC)", "Policy/Guard", "Authorizes commands/queries")
  Component(app_proj, "ProjectManagementService", "Application Service", "Use-case orchestration")
  Component(domain_match, "SkillMatcher", "Domain Service", "Matching algorithm")
  Component(repo_proj, "ProjectRepository", "Repository", "RDBMS persistence for Projects/RoleDefs")
  Component(app_import, "ImportOrchestrator", "Application Service", "CSV import endpoints & jobs")
  Component(repo_import, "ImportJobRepository", "Repository", "RDBMS persistence for Import Jobs/Staging")
  Component(obs, "Observability Adapter", "Adapter", "Logs/Metrics/Traces")
}

ContainerDb(db, "Primary Database", "RDBMS", "Projects, RoleDefinitions, RoleAssignments, ImportJobs, Staging")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(obs_sys, "Observability/Telemetry Platform", "Logs/metrics/traces")
System_Ext(csv_src, "External CSV Source", "Uploads CSV files")

Rel(csv_src, api_adapter, "Upload CSV", "HTTP file upload")
Rel(api_adapter, oidc_mapper, "Validate JWT/claims", "HTTP/JWT + library")
Rel(oidc_mapper, policy, "Provide Principal/claims")
Rel(api_adapter, policy, "Authorize request")
Rel(api_adapter, app_proj, "Calls use cases")
Rel(app_proj, domain_match, "Invoke matching")
Rel(app_proj, repo_proj, "Load/Save aggregates", "SQL")
Rel(api_adapter, app_import, "Calls import endpoints")
Rel(app_import, repo_import, "Persist jobs/staging", "SQL")
Rel(repo_proj, db, "Read/Write", "SQL")
Rel(repo_import, db, "Read/Write", "SQL")
Rel(obs, obs_sys, "Export telemetry (async)")

@enduml
```

## Failure and recovery points

- Primary Database outage/high latency: Short, bounded timeouts and retries with jitter; circuit breaker on repositories; degrade to read-only where safe; queue/deny non-critical writes with clear error semantics; preserve idempotency on retried imports.
- IdP/JWT validation latency or key rotation: Cache JWKs; short TTLs with backoff; fail fast with 401/403; monitor token errors and rotate keys proactively.
- CSV import spikes or poison files: Enforce size/type limits; streaming parse with backpressure; idempotent ImportJob creation; quarantine invalid data with detailed diagnostics; retry with exponential backoff.

## Assumptions/Unknowns

- No event stream/outbox mandated by ADR; design assumes synchronous persistence only (TBD if domain events are added later).
- Exact ORM/data access layer is unspecified; repositories assume SQL/RDBMS with transactions.
- Server-side Feature Flag evaluation is optional per container; omitted from component list pending concrete usage policy (TBD).
- Tenant model and uniqueness scopes (e.g., role-name) are TBD; policies will integrate once defined.
