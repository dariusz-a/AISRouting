# C4 Component Diagram Generator

## Role
You are an experienced software architect who writes concise, high-signal component views that enforce clear boundaries (DDD where relevant) and reuse established vocabulary.

## Task
Produce a C4 Component view for a selected container from the C4-Container diagram. The prompt accepts a parameter `container_name` that must match the name of one container defined in `docs/tech_design/architecture/c4_container.md`. Focus on internal components (adapters, application/domain services, repositories, orchestrations) and how boundaries are enforced.

If your environment supports writing files to the repository, create the folder `docs/tech_design/architecture` (if it doesn't already exist) and write the result to `docs/tech_design/architecture/c4_component_<container_slug>.md` (overwrite if it exists), where `container_slug` is a kebab-case version of `container_name`. Otherwise, return the full Markdown content and explicitly state the intended path.

## Parameters
- container_name (string, required): Must exactly match one of the containers in `docs/tech_design/architecture/c4_container.md` (e.g., "Backend API", "Shell", "Capability MFEs", "Import/ETL Worker").

## Input Sources

### C4-Container (text): 
- Location: `docs/tech_design/architecture/c4_container.md`
- Non-functionals: Provide or extract targets from ADR/OCR. If not present, accept as parameters:
	- Latency target (e.g., p95 < 300 ms)
	- Throughput (baseline RPS and/or events/s)
	- Availability target (e.g., 99.9% or 99.95%)
	- Data residency/compliance (e.g., GDPR, region constraints)

### ADR v1 (decision & consequences): 
- Location: `docs/tech_design/architecture/ADR_001.md`

Reuse the same vocabulary and boundaries (e.g., Identity Provider (OIDC), Observability/Telemetry Platform, Feature Flag Service, Primary Database). Do not invent external systems that are not present in ADR/Container; list gaps under Assumptions/Unknowns.

## Scope and Constraints
- Level: C4 Level 3 — Component view of the selected container only.
- Include: Internal components such as Adapters (inbound/outbound), Application Services, Domain Services, Repositories, Orchestrations/Sagas, Policies/Guards, Mappers/ACLs, Caching.
- Exclude: Class-level detail and deployment specifics (covered by Deployment view).
- Reflect DDD boundaries: Aggregate invariants, application vs domain responsibilities, single-writer patterns, anti-corruption mappings.

## Output Format (exactly this order)

1) C4-Component (text form)
	 - Container: <container_name>
	 - Components (5–9 entries). For each component:
		 - Name and Type (Adapter | Application Service | Domain Service | Repository | Orchestration/Saga | Policy/Guard | Mapper/ACL | Cache)
		 - Responsibility (concise, high-signal)
		 - Interfaces/Ports (inputs/outputs; protocols if relevant, e.g., HTTP, SQL, SDK)
		 - Data touched/owned (for Repos/Caches) and consistency/transaction notes
		 - Boundary enforcement (how it preserves DDD boundaries, invariants, or anti-corruption)

2) C4-PlantUML (Component)
	 - Provide a minimal PlantUML diagram using C4-PlantUML Component syntax.
	 - Use `!include C4_Component.puml` and wrap components inside a `Container_Boundary` for the selected container.
	 - Use `Component`, `ComponentDb`, `Queue` (if needed) and draw relations among components and to external systems/data stores referenced by the container.
	 - Label edges with purpose + protocol; mark async where applicable.

3) Failure and recovery points (2–3)
	 - Identify critical failure points (e.g., database outage, IdP latency, cache stampede) and how the design recovers (timeouts/retries/backoff/idempotency/fallbacks).

4) Assumptions/Unknowns
	 - Capture any items not explicit in ADR/Container; keep short and mark TBD where necessary.

## Derivation Guidance
- Start from the responsibilities of the selected container in `c4_container.md` and create 5–9 cohesive components.
- Typical patterns by container:
	- Backend API: HTTP Adapter (controller), AuthN/Claims Mapper (OIDC), Policy/Guard (RBAC), Application Services per capability, Domain Services for core algorithms, Repositories (RDBMS), Outbox/Publisher (if events), Observability Adapter.
	- Shell/Capability MFEs: UI Adapter (router), Data Access SDK, Feature Flag Adapter, Telemetry Adapter, Cache; avoid durable data ownership.
	- Import/ETL Worker: Job Orchestrator, Parser/Validator, Mapper/ACL, Upsert Application Service, Idempotency/Checkpoint Store, Observability Adapter.
- Enforce boundaries:
	- Application Services orchestrate use cases and transactions; Domain Services hold domain logic without I/O; Repositories encapsulate persistence; Adapters translate protocols.
	- Use ACL mappers to avoid leaking external schemas into domain models.
	- Note single-writer and aggregate invariants where relevant.

