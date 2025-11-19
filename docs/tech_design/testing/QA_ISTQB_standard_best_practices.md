# QA Specification: Best Practices for Labels, IDs, Logging, and Assertions (ISTQB-Aligned)

## Purpose
This document defines best practices for using labels, aria-labels, IDs, logging, and assertions in UI development and test automation, aligned with ISTQB standards for quality assurance and accessibility.

---

## 1. General Principles
- All interactive elements (inputs, buttons, links, etc.) must have clear, descriptive, and unique labels or aria-labels.
- IDs and data-testid attributes must be unique within the DOM and follow a consistent naming convention.
- Use semantic HTML elements and proper ARIA attributes to maximize accessibility and test reliability.

---

## 2. Label and Aria-Label Best Practices
- **Clarity**: Use concise, descriptive text that accurately reflects the element’s purpose (e.g., `aria-label="Search vessels"`).
- **Uniqueness**: Each label or aria-label should be unique within its context to avoid ambiguity.
- **Readability**: Use sentence case (capitalize only the first word and proper nouns).
- **Avoid Jargon**: Do not use abbreviations or acronyms unless they are widely recognized by your users.
- **No Redundancy**: Do not duplicate visible text in aria-labels unless necessary for screen readers.
- **Accessibility**: Always provide labels for form fields and controls, using `<label for="...">` or `aria-label` as appropriate.
- **Localization**: Ensure labels are localizable for internationalization.

---

## 3. ID and data-testid Best Practices
- **Uniqueness**: IDs and data-testid values must be unique within the page.
- **Naming Convention**: Use kebab-case or lower_snake_case (e.g., `vessel-list-item`, `pin-mv-aurora`).
- **Descriptive**: IDs should describe the element’s role or content (e.g., `id="search-vessels"`).
- **Testing**: Use `data-testid` for test selectors instead of IDs when possible to avoid conflicts with production code.
- **No Dynamic IDs**: Avoid using dynamically generated IDs that change between renders or sessions.

---

## 4. ISTQB Standards Alignment
- **Traceability**: Ensure all labels and IDs can be traced to requirements and test cases.
- **Testability**: Use accessibility-first selectors (getByLabel, getByRole) in tests for robust automation.
- **Maintainability**: Consistent naming and labeling make tests easier to maintain and update.
- **Accessibility**: Follow WCAG and ARIA guidelines to ensure all users, including those using assistive technologies, can interact with the UI.
- **Documentation**: Document all label and ID conventions in the project QA specification.

---

## 5. Example Patterns

### Good Examples
- `<label for="search-vessels">Search vessels</label>`
- `<input id="search-vessels" aria-label="Search vessels">`
- `<button aria-label="Pin MV Aurora" data-testid="pin-mv-aurora">📌</button>`
- `<div data-testid="vessel-item-mv-aurora">...</div>`

### Bad Examples
- `<label>Click here</label>` (ambiguous)
- `<input id="123">` (non-descriptive)
- `<button>Pin</button>` (not unique, not accessible)
- `<div id="item1">...</div>` (non-descriptive, not unique)

---

## 6. Logging Best Practices (ISTQB-Aligned)
- **Minimal Console Logging in Tests**: Avoid using `console.log` in test code except for debugging. Remove or comment out logs before finalizing test suites.
- **Structured Logging**: Use structured logging frameworks for application logs, ensuring logs are meaningful, consistent, and traceable to test cases or requirements.
- **Error and Event Logging**: Log only relevant errors, warnings, and significant events. Avoid logging sensitive data.
- **Log Levels**: Use appropriate log levels (info, warning, error) and ensure logs can be filtered by level.
- **Test Report Integration**: Prefer test framework reporting (e.g., Playwright, Jest) over manual logs for test results and failures.

---

## 7. Assertion Best Practices (ISTQB-Aligned)
- **Clear Assertions**: Write assertions that clearly state the expected outcome, using descriptive messages where possible.
- **Single Responsibility**: Each assertion should check one condition to make failures easy to diagnose.
- **Accessibility-First**: Assert on visible text, roles, and accessible names to ensure UI is usable for all users.
- **Traceability**: Link assertions to requirements or acceptance criteria for full coverage.
- **Avoid Over-assertion**: Do not assert on implementation details that may change (e.g., CSS classes) unless required by the specification.
- **Fail Fast**: Use assertions early in test steps to catch issues as soon as they occur.

---

## 8. Review and Continuous Improvement
- Regularly review labels, IDs, logs, and assertions for clarity, uniqueness, and accessibility.
- Update this specification as new requirements or standards emerge.

---

## References
- ISTQB Foundation Level Syllabus
- WCAG 2.1 Guidelines
- ARIA Authoring Practices
- Project Technical Design Documents
