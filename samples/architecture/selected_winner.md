# Selected Winner — Architecture Trade‑off Decision

## QA scoring (evidence‑based)

Assumption: equal weights across QAs (no weights provided). Delivery Risk is scored with Low=3, Medium=2, High=1 per instruction.

| Attribute | Candidate A Score | Candidate B Score | Candidate C Score |
|---|---|---|---|
| Latency | 3 | 2 | 3 |
| Scalability | 2 | 3 | 3 |
| Consistency | 2 | 3 | 2 |
| Reliability/HA | 2 | 3 | 3 |
| Changeability | 3 | 2 | 2 |
| Security/Compliance | 2 | 3 | 2 |
| Cost | 3 | 2 | 2 |
| Delivery Risk (Low=3) | 3 | 2 | 2 |

- Totals:
  - Candidate A: 20
  - Candidate B: 20
  - Candidate C: 19

## Where evidence is weak or missing

- Latency targets are stated but not backed by measured benchmarks (e.g., “p95 ≤ 200 ms”, “p95 ≤ 250 ms”, “p95 ≤ 150 ms”) [alt_arch_matrix.md:L9].
- Scalability limits/claims are rough estimates without empirical capacity tests (e.g., “LocalStorage quota ~5–10 MB”, “100k+ events”) [alt_arch_matrix.md:L10].
- Consistency model details for C (e.g., “CRDT‑like merges and provenance rules”) lack a concrete merge policy/test plan [alt_arch_matrix.md:L11].
- Security/Compliance lacks mapping to concrete controls and audit mechanisms (e.g., “audit readiness”, “encrypt‑at‑rest (WebCrypto)”) [alt_arch_matrix.md:L14].
- Delivery Risk mentions risks without mitigation evidence (e.g., “contract drift risk, API dependency”, “IndexedDB quirks mitigations needed”) [alt_arch_matrix.md:L16].
- Summary guidance is directional but not validated with prototypes or field trials (e.g., “Best when scope is client‑only and near‑term”; “Strong choice for enterprise scale”) [alt_arch_matrix.md:L19–L20].

## Recommendation (provisional)

Select Candidate B — Capability‑aligned Micro Frontends (API‑ready) as the provisional winner.

Rationale:
- Ties A on total score but dominates on enterprise‑critical QAs: Scalability, Consistency, Reliability/HA, and Security/Compliance [alt_arch_matrix.md:L10–L16].
- Summary explicitly positions B for “enterprise scale, central governance (OIDC/RBAC), and operational resilience” [alt_arch_matrix.md:L20].
- While A excels at cost and speed of delivery [alt_arch_matrix.md:L15, L19], its limitations in cross‑user consistency and data volume make it a risk for long‑term growth [alt_arch_matrix.md:L10–L11, L19].

Note: If the near‑term goal is a client‑only MVP with minimal ops, Candidate A remains a viable short‑term path; however, for sustained evolution and governance, B is preferable based on the provided evidence.

## Top 2 validation steps to de‑risk Candidate B

1) Vertical slice with enterprise controls and performance:
   - Build a thin MFE + API vertical (one core CRUD flow) with OIDC login and server‑side RBAC/policy enforcement.
   - Instrument client and API to measure end‑to‑end p95 latency under realistic load; target p95 ≤ 250 ms with SWR caching [alt_arch_matrix.md:L9–L10, L14, L20].
   - Success criteria: Auth works; policy enforced; p95 target met for 95% of requests; errors < 1%.

2) Contract resilience and partial‑failure handling:
   - Add consumer–provider contract tests between MFE(s) and API; introduce controlled API outages/latency to validate shell‑level graceful degradation and independent MFE resilience.
   - Success criteria: No cascading failures; degraded UI remains operable; error states surfaced; recovery without reload; observability captures SLO breaches [alt_arch_matrix.md:L12, L16, L20].

## Appendix — Source summary excerpts

Candidate A

- “In‑tab Pinia store, no network on core CRUD; p95 ≤ 200 ms” [alt_arch_matrix.md:L9]
- “Browser‑bound; OK up to ~5k roles/~50k links; LocalStorage quota ~5–10 MB” [alt_arch_matrix.md:L10]
- “Optimized for speed of delivery, low cost, and excellent perceived latency” [alt_arch_matrix.md:L19]

Candidate B

- “Backend/API scale‑out; MFEs CDN‑cached; horizontal API scaling” [alt_arch_matrix.md:L10]
- “Central auth (OIDC), server‑side RBAC/policy, audit readiness” [alt_arch_matrix.md:L14]
- “Strong choice for enterprise scale, central governance (OIDC/RBAC), and operational resilience” [alt_arch_matrix.md:L20]

Candidate C

- “Local event commit + materialized views keep UI hot; p95 ≤ 150 ms” [alt_arch_matrix.md:L9]
- “Strong local; eventual global with CRDT‑like merges and provenance rules” [alt_arch_matrix.md:L11]
- “Fully offline operations; sync backlog survives reload; deterministic replay/recovery” [alt_arch_matrix.md:L12]
