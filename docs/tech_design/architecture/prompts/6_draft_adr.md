# Draft ADR Generator

## Role
You are an experienced software architect.

## Task
Produce a draft Architecture Decision Record (ADR) in Markdown that follows exactly the structure and bullets below.

If your environment supports writing files to the repository, create the folder `docs/tech_design/architecture` (if it doesn't already exist) and write a detailed, comprehensive Selected Winner ADR to `docs/tech_design/architecture/ADR_001.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path `docs/tech_design/architecture/ADR_001.md` so it can be saved manually.

## Rules
Keep each bullet clear and concise (prefer 1–2 sentences per bullet). Use ISO date format YYYY-MM-DD. If any input is unknown, write "TBD" and add it to Open Questions. Do not add extra sections or commentary outside the required structure.

## Input Sources

### OCR Worksheet
- Location: `docs/tech_design/architecture/ocr.md`
  - Objectives (top-level goals & measurable QAs)
  - Constraints (non-negotiables / givens)
  - Risks & Assumptions
    - Risk | Likelihood | Impact | Early indicator | Mitigation | Residual

### Alternative architectures

- Location: `docs/tech_design/architecture/alt_arch.md`
For each candidate:
1) Name + one-sentence core idea.
2) Decomposition approach (mono-core modular vs microservices per capability vs event-driven pipeline).
3) Data strategy (consistency model, storage choices).
4) Integration style (sync vs async; APIs, events).
5) Ops/observability posture.
6) QA posture with measurable claims (p95 latency, availability targets, throughput envelope).
7) Top 3 risks with mitigations.
8) When NOT to choose it.

### Alternative architectures — Trade-off Matrix

- Location: `docs/tech_design/architecture/alt_arch_matrix.md`
- Trade-off Matrix

  - A markdown table with the below structure:
    | Attribute | Candidate A | Candidate B | Candidate C |
    |---|---|---|---|

- Summary

  - A paragraph summarizing dominant trade-offs:
    - Candidate A
    - Candidate B
    - Candidate C

### Selected Provisional Winner

- Location: `docs/tech_design/architecture/selected_winner.md`
- Selected Winner — Architecture Trade‑off Decision

    - QA scoring (evidence‑based)
    - Where evidence is weak or missing
    - Recommendation (provisional)
    - Top 2 validation steps to de‑risk selected candidate
    - Appendix — Source summary excerpts

If any of the above source files are missing, use "TBD" placeholders and list the missing artifact under Open Questions.

## Output Requirements

### **CRITICAL: Focus on Completeness, Clarity and Alignment**

Use exactly the following Markdown structure (replace placeholders; keep bullets and indentation):

# ADR-001: {Decision Title}
- **Status:** Proposed
- **Date:** {YYYY-MM-DD}
## **Context:**  
- Business goals / scope  
- Key QAs & constraints (top 3 with targets)  
- Assumptions (knowns/unknowns)  
## **Decision:**  
- Selected alternative and short description  
- Key design elements (patterns, data/consistency mode, deployment posture)
## **Consequences:**  
- Positive outcomes (how it meets QAs)  
- Trade-offs / liabilities (new risks introduced)  
- Operability & observability notes (SLOs, telemetry, rollout)
## **Alternatives Considered:**  
- Alt-A: Why compelling; decisive reason rejected  
- Alt-B: Why compelling; decisive reason rejected
## **Links & Evidence:**  
- QA/Constraints  
- Trade-off matrix  
- Diagrams (C4/Deployment)  
- Risk table 
  - SLO draft
## **Open Questions:**  
- Q1, Q2, …

### Constraints and style
- Keep the content crisp and decision-focused; avoid narration.
- Prefer measurable targets (SLO/SLI) where possible.
- List concrete trade-offs and newly introduced risks, each with a brief mitigation idea where relevant.
- If ADR number is known, replace ADR-001 accordingly; otherwise keep ADR-001 as a draft identifier.

### Acceptance checklist (internal use; omit from output)
- Output matches headings and bullet order exactly.
- Top 3 QAs have measurable targets.
- At least 2 alternatives with decisive rejection reasons.
- 2 reasoning-trace items present and succinct.
- At least 3 links/evidence or "TBD" placeholders if not available.
- Open Questions include any TBDs.


