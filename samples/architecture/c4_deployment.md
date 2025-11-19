# C4 — Deployment View: Knowledge Accounting Platform

1) Deployment (text form)

- Runtime Nodes
  - Regions/Zones
    - EU Region (GDPR-compliant), Multi-AZ (a/b): Primary production footprint to keep data in-region and meet GDPR. Optional DR region (warm standby) TBD.
  - Compute
    - Kubernetes Cluster (prod): Multi-AZ worker nodes.
      - Public segment: API Gateway/Ingress Controller (e.g., NGINX/Envoy).
      - Private segment: Application workloads and data access (no public IPs).
      - Node pools: General-purpose pool for stateless services (Backend API); batch pool for jobs (Import/ETL Worker).
    - CDN/Edge Delivery: Global edge for static assets (Shell and Capability MFEs).
    - Managed RDBMS (Primary Database): Multi-AZ managed relational database service.
  - Networking
    - Public vs Private subnets: North-south via CDN and API Gateway; east-west inside cluster VPC only.
    - Optional service mesh: If adopted, enforce mTLS, retries, and per-route timeouts (assumed optional; TBD).

- Placement
  - Shell: Built static assets served via CDN/Edge Delivery (immutable, versioned). No server-side state.
  - Capability MFEs: Static assets delivered via CDN/Edge and composed by Shell at runtime (module federation/SPA composition).
  - Backend API: Kubernetes Deployment in private segment; exposes HTTPS via API Gateway/Ingress. Sidecars/agents: telemetry exporter; mesh proxy (if mesh enabled).
  - Import/ETL Worker: Kubernetes Job/CronJob in private segment; triggered by CSV import workflow; writes via Backend API only (Anti-Corruption Layer).
  - Primary Database (RDBMS): Managed service, private networking, TLS in transit, encryption at rest; single writer pattern via Backend API.
  - External Systems: Identity Provider (OIDC), Feature Flag Service, Observability/Telemetry Platform, External CSV Source, CDN/Edge Delivery.

- Edges
  - Internet → CDN/Edge Delivery → Shell & Capability MFEs (HTTPS). Static cache with short TTLs and immutable asset hashing.
  - Internet → API Gateway/Ingress → Backend API (HTTPS; mTLS east-west if service mesh enabled).
  - Backend API → Primary Database (private network, TLS; least-privilege DB user; single-writer).
  - Shell/Capability MFEs → Backend API (HTTPS REST/JSON via typed SDK; JWT in Authorization header).
  - Shell ↔ Identity Provider (OIDC) (Auth redirects + token exchange via HTTPS/OIDC).
  - Backend API → Identity Provider (OIDC) (JWT validation/keys via HTTPS/JWKs).
  - Shell → Feature Flag Service (HTTPS evaluations; client SDK; cache + bootstrap flags to minimize latency).
  - Backend API → Feature Flag Service (optional server-side evaluations over HTTPS).
  - External CSV Source → API Gateway/Ingress → Backend API (HTTPS file upload; size limits, AV scan optional).
  - Workloads → Observability/Telemetry Platform (async export of logs/metrics/traces via OTLP/HTTPS; sampling enabled).

- Observability
  - Collection: Structured logs, metrics, and distributed traces from Backend API and Import/ETL Worker; web-vitals from Shell/MFEs.
  - Export: Sidecar/agent exports to Observability/Telemetry Platform (OTLP/HTTPS). Retention and sampling policies tuned per environment.
  - Health checks: Liveness/readiness probes on API and Worker; Ingress/Gateway health; synthetic checks for core APIs from multiple regions.
  - SLO probes: API p95 latency and availability dashboards; import job duration and success rate; CDN TTFB for key assets.

- Autoscaling
  - Backend API: HPA on CPU 70% and/or custom metric p95 latency > 250 ms for 3 min; min 3, max 12 replicas; pod disruption budgets (maxUnavailable=1).
  - Import/ETL Worker: Concurrency limited via Job parallelism=2; backoffLimit=6 with exponential backoff; rate-limit import endpoint at Ingress.
  - Ingress/Gateway: Connection and RPS limits; burst handling with queueing and 429s. CDN handles edge burst via caching.
  - Database: Vertical scaling with read replica optional for read-heavy scenarios (read-only endpoints behind API, if enabled; TBD).

- Data
  - Placement/Replication: Managed RDBMS with multi-AZ synchronous replication for high availability.
  - Backup/Restore: Daily full backups + PITR (point-in-time recovery). Quarterly restore drills (TBD). Encryption at rest enabled.
  - Residency/Compliance: EU region for GDPR; PII minimization in line with OCR. Access controls and auditing at DB and API layers.

