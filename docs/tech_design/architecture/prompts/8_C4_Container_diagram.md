# C4 Container Diagram Generator

## Role
You are an experienced software architect who writes concise, high-signal design documents that align with previously established vocabulary.

## Task
Produce a clear C4 Container view for the Knowledge Accounting system in text form, strictly reusing the same names/terms from the C4-Context and ADR v1 decision. Emphasize boundaries, interfaces, and data ownership.

If your environment supports writing files to the repository, create the folder `docs/tech_design/architecture` (if it doesn't already exist) and write the result to `docs/tech_design/architecture/c4_container.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path `docs/tech_design/architecture/c4_container.md` so it can be saved manually.

## Input Sources

### C4-Context (text)
- Location: `docs/tech_design/architecture/c4_context.md`

### ADR v1 (decision & consequences): 
- Location: `docs/tech_design/architecture/ADR_001.md`

Re-use the exact vocabulary from these sources (e.g., system name, IdP naming, CSV import, observability). Do not introduce new terms unless clearly required by ADR; if necessary, call them out in Assumptions/Unknowns.

## Scope and Constraints
- Level: C4 Level 2 — Containers only (no components).
- Audience: Architects and senior engineers.
- Focus: Containers, owned data, interfaces, and inter-container protocols.
- Include both synchronous (HTTP/gRPC) and asynchronous (messaging/stream) edges as applicable per ADR.
- If an anti-corruption layer (ACL) is relevant on any external/system boundary, include it and describe its role.

## Output Format (exactly this order)

1) C4-Container (text form)
	 - Containers (for each):
		 - Name (exact ADR/C4-Context term)
		 - Purpose (one or two sentences)
		 - Owned data (logical ownership; list main aggregates or datasets)
		 - Key interfaces (public APIs/ports/endpoints; ingress and egress)
	 - Data Stores:
		 - Name and type (RDBMS, KV, document, stream, blob, cache)
		 - Primary owners (which container(s) write), read accessors, and consistency mode if stated (e.g., eventual, strong)
	 - Edges and Integration Notes:
		 - Sync edges: label with purpose + protocol (e.g., "Query profile via HTTP JSON")
		 - Async edges: label with purpose + channel/stream/topic (e.g., "Publish domain events to Kafka topic X")
		 - Any Anti-Corruption Layer: scope, mapping responsibilities, and positioning between containers/systems
	 - Trade-offs (3):
		 - List three explicit trade-offs relevant to the chosen design/alternative in ADR v1 (e.g., consistency vs latency, ops complexity vs flexibility, cost vs observability depth)

2) C4-PlantUML (Container)
	 - Provide a minimal PlantUML diagram using the C4-PlantUML syntax for Container level.
	 - Use `System_Boundary` for the SUD, and `Container`, `ContainerDb`, `ContainerQueue` (if needed) for internal containers/data stores.
	 - Use `System_Ext` for external systems. Keep IDs short and readable.
	 - Label edges with purpose + protocol/channel; mark async where applicable.

3) Hotspots to validate
	 - Coupling (e.g., cross-container chatty calls; shared schemas)
	 - Data ownership (single-writer per aggregate; write hotspots)
	 - Consistency modes (sync vs async boundaries; reprocessing/backfill concerns)
	 - Failure modes (timeouts, retries, idempotency, poison messages)
	 - Security boundaries (authn/z enforcement points; data-at-rest vs in-transit)

4) Assumptions/Unknowns
	 - List any inferred elements not explicitly covered by ADR/C4-Context. Keep short and mark as TBD where appropriate.

## Derivation Guidance
- Reuse exact names from `c4_context.md` for the system under design and external systems (e.g., Identity Provider (OIDC), CSV import source, Observability backend), but include only what ADR explicitly supports.
- Derive containers by functional boundary, not by team or deployment unit unless ADR dictates. Examples: Web App (UI), Backend/API, Import/ETL worker, Auth Gateway/Adapter, Observability/Telemetry Agent.
- For each container, identify:
	- Primary responsibilities aligned to ADR and feature scenarios
	- What data it owns vs reads (single-writer principle where applicable)
	- Interfaces it exposes (REST/gRPC), subscribes to, or publishes (events/streams)
	- External dependencies (IdP, file storage, HR CSV source) with protocol labels
- Reflect quality attributes from ADR (e.g., <300ms API latency target, GDPR compliance) as brief notes at relevant interfaces or data stores.

