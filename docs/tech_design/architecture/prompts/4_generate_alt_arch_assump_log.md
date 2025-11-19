# Build the Alternative Architectures — Assumptions & unknowns:

## Task: 

Create a folder named docs/tech_design/architecture (if it doesn't already exist).

You MUST Write a detailed, comprehensive, Alternative Architectures Trade-off  document in `docs/tech_design/architecture/alt_arch_assump_log.md`

You MUST update any existing files with the same name.

Extract assumptions and unknowns from each alternative. 
For each, add:
- Impact if wrong, 
- How to validate, 
- Suggested owner. 
- Priority

### Rules:
- Priority reflects impact × likelihood as currently understood. Revisit after initial spikes/POCs.
- Owners are accountable for planning and completing validation within their area.
- Where Unknown is marked, add a short spike and convert to either an accepted Assumption (with evidence) or a risk.


# Input Sources

### Alernative architecures

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

## Output Requirements:


### **CRITICAL: Focus on Completeness, Clarity and Alignment**

Readiness Check:
- Assumptions & Unknowns
    - Thorough, prioritized by impact, owners assigned.

### **Document Structure Requirements**

The design document MUST follow this structure:

```markdown
# Alternative Architectures — Assumptions & unknowns log

```

The document MUST be structured with the following sections:

## Assumptions & unknowns:

a markdown table with the bleo structure.

| Alternative | Assumption / Unknown | Type | Impact if wrong | How to validate | Suggested owner | Priority |
|---|---|---|---|---|---|---|

## Notes

