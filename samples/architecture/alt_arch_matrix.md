# Alternative Architectures — Trade-off Matrix 

## Trade-off Matrix

Legend: Each cell rates fit as High/Medium/Low with one-line mechanism evidence.

| Attribute | Candidate A — Modular SPA on Local Storage | Candidate B — Capability‑aligned Micro Frontends (API‑ready) | Candidate C — Event‑driven Offline‑first Client with Sync |
|---|---|---|---|
| Latency | High — In‑tab Pinia store, no network on core CRUD; p95 ≤ 200 ms from spec | Medium — Network hop per CRUD; optimistic updates + SWR target p95 ≤ 250 ms | High — Local event commit + materialized views keep UI hot; p95 ≤ 150 ms |
| Scalability | Medium — Browser‑bound; OK up to ~5k roles/~50k links; LocalStorage quota ~5–10 MB | High — Backend/API scale‑out; MFEs CDN‑cached; horizontal API scaling | High — IndexedDB store + snapshots scale locally (100k+ events); remote scale via async sync |
| Consistency | Medium — Strong in‑tab; cross‑tab eventual (≤ 1 s) via StorageEvent; idempotent writes | High — Backend as source of truth; SWR caches with short TTL; idempotent APIs | Medium — Strong local; eventual global with CRDT‑like merges and provenance rules |
| Reliability/HA | Medium — 99.9% static hosting; single‑client state; offline within session only | High — 99.95% with CDN + HA API; MFEs degrade independently; shell shows partial availability | High — Fully offline operations; sync backlog survives reload; deterministic replay/recovery |
| Changeability | High — Mono‑core with storage adapter boundary; low cognitive load for small team | Medium — Contracts across MFEs/API; versioning + coordination overhead | Medium — Event model/reducers require migrations and discipline; harder to evolve |
| Security/Compliance | Medium — Client‑only with no PII; basic input validation/XSS safety; limited enterprise controls | High — Central auth (OIDC), server‑side RBAC/policy, audit readiness | Medium — No PII; optional encrypt‑at‑rest (WebCrypto); central policy enforcement is harder |
| Cost | High — Static hosting + CDN only; near‑zero ops | Medium — API + MFE builds, CI/CD, observability increase spend | Medium — More client engineering (sync/backfill/tooling) but low infra costs |
| Delivery Risk | Low — Familiar CRUD SPA; smallest surface; fast to deliver | Medium — Architectural overhead, contract drift risk, API dependency | Medium — Complexity in event modeling/reconciliation; IndexedDB quirks mitigations needed |

## Summary
- Candidate A — Modular SPA on Local Storage: Optimized for speed of delivery, low cost, and excellent perceived latency in a single‑client context. Limits show up in cross‑user consistency and data volume (browser storage quotas). Best when scope is client‑only and near‑term.
- Candidate B — Capability‑aligned Micro Frontends (API‑ready): Strong choice for enterprise scale, central governance (OIDC/RBAC), and operational resilience. Trades higher cost and coordination (SDK contracts, API availability) for long‑term scalability and consistency.
- Candidate C — Event‑driven Offline‑first with Sync: Excels at offline UX, resilience, and auditability via events with strong local performance. Introduces higher modeling complexity and eventual‑consistency wrinkles; choose when offline/field operations and traceability are priorities.
