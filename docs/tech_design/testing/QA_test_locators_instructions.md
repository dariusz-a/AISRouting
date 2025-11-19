# Test Locators Instructions  

This document defines how to add locators in application code and how to generate Playwright tests.
It targets both developers and test authors.  

## Core Principles (from the ADR Decision Outcome)  
- Prefer user-facing locators first, then stable test attributes.  
- Priority order for selecting elements:  
  1) ARIA roles and accessible names (getByRole, getByLabel)  
  2) Dedicated test attributes data-testid (getByTestId)  
  3) Visible, stable text content (getByText)  
  Fallback: Stable IDs, then simple CSS attributes only as a last resort. Avoid XPath, DOM-structure, class-based selectors.  

## ARIA Rules You Must Apply  
Follow these rules whenever adding or modifying UI components.  

Do:  
- Use aria-label when no visible label exists.  
- Prefer aria-labelledby over aria-label when a visible label exists and can be referenced. The targets for aria-labelledby must exist, be unique, and reference visible text.  
- Ensure every required UI component has an accessible name.  
- Use ARIA with elements with the following roles: alertdialog, application, combobox, grid, img, listbox, meter, progressbar, radiogroup, region, searchbox, slider, spinbutton, table, tabpanel, textbox, tree, treegrid, button, cell, checkbox, columnheader, gridcell, heading, link, menuitem, menuitemcheckbox, menuitemradio, option, radio, row, rowheader, switch, tab, tooltip, treeitem  

Avoid:  
- Do not use both aria-label and aria-labelledby on the same element.  
- Do not add names to roles where naming is discouraged or prohibited.  
- Do not use aria-hidden="true" on focusable elements.  
- Do not put ARIA on generic elements (div, span) without a specific role, and avoid inline structural roles (code, term) for naming.  
- Do not name presentational roles (presentation, none).  
- Do not use ARIA with elements with the following roles: caption, code, deletion, emphasis, generic, insertion, mark, none, paragraph, presentation, strong, subscript, superscript  

## Naming Conventions for ARIA labels  
- Use clear, descriptive language: `aria-label="Submit user registration form"`  
- Ensure labels describe the element's purpose, not just its appearance  

## Naming Conventions for data-testid  
- Use kebab-case, descriptive, and context-specific names: submit-form-btn, user-profile-edit-btn.  
- Include type or role when helpful: search-results-table, cart-checkout-btn.  
- For repeated items: product-card-{id}, nav-menu-item-{index}.  
- Keep names stable across refactors; update tests if a contract changes.  

Stable IDs for repeated items  
- Prefer domain identifiers over positional indexes when available: product-card-{productId} instead of product-card-{index}.  
- If no domain ID exists, use a stable business key or slug that does not change across renders: order-row-{orderNumber}, user-item-{username}.  
- Use an index only as a last resort; when you must use it, scope under a stable parent to reduce fragility: page.getByTestId('product-list').getByTestId('product-card-3').  
- Do not include personally identifiable information (PII) in test IDs.  
- Keep test IDs immutable once published to tests; introduce new IDs rather than repurposing existing ones.  

## Rules for Application Code Generation (Angular / AngularJS / HTML)  
Goal: make elements reliably discoverable with ARIA first, then data-testid where needed.  

General guidance:  
- Prefer semantic elements with implicit roles (button, a, input[type]) and proper labels.  
- If no visible label, provide aria-label or use aria-labelledby to reference visible text.  
- Add data-testid to interactive or critical elements when ARIA or text would be ambiguous or unstable.  
- Avoid class- or structure-dependent selectors; tests should not rely on styling or DOM shape.  

1) Buttons (text, icon-only, or mixed)  
HTML/Angular:  
```html
<!-- Text button: implicit role "button"; accessible name from content -->
<button type="button" data-testid="save-profile-btn">Save profile</button>

<!-- Icon-only button: provide accessible name -->
<button type="button" aria-label="Open settings" data-testid="open-settings-btn">
  <span class="icon icon-settings" aria-hidden="true"></span>
</button>

<!-- Button with visible label element -->
<label id="exportLbl" class="sr-only">Export data</label>
<button type="button" aria-labelledby="exportLbl" data-testid="export-btn">
  <span class="icon icon-download" aria-hidden="true"></span>
</button>
```

