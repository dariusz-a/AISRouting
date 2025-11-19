# Testing Organization


## Testing Strategy

1. **E2E Testing with Playwright**
   - Test-first development approach
   - Comprehensive test coverage
   - Centralized test fixtures in `tests/fixtures/` directory
   - Single source of truth for prototype and test data
   - Cross-browser testing

2. **Test Data Management**
   - Centralized fixtures approach using TypeScript files
   - Type-safe test data with interfaces
   - Shared constants for consistent category names
   - Helper functions for test data manipulation
   - Services import from test fixtures for prototype functionality

## Tests Structure
```
./                      # Project root (where this QA_testing.md is located)
  ├── src/               # Source code
  │   ├── mocks/        
        └── mockData.ts  # Centralized mock data and helper functions
  ├── tests/            # Test files
        ├── fixtures/    # Test fixtures importing from src/mocks/mockData.ts
        └── manage_skills.spec.ts  # Feature test files
```

## Test Data Management

### Centralized Mock Data Approach

To ensure consistency and maintainability, all mock data for services and test cases is now centralized in a single file: `src/mocks/mockData.ts`.

#### Benefits of Centralized Mock Data
- **Single Source of Truth**: All mock data is stored in one location, reducing duplication and ensuring consistency across the application.
- **Reusability**: Mock data can be easily imported and reused in both services and test cases.
- **Maintainability**: Updates to mock data are made in one place, simplifying maintenance.
- **Type Safety**: The mock data adheres to TypeScript interfaces, ensuring type safety.

#### Example Usage

**In Services:**
```typescript
import { getMockPeopleRecord } from '../mocks/mockData';

const peopleRecord = getMockPeopleRecord();
storage.setReactive('people', peopleRecord);
```

**In Fixtures**
```typescript
import { mockPeople } from '../../src/mocks/mockData';

export const defaultPeople = mockPeople;
```


**In Test Cases:**
```typescript
// tests/manage_skills.spec.ts
import { SKILL_CATEGORIES } from './fixtures/skillsTestData';

test('Category filtering', async ({ page }) => {
  await page.getByRole('button', { 
    name: SKILL_CATEGORIES.WEB_DEVELOPMENT 
  }).click();
});
```

#### File Structure
The `mockData.ts` file contains mock data for all entities used in the application, such as `Person`, `User`, and others. It also provides helper functions to transform the data into formats suitable for specific use cases.

## Testing Organization

### E2E Test Structure
```typescript
// Login test example
test('successful login flow', async ({ page }) => {
  await page.goto('/auth/login');
  await page.fill('[data-test="email-input"]', 'alice.smith@company.com');
  await page.fill('[data-test="password-input"]', 'SecurePass123!');
  await page.click('[data-test="login-button"]');
  await expect(page).toHaveURL('/dashboard');
});
```

## Testing Patterns

### Helper Functions for Safe Element Interaction
```typescript
// Helper function to safely check if element exists without throwing errors
async function elementExistsSafely(locator) {
  try {
    return await locator.isVisible();
  } catch {
    return false;
  }
}

```

### Character Limit Testing Pattern
```typescript
test('Scenario: Exceed character limits', async ({ page }) => {
  const longInput = 'A'.repeat(81);
  await page.getByLabel('Input Field').fill(longInput);

  // Browser enforces maxlength, no validation message needed
  const expectedValue = 'A'.repeat(80);
  await expect(page.getByLabel('Input Field')).toHaveValue(expectedValue);
});
```

### Conditional Feature Testing Pattern
```typescript
test('Scenario: Conditional feature interaction', async ({ page }) => {
  const elementCount = await getElementsCount(page);

  if (elementCount > 0) {
    // Interact with elements if they exist
    const firstElement = page.getByTestId('[data-testid*="prefix-"]').first();
    const isVisible = await elementExistsSafely(firstElement);
    if (isVisible) {
      await firstElement.hover();
    }
  }

  // Always verify base state regardless of conditional outcomes
  await expect(page.getByText('Main Page')).toBeVisible();
});
```
