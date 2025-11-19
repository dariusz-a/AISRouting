This file describes authentication, authorization, data access controls, API security measures, compliance features, and security testing approaches for AISRouting.

Authentication and authorization framework

- Authentication: OAuth2 / OpenID Connect for user authentication. Support external identity providers (Azure AD, Google) and local fallback.
- Session: Frontend stores short-lived tokens in memory; refresh tokens stored in secure, HttpOnly cookies.
- Authorization: Role-based access control (RBAC) with roles (admin, planner, viewer). Use attribute-based access control for finer-grained permissions (e.g., team-level route edit permissions).

Data access controls

- Principle of least privilege: services and users get minimum permissions.
- Database access: use service accounts with scoped roles. Admin-only operations require elevated service role and audit logging.
- Row-level security (RLS) in Postgres for tenant isolation or team-based access control.

API security measures

- Use TLS (HTTPS) everywhere. Reject plaintext HTTP.
- Validate and sanitize all inputs. Use parameterized queries to prevent SQL injection.
- Rate limiting and throttling via API gateway.
- Use mTLS for inter-service communication where required.
- JWT validation and scope checks on each service endpoint.

Compliance features

- Logging and audit trails for critical actions (route exports, user role changes).
- Data retention policies for logs and exported files.
- PII handling: encrypt sensitive fields, mask in logs.

Security testing approach

- Automated security tests in CI: dependency vulnerability scans (Dependabot, Snyk), static analysis (ESLint with security rules), and infrastructure-as-code checks.
- Penetration testing and periodic security reviews.
- Unit and integration tests for auth flows; Playwright E2E tests use test credentials defined in testing docs.

Happy and negative paths

- Happy path: Alice logs in via OIDC, successfully generates and exports a route to which she has access.
- Negative cases: expired token -> frontend redirects to login; insufficient permissions -> 403 with clear message; repeated failed login attempts -> lockout policies applied.

Cross references

- See overall_architecture.md for auth service placement.
- See testing/QA_playwright_authentication.md for test credentials and authentication procedure for automated tests.