## Acceptance Checklist (internal; do not print these labels in the output)
- Names match the vocabulary of `c4_context.md` and `ADR_001.md`.
- Only containers and data stores (no components/classes).
- Each container includes purpose, owned data, and key interfaces.
- Sync vs async edges are labeled with protocol/channel.
- Anti-corruption layer called out if applicable.
- Exactly three trade-offs listed.
- C4-PlantUML is present and valid at Container level.
- Hotspots section present with concrete risks to validate.
- Output written to `docs/tech_design/architecture/c4_container.md` or returned with intended path stated.

---

## Example structure note (do not copy verbatim; derive from ADR vocabulary)

### C4-Container (text form)
- Web Application (UI)
	- Purpose: SPA delivering user workflows and RBAC-aware UX.
	- Owned data: none (session state only, no durable writes).
	- Key interfaces: HTTP to Backend/API; OIDC redirects to IdP.
- Backend/API
	- Purpose: Orchestrates domain logic; exposes REST/gRPC.
	- Owned data: Core domain aggregates (Projects, Roles, Skills, Teams, Assessments).
	- Key interfaces: REST/JSON for UI; publishes domain events; integrates with IdP (OIDC) and Observability.
- Import/ETL Worker
	- Purpose: Validates and imports CSV from external HR source.
	- Owned data: Import jobs, staging tables; writes into core aggregates via Backend or directly (per ADR).
	- Key interfaces: Subscribes to CSV upload events; writes via internal API; publishes import results.
- Data Stores
	- Primary DB (RDBMS): owned by Backend/API; read by UI via API; strong consistency for writes.
	- Event Stream (stream): owned by Backend/API; consumed by Import/ETL Worker and Observability.

Edges and Integration Notes
- UI -> Backend/API: HTTP JSON (sync)
- Backend/API -> IdP: OIDC auth (sync)
- Import/ETL Worker -> Backend/API: HTTP JSON (sync) for commands
- Backend/API -> Event Stream: Publish domain events (async)
- Event Stream -> Import/ETL Worker: Subscribe to events (async)
- Backend/API -> Observability: Telemetry/metrics/logs (async)

Trade-offs
- Event-driven updates reduce coupling but introduce eventual consistency.
- Central API simplifies governance but can become a bottleneck if not scaled.
- RDBMS ensures transactional integrity but adds migration overhead vs schemaless stores.

### Hotspots to validate
- Single-writer enforcement for core aggregates in the Primary DB.
- Chatty UI-to-API calls impacting <300ms latency SLO.
- Retry/idempotency for CSV import and event processing.

### Assumptions/Unknowns
- Replace container names with ADR-approved terms.
- If CSV source or Observability aren’t in ADR, omit and list here as TBD.

### C4-PlantUML (Container)
```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

System_Boundary(sud, "Knowledge Accounting Platform") {
	Container(shell, "Shell", "SPA/Static assets", "Routes MFEs, OIDC redirects, typed SDK")
	Container(mfe, "Capability MFEs", "SPA modules", "Capability UIs (Roles, Skills, People, Projects, Assessments)")
	Container(api, "Backend API", "HTTP REST/JSON (GraphQL optional)", "System of record; RBAC/policy; domain APIs")
	Container(worker, "Import/ETL Worker", "Job/Worker", "Validate and import CSV via API")
	ContainerDb(db, "Primary Database", "RDBMS", "Core aggregates: Roles, Skills, People, Projects, Teams, Assessments, Import Jobs")
}

System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(csv, "External CSV Source", "Provides CSV files for import")
System_Ext(obs, "Observability/Telemetry Platform", "Logs/metrics/traces")
System_Ext(ff, "Feature Flag Service", "Feature toggles")

Rel(shell, api, "Capability CRUD/queries", "HTTP REST/JSON")
Rel(mfe, api, "Calls via Shell SDK", "HTTP REST/JSON")
Rel(shell, idp, "OIDC auth redirects/token exchange", "HTTP/OIDC")
Rel(api, idp, "Validate JWT/claims", "HTTP/JWT")
Rel(shell, ff, "Evaluate feature flags", "HTTP")
Rel(api, ff, "Server-side flag evaluation", "HTTP")
Rel(csv, api, "Upload CSV for import", "HTTP")
Rel(api, db, "Reads/Writes core aggregates", "SQL")
Rel(shell, obs, "Web-vitals/logs/traces (async)")
Rel(api, obs, "Structured logs/metrics/traces (async)")
Rel(worker, obs, "Import job metrics/logs (async)")

@enduml
```
