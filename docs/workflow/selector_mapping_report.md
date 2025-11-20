Selector Mapping Report

Summary:
- I scanned the repository for `data-testid="..."` and `aria-label="..."` occurrences.
- No actual test files (Playwright/Jest) or application HTML source files were found in the repository root — most matches are in documentation files under `docs/`.
- As a result, there are no test-to-application mappings to reconcile; the selectors appear only in docs/examples, not in runnable tests or app code.

Extracted selectors (unique) and source locations:

- `save-profile-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `open-settings-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `export-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `user-form` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `first-name-input` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `email-input` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `save-user-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `country-select` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `city-combobox` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `plan-radiogroup` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `notifications-switch` (aria-label: "Enable notifications") : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `delete-dialog` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `confirm-delete-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `cancel-delete-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `orders-table` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `main-nav` (aria-label: "Main") : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `nav-home` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `nav-products` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `header-logo` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `refresh-btn` (aria-label: "Refresh data") : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `product-list` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `product-card-42` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `add-to-cart-42` (aria-label: "Add Widget to cart") : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `login-section` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `login-form` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `username-input` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `password-input` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `submit-login-btn` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `vessels-table` : `docs/tech_design/testing/QA_test_locators_instructions.md`
- `pin-mv-aurora` (aria-label: "Pin MV Aurora") : `docs/tech_design/testing/QA_ISTQB_standard_best_practices.md`
- `vessel-item-mv-aurora` : `docs/tech_design/testing/QA_ISTQB_standard_best_practices.md`
- `input-folder-button`, `ship-combo`, `no-vessels-warning`, `select-input-folder` : `docs/tech_design/core_features/getting_started_design.md`
- `search-vessels` (aria-label: "Search vessels") : `docs/tech_design/testing/QA_ISTQB_standard_best_practices.md`

Mapping status and next actions:
- Tests found: 0 (no Playwright/Jest test files discovered).
- Application HTML/JSX files found: 0 (no HTML or UI source files discovered containing these selectors).
- Therefore: every selector occurrence exists only in documentation and examples. There are no mismatches to patch between test files and app source in this repository.

Recommendations (choose one):
- I can scaffold Playwright tests that use the documented `data-testid` values (adds runnable tests to `tests/`), or
- I can prepare a patch that injects the documented `data-testid`/`aria-label` attributes into application HTML/components — if you provide the app source location or confirm where to add them, or
- If the application and tests live in another repo/monorepo, I can help generate a precise mapping report (this file) and a minimal PR patch to apply once given the code paths.

What I did now:
- Created this mapping report at `docs/workflow/selector_mapping_report.md`.

What I suggest next:
- Tell me whether you want me to scaffold tests (`Playwright`) using these selectors, or to wait until you point me at the application UI files to align attributes there.
