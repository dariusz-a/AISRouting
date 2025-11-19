# OCR Worksheet (v1)

## Objectives (top-level goals & measurable QAs)
- Primary Objective:
  - Deliver a Roles Management capability that lets administrators create, edit, import, and govern roles, including assigning skill categories and individual skills, with clear validation and auditability.

- Secondary Objectives:
  - Provide a smooth, responsive Vue 3 SPA experience with predictable state and clear error handling.
  - Prevent duplicate roles and duplicate skill assignments (via category and individually) with idempotent operations and user guidance.
  - Support CSV import with row-level validation and feedback without corrupting existing data.
  - Establish a maintainable stack based on Vite + Vue 3 + TypeScript + Pinia + PrimeVue with Playwright E2E coverage.

- Quality Attributes (with targets):
  - Latency:
    - p95 route-to-interactive for in-app navigation: ≤150 ms on modern desktop, ≤250 ms on typical laptop; initial load TTI ≤2.5 s over 4G fast.
    - p95 save/edit role or assignment round-trip (excluding network): ≤50 ms client processing; end-to-end API p95 ≤400 ms, p99 ≤800 ms.
  - Availability:
    - Frontend static assets served via CDN: 99.9% monthly availability.
    - Critical admin flows (create/edit role, assign skills): 99.5% success rate per day (excluding user validation errors).
  - Throughput:
    - Sustain ≥50 write ops/minute for admin operations without noticeable UI degradation; list views support ≥10k roles with virtualized rendering.
  - Data consistency:
    - Read-your-writes consistency for the same user session after create/update/delete of roles/assignments.
    - No duplicate role names within the same tenant/workspace; no duplicate skill assignments (category + individual) for a role.
  - Security:
    - All traffic over TLS 1.2+; CSP and XSS protections enabled; no PII stored in localStorage beyond non-sensitive preferences.
    - AuthN via SSO/OIDC or session cookies (httpOnly) where available; RBAC enforced so only admins can mutate roles.
  - Compliance:
    - GDPR for any PII processed (consent, data minimization, export/delete on request); audit trail for role mutations retained ≥12 months.
  - Cost envelope:
    - Frontend hosting/CDN plus E2E test runs ≤ $100/month for this app at current usage; dependency updates and CI ≤ 1 developer-day/month.

## SMART QA constraints

| QA | Measurable constraint | Assumptions (workload/env) | Verification method | Stakeholder |
|---|---|---|---|---|
| Latency – in-app navigation | SLO: p95 route-to-interactive for in-app route changes ≤150 ms desktop, ≤250 ms typical laptop; p99 ≤400 ms. | Pre-prod with seeded data; Chrome stable; desktop i7/16GB and mid-range laptop profiles; no throttling. | Playwright synthetic nav suite capturing Performance API; assert percentiles over 500 iterations; fail CI if p95/p99 exceed thresholds. | Eng Lead, SRE |
| Latency – initial load (TTI) | SLA: p95 TTI ≤2.5 s on 4G Fast; p99 ≤4.0 s. | Lighthouse 4G Fast throttling; cold cache; CDN enabled; production build. | Lighthouse CI run on pre-prod, 5 runs median; assert TTI p95/p99 thresholds; store trends. | Eng Lead, Product |
| Latency – save/edit role | SLO: client processing p95 ≤50 ms; end-to-end API p95 ≤400 ms, p99 ≤800 ms. | API reachable in same region; payload ≤10 KB; no server cold start. | Playwright spans around click→200 OK; capture client processing via PerformanceObserver; export p95/p99. | Eng Lead, SRE |
| Availability – static assets | Monthly availability for CDN asset delivery ≥99.9%. | CDN health; single region acceptable; pre-prod mirrors prod config. | Synthetic checks every 1 min from 3 regions for index.html and main.js; compute monthly availability. | SRE |
| Availability – admin critical flows | Daily success rate of create/edit role and assign skills ≥99.5% excluding validation errors. | Backend stable; test creds admin; stable test data. | Scheduled Playwright cron runs hourly; count green runs vs total; exclude scenarios failing on expected validation. | Product, SRE |
| Throughput – admin writes | System sustains ≥50 write ops/min for 15 min with API p95 latency ≤400 ms, error rate ≤1%. | 5 concurrent admin users; typical payloads; DB warm; pre-prod size mirrors prod schema. | Controlled Playwright loop (or light k6) issuing writes; capture latency and error rate; export summary. | Eng Lead, SRE |
| Throughput – list views scale | Role directory renders and remains responsive with 10k roles: First contentful render ≤1.0 s (cached), scroll jank (<5% frames >16 ms). | Virtualized list enabled; local machine mid-range; cached assets; seeded 10k roles. | Playwright + trace: measure FCP via Performance API; use RAF sampling to compute dropped frames while scrolling scripted path. | Eng Lead, Product |
| Data consistency – read-your-writes | After a successful write, subsequent GET by same user reflects change p99 ≤2 s; zero stale reads beyond 2 s. | Eventual consistency allowed ≤2 s; same region; no cache in front of API for writes. | Playwright: write then poll GET every 200 ms; record time to visibility over 200 trials; assert p99 ≤2 s. | Eng Lead |
| Data consistency – uniqueness/dedup | No duplicate role names within tenant; duplicate skill via category/individual is prevented or idempotently ignored (API 409 or 200 with no-op). | Tenant model defined; backend enforces unique index. | API contract tests: attempt duplicate create/assign; assert status code/response and post-state snapshot shows no duplicates. | Eng Lead, Product |
| Security – transport & storage | 100% of requests use HTTPS/TLS1.2+ with HSTS; no PII in localStorage/sessionStorage; CSP blocks inline scripts. | Deployed behind HTTPS/CDN; CSP header configured. | ZAP/Burp passive scan in pre-prod; automated checks for CSP/HSTS headers; Playwright script to enumerate storage keys and assert none contain PII patterns. | Security Lead |
| Security – authn/z & RBAC | Admin-only mutations enforced: 0% unauthorized write success across 500 attempts; all admin flows require authenticated session (no anonymous success). | Test users: admin and non-admin; backend RBAC configured. | Negative Playwright tests using non-admin token attempting create/edit/delete; assert 401/403 and no state change. | Security Lead, QA Lead |
| Compliance – GDPR & audit | Audit trail exists for role mutations with user id, timestamp, entity ids; retention policy configured ≥12 months. | Audit sink available; clock synced; PII minimization applied. | Execute sample mutations in pre-prod; query audit store; verify fields present and retention config set; document evidence. | Compliance Officer, Eng Lead |
| Cost envelope – hosting & CI | Forecasted monthly cost for CDN + hosting + CI + E2E ≤ $100 at current usage; dependency maintenance ≤ 1 dev-day/month. | Traffic: ≤50 admin writes/min peak, 10k roles; CI minutes budget set; region pricing baseline. | Cloud cost calculator export + CI minutes projection; compare against budget; create cost report artifact in repo. | Product, Eng Manager |

