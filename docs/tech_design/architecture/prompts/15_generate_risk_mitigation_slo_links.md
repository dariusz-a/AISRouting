# Generate Risk → Mitigation → SLO Cross-Links

## Role
You are my architecture observability planner.

## Input

### Markdown Risk & Mitigation Table
- location: `docs/tech_design/architecture/risks_mitigation.md`
- If the table contains more than 3 risks, select the top 3 by (Impact desc: High > Med > Low, then Likelihood desc), breaking ties by the order they appear. Preserve the original Risk text exactly.
 | Lens | Risk | Impact | Likelihood | Mitigation (Design) | Verification (Test) | Metric/SLO | Owner |
|------|------|---------|-------------|---------------------|---------------------|-------------|--------|
| Failure | [describe risk] | High/Med/Low | High/Med/Low | [mechanism or pattern] | [test or chaos check] | [numeric or qualitative metric] | [team/role] |
| Performance | ... | ... | ... | ... | ... | ... | ... |
| Evolution | ... | ... | ... | ... | ... | ... | ... |
| Policy | ... | ... | ... | ... | ... | ... | ... |

### DesignDoc v0.9 (for SLO normalization)
- location: `docs/tech_design/architecture/design_doc_v1_0.md`

### ADR v1 (for linked section reference)
- location: `docs/tech_design/architecture/ADR_001.md`

## Task

1. For each risk, create one traceability entry linking:

    - the Risk ID or short title,

    - its Mitigation mechanism, and

    - a corresponding SLO / metric / alert that proves the mitigation works.

2. Normalize the SLOs so each contains:

    - Metric name (e.g., latency, error rate, message loss, replay delay)

    - Target & window (e.g., P99 < 800 ms, 99.9 % success / 30 days)

    - Alert threshold & action (e.g., alert > 3 breaches / week → page platform team)

If your environment supports writing files to the repository, write the result to `docs/tech_design/architecture/risk_mitigation_slo_links.md` (overwrite if it exists). Otherwise, return the full Markdown content and explicitly state the intended path.

## Behavior
- Parse `risks_mitigation.md` to obtain candidate risks; if more than 3, select the top 3 as specified above.
- For each selected risk, derive the Mitigation from its "Mitigation (Design)" column.
- Normalize the Metric/SLO using Section 6 of `design_doc_v1_0.md`:
    - Include metric name, target, and window (e.g., p95_latency_ms < 300 over 30 days; availability ≥ 99.9% monthly).
    - If the risk’s Metric/SLO column specifies a different target and it conflicts with Section 6, choose the stricter (tighter) one and note that choice in the verification text.
- Define an Alert Threshold consistent with the SLO (e.g., 3 consecutive 5‑min breaches or error budget burn rate > 2.0 over 1h) and include a concrete action (page Owner or create incident).
- Set a Verification Method referencing either a chaos/synthetic test from the risk table or an SLO probe from Section 6.
- Set Linked ADR Section to the most relevant section anchor from `ADR_001.md` (e.g., #decision, #consequences, or the exact subsection title containing related terms); if ambiguous, use "ADR_001.md#consequences".
- Write only the Markdown table described below to `docs/tech_design/architecture/risk_mitigation_slo_links.md`. No extra prose.

# Output format

Output as a Markdown table in this exact structure:

| Risk | Mitigation | Metric / SLO | Alert Threshold | Verification Method | Linked ADR Section |
|------|-------------|--------------|-----------------|---------------------|--------------------|
| [risk title] | [mechanism] | [metric name + target] | [when to alert] | [test or telemetry check] | [ADR# or subsection] |

## Acceptance Checklist (internal; do not print these labels in the output)
- Exactly 3 rows (one per selected risk); header present; no extra text
- Mitigation reflects the table’s Mitigation column verbatim when meaningful; otherwise clarified minimally
- Metric/SLO normalized to Section 6 naming and includes metric name + target + window
- Alert Threshold includes clear condition and action (page/incident)
- Verification mentions specific test or telemetry query
- Linked ADR Section points to a concrete anchor or subsection in ADR_001.md

## Derivation Guidance
- Prefer SLOs already defined in `design_doc_v1_0.md` Section 6; if missing, derive from OCR/Deployment and mark with the strictest plausible target used elsewhere (e.g., API p95 < 300 ms).
- Typical metric names: http_request_duration_seconds (histogram), error_rate, availability_percent, queue_depth, db_replication_lag_seconds, job_duration_seconds.
- Alert patterns: 3× 5‑min window breach; burn rate > 2.0 over 1h; sustained queue depth above threshold for 10 min.
