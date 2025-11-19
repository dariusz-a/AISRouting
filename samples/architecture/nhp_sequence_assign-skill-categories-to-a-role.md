### Non-Happy Path Sequence: Assign skill categories to a role

```
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Sequence.puml

title NHP: Assign skill category → DB lock timeout with retries and circuit breaker

actor Administrator as Admin
Container(Shell, "Shell", "Vue 3 SPA")
Container(MFEs, "Capability MFEs (Roles UI)")
Container(ApiGateway, "API Gateway/Ingress")
Container(BackendAPI, "Backend API")
ContainerDb(PrimaryDB, "Primary Database", "RDBMS")
System_Ext(IdP, "Identity Provider (OIDC)")
System_Ext(Obs, "Observability/Telemetry Platform")

Admin -> Shell : Open role "Data Analyst" → Assign category "Programming Languages"
Shell -> MFEs : Route to Roles UI
MFEs -> Shell : Prepare save request
Shell -> ApiGateway : POST /roles/{id}/assign-category\nIdempotency-Key: sha256(userId,roleId,categoryId,requestId)
ApiGateway -> BackendAPI : Forward request (trace_id)

BackendAPI -> IdP : Validate JWT (use cached JWKs)
IdP --> BackendAPI : OK (cache hit)

BackendAPI -> BackendAPI : Check/Create idempotency ledger entry
note over BackendAPI
Idempotency: key = sha256(userId, roleId, categoryId, requestId)
No duplicate effect if retried
end note

BackendAPI -> PrimaryDB : Upsert role_category → role_skill rows
... Fault occurs ...

note over BackendAPI, PrimaryDB
Fault: DB lock wait timeout during upsert (contention)
Retry 3x with exponential backoff (50ms, 100ms, 200ms)
Per-attempt DB statement_timeout = 400ms
Total retry window ≤ ~1s (aligns with CRUD p95 ≤ 250 ms target under abnormal conditions)
end note

PrimaryDB -x BackendAPI : Timeout/lock error (attempts 1..3)

note over BackendAPI
If rolling failures persist ≥ 30 s, open Circuit Breaker\nOpen duration: 30 s; probe every 5 s
Short-circuit further DB calls while open
end note

BackendAPI -> Obs : Emit metrics/logs/traces\nhttp_5xx, db_lock_timeout_total, slo_breach_total, span.status=error
BackendAPI --> ApiGateway : 503 Service Unavailable\nRetry-After: 1
ApiGateway --> Shell : 503 (error)

Shell -> Shell : Show error toast "Save failed; please retry"
Shell -> MFEs : Revert optimistic UI change

... Later, user retries ...
Admin -> Shell : Click Save again
Shell -> ApiGateway : POST assign-category (same Idempotency-Key)
ApiGateway -> BackendAPI : Forward

alt Prior write actually succeeded (race)
  BackendAPI -> BackendAPI : Idempotency check → key exists
  BackendAPI --> ApiGateway : 200 OK (no-op idempotent)
else DB recovered
  BackendAPI -> PrimaryDB : Upsert succeeds
  BackendAPI --> ApiGateway : 200 OK
end

ApiGateway --> Shell : 200 OK
Shell -> Shell : Show success "Assigned 1 category"

@enduml
```

### Failure Analysis

| Dimension | Description |
|---|---|
| Failure point | Primary Database lock wait timeout during upsert of role→skill assignments (concurrent writes/held locks cause statement_timeout and/or deadlock). |
| Detection | Elevated `http_5xx_total`, spikes in `http_server_request_duration_seconds` on assign route, `db_lock_timeout_total`/`db_wait_seconds` from DB client, OpenTelemetry spans with `span.status=error` and `db.statement` attributes; alert when Backend API p95 > 250 ms for 3 min or lock timeouts > threshold. |
| Recovery mechanism | Backend API retries DB write 3x with exponential backoff and per-attempt statement_timeout=400 ms; opens a circuit breaker after sustained failures in a 30 s window to shed load; client retries using the same Idempotency-Key ensuring at-most-once effect. |
| Residual risk | User-facing error and possible confusion if initial write actually succeeded but UI shows failure until retry; temporary unavailability (503) while breaker is open; some clients may not retry, leaving desired state unapplied until a later attempt; error-budget burn during incident window. |
| Improvement hint | Add/verify unique composite index (role_id, skill_id) with ON CONFLICT upsert; tune lock order and transaction scope to avoid contention; lower client timeout below server DB timeout to fail fast; add a small async retry/job for high-contention updates; add a health/SLO panel with lock-timeout alerts and breaker-open gauge. |

### Follow-up
This NHP sequence highlights a trade-off favoring strong consistency (single-writer DB) with transient availability loss under contention; idempotency plus circuit breaking keeps correctness while increasing complexity and requiring clear UX for retries.

Assertions we can test automatically:
- When the assign-category endpoint simulates DB lock timeouts, the API retries ≤ 3 times and returns 503 with `Retry-After` within ≤ 1.2 s, and increments `slo_breach_total` and `db_lock_timeout_total` metrics.
- A second POST with the same Idempotency-Key after a failed/unknown first attempt returns 200 OK without creating duplicate role→skill rows (idempotent no-op verified by state).
- After N consecutive failures in a 30 s window, subsequent requests are short-circuited by the circuit breaker (no DB call made), return 503, and an `breaker_open=1` metric/trace attribute is observable.
