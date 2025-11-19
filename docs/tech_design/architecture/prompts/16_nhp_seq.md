# Generate and Analyze a Non-Happy Path (NHP) Sequence

# Role
You are my architectural resilience modeler.

## Inputs

### Feature Specification

- Location: `docs/tech_design/architecture/inputs/Functionality.md`
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

### DesignDoc v0.9 (for SLO normalization)
- location: `docs/tech_design/architecture/design_doc_v1_0.md`

### OCR Worksheet
- Location: `docs/tech_design/architecture/ocr.md`
  - Objectives (top-level goals & measurable QAs)
  - Constraints (non-negotiables / givens)
  - Risks & Assumptions
    - Risk | Likelihood | Impact | Early indicator | Mitigation | Residual

### Selected critical flow
- Location: `docs/tech_design/architecture/inputs/Selected_critical_flow.md`
- A flow that would severely affect reliability or business continuity if it failed

### Architecture Vocabulary (for participant names)
- C4 Container: `docs/tech_design/architecture/c4_container.md`
- C4 Component (optional, if present for selected container): any `docs/tech_design/architecture/c4_component_*.md`
- ADR v1: `docs/tech_design/architecture/ADR_001.md`

If your environment supports writing files to the repository, write the result to `docs/tech_design/architecture/nhp_sequence_<flow_slug>.md` (overwrite if it exists), where `flow_slug` is a kebab-case version of the selected flow name. Otherwise, return the full Markdown content and explicitly state the intended path.

## Task

1. Given the selected critical describe a plausible failure event (e.g., dependency outage, timeout, schema mismatch, partition, etc.).

2. Generate a Non-Happy Path (NHP) version of that flow as a C4-PlantUML sequence diagram, explicitly showing:

- Where the fault occurs.

- Which components retry, back off, or compensate.

- Any idempotency, DLQ, circuit breaker, or fallback logic.

- When the client receives feedback (error or async confirmation).

3. After the diagram, produce a structured analysis covering:

- Failure point: what broke and why.

- Detection: how it’s observed (metric, trace, alert).

- Recovery mechanism: retry, replay, compensation, failover, etc.

- Residual risk: what remains exposed after mitigation.

- Improvement hint: small design or observability enhancement that could reduce MTTD or MTTR.

## Behavior
- Determine the flow name from `Selected_critical_flow.md` (use first Markdown heading or explicit name within). If missing, use the first Feature header in `inputs/Functionality.md`. Generate `flow_slug` from the flow name (kebab-case).
- Choose a plausible failure event aligned with OCR Risks and the selected flow (e.g., IdP latency, DB outage/replication lag, CSV import validation error, gateway timeout). Prefer the highest Impact/Likelihood risks.
- Use exact container/component vocabulary from `c4_container.md` and component docs for sequence participants (e.g., Shell, Backend API, Import/ETL Worker, Primary Database, Identity Provider (OIDC), API Gateway/Ingress if relevant).
- Reference SLOs from Section 6 of `design_doc_v1_0.md` to set retry/backoff windows, timeouts, and acceptance criteria in notes.
- Sequence must explicitly show: fault location; retries/backoff; circuit breaker and/or DLQ; idempotency on consumer side; and client-facing feedback (error or async acceptance).
- After the diagram, output the Failure Analysis table exactly as defined below, then the Follow-up paragraph with 3 concrete, testable assertions.
- Write the complete output to `docs/tech_design/architecture/nhp_sequence_<flow_slug>.md` or return it with intended path if writing is not possible.

## Output Format (exactly this order)

### Non-Happy Path Sequence: [flow name]

Output as a C4-PlantUML sequence diagram written using the C4 sequence syntax that fits within the C4-PlantUML model conventions (System, Container, Component, etc.). Do not copy the example verbatim; instantiate with your selected flow and architecture names.

```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Sequence.puml

title Message Flow: Client → API Gateway → Services → Queue

actor Client as C
Container(ApiGateway, "API Gateway")
Container(ServiceA, "Service A")
Container(MessageQueue, "Message Queue")
Container(ServiceB, "Service B")

C -> ApiGateway : [request/action]
ApiGateway -> ServiceA : [call]
ServiceA -> MessageQueue : [publish event fails or delays]

note over ServiceA, MessageQueue
Retry 3x with exponential backoff
Then send to Dead Letter Queue (DLQ)
end note

MessageQueue -> ServiceB : [replay later]
ServiceB -> ServiceB : [apply idempotency check]
ServiceB -> ApiGateway : [ack]
ApiGateway -> C : [accepted / pending notice]

@enduml
```

### Failure Analysis

Output as a Markdown table in this exact structure:

| Dimension | Description |
|---|---|
| Failure point | {{what broke and why}} |
| Detection | {{how it's observed — metric, trace, alert}} |
| Recovery mechanism | {{retry, replay, compensation, failover}} |
| Residual risk | {{what remains exposed after mitigation}} |
| Improvement hint | {{small design or observability enhancement}} |

 
**Tone:** concise, technical, traceable — suitable for embedding in a Technical Design Document or linking to the ADR’s “Consequences” section.  
 
### Follow-up  
A short paragraph summarizing what this NHP sequence *reveals* about resilience trade-offs (e.g., consistency vs. availability, complexity vs. recovery time) and 3 assertions we can automatically test.

## Acceptance Checklist (internal; do not print these labels in the output)
- Uses participant names from C4 docs; aligns with ADR vocabulary
- Diagram uses `C4_Sequence.puml` and marks: fault, retries/backoff, circuit breaker/DLQ, idempotency, and client feedback
- Failure Analysis table present with all 5 rows; no placeholders remain
- Follow-up paragraph present with 3 explicit, testable assertions
- Output written to `docs/tech_design/architecture/nhp_sequence_<flow_slug>.md` or returned with path if not writable

## Derivation Guidance
- Prefer failure events already listed in OCR Risks that intersect the selected flow (e.g., import job failures, IdP outages, DB write contention).
- Set retry/backoff based on Section 6 SLOs (e.g., p95 ≤ 250 ms API; retries ≤ 3 with exponential backoff, circuit break after 30 s).
- For imports, show idempotent upserts and DLQ with replay; for auth, show cached JWKs and clear 401 to client after timeout.
- In Detection, reference concrete metrics/traces (e.g., http_request_duration_seconds, error_rate, db_replication_lag_seconds, job_duration_seconds, otel span attributes) and alerts.

