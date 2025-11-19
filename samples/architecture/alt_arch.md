# Alternative architectures (v1)

## Candidate A — Modular SPA on Local Storage

### Core idea
A single Vue 3 + Vite SPA with a mono-core modular structure (domains as modules) using Pinia stores and a typed Local Storage wrapper as the authoritative store; optimized for simplicity and fast delivery while covering all Manage Roles scenarios.

### Key design moves
- Decomposition: Mono-core SPA with domain modules (auth, roles, skills, categories, people, projects). Shared UI via PrimeVue/Primer CSS. View → Service → Store enforced.
- Data: Strong consistency within-tab using Pinia; Local Storage as persistence; idempotent write APIs to prevent duplicates; cross-tab eventual consistency (≤ 1 s) via StorageEvent.
- Integration: None required initially; optional adapter layer to switch storage (LocalStorage → REST) without touching views.
- Ops/observability: Static hosting + CDN, Playwright E2E with traces on retry; console + Performance API marks for key flows; source maps; zero-backend ops.

### QA posture (targets/constraints)
- Latency (p95): open role modal/save/assign ≤ 200 ms (cold start ≤ 400 ms); list refresh/filter ≤ 150 ms for up to 5k roles and 50k links.
- Throughput/TPS: sustained ≥ 1.0 TPS (role mutations) p95 without long tasks (>50 ms) exceeding 5/min; burst ≥ 3 TPS for 30 s without frame drops > 5%.
- Bulk ops: CSV import of 5k rows completes ≤ 10 s p95 with atomic commit; CPU ≤ 70% and UI TTI degradation ≤ 200 ms during import.
- Consistency: Strong within-tab; cross-tab convergence ≤ 1 s p95 via StorageEvent; uniqueness enforced synchronously at write with idempotent keys.
- Availability/SLOs: 99.9% monthly for static hosting; offline within active session with read/write durability to Local Storage.
- RPO/RTO: RPO(in-tab) = 0 (writes persisted synchronously to Local Storage); RTO(reload/crash) ≤ 2 s to restore state; cross-tab visibility ≤ 1 s p95.
- Compliance/Security: Scope excludes PII; schema-level validation prevents PII fields. XSS-safe templates; content security checks in CI. GDPR purge removes all app-prefixed keys within ≤ 5 s and exports optional user-held backup.

### Risks & mitigations
- Risk: Local Storage quota (~5–10 MB) | Mitigation: compact schema, compress lists, export/clear fixtures, documented backend path.
- Risk: Cross-tab race conditions | Mitigation: idempotent writes; reconcile on focus; disable duplicate UI actions.
- Risk: Version drift of Vue/PrimeVue/Vite | Mitigation: pin versions, CI build + E2E; compatibility matrix.

### When NOT to choose this
- When multi-tenant, server-backed RBAC and audit trails are required immediately.
- When concurrent edits across many users must converge in real time.
- When role/skill catalogs exceed browser storage limits or require enterprise data governance now.

## Candidate B — Capability-aligned Micro Frontends (API-ready)

### Core idea
Split the UI into micro frontends per capability (Roles, Skills, People, Projects) composed in a shell; introduce a typed data-access SDK that targets REST today (mock via Local Storage) and a real API later.

### Key design moves
- Decomposition: Micro frontends (MFE) per capability, shared design system and router integration; contracts via TypeScript SDK; composition in shell.
- Data: SDK abstracts persistence; initial mock provider uses Local Storage; production provider targets REST/GraphQL; cache per MFE with stale-while-revalidate.
- Integration: Sync APIs (REST/GraphQL) with optimistic updates; events (CustomEvent or message bus) for cross-MFE notifications.
- Ops/observability: Independent build/deploy per MFE; shell manages routing and versioning; observability via structured logs, web-vitals, per-MFE feature flags.

