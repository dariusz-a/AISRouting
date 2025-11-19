# C4 Deployment Diagram Generator

## Role
You are an experienced software architect who writes concise, high-signal deployment views that instantiate the Container view onto runtime infrastructure.

## Task
Produce a C4 Deployment view for the Knowledge Accounting system that maps containers onto runtime nodes (regions/zones, clusters/VMs/functions), networks, and operational concerns. Reuse vocabulary from the C4-Container and ADR.

If your environment supports writing files to the repository, create the folder `docs/tech_design/architecture` (if it doesn't already exist) and write the result to `docs/tech_design/architecture/c4_deployment.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path `docs/tech_design/architecture/c4_deployment.md` so it can be saved manually.

## Input Sources

### C4-Container (text): 
- Location: `docs/tech_design/architecture/c4_container.md`
- Non-functionals: Provide or extract targets from ADR/OCR. If not present, accept as parameters:
	- Latency target (e.g., p95 < 300 ms)
	- Throughput (baseline RPS and/or events/s)
	- Availability target (e.g., 99.9% or 99.95%)
	- Data residency/compliance (e.g., GDPR, region constraints)

### OCR

- Location: `docs/tech_design/architecture/ocr.md`
- Lists the identified risks
- Objective: Support asynchronous order processing for 10k daily transactions
- Constraint: Response time < 300ms for API requests
- Constraint: Must comply with GDPR data storage requirements
- Risk: Eventual consistency may cause temporary data mismatch

## Scope and Constraints
- Level: C4 Level 1 — System Context only.
- Elements allowed: People (actors) and Software Systems (the system under design + external systems). Do NOT include containers/components.
- Size: Aim for 6–10 total elements to keep the view consumable.
- Naming: Reuse names and terms from the ADR verbatim when possible (e.g., OIDC/IdP, API, CSV import).
- Relationships: Use short, active labels (Verb + Purpose [+ Protocol if known]). Example: "Authenticates via OIDC"; "Uploads CSV for import"; "Reads/Writes domain data".
- Non-functional cues: Where relevant, reflect key QAs from the ADR (availability, latency) in brief notes, without turning the context view into a performance diagram.

Re-use exact names and terms from the container view (e.g., Shell, Backend API, Import/ETL Worker, Identity Provider (OIDC), Observability/Telemetry Platform, Feature Flag Service).

## Scope and Constraints
- Level: C4 Level 3 — Deployment.
- Focus: Runtime nodes, networking, observability, and scaling that instantiate the container model.
- Include: Regions/zones, K8s clusters or serverless functions, VMs; network segments (public/private), ingress/egress, API gateway, service mesh (if any); observability/logs/metrics/traces; autoscaling rules; data placement/replication and backup/restore notes.
- Keep infrastructure generic unless ADR states specific cloud/vendor; list unknowns under Assumptions/Unknowns.

## Output Format (exactly this order)

1) Deployment (text form)
	 - Runtime Nodes:
		 - Regions/Zones (e.g., eu-central-1 a/b) and purpose
		 - Compute: K8s cluster(s) or serverless runtimes or VMs; node pools/profiles where relevant
		 - Networking: public vs private subnets/segments; API Gateway/Ingress; Service Mesh (if any)
	 - Placement:
		 - Map each container to node(s) or serverless function(s); note sidecars/agents (e.g., telemetry, service mesh proxies)
	 - Edges:
		 - Ingress/Egress paths, north-south and east-west traffic; protocols; mTLS if applicable
	 - Observability:
		 - Logs, metrics, traces collection and export; health checks; SLO probes and synthetic tests
	 - Autoscaling:
		 - Metric(s), threshold, min/max replicas for each scalable workload (API, worker, etc.)
	 - Data:
		 - Data placement/replication (primary/replica, multi-AZ/region), backup/restore cadence and RPO/RTO notes

2) C4-PlantUML (Deployment)
	 - Provide a minimal PlantUML diagram using the C4-PlantUML syntax for Deployment level.
	 - Use `Deployment_Node` to model regions/zones, clusters/VMs, and runtime nodes; place containers (`Container`, `ContainerDb`) within nodes.
	 - Use `System_Ext` for external systems (IdP, Feature Flags, Observability, CSV Source, CDN) as appropriate.
	 - Label edges with purpose + protocol; mark async and mTLS where applicable.

3) SLOs & Capacity Assumptions
	 - Target p95 latency per interface; baseline RPS/events; expected concurrency; burst handling; any rate limits.

4) Risks & Mitigations (3 items)
	 - List three concrete risks and a short mitigation for each (e.g., zone outage, noisy neighbor, backpressure on imports).

5) Assumptions/Unknowns
	 - List any items not specified in the ADR/Container doc (cloud vendor, specific services, exact regions). Mark TBDs clearly.

## Derivation Guidance
- Instantiate the containers defined in `c4_container.md` onto runtime nodes. Typical mapping examples:
	- Shell + MFEs: Served from CDN/Edge; built artifacts stored in object storage or container image registry if SSR is used (if SSR not used, keep as static hosting via CDN).
	- Backend API: Deployed on K8s Deployment (HPA rules) or serverless HTTP function; behind API Gateway/Ingress.
	- Import/ETL Worker: K8s CronJob/Job or serverless function triggered by HTTP/queue; ensure idempotency and backoff.
	- Primary Database: Managed RDBMS with multi-AZ; backups (daily), PITR; encryption at rest; read replicas if permitted by ADR.
	- Observability: Agent/sidecar exporters; central telemetry backend (external system).
