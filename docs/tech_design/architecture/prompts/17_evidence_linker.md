# ADR Evidence Cross-Linker

## Role
You are my architecture documentation cross-linker. Your job is to add concrete links from ADR v1 sections to evidence in the design documents (diagrams, tables, and trade-off matrix).

## Inputs
- ADR v1 (target to update): `docs/tech_design/architecture/ADR_001.md`
- DesignDoc v0.9: `docs/tech_design/architecture/design_doc_v1_0.md`
- Trade-off matrix: `docs/tech_design/architecture/alt_arch_matrix.md`
- Risk & Mitigation Table: `docs/tech_design/architecture/risks_mitigation.md` (if present)
- Risk → Mitigation → SLO links: `docs/tech_design/architecture/risk_mitigation_slo_links.md` (if present)
- Optional sections in the design doc (reference only; do not edit them). Use section NAMES:
	- "C4 Context View", "C4 Container View", "Deployment Topology", "SLOs & Capacity Assumptions", "Risks & Mitigations", "Assumptions & Open Questions"

## Task
Cross-link ADR v1 sections to the most relevant evidence. Insert one compact “Evidence Links” line under each of these ADR sections: Context, Decision, Consequences, Alternatives.

Use section NAMES from the design doc when linking. Before writing links, open and scan the target files to confirm which of these section names actually exist; only include section names that exist. For trade-offs, reference the most relevant rows in the trade-off matrix by row number or the exact row key text from the first column.

## Behavior
1) Parse `ADR_001.md` to locate section headings:
	- `## **Context:**`
	- `## **Decision:**`
	- `## **Consequences:**`
	- `## **Alternatives Considered:**`

2) For each section, compose a single line immediately after the section heading using this exact format (keep brackets and arrows). Include 1–3 sections per line, but only those that actually exist in the target file:
	- Use section names exactly as they appear (do not invent new sections). Do NOT include placeholder anchors or fragments; only include section names that exist in `design_doc_v1_0.md`.
	- Links should display as `[design_doc_v1_0.md § <Section Name>]` and point to `design_doc_v1_0.md` (no `#...` fragment).
	- For Alternatives, select up to three rows from `alt_arch_matrix.md` that are most relevant to the selected decision (prefer rows that justify the chosen candidate, e.g., Consistency, Reliability/HA, Security/Compliance, Latency, Operability). Use the visible row numbers or the exact row key text from the first column as the identifier: `rows 3, 7, 9` or `rows Consistency, Reliability/HA, Security/Compliance`.
	- The "Output Example" below is illustrative only. Do not copy it verbatim; derive links and rows from the actual files you just scanned.

3) Keep all existing ADR content unchanged. Insert a new line "Evidence Links: …" directly after each section heading line. If an "Evidence Links:" line already exists under a section, replace it.

4) Write the modified ADR back to `docs/tech_design/architecture/ADR_001.md`. If your environment cannot write files, return the full updated ADR content and explicitly state the intended path.

5) Validation: Ensure every inserted link references section names that exist in the target document (the heading text must be present in `design_doc_v1_0.md`). Ensure each Alternatives row identifier exactly matches an existing row header (or a visible row number) in `alt_arch_matrix.md`.

## Output Example (do not copy, derive from files)

Evidence Links: Context → [design_doc_v1_0.md § C4 Context View], [design_doc_v1_0.md § SLOs & Capacity Assumptions], [design_doc_v1_0.md § Assumptions & Open Questions]

Evidence Links: Decision → [design_doc_v1_0.md § C4 Container View], [design_doc_v1_0.md § Deployment Topology]

Evidence Links: Consequences → [design_doc_v1_0.md § Risks & Mitigations], [design_doc_v1_0.md § SLOs & Capacity Assumptions]

Evidence Links: Alternatives → [alt_arch_matrix.md rows Consistency, Reliability/HA, Security/Compliance]

## Acceptance Checklist (internal; do not print these labels in the output)
- A single "Evidence Links:" line is present under each of the four ADR sections.
- Links only use the approved section names and relative repository paths shown above, and every section name included actually exists in `design_doc_v1_0.md`.
- Alternatives line cites up to three matrix rows relevant to the chosen decision, using exact row header text from the first column (e.g., "Consistency") or visible row numbers; each cited row exists in `alt_arch_matrix.md`.
- The "Output Example" was not copied verbatim; links and rows were derived from the repository files.
- All original ADR text remains unmodified aside from inserted/replaced evidence lines.
- Output is written back to `docs/tech_design/architecture/ADR_001.md` or returned with the intended path if not writable.

## Derivation Guidance
- To pick trade-off rows, scan `alt_arch_matrix.md` for the selected candidate described in ADR Decision (e.g., "Candidate B") and choose the three rows that best justify the decision (latency, operability, governance, etc.).
- If `risks_mitigation.md` exists, ensure the Consequences link to the "Risks & Mitigations" section reflects risks identified there. If `risk_mitigation_slo_links.md` exists, ensure the "SLOs & Capacity Assumptions" section mapping reflects the SLO summary in the design doc.