AngularJS is analogous; add aria-* and data-testid directly on the elements.  

2) Inputs, labels, and forms  
- Prefer a label element associated via for/id.  
- If no visible label, use aria-label or aria-labelledby.  
- Add aria-describedby for hints or validation messages.  
- Add data-testid to key inputs and forms as needed.  
```html
<form aria-labelledby="userFormTitle" data-testid="user-form">
  <h2 id="userFormTitle">User profile</h2>

  <div>
    <label for="firstName">First name</label>
    <input id="firstName" name="firstName" type="text" data-testid="first-name-input" />
  </div>

  <div>
    <label for="email">Email</label>
    <input id="email" name="email" type="email" aria-describedby="emailHint" data-testid="email-input" />
    <div id="emailHint" class="hint">We'll never share your email.</div>
  </div>

  <button type="submit" data-testid="save-user-btn">Save</button>
</form>
```

3) Selects, listboxes, and combobox/autocomplete  
```html
<label for="country">Country</label>
<select id="country" name="country" data-testid="country-select">
  <option value="de">Germany</option>
  <option value="us">United States</option>
</select>

<!-- Autocomplete/combobox pattern -->
<div role="combobox" aria-expanded="false" aria-owns="city-list" aria-haspopup="listbox" aria-labelledby="cityLabel" data-testid="city-combobox">
  <label id="cityLabel" class="sr-only">City</label>
  <input type="text" role="searchbox" aria-autocomplete="list" aria-controls="city-list" />
  <ul id="city-list" role="listbox">
    <li role="option">Berlin</li>
    <li role="option">Bern</li>
    <li role="option">Bergen</li>
  </ul>
</div>
```

4) Checkboxes, radios, and switches  
```html
<fieldset role="radiogroup" aria-labelledby="planTitle" data-testid="plan-radiogroup">
  <legend id="planTitle">Choose a plan</legend>
  <label><input type="radio" name="plan" value="basic" />Basic</label>
  <label><input type="radio" name="plan" value="pro" />Pro</label>
</fieldset>

<label class="switch">
  <input type="checkbox" role="switch" aria-label="Enable notifications" data-testid="notifications-switch" />
  <span class="slider" aria-hidden="true"></span>
</label>
```

5) Dialogs and alerts  
```html
<div role="dialog" aria-modal="true" aria-labelledby="dlgTitle" aria-describedby="dlgDesc" data-testid="delete-dialog">
  <h2 id="dlgTitle">Delete item</h2>
  <p id="dlgDesc">This action cannot be undone.</p>
  <button type="button" data-testid="confirm-delete-btn">Delete</button>
  <button type="button" data-testid="cancel-delete-btn">Cancel</button>
</div>
```

6) Tables and grids  
```html
<table aria-labelledby="ordersTitle" data-testid="orders-table">
  <caption id="ordersTitle">Recent orders</caption>
  <thead>
    <tr>
      <th scope="col">Order</th>
      <th scope="col">Customer</th>
      <th scope="col">Total</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>#1001</td>
      <td>Ada</td>
      <td>€52.00</td>
    </tr>
  </tbody>
</table>
```

7) Navigation and menus  
```html
<nav aria-label="Main" data-testid="main-nav">
  <ul role="menubar">
    <li role="none"><a role="menuitem" href="/home" data-testid="nav-home">Home</a></li>
    <li role="none"><a role="menuitem" href="/products" data-testid="nav-products">Products</a></li>
  </ul>
</nav>
```

8) Images and icon buttons  
```html
<img src="/logo.svg" alt="Acme portal logo" data-testid="header-logo" />

<button type="button" aria-label="Refresh data" data-testid="refresh-btn">
  <svg aria-hidden="true"><!-- icon --></svg>
</button>
```

9) Lists and repeated items  
```html
<ul data-testid="product-list">
  <li data-testid="product-card-42">
    <h3>Widget</h3>
    <button type="button" aria-label="Add Widget to cart" data-testid="add-to-cart-42">Add</button>
  </li>
</ul>
```

Angular specifics:  
- In templates, you can set static attributes directly: <button data-testid="x">.  
- Prefer native elements with implicit roles; only add role when needed to convey semantics.  

## Rules for Playwright Test Generation  

