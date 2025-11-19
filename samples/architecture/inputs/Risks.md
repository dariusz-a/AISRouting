Risks to consider: cold starts, cache staleness, cross-region consistency, model latency, fallback strategies.
# Risks & Non-Happy Paths

- Potential retry storms if upstream failures persist.
- Duplicate order creation if idempotency not enforced.
- Kafka topic partitioning mismatch causing out-of-order retries.
- Misconfigured backoff leading to SLO violations.
- Increased complexity in tracing and logging retries.