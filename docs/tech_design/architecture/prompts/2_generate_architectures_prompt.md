# Generate materially different alternatives

## Task: 

Create a folder named docs/tech_design/architecture (if it doesn't already exist).

You MUST Write a detailed, comprehensive, Alternative Architectures  document in `docs/tech_design/architecture/alt_arch.md`

You MUST update any existing files with the same name.

Generate 3 **materially different** architecture candidates.

For each candidate:
1) Name + one-sentence core idea.
2) Decomposition approach (mono-core modular vs microservices per capability vs event-driven pipeline).
3) Data strategy (consistency model, storage choices).
4) Integration style (sync vs async; APIs, events).
5) Ops/observability posture.
6) QA posture with measurable claims (p95 latency, availability targets, throughput envelope).
7) Top 3 risks with mitigations.
8) When NOT to choose it.

Insist on non-trivial diversity. If two are too similar, replace one with a more divergent option.

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

2. `docs/tech_design/architecture/ocr.md`
   - Lists the identified risks
   - Risk | Likelihood | Impact | Early indicator | Mitigation | Residual


## Output Requirements:


### **CRITICAL: Focus on Completeness, Clarity and Alignment**

Readiness Check:
- Diversity of Alternatives
    - Strongly divergent, each fits different QA envelopes and delivery contexts.

- QA Alignment & Measurability
    - Comprehensive measurable targets; conflicts identified and addressed.

- Consistency & Coherence
    - Internally consistent; QA claims, topology, and constraints align.

### **Document Structure Requirements**

The design document MUST follow this structure:

```markdown
# Alternative architectures (v1)

```

Each candidate architecture MUST be placed under its own top-level section with a lettered heading:
- `## Candidate A`
- `## Candidate B`
- `## Candidate C`

Append the candidate name to the heading when available, e.g., `## Candidate A — Modular SPA`.

Each of the three sections MUST be structured with the following sections:

### Core idea
(1–2 sentences; how it meets the brief)

### Key design moves
- Decomposition: …
- Data: …
- Integration: …
- Ops/observability: …

### QA posture (targets/constraints)
- Latency (p95): …
- Scalability/throughput: …
- Consistency model: …
- Availability: …
- Compliance/Security: …

### Risks & mitigations
- Risk: … | Mitigation: …
- Risk: … | Mitigation: …

### When NOT to choose this
...