Locator selection algorithm (apply in order):  
1) Try getByRole with accessible name (exact when possible).  
2) If ambiguous or not expressible via role/name, use getByTestId.  
3) If the element has stable, unique visible text that won't localize, use getByText.  
4) Fallbacks: #id selectors only if guaranteed stable; simple attribute CSS as last resort. Avoid XPath, DOM structure, class names, and nth-child/nth-match indices.  

General tips:  
- Scope searches to regions (e.g., within a dialog or form) using locator scoping.  
- Use filter({ hasText }) or locator chaining to disambiguate rather than nth().  
- Prefer user-centric queries: getByRole, getByLabel, getByPlaceholder, getByAltText, getByTitle, getByTestId.  

Examples  
Buttons  
```ts
// Text button
await page.getByRole('button', { name: 'Save profile' }).click();

// Icon-only button (has aria-label)
await page.getByRole('button', { name: 'Open settings' }).click();

// Disambiguate within a card
const card = page.getByTestId('product-card-42');
await card.getByRole('button', { name: /add/i }).click();
```

Inputs and labels  
```ts
await page.getByLabel('First name').fill('Grace');
await page.getByLabel('Email').fill('grace@example.com');
await page.getByRole('button', { name: 'Save' }).click();
```

Combobox/autocomplete  
```ts
const combobox = page.getByTestId('city-combobox');
await combobox.getByRole('searchbox').fill('Ber');
await page.getByRole('option', { name: 'Berlin' }).click();
```

Radios, switches  
```ts
await page.getByRole('radiogroup', { name: 'Choose a plan' })
  .getByRole('radio', { name: 'Pro' })
  .check();

await page.getByRole('switch', { name: 'Enable notifications' }).check();
```

Dialogs  
```ts
const dialog = page.getByRole('dialog', { name: 'Delete item' });
await dialog.getByRole('button', { name: 'Delete' }).click();
await dialog.getByRole('button', { name: 'Cancel' }).click();
```

Tables  
```ts
const table = page.getByTestId('orders-table');
await expect(table.getByRole('row', { name: /#1001/i }).getByRole('cell', { name: /Ada/i })).toBeVisible();
```

Navigation  
```ts
await page.getByRole('navigation', { name: 'Main' }).getByRole('menuitem', { name: 'Products' }).click();
```

Images and alt text  
```ts
await expect(page.getByRole('img', { name: 'Acme portal logo' })).toBeVisible();
```

Repeated lists  
```ts
const list = page.getByTestId('product-list');
await list.getByTestId('product-card-42').getByRole('button', { name: /add/i }).click();
```

Disambiguation without nth()  
```ts
// Scope to section instead of using nth()
const section = page.getByRole('region', { name: 'User profile' });
await section.getByRole('button', { name: 'Save' }).click();
```

## When to Add data-testid  
Add data-testid when any of the following are true:  
- Also add data-testid to critical CTAs and key containers (forms, dialogs, tables, major navigation) to provide stable scoping and resilience to localization/copy changes, even when ARIA is sufficient.  
- Accessible name or role would be ambiguous (e.g., multiple identical buttons).  
- Visible text is dynamic or localized and may change.  
- Element is critical to end-to-end flows and warrants a stable contract.  
- Component is rendered multiple times in a list; include a stable suffix like an ID or index.  

## Anti-Patterns to Avoid  
- XPath selectors, DOM-structure chains, nth-child, parent > child relationships as primary locators.  
- Class-based selectors that couple tests to styling.  
- Dynamic or auto-generated IDs as primary locators.  
- Using both aria-label and aria-labelledby on the same element.  
- Using aria-hidden="true" on elements that can receive focus.  

## End-to-End Example  
Application code  
```html
<section role="region" aria-labelledby="loginTitle" data-testid="login-section">
  <h1 id="loginTitle">Sign in</h1>
  <form aria-labelledby="loginTitle" data-testid="login-form">
    <div>
      <label for="username">Username</label>
      <input id="username" name="username" type="text" data-testid="username-input" />
    </div>
    <div>
      <label for="password">Password</label>
      <input id="password" name="password" type="password" data-testid="password-input" />
    </div>
    <button type="submit" data-testid="submit-login-btn">Sign in</button>
  </form>
</section>
```

