# C4 System Context Diagram Generator

## Role
You are an experienced software architect who writes concise, high-signal diagrams and documentation.

## Task
Produce a concise C4 System Context view for the Knowledge Accounting system as text, derived strictly from the selected ADR and Feature Specification.

Keep the diagram focused and readable for architect stakeholders.

If your environment supports writing files to the repository, create the folder `docs/tech_design/architecture` (if it doesn't already exist) and write a detailed, comprehensive Selected Winner ADR to `docs/tech_design/architecture/c4_context.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path `docs/tech_design/architecture/c4_context.md` so it can be saved manually.

## Input Source

### ADR
- Location:  `docs/tech_design/architecture/ADR_001.md` 
- authoritative for architectural decisions, external dependencies, and quality attributes

### Feature Specification

- Location: `docs/tech_design/architecture/inputs/Functionality.md`
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

If any element is not explicitly supported by the ADR, do not invent it. Use a short "Assumptions/Unknowns" section to list gaps.

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

## Output Format (exactly this order)

1) Plain-text summary (concise)
	 - People
	 - Software Systems
	 - Relationships (A -> B: label)

2) C4-PlantUML (Context)
	 - Provide a minimal PlantUML diagram using the C4-PlantUML syntax for Context level.
	 - Use `Person`, `System`, `System_Ext` as appropriate. Keep IDs short and readable.

3) Assumptions/Unknowns
	 - List any items that were not explicitly stated in the ADR and that you inferred or left TBD.

## Derivation Guidance (from ADR)
- System under design: Treat the entire product as one "Software System" for this level (e.g., "Knowledge Accounting Platform" or ADR’s exact product name).
- External systems likely present in ADR: Identity Provider (OIDC), CDN/Edge delivery, external CSV sources (e.g., HR system or file provider), Observability/Telemetry backend, Feature Flag service (if external). Include only those clearly supported by the ADR; otherwise put into Assumptions/Unknowns.
- Actors: Include at least the primary end user; if ADR indicates governance/administrative roles (e.g., Org Admin), include them as people actors if clearly implied.
- Relationships: Keep labels succinct and aligned with ADR claims (e.g., OIDC auth, CSV import, API usage, governance/RBAC).

## Acceptance Checklist (internal; do not print these labels in the output)
- 6–10 total elements.
- No containers/components.
- Names and relationships traceable to ADR_001.md.
- C4-PlantUML is present and valid at Context level.
- Brief Assumptions/Unknowns included for any gaps.

---

## Example structure note (do not copy verbatim; derive from ADR)

Plain-text summary
- People: End User, Org Admin
- Systems: Knowledge Accounting Platform (SUD), Identity Provider (OIDC), External CSV Source, Observability Platform
- Relationships: End User -> SUD: Uses; SUD -> IdP: Authenticates via OIDC; Org Admin -> SUD: Configures governance; External CSV Source -> SUD: Provides CSV for import; SUD -> Observability Platform: Sends telemetry

C4-PlantUML
```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

Person(user, "End User", "Primary user of the web application")
Person(admin, "Org Admin", "Configures governance and roles")

System(sud, "Knowledge Accounting Platform", "System under design")
System_Ext(idp, "Identity Provider (OIDC)", "External IdP for authentication")
System_Ext(csv, "External CSV Source", "Provides CSV files for import")
System_Ext(obs, "Observability Platform", "Telemetry and monitoring backend")

Rel(user, sud, "Uses")
Rel(admin, sud, "Configures governance/RBAC")
Rel(sud, idp, "Authenticates via OIDC")
Rel(csv, sud, "Provides CSV for import")
Rel(sud, obs, "Sends telemetry")

@enduml
```

Assumptions/Unknowns
- Replace actor/system names and relationship labels with those explicitly present in ADR_001.md; remove any not supported by the ADR.
- If CSV source, observability, or feature flag services are not in ADR_001.md, omit them and add a short note here.
