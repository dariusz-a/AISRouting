# Generate DesignDoc v0.9 (C4 + Deployment Topology)

# Role
You are my architectural design assistant. 
Your task is to produce the first version of a Technical Design Document (DesignDoc v0.9) 
that implements the architecture described in my ADR v1 and OCR sheet.

# Context
The architect (me) has already:
- Drafted ADR v1 (context, decision, alternatives, consequences)
- Defined Objectives, Constraints, and Risks (OCR sheet)
- Produced a trade-off matrix comparing candidate architectures

Your outputs must align EXACTLY with the terminology used in those inputs.

# Rules
- Reuse the exact names of systems, services, and components from ADR v1 and OCR.
- Use text-first diagram formats (Mermaid, PlantUML, or bullet “C4-as-text” form).
- Keep the document lightweight and self-contained — markdown sections only.
- Mark this as **DesignDoc v0.9 (pre–stress-test draft)**.
- At the end, include a “Linked Artifacts” section that points back to ADR v1.

If your environment supports writing files to the repository, write the final output to `docs/tech_design/architecture/design_doc_v1_0.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path.

# Inputs (read from repository)
- ADR v1: `docs/tech_design/architecture/ADR_001.md` (use the sections: Context, Decision, Consequences)
- OCR sheet: `docs/tech_design/architecture/ocr.md` (Objectives, Constraints, Risks)
- Trade-off matrix: `docs/tech_design/architecture/alt_arch_matrix.md` (extract the key comparison and winner rationale)
- C4 Views (for consistency and reuse of vocabulary):
	- Context: `docs/tech_design/architecture/c4_context.md`
	- Container: `docs/tech_design/architecture/c4_container.md`
	- Deployment: `docs/tech_design/architecture/c4_deployment.md` (if present)
	- Optional Component (for §4): any `docs/tech_design/architecture/c4_component_*.md` matching the most critical container (use if present; otherwise, skip §4)

Replace any template placeholders (e.g., `{{...}}`) with content extracted from these files. Do not invent new systems or names; if details are missing, add them to Assumptions/Open Questions.

# Output Structure
Generate the full text below and either write it to `docs/tech_design/architecture/design_doc_v1_0.md` or return the full Markdown with that intended path clearly stated.

``` markdown

# Design Document v0.9
Version: 0.9 (pre–stress-test)
Linked ADR: docs/tech_design/architecture/ADR_001.md

## 1. Overview
- Brief purpose of this document  
- Summary of selected architecture (1–2 sentences)

## 2. C4 Context View
Describe external actors, primary system, and neighboring systems.  

Format:
C4-Context:

Person: ...

System: ...

External System: ...

Rels: ...

QAs driving this view: ...

List up to 3 assumptions and 3 open questions.

## 3. C4 Container View
List all containers/services, their responsibilities, data ownership, and communication styles.
Include synchronous/async edges and data stores.
End with “Hotspots to validate” (coupling, consistency, availability).

## 4. (Optional) C4 Component View — Critical Service
Pick the most critical container and decompose it into internal components.
Show adapters, domain services, repositories, and key dependency directions.

## 5. Deployment Topology
Map the container view onto runtime infrastructure.
Include regions, clusters, network boundaries, observability hooks, and scaling rules.

Format example:

Deployment:

Region: ...

Nodes/Clusters: ...

Autoscaling: ...

Observability: ...

Data replication & backup: ...


## 6. SLOs & Capacity Assumptions
State numeric targets (latency, throughput, availability) and scaling triggers.
Explain how each target connects to constraints in the OCR sheet.

## 7. Risks & Mitigations
List 3–5 key risks discovered so far and proposed mitigations.
Tag each risk with its origin: (R) from OCR, (N) new found in design.

## 8. Assumptions & Open Questions
Capture unresolved uncertainties or dependencies on other teams/systems.

## 9. Linked Artifacts
- ADR v1: {{title or file link}}
- OCR sheet: {{reference}}
- Trade-off matrix: {{reference}}

```

# Acceptance Checklist (internal; do not print these labels in the output)
- Uses exact vocabulary from ADR_001.md, OCR, and C4 docs (no renames)
- Includes all sections 1–9 with concise, high-signal content
- Context/Container/Deployment sections align with existing C4 docs
- SLOs map back to OCR constraints (e.g., <300ms API latency, GDPR data residency)
- Linked Artifacts reference local file paths in `docs/tech_design/architecture`

# Generation Notes
- Prefer text-first (“C4-as-text”) for views; PlantUML optional but allowed.
- If Deployment is missing, include a short placeholder and add to Assumptions/Open Questions.
- For numeric capacity: derive baseline RPS from OCR (e.g., 10k daily ~ 0.12 RPS avg; state burst considerations) and tie autoscaling rules to SLOs.