## Acceptance Checklist (internal; do not print these labels in the output)
- `container_name` matches a container in `c4_container.md`.
- 5–9 components listed with responsibilities, ports, data, and boundary notes.
- PlantUML Component diagram present and valid; edges labeled and aligned with text.
- 2–3 Failure and recovery points included.
- Assumptions/Unknowns present for gaps.

---

## Example structure note (do not copy verbatim; derive from ADR vocabulary)

Parameter
- container_name = "Backend API"

### C4-Component (text form)
- HTTP API Adapter (Adapter)
	- Responsibility: Accepts REST/JSON (GraphQL optional), request validation and mapping to application services.
	- Interfaces/Ports: Ingress HTTP; egress to Application Services (in-process calls).
	- Data: None; stateless.
	- Boundary enforcement: Keeps protocol concerns out of domain; maps DTOs to commands.
- OIDC Claims Mapper (Mapper/ACL)
	- Responsibility: Validates JWT, extracts claims, normalizes to internal identity model.
	- Interfaces/Ports: Talks to IdP libraries; provides normalized principal to Policy.
	- Data: None durable.
	- Boundary enforcement: Prevents IdP-specific claims from leaking into domain.
- Policy/Guard (RBAC) (Policy/Guard)
	- Responsibility: Authorizes commands/queries based on roles/policies.
	- Interfaces/Ports: Invoked by Application Services.
	- Data: Reads role assignments from repository.
	- Boundary enforcement: Centralizes authz before domain mutation.
- ProjectManagementService (Application Service)
	- Responsibility: Orchestrates project create/update and role assignment workflows.
	- Interfaces/Ports: Called by API Adapter; calls Repositories and Domain Services.
	- Data: Coordinates transactional updates to Projects/Roles aggregates.
	- Boundary enforcement: Enforces aggregate invariants and single-writer rules.
- SkillMatcher (Domain Service)
	- Responsibility: Matches people skills to role requirements.
	- Interfaces/Ports: Pure function over domain entities.
	- Data: None; stateless.
	- Boundary enforcement: Keeps algorithmic logic free of I/O concerns.
- ProjectRepository (Repository)
	- Responsibility: Persistence for Project aggregate (RDBMS).
	- Interfaces/Ports: SQL via data access layer; returns domain aggregates.
	- Data: Projects table(s); transactional consistency.
	- Boundary enforcement: Encapsulates schema; no SQL in services.
- Observability Adapter (Adapter)
	- Responsibility: Emits structured logs/metrics/traces.
	- Interfaces/Ports: Exporters/SDK (async).
	- Data: None durable.
	- Boundary enforcement: Keeps telemetry concerns out of domain.

### C4-PlantUML (Component)
```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

Container_Boundary(api, "Backend API") {
	Component(api_adapter, "HTTP API Adapter", "Adapter", "REST/JSON ingress")
	Component(oidc_mapper, "OIDC Claims Mapper", "Mapper/ACL", "JWT -> Principal")
	Component(policy, "Policy/Guard (RBAC)", "Policy", "Authorizes commands")
	Component(app_svc, "ProjectManagementService", "Application Service", "Use case orchestration")
	Component(domain_svc, "SkillMatcher", "Domain Service", "Matching algorithm")
	Component(repo, "ProjectRepository", "Repository", "RDBMS persistence")
	Component(obs, "Observability Adapter", "Adapter", "Logs/Metrics/Traces")
}

ContainerDb(db, "Primary Database", "RDBMS", "Projects, Roles, Skills, ...")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(obs_sys, "Observability Platform", "Logs/metrics/traces")

Rel(api_adapter, app_svc, "Calls use cases")
Rel(oidc_mapper, policy, "Provides principal/claims")
Rel(api_adapter, policy, "Authorize request")
Rel(app_svc, repo, "Load/Save aggregates", "SQL")
Rel(domain_svc, app_svc, "Invoked for matching")
Rel(repo, db, "Read/Write", "SQL")
Rel(obs, obs_sys, "Export telemetry (async)")
Rel(api_adapter, oidc_mapper, "Validate JWT/claims")

@enduml
```

### Failure and recovery points
- DB outage or high latency: Circuit breaker + retries with jitter; degrade to read-only where safe; queue non-critical writes.
- IdP latency/validation failure: Short timeouts with fallback to 401; cache JWKs; monitor token validation errors.
- Cache stampede on hot reads: Staggered revalidation (SWR), request coalescing, TTL jitter.

### Assumptions/Unknowns
- Exact data access layer/ORM is unspecified.
- Event outbox is not mandated by ADR; add if events are introduced.
