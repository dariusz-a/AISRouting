## Playwright Authentication Process for Automated Tests

All Playwright tests MUST use the following credentials for authentication:

- **Email**: `alice.smith@company.com`
- **Password**: `SecurePass123!`

### Implementation Notes

- Credentials MUST be entered exactly as specified above.
- After login, verify that the application loads the expected landing page.

This authentication process MUST be followed for every test that requires a logged-in user.
