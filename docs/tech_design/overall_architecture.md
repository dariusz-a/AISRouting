This file describes the overall architecture and technology choices for the AISRouting project.

Technology stack details

- Frontend: TypeScript + React (or Vue) + Vite/CRA, React Router, Map library (Mapbox GL JS / Leaflet). UI tests use Playwright + TypeScript.
- Backend: Node.js (TypeScript) microservices with Express / Fastify. Optional Python services for heavy geospatial processing (e.g., route optimization).
- Data storage: PostgreSQL with PostGIS for spatial data (routes, waypoints), Redis for caching, S3-compatible object storage for large files (exported routes, logs).
- Messaging & Integration: Kafka or RabbitMQ for event streaming (route generation, background tasks). External integrations via REST and WebSocket for AIS live feeds.
- Deployment: Docker containers, Kubernetes for orchestration, CI/CD via GitHub Actions / Azure DevOps.
- Observability: Prometheus + Grafana metrics, ELK stack (Elasticsearch / Logstash / Kibana) for logs, distributed tracing via OpenTelemetry.

System architecture

- User clients (browser) -> CDN -> Frontend SPA
- Frontend calls Backend API Gateway (REST) for application data and actions
- API Gateway routes requests to microservices:
  - Auth Service (OAuth2 / OpenID Connect)
  - Routing Service (route compute, waypoint mapping)
  - Vessel Service (AIS feed ingestion and vessel state)
  - Data Service (Postgres/PostGIS CRUD)
  - Export Service (generates GPX/KML and stores to S3)
- Background workers (K8s Jobs or worker pool) process heavy tasks (route optimization, map matching) and publish events to messaging system.

Key architectural decisions

1. Use Postgres + PostGIS for primary persistence
   - Rationale: spatial queries, performance for geospatial indexes, known open-source tooling.
   - Cross-ref: data_models.md

2. Service-based architecture (microservices) with well-defined REST APIs
   - Rationale: separation of concerns, independent scaling (e.g., routing compute scaled separately).
   - Trade-offs: operational complexity vs. scaling benefits.

3. Caching layer with Redis
   - Rationale: accelerate repeated route lookups, reduce load on compute service.

4. Use JWT for inter-service auth and OAuth2/OIDC for user-facing authentication
   - Rationale: standard patterns for SSO and secure APIs. See security_architecture.md for details.

5. Progressive enhancement for offline/limited connectivity
   - Frontend stores temporary route drafts in localStorage; background sync to server when online.

Happy path and edge cases

- Happy path: Alice (planner, team: Marketing) logs in, selects vessel "MV Aurora", defines time range, generates a route, hits Export -> receives GPX in S3 and can download.
- Negative cases / edge cases:
  - AIS feed lag causes stale vessel positions -> routing service falls back to last-known position and notifies user.
  - PostGIS query failure -> system returns 503 and triggers background job to re-run route computation; retries with exponential backoff.
  - Large route export fails due to transient S3 error -> Export Service retries and logs failure to observability stack.

Cross references

- See data_models.md for storage model and relationships.
- See application_organization.md for project layout and code ownership.
- See security_architecture.md for authentication, authorization, and audit requirements.