- Networking:
	- Public ingress via CDN/Edge and/or API Gateway to public endpoints; private east-west traffic inside cluster/VPC.
	- Consider service mesh for mTLS and retries if ADR indicates; otherwise, note as assumption.
- Non-functionals:
	- Reflect latency target (<300ms API), throughput (RPS/events), availability target, and data residency/compliance (e.g., GDPR) at relevant nodes and edges.

## Acceptance Checklist (internal; do not print these labels in the output)
- Names match the vocabulary of `c4_container.md` and ADR.
- Runtime nodes, networks, scaling, and observability are present.
- Autoscaling rules and data placement/replication/backup are specified.
- C4-PlantUML Deployment diagram present and valid.
- SLOs & Capacity Assumptions present.
- Exactly three Risks & Mitigations.
- Assumptions/Unknowns capture gaps.

---

## Example structure note (do not copy verbatim; derive from ADR vocabulary)

### Deployment (text form)
- Regions/Zones: EU Central (2 AZs) — primary; optional DR region (warm standby, TBD).
- K8s Cluster (prod):
	- Public Subnet: Ingress Controller + API Gateway; exposes Backend API.
	- Private Subnet: Backend API Deployment (min 3, max 12; HPA on CPU 70% or p95 latency > 250ms), Import/ETL Worker Job/CronJob.
	- Sidecars: Telemetry exporter (logs/metrics/traces), optional service mesh proxy for mTLS/retries.
- CDN/Edge: Serves Shell/MFEs static assets; health checks from multiple PoPs.
- Primary Database (RDBMS managed): Multi-AZ; daily full backups + PITR; encryption at rest; read replica (optional, read-only).
- Observability Platform (external): Receives logs/metrics/traces; SLO probes from multiple regions.
- Feature Flag Service (external): Evaluations from UI and API over HTTPS.

Edges
- Internet -> CDN -> Shell assets (HTTPS)
- Internet -> API Gateway/Ingress -> Backend API (HTTPS, mTLS to services if mesh)
- Backend API -> Primary DB (private, TLS)
- Backend API -> Observability (async exporters)
- Import/ETL Worker -> Backend API (HTTPS) -> DB (writes via API)

Autoscaling
- Backend API: HPA on CPU 70% or p95 latency > 250ms; min 3, max 12 replicas
- Import/ETL Worker: Concurrency limited to 2 jobs; backoff and dead-letter on repeated failure

Data
- DB: Multi-AZ synchronous replication; daily backups, PITR; restore test quarterly (TBD)

### C4-PlantUML (Deployment)
```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Deployment.puml

Deployment_Node(region, "EU Central", "Region") {
	Deployment_Node(az, "AZ a/b", "Zones") {
		Deployment_Node(cluster, "K8s Cluster (prod)", "Kubernetes") {
			Deployment_Node(public, "Public Subnet", "Network") {
				Container(ing, "API Gateway/Ingress", "Nginx/Envoy", "Public HTTPS entrypoint")
			}
			Deployment_Node(private, "Private Subnet", "Network") {
				Container(api, "Backend API", "Container", "HTTP REST/JSON")
				Container(worker, "Import/ETL Worker", "Job/CronJob", "CSV import")
			}
		}
	}
}

Deployment_Node(cdn, "CDN/Edge", "Edge") {
	Container(shell, "Shell & MFEs", "Static assets", "SPA")
}

Deployment_Node(dbnode, "Managed RDBMS (Multi-AZ)", "Database") {
	ContainerDb(db, "Primary Database", "RDBMS", "Core aggregates")
}

System_Ext(obs, "Observability/Telemetry Platform", "Logs/metrics/traces")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(ff, "Feature Flag Service", "Feature toggles")
System_Ext(csv, "External CSV Source", "Uploads CSV")

Rel(shell, ing, "HTTPS")
Rel(ing, api, "HTTP(S)")
Rel(api, db, "TLS/private network")
Rel(worker, api, "HTTPS")
Rel(api, obs, "Logs/metrics/traces (async)")
Rel(worker, obs, "Job metrics/logs (async)")
Rel(api, idp, "OIDC/JWT validation")
Rel(shell, ff, "HTTPS")
Rel(api, ff, "HTTPS")
Rel(csv, ing, "HTTPS CSV upload")

@enduml
```

### SLOs & Capacity Assumptions
- p95 latency: API < 300 ms, UI static asset TTFB < 100 ms via CDN
- Baseline throughput: 50–200 RPS API; events: N/A (no internal broker)
- Concurrency: 500–2,000 concurrent users; import jobs 1–2 concurrent with backoff

### Risks & Mitigations
- Zone outage: Multi-AZ DB and cluster; rollout strategy with surge; restore runbook
- Traffic spikes: CDN caching + HPA on API latency; rate limiting at ingress
- Import backpressure: Job concurrency caps; DLQ and retry with exponential backoff

### Assumptions/Unknowns
- Cloud/vendor and exact regions not specified; use EU region for GDPR as baseline.
- Service mesh is optional; if adopted, enforce mTLS and retries; otherwise, rely on gateway timeouts/retries.
- Specific metrics for HPA (CPU vs custom latency SLI) TBD.
