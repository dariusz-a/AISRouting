**AISRouting E2E Test Scaffold (Playwright)**

Prerequisites:
- Node.js (16+ recommended)
- `pwsh.exe` is the default shell for these instructions (Windows PowerShell Core / PowerShell)

Install dependencies and Playwright browsers:

```powershell
npm install
npx playwright install --with-deps
```

Start the static server (in one terminal):

```powershell
node server.js
```

Run tests (in another terminal):

```powershell
npm test
```

Notes:
- The scaffold creates a minimal static `app/index.html` containing the documented `data-testid` and `aria-label` attributes.
- Tests are in `tests/` and use `getByTestId()` as the first-choice selector, per your validation guide.
- This is an example harness — adapt tests and the app as your real UI becomes available.