2) C4-PlantUML (Deployment)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Deployment.puml

Deployment_Node(region, "EU Region (GDPR)", "Region") {
  Deployment_Node(az, "Multi-AZ (a/b)", "Zones") {
    Deployment_Node(cluster, "K8s Cluster (prod)", "Kubernetes") {
      Deployment_Node(public, "Public Segment", "Network") {
        Container(ing, "API Gateway/Ingress", "Envoy/NGINX", "Public HTTPS entrypoint")
      }
      Deployment_Node(private, "Private Segment", "Network") {
        Container(api, "Backend API", "Container", "HTTP REST/JSON; RBAC; SoT")
        Container(worker, "Import/ETL Worker", "Job/CronJob", "CSV validation/import via ACL")
      }
    }
  }
}

Deployment_Node(cdn, "CDN/Edge Delivery", "Edge") {
  Container(shell, "Shell", "Static assets", "Vue 3 SPA; routes MFEs; OIDC redirects")
  Container(mfes, "Capability MFEs", "Static assets", "Roles, Skills, People, Projects, Assessments UIs")
}

Deployment_Node(dbnode, "Managed RDBMS (Multi-AZ)", "Database") {
  ContainerDb(db, "Primary Database", "RDBMS", "Core aggregates and import artifacts")
}

System_Ext(obs, "Observability/Telemetry Platform", "Logs/metrics/traces")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP")
System_Ext(ff, "Feature Flag Service", "Feature toggles")
System_Ext(csv, "External CSV Source", "Uploads CSV files")

Rel(shell, ing, "Calls APIs", "HTTPS")
Rel(mfes, ing, "Calls APIs via Shell SDK", "HTTPS")
Rel(ing, api, "Routes to service", "HTTP(S)")
Rel(api, db, "Reads/Writes", "TLS/private network")
Rel(worker, api, "Commands for import", "HTTPS")
Rel(api, obs, "Logs/metrics/traces (async)")
Rel(worker, obs, "Job metrics/logs (async)")
Rel(shell, idp, "OIDC redirects/token exchange", "HTTPS/OIDC")
Rel(api, idp, "Validate JWT/claims", "HTTPS/JWKs")
Rel(shell, ff, "Client flag evaluation", "HTTPS")
Rel(api, ff, "Server-side flag evaluation (opt)", "HTTPS")
Rel(csv, ing, "Upload CSV for import", "HTTPS")

@enduml
```

3) SLOs & Capacity Assumptions

- Latency targets (p95)
  - Backend API: ≤ 300 ms for capability CRUD/queries; import control-plane endpoints ≤ 400 ms.
  - Shell/MFEs static assets via CDN: TTFB ≤ 100 ms; FCP ≤ 1.8 s; TTI ≤ 2.5 s (per OCR).
- Throughput & Concurrency
  - Baseline API throughput: 50–200 RPS steady; bursts 500 RPS for 5 min sustained.
  - Concurrent users: 500–2,000 active; CDN offloads static traffic.
  - Import: single file up to 5,000 rows completes ≤ 10 s p95; worker parallelism ≤ 2 to protect DB.
- Rate limits & backpressure
  - Ingress rate limiting per client and per route; 429 + Retry-After on overload.
  - Import endpoints gated by queue depth; exponential backoff on retries.

4) Risks & Mitigations (3 items)

- Zone/Node outage impacts API availability
  - Mitigation: Multi-AZ cluster and DB; min replicas ≥ 3; PDBs; readiness gates; runbook for failover.
- Traffic spikes cause elevated latency and timeouts
  - Mitigation: CDN caching for assets; HPA on CPU/latency for API; ingress rate limits; circuit breakers and timeouts.
- Import workload backpressure saturates DB
  - Mitigation: Limit worker parallelism; idempotent upserts; staged validation; queue depth alarms; DLQ/quarantine for poison data.

5) Assumptions/Unknowns

- Cloud vendor, exact regions, and managed DB engine are not specified (TBD). EU region chosen to satisfy GDPR by default.
- Service mesh adoption is optional (TBD). If enabled, all east-west traffic uses mTLS and retries.
- Custom HPA metrics (latency SLI) and thresholds may require telemetry integration (TBD); CPU-based fallback always available.
- Feature Flag Service and Observability/Telemetry Platform vendors are not specified (TBD).
- Current prototype scope in OCR is client-side only for Manage Roles; this deployment reflects the target platform in the Container view.