### QA posture (targets/constraints)
- Latency (p95): shell boot ≤ 1.5 s FCP; MFE route change ≤ 250 ms; role ops ≤ 250 ms p95 (client ≤ 100 ms, network ≤ 150 ms budget).
- Throughput/TPS: UI targets ≥ 2 TPS sustained across MFEs (measured at API layer) with burst ≥ 5 TPS for 60 s p95; client batches coalesce to ≤ 1 request per 100 ms per MFE.
- Consistency: Strong within-view; backend is source of truth. SWR staleness window 1–5 s; cache revalidation p95 ≤ 1 s after mutation; cross-MFE event propagation ≤ 2 s p95.
- Availability/SLOs: 99.95% monthly with CDN + API HA; MFEs degrade independently with shell-level status banner.
- RPO/RTO: Backend RPO ≤ 60 s; Backend RTO ≤ 5 min for regional failover. Client-side RTO after API recovery ≤ 2 s to resync caches; token refresh drift ≤ 30 s.
- Compliance/Security: OIDC at shell; per-MFE RBAC via JWT/OAuth claims; no client-stored secrets. PII permitted only via backend with DPA; client logs redact PII 100%.

### Risks & mitigations
- Risk: Overhead/complexity for team size | Mitigation: Start as single repo with module federation-lite; promote to true MFEs when team grows.
- Risk: Contract drift between MFEs and API | Mitigation: Type-safe SDK, CI contract tests, semantic versioning.
- Risk: Latency regressions due to network | Mitigation: coalesce requests, batch mutations, HTTP/2, CDN caching.

### When NOT to choose this
- When the team is small and a single SPA suffices for near-term scope.
- When there is no backend on the horizon (you’ll pay complexity with little gain).
- When E2E test matrix and deployment complexity must remain minimal.

## Candidate C — Event-driven Offline-first Client with Sync

### Core idea
A client-side event log (append-only) powers state; views derive from a materialized cache. Background sync reconciles with a server (future) using deterministic reducers; built for conflict tolerance and offline-first workflows.

### Key design moves
- Decomposition: Mono-repo with domain event reducers; UI reads from materialized views; background worker for sync/replay.
- Data: Event store in IndexedDB (larger than Local Storage) with snapshots; CRDT-like merge for role/skill assignments; idempotent event application.
- Integration: Async sync channel (later WebSocket/HTTP) exchanging events; local-first commit with eventual remote reconciliation.
- Ops/observability: Event tracing with sequence numbers; debug timeline inspector; health marks for backlog size and replay time.

### QA posture (targets/constraints)
- Latency (p95): local commits ≤ 100 ms; modal open/assign flows ≤ 150 ms; initial materialization ≤ 500 ms after cold start.
- Throughput/EPS: sustained ≥ 3 EPS locally (events per second) with burst ≥ 20 EPS for 30 s p95 without UI jank; snapshot creation ≤ 200 ms p95 every 1k events.
- Replay/bulk: deterministic replay of 100k events ≤ 5 s p95 with periodic snapshots every 1k events; background compaction keeps DB size growth ≤ 5% per 10k events.
- Consistency: Strong local; background sync interval when online ≤ 2 s; remote propagation p95 ≤ 3 s; conflict resolution defined (LWW per field + provenance rules for category vs skill) with deterministic reducers and idempotent event application.
- Availability/SLOs: 99.9% monthly with fully offline operations; sync backlog survives reload; recovery from partial failures without user action.
- RPO/RTO: Local RPO = 0 (append-only); Local RTO ≤ 1.5 s to restore last snapshot after crash. Remote RPO ≤ 3 s under stable connectivity; Remote RTO after server recovery ≤ 10 s to full sync.
- Compliance/Security: No PII in event payloads; redact names by schema; optional at-rest encryption via WebCrypto (AES-GCM) with per-session keys; audit trail coverage = 100% of mutations.

### Risks & mitigations
- Risk: Complexity of event modeling and reconciliation | Mitigation: restrict event vocabulary, provide golden tests for reducers, snapshotting discipline.
- Risk: IndexedDB quirks and browser differences | Mitigation: use a proven wrapper (Dexie), feature-detect, fallback to Local Storage with reduced limits.
- Risk: Harder analytics/debugging vs CRUD | Mitigation: dev tools for timeline inspection; deterministic replay in tests.

### When NOT to choose this
- When requirements are simple, online-only, and CRUD suffices.
- When team lacks experience with event-sourced thinking and needs quick onboarding.
- When immediate server integration with strict strong consistency is mandatory.
