# Constraints

## Technical
- Must run within the existing Kubernetes cluster.
- Only approved data stores: PostgreSQL, Redis, Kafka.
- Must expose REST API; gRPC not permitted for this release.

## Performance
- Retry latency < 500ms (p95) under normal load.
- Must handle 1000 concurrent failed orders without degradation.

## Compliance
- Must log all retries for audit trail (GDPR retention 90 days).
- All services must use service mesh mTLS.

## Organizational
- Deployment frequency: once per sprint.
- Observability stack: Prometheus + Grafana only (no new tools allowed).
