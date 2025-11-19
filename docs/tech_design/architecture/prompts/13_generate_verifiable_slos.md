# Generate LLM-Verifiable Constraints (SLOs, Throughput, Scaling Triggers)


## Role
You are my architecture validation assistant.
Your goal is to derive explicit, verifiable non-functional constraints from my architecture.

# Context
Below are my architecture’s objectives, constraints, and design decisions.
You must translate them into *LLM-verifiable* quantitative targets
for performance, capacity, and scaling behavior.

## Inputs (read from repository)
- OCR sheet (Objectives, Constraints, Risks): `docs/tech_design/architecture/ocr.md`
- ADR v1 (context + decision + consequences): `docs/tech_design/architecture/ADR_001.md`
- DesignDoc v0.9 (to update Section 6): `docs/tech_design/architecture/design_doc_v1_0.md`
- Deployment topology (Section 5 source): `docs/tech_design/architecture/c4_deployment.md` (if present)
- C4 Container and Context (for vocabulary alignment):
   - `docs/tech_design/architecture/c4_container.md`
   - `docs/tech_design/architecture/c4_context.md`

# Rules
1. Use measurable parameters only (no vague adjectives).
2. Each constraint must be testable from logs, metrics, or simulated scenarios.
3. Express constraints in JSON-like key/value format and also as readable statements.
4. Cover at least:
   - Latency and response time
   - Throughput or transactions per unit time
   - Availability or error budgets
   - Scaling triggers and thresholds
   - Resource ceilings (CPU, memory, I/O)
   - Data replication / recovery targets
   - Observability hooks or metrics
5. Include a section “Verification Methods” describing how AI or monitoring tools could test each constraint.

# Behavior
- Parse the referenced files to extract non-functional requirements (latency targets, throughput, availability, data residency/compliance) and deployment/scaling details.
- Derive explicit, measurable constraints. Where numbers are missing, calculate reasonable baselines from OCR (e.g., 10k/day ≈ 0.12 RPS average; state assumptions clearly) and mark TBDs.
- Replace the entire section starting at the heading `## 6. SLOs & Capacity Assumptions` up to (but not including) the next top-level heading `## 7.` in `docs/tech_design/architecture/design_doc_v1_0.md` with the generated output below. Preserve all other content in the file unchanged.
- If `## 7.` is not found, append the generated section at the end of the file after removing any existing Section 6 content.
- If your environment cannot write files, return the full generated Section 6 and explicitly state that it is intended to replace that section in `docs/tech_design/architecture/design_doc_v1_0.md`.

# Output Format
Produce the following sections and replace Section 6 in `docs/tech_design/architecture/design_doc_v1_0.md`.
---

## 6. SLOs & Capacity Assumptions
*(Derived from Objectives, Constraints, and Deployment design.)*

### 6.1 Purpose
This section translates non-functional constraints into **measurable, AI-verifiable targets**.
These serve as the basis for the next session’s stress-tests and policy checks.

### 6.2 Input References
- OCR Sheet: `docs/tech_design/architecture/ocr.md`
- ADR v1: `docs/tech_design/architecture/ADR_001.md`
- Deployment Topology: `docs/tech_design/architecture/c4_deployment.md`

---

### 6.3 Latency & Availability
| Service / API | Target | Metric | Verification Hook |
|----------------|---------|---------|--------------------|
| Example: OrderService.API | p95 < 300 ms; p99 < 500 ms | `http_request_duration_seconds` | synthetic probe + Prometheus query |
| Example: InventoryEvents | end-to-end < 1 s | `event_processing_duration_ms` | trace sampling via OpenTelemetry |

**Availability target:** 99.9% monthly (≤ 43 min downtime)

---

### 6.4 Throughput & Capacity
- Baseline load: <derive_from_OCR_or_Deployment> requests / s  
- Peak load: <derive> × baseline with ≤ <percent> latency degradation  
- Expected concurrency: <derive>

---

### 6.5 Scaling Triggers
| Metric | Threshold | Action | Max Replicas |
|---------|------------|---------|--------------|
| CPU usage | > 70% for 5 min | add 1 replica | 8 |
| Queue depth | > 100 messages | spawn async worker | 4 |

---

### 6.6 Resource & Data Limits
- Database size limit: <GB> GB; replication lag < <seconds> s  
- Retention policy: <days> days; RPO ≤ <minutes> min  
- RTO (target recovery time): <minutes> min

---

### 6.7 Observability Hooks
List the metrics, traces, and logs that validate or falsify constraints.  
Example:
```
- Metrics: Prometheus (latency_p95, error_rate)
- Traces: OpenTelemetry (span_duration_ms)
- Logs: healthcheck.log events, “SLO breach” markers
```

---

### 6.8 Verification Methods
Explain how these constraints can be **stress-tested by AI agents** or monitoring pipelines.

Example:
1. Simulate 5× traffic using a synthetic workload.  
2. Observe latency metrics vs. targets; autoscaling should trigger within 2 min.  
3. Verify no SLO breach events in logs during test window.

---

### 6.9 Summary Table
| Category | Target | Verification Metric | Expected Behavior |
|-----------|---------|--------------------|-------------------|
| Latency | p95 < 300 ms | Prometheus metric | No alerts |
| Availability | 99.9% | Uptime probe | ≤ 43 min downtime |
| Throughput | 10 k orders/day | RPS counter | stable latency |
| Scaling | CPU > 70% → new replica | HPA event log | scale within 2 min |

---

# Acceptance Checklist (internal; do not print these labels in the output)
- Uses exactly the vocabulary from ADR/C4 docs (system and container names)
- Constraints are numeric and testable from metrics/logs/traces or simulations
- Includes both JSON-like key/values and readable statements where applicable
- Scaling triggers include metric, threshold, action, and min/max safeguards
- Data replication and recovery include RPO/RTO and verification notes
- Writes updated Section 6 into `design_doc_v1_0.md` or returns it with intended path if write not possible

# Derivation Guidance
- Latency: Use ADR/OCR targets (e.g., API p95 ≤ 300 ms, UI FCP ≤ 1.8 s). If both exist, choose stricter; note exceptions per endpoint (e.g., import ≤ 400 ms).
- Throughput: Convert daily objectives (e.g., 10k/day) to RPS (avg) and define peak/burst multipliers based on Deployment autoscaling rules.
- Availability: Map OCR/ADR availability to monthly error budgets (e.g., 99.9% ≈ 43 min downtime/month).
- Scaling: Align with Deployment HPA thresholds (CPU %, p95 latency windows, queue depth) and specify min/max replicas.
- Resource caps: Reflect managed DB capabilities (storage GB, replication lag), cache limits, and worker concurrency from Deployment.
- Observability: Point to concrete metrics (Prometheus, OTEL spans) and log markers; add synthetic probes.








