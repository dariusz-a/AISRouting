# OCR Drafting

## Task: 

Create a folder named docs/tech_design/architecture (if it doesn't already exist).

You MUST Write a detailed, comprehensive, Objectives–Constraints–Risks (OCR)  document in `docs/tech_design/architecture/ocr.md`

You MUST update any existing files with the same name.

Use my domain brief to:
1) Extract candidate Objectives, Constraints, and Risks. 
2) Propose measurable Quality Attribute targets (p95/p99 latency, availability, cost envelope, data consistency, etc.). 
3) Identify top 5 risks with brief mitigations. 
4) List assumptions and open questions.

## Ground rules:
- Prefer explicit, measurable quality attributes.
- Call out contradictions, missing data, or risky assumptions.
- Keep a running “assumptions & open questions” list whenever needed.

## Input Sources


### Feature Specification

- Location: `docs/tech_design/architecture/inputs/Functionality.md`
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

### Project Design Documents

1. `docs/tech_design/architecture/inputs/Overall_architecture.md`
   - Technology stack details
   - System architecture
   - Key architectural decisions

2. `docs/tech_design/architecture/inputs/Risks.md`
   - Lists the identified risks
   - Risk | Likelihood | Impact | Early indicator | Mitigation | Residual

3. `docs/tech_design/architecture/inputs/Constraints.md`
   - Lists the limitations and rules that a system's design must adhere to


## Output Requirements:


### **CRITICAL: Focus on Completeness, Clarity and Alignment**

Readiness Check:
- Completeness 
    - Objectives include at least 3 measurable QAs with concrete targets.
    - Constraints list includes platform, integration, and compliance items.
    - Risks section includes ≥ 5 risks with a one-line mitigation each.

- Clarity
    - Language is specific; avoids vague terms (“fast”, “robust”) without numbers.
    - Assumptions are explicit; unknowns captured in Open Questions.

- Alignment
    - OCR reflects the scenario’s domain realities (e.g., Kafka constraints for event-driven).
    - No contradictions across Objectives, Constraints, Risks.


### **Document Structure Requirements**

The design document MUST follow this structure:

```markdown
# OCR Worksheet (v1)

```

The document MUST be structured with the following sections:

## Objectives (top-level goals & measurable QAs)
- Primary Objective:
- Secondary Objectives:
- Quality Attributes (with targets): 
  - Latency:
  - Availability:
  - Throughput:
  - Data consistency:
  - Security:
  - Compliance:
  - Cost envelope:

## Constraints (non-negotiables / givens)
- Platform/Infra:
- Integrations & protocols:
- Compliance & audit:
- Data & state constraints:
- Tooling/observability:

## Risks
- Top risks (5):
| Risk | Likelihood | Impact | Early indicator | Mitigation | Residual |
|---|---|---|---|---|---|

## Assumptions & Open Questions
- Key assumptions (3–5):
- Open questions (clarifications needed):