Notes:
- Time-bound: All SLO/SLA targets apply by Beta cutover; verification runs integrated into CI within the current iteration and must remain green for 7 consecutive days before release.
- Minimum viable measurements: All checks use existing stack (Playwright, Lighthouse CI optional) with synthetic data; no production telemetry required.

## Constraints (non-negotiables / givens)
- Platform/Infra:
  - Vue 3 (Composition API), Vite build, TypeScript; Node.js ≥18; Pinia for state, Vue Router (history mode); PrimeVue + @primevue/themes; Primer CSS.
  - Playwright for E2E testing; repository already structured as a Vite + Vue 3 SPA.
  - Browser support: last 2 stable versions of Chrome/Edge/Firefox; Safari 2 latest major versions.

- Integrations & protocols:
  - Backend API assumed REST/JSON over HTTPS; CSV import adheres to RFC 4180 (UTF‑8, quoted fields); max CSV size (assume) 5 MB pending confirmation.
  - Potential HR system integration for role/skill catalog synchronization (scope TBC) via scheduled import or webhook.

- Compliance & audit:
  - GDPR compliance for any PII events (from Constraints.md); audit logs for role create/update/delete and assignment changes.
  - Error messages avoid leaking PII; user actions logged with user id, timestamp, before/after snapshots (field-level diff where permissible).

- Data & state constraints:
  - Canonical entities: Role, Skill, SkillCategory; many-to-many Role↔Skill; Category groups skills; assignment may be direct or via category expansion.
  - Deduplication rules: unique role name per tenant; a skill assigned via category should not be duplicated by individual assignment.
  - Pagination/virtualization required for lists ≥1k entries; client-side caching with explicit refresh.

- Tooling/observability:
  - Use Playwright with stable test locators and traces enabled for failures; include network request logging for admin flows.
  - Linting/formatting via ESLint/Prettier; dependency versions pinned with compatibility notes from Overall_architecture.md.

## Risks
- Top risks (5):

| Risk | Likelihood | Impact | Early indicator | Mitigation | Residual |
|---|---|---|---|---|---|
| Duplicate role/skill assignments due to category + individual overlap | Medium | High | Users report “already assigned” confusion; inconsistent counts | Enforce server-side idempotency and uniqueness; clear UI warnings and disable duplicate selections | Low |
| CSV import quality (malformed/large files) corrupts data or blocks admins | Medium | High | Rising import failures; long upload times | Strict schema validation, size limits (e.g., 5 MB), row-level errors, dry-run preview | Low-Med |
| Data model drift between frontend and backend (skills/categories) | Medium | High | Frequent API contract changes; UI mismatches | Typed API clients, contract tests, versioned endpoints, feature flags | Medium |
| E2E test flakiness slows releases | High | Medium | Non-deterministic Playwright failures; frequent retries | Use testIDs, network waits on specific calls, deterministic fixtures, CI retries with trace/video | Low |
| Dependency compatibility (Vue/PrimeVue/Vite) breaks build/runtime | Medium | Medium-High | Build warnings; runtime UI regressions | Pin versions, renovate with compatibility matrix, stage updates in branches with E2E smoke | Low |

Additional context risks from inputs: cold starts, cache staleness, cross‑region consistency, model latency, fallback strategies → relevant if/when SSR, edge caching, or ML categorization are introduced. Track but not in current critical path for SPA-only frontend.

## Assumptions & Open Questions
- Key assumptions (3–5):
  - A backend service exposes REST endpoints for roles, skills, categories, and imports; read-your-writes is guaranteed per user session.
  - Authentication and RBAC exist; only Admin users can mutate roles and assignments.
  - CSV import is processed server-side; the frontend provides client-side pre-validation and clear row-level error reporting.
  - Hosting is static SPA behind CDN; no SSR required at this stage.

- Open questions (clarifications needed):
  - What is the authoritative system of record for roles/skills/categories (this app vs HR/other)? Who owns deduplication rules?
  - Expected data scale: max roles, skills, categories; peak concurrent admin users; import file size and row limits?
  - Exact PII involved (if any) in roles/skills context; do audit logs need masking or encryption-at-rest requirements beyond backend defaults?
  - Required availability/SLOs for admin flows vs general users; is 99.9% CDN availability sufficient or are stricter SLAs needed?
  - Should the UI support multi-tenant separation and role name uniqueness per tenant or globally?