Playwright test  
```ts
import { test, expect } from '@playwright/test';

test('user can sign in', async ({ page }) => {
  await page.goto('/login');
  const section = page.getByRole('region', { name: 'Sign in' });
  await section.getByLabel('Username').fill('ada');
  await section.getByLabel('Password').fill('correcthorsebatterystaple');
  await section.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).toHaveURL('/dashboard');
});
```

Alternative fallback using data-testid (for localization or ambiguity)  
```ts
import { test, expect } from '@playwright/test';

test('user can sign in (stable fallback)', async ({ page }) => {
  await page.goto('/login');
  const form = page.getByTestId('login-form');
  await form.getByTestId('username-input').fill('ada');
  await form.getByTestId('password-input').fill('correcthorsebatterystaple');
  await form.getByTestId('submit-login-btn').click();
  await expect(page).toHaveURL('/dashboard');
});
```

- Prefer the ARIA-based test above. Use the data-testid fallback when accessible names vary by locale/copy or when multiple similar CTAs exist.  
- When multiple "Sign in" buttons exist, scope by region/form or use the stable CTA test id:  
```ts
await page.getByTestId('login-form').getByRole('button', { name: 'Sign in' }).click();
// or
await page.getByTestId('submit-login-btn').click();
```  

## Angular Example — Vessels table (domain IDs and ARIA-first)  
Application code (Angular)  
```html
<!-- vessels-table.component.html -->
<table aria-labelledby="vesselsTitle" data-testid="vessels-table">
  <caption id="vesselsTitle">Vessels</caption>
  <thead>
    <tr>
      <th scope="col">IMO Number</th>
      <th scope="col">Name</th>
      <th scope="col">Connectivity Status</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let v of vessels$ | async; trackBy: trackByImo" [attr.data-testid]="'vessel-row-' + v.imo">
      <td [attr.data-testid]="'vessel-imo-' + v.imo">{{ v.imo }}</td>
      <td [attr.data-testid]="'vessel-name-' + v.imo">{{ v.name }}</td>
      <td [attr.data-testid]="'vessel-status-' + v.imo">{{ v.status }}</td>
    </tr>
  </tbody>
</table>
```

```ts
// vessels-table.component.ts
import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

interface Vessel { imo: number; name: string; status: 'Online' | 'Offline'; }

@Component({
  selector: 'app-vessels-table',
  templateUrl: './vessels-table.component.html',
})
export class VesselsTableComponent {
  vessels$: Observable<Vessel[]>;
  constructor(private http: HttpClient) {
    this.vessels$ = this.http.get<Vessel[]>('/api/vessels');
  }
  trackByImo = (_: number, v: Vessel) => v.imo;
}
```

Playwright tests  
ARIA-first  
```ts
import { test, expect } from '@playwright/test';

test('vessel status is visible (ARIA-first)', async ({ page }) => {
  await page.goto('/vessels');
  const table = page.getByRole('table', { name: 'Vessels' });
  const row = table.getByRole('row', { name: /1234567/ }); // 1234567 is an example IMO
  await expect(row.getByRole('cell', { name: 'Online' })).toBeVisible();
});
```

Stable fallback using data-testid (domain IDs)  
```ts
import { test, expect } from '@playwright/test';

test('vessel status is visible (stable fallback)', async ({ page }) => {
  await page.goto('/vessels');
  const table = page.getByTestId('vessels-table');
  const row = table.getByTestId('vessel-row-1234567');
  await expect(row.getByTestId('vessel-status-1234567')).toHaveText('Online');
});
```

Notes:  
- The IMO number is a domain-unique identifier; use it for stable data-testid suffixes (see “Stable IDs for repeated items”).  
- Prefer the ARIA-based test; use the data-testid fallback to resist localization or when multiple similar rows cause ambiguity.  

## Checklist for Agents  
For each interactive element you create or modify:  
- Does it have a clear accessible role (implicit or explicit)?  
- Does it have an accessible name (via visible text, aria-labelledby, or aria-label)?  
- If ambiguity remains, did you add a stable data-testid in kebab-case?  
- If in a list/repeated context, did you suffix the test ID with a unique token?  
- Are you avoiding class- and structure-based selectors in tests?  
- Do tests use getByRole/getByLabel first, then getByTestId, then getByText?


