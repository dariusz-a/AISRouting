# C4 System Context — Knowledge Accounting

## 1) Plain-text summary

- People
  - Administrator — manages roles/skills/governance (RBAC)

- Software Systems
  - Knowledge Accounting Platform (System under design)
  - Identity Provider (OIDC)
  - CDN/Edge Delivery
  - External CSV Source (for import)
  - Observability/Telemetry Platform
  - Feature Flag Service

- Relationships
  - Administrator -> Knowledge Accounting Platform: Manages roles/skills and governance (RBAC)
  - Knowledge Accounting Platform -> Identity Provider (OIDC): Authenticates via OIDC
  - Knowledge Accounting Platform -> CDN/Edge Delivery: Serves shell and MFEs via CDN
  - External CSV Source -> Knowledge Accounting Platform: Provides CSV for import
  - Knowledge Accounting Platform -> Observability/Telemetry Platform: Sends telemetry and SLO metrics
  - Knowledge Accounting Platform -> Feature Flag Service: Evaluates feature flags for rollout

Notes (QAs): FCP ≤ 1.5s p95; route/CRUD ≤ 250ms p95; Availability ≥ 99.95% via CDN + HA API; backend is system of record with short-lived caches and revalidation ≤ 1s p95.

## 2) C4-PlantUML (Context)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

Person(admin, "Administrator", "Manages roles/skills and governance (RBAC)")

System(sud, "Knowledge Accounting Platform", "Shell + capability-aligned MFEs; backend API is SoT")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP for authentication")
System_Ext(cdn, "CDN/Edge Delivery", "Delivers shell and MFEs with high availability")
System_Ext(csv, "External CSV Source", "Provides CSV files for data import")
System_Ext(obs, "Observability/Telemetry Platform", "Logs, metrics, traces and SLO dashboards")
System_Ext(ff, "Feature Flag Service", "Controls rollout and canary per MFE")

Rel(admin, sud, "Manages roles/skills and governance")
Rel(sud, idp, "Authenticates via OIDC")
Rel(sud, cdn, "Serves shell/MFEs via CDN")
Rel(csv, sud, "Provides CSV for import")
Rel(sud, obs, "Sends telemetry and SLO metrics")
Rel(sud, ff, "Evaluates feature flags")

SHOW_LEGEND()
@enduml
```

## 3) Assumptions/Unknowns

- Exact product/system name isn’t explicitly fixed; using “Knowledge Accounting Platform” per ADR/repo.
- Identity Provider vendor is not chosen; ADR specifies OIDC only.
- Observability backend/tooling and Feature Flag provider are unnamed; ADR specifies capabilities, not brands.
- CSV source is implied by import flows; authoritative provider, schema, and encoding remain open (ADR Open Questions).
- Tenant model and uniqueness rules are TBD; diagram is tenant-agnostic.
- Current scope (OCR) allows client-only for Manage Roles; ADR positions backend API as SoT for evolution—context view reflects that future posture without modeling containers.
