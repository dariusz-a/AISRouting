# Test File Generation Prompt: Feature 7.3: Person Profile

## Task: 
Generate a new file named (manage_people.spec.ts) or update if it is existing, following the guidelines below. 

If the guidelines are ambiguous you MUST ask a single clear follow-up question and wait.

## Role: 
When executing this prompt, you MUST assume the role Assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, Vue, TypeScript, and resilient test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.
Assume all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, and other accessibility-first selectors.

## References
- BDD Scenarios: docs/spec_scenarios/manage_people_spec.md
- Test File: tests/manage_people.spec.ts
- Technical Design Document: docs/tech_design/core_features/people_management_design.md
- Implementation Plan: docs/tech_prompts/manage_people_implementation_plan.md

## Output Requirements

You MUST update any existing files with the same name.

## Test File Structure with authentication setup

## Test Data Management 
Follow the detailed requirements and code examples in the "Test Data Management" section of `docs/tech_design/application_organization.md`. 
- Create a dedicated test data fixture file for the feature (e.g., `tests/fixtures/peopleTestData.ts`).
- Provide code examples for:
  - Defining default test data objects (e.g., people, teams, roles).
  - Helper functions for dynamic test data creation (e.g., `createTestPerson()`).
- Show how to import and use fixture data in Playwright tests.
- Describe strategies for:
  - Direct fixture import.
  - Dynamic creation with unique values (e.g., using timestamps).
  - Helper functions for common test data patterns.
- Advise on referencing related entities (e.g., teams, roles, security groups) from their own fixture files.
- Include guidance for cleaning up test data after each test (e.g., using `test.afterEach()`).
- Note considerations for test data persistence and isolation (e.g., local storage, unique names).

## Critical Selector Strategy Updates with ❌/✅ examples

## Resilient Test Patterns with complete code examples

## Locator Patterns specific to the feature
...

## Common Actions with helper functions

## Test Implementation Guidelines with conditional logic

## Character Limit Testing Pattern addressing browser behavior

## Practical Validation Testing with realistic expectations

## Success Criteria for the generated test file

## Critical Selector Strategy Updates: 
Show what to avoid and what to use instead

## Resilient Test Patterns: 
Complete code examples for conditional interactions

## Practical Validation Testing: 
Realistic expectations vs assumed features

## Character Limit Testing Pattern: 
Browser behavior vs validation messages

## Async Operations and Waiting: 
Proper handling with conditional logic

## Test Isolation: 
While maintaining resilience to missing features

## Code Examples

### Required Content Extraction for Test Generation Prompts

The test generation prompt must include these specific sections with extracted content:

#### 1. BDD Scenarios (Full Text)
Extract and include the complete BDD scenarios from the specification file:
- All positive scenarios with Given/When/Then steps
- All negative scenarios with validation and error cases
- Complete scenario context including data tables and examples
- Do NOT just reference the file - include the full scenario text

#### 2. Technical Design Summary
Extract and include key technical design information:
- Feature architecture overview
- Data models and interfaces relevant to the feature
- Component integration patterns
- Service layer implementation details
- Validation rules and business logic
- Do NOT just reference the design document - inline the relevant sections

#### 3. Data Models (Inline)
Extract and include relevant data model definitions:
- TypeScript interfaces for entities involved in the feature
- Relationship definitions between entities
- Storage structure and persistence patterns
- Mock data structure examples

### Content Guidelines for Test Files Generation

The test files generation prompt must include:
1. Test File Structure specific to that BDD scenario
   ```typescript
   import { test, expect } from '@playwright/test';
   
   test.describe('Feature: Person Profile', () => {
     // Setup authentication for all tests
     test.beforeEach(async ({ page }) => {
       await page.goto('/login');
       await page.getByLabel('Email').fill('alice.smith@company.com');
       await page.getByLabel('Password').fill('SecurePass123!');
       await page.getByRole('button', { name: 'Login' }).click();
       await page.waitForURL('/dashboard');
     });
     
     test('Scenario: Implement Person\'s Profile', async ({ page }) => {
       // Test steps for this specific scenario
     });
   });
   ```

2. Critical Selector Strategy Guidelines
   - **Selector Precision Requirements**: Include explicit guidance on avoiding ambiguous selectors
   - **❌ Avoid Ambiguous Selectors**: Warn against selectors that can match multiple elements
   - **✅ Use Specific Selectors**: Recommend precise alternatives (e.g., `getByRole('heading')` for titles, `getByLabel()` for form inputs)
   - **Form Implementation Reality**: Address common misconceptions (e.g., text inputs vs select dropdowns)
   - Include examples of what NOT to do alongside correct patterns

3. Resilient Test Patterns
   - **Handle Missing Elements Gracefully**: Include patterns for conditional interactions
   - **Element Counting and Conditional Logic**: Provide helper functions for safe element interaction
   - **Error Handling Patterns**: Show how to use `.catch(() => false)` and conditional checks
   - **Always Verify Base State**: Ensure tests always verify core functionality regardless of conditional outcomes

4. Locator Patterns specific to the feature
   - Navigation patterns for this feature
   - Form interactions with realistic implementation details
   - User actions relevant to this BDD scenario
   - Assertions specific to this feature
   - Helper functions for common actions in this feature

5. Practical Testing Approach
   - **Test What's Actually There**: Focus on form interaction over immediate persistence
   - **Realistic Validation Expectations**: Distinguish between implemented vs assumed validation
   - **Character Limit Testing**: Account for browser `maxlength` behavior vs validation messages
   - **Conditional Feature Testing**: Handle features that may not be fully implemented
   - Balance between idealistic scenarios and practical implementation state

6. Test Implementation Guidelines
   Convert the specific BDD scenarios to Playwright tests using resilient patterns. The Playwright test file should:
   - Focus only on the single test file for this feature
   - Maintain test isolation while being resilient to missing features
   - Handle async operations properly with conditional logic
   - Include proper waiting strategies and error handling
   - Provide helper functions for safe element interaction
   - Use the Feature: line as the outer describe() block
   - Convert each Scenario: into a separate test() block
   - Implement Given/When/Then steps using Playwright test logic
   - Use standard Playwright commands (page.goto(), page.locator(), page.fill(), page.click(), etc.)
   - Infer selectors based on best practices and visible text (e.g., page.getByRole('button', { name: 'Save' }))
   - Simulate realistic user behavior
   - Assume UI elements will be built to support these tests   

7. The test generation prompt must include a copy of these specific code examples from the "Testing Patterns" section of the [Overall Architecture](../tech_design/overall_architecture.md) document:

   1. **Helper Functions for Safe Element Interaction**
   2. **Resilient Element Counting Pattern**
   3. **Character Limit Testing Pattern**
   4. **Conditional Feature Testing Pattern**

## BDD Scenarios (Full Text)

### Scenario: Implement Person's Profile
  Given a user is on the People Directory page
  When they click on on person's name
  Then the Person's Profile for the selected person will open
  And they should see:
    | Tab                    | Content Description                          |
    | Summary                | Basic details like name, email.              |
    | Roles                  | List of assigned roles                       |
    | Skills.                | Skills associated with the person            |
    | Security.              | Current security group                       |

## Technical Design Summary

### Feature Architecture Overview
The Person Profile feature provides a comprehensive view of a person's information through a tabbed interface. This feature addresses the need for users to access detailed information about individuals in the system, including their basic details, organizational roles, skill assessments, and security configuration.

### Component Architecture
The Person Profile uses a **hierarchical component structure** where the main profile component serves as a coordinator for multiple specialized tab components:

```
PersonProfile (Container)
├── PersonSummaryTab (Basic Information)
├── PersonRolesTab (Role Assignments)
├── PersonSkillsTab (Skill Assessments)
└── PersonSecurityTab (Security Information)
```

### Tabbed Interface Design
The tabbed interface uses **PrimeVue v4 Tabs components** to provide a modern, accessible user experience:

1. **Summary Tab**: Provides an overview of essential person information
2. **Roles Tab**: Shows organizational role assignments and their implications
3. **Skills Tab**: Displays skill assessments organized by categories
4. **Security Tab**: Presents security and access control information

### Data Integration Strategy
The Person Profile integrates with multiple services to gather comprehensive information about a person:
- **People Service**: Core person information and relationships
- **Teams Service**: Team membership and organizational structure
- **Roles Service**: Role assignments and associated skills
- **Skills Service**: Skill categories and assessment data
- **User Service**: Login access and account information
- **Security Groups Service**: Permissions and access control

## Data Models (Inline)

### Person Interface
```typescript
interface Person {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  teamId?: string;
  roleIds: string[];
  skillIds: string[];
  userId?: string;
  createdAt: Date;
  updatedAt: Date;
}
```

### Tab Component Props
```typescript
interface PersonTabProps {
  person: Person | null;
}
```

### Navigation Pattern
The Person Profile is accessible through navigation from the People Directory by clicking on a person's name, following the **drill-down pattern**.

## Test File Structure with authentication setup

```typescript
import { test, expect } from '@playwright/test';
import { peopleTestData } from '../fixtures/peopleTestData';

test.describe('Feature: Person Profile', () => {
  // Setup authentication for all tests
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill('alice.smith@company.com');
    await page.getByLabel('Password').fill('SecurePass123!');
    await page.getByRole('button', { name: 'Login' }).click();
    await page.waitForURL('/dashboard');
  });
  
  test('Scenario: Implement Person\'s Profile', async ({ page }) => {
    // Test implementation for Person Profile scenario
  });
});
```

## Critical Selector Strategy Updates: 
Show what to avoid and what to use instead

### ❌ Avoid Ambiguous Selectors
```typescript
// ❌ DON'T: Use generic selectors that can match multiple elements
await page.locator('button').click();
await page.locator('.tab').click();
await page.locator('div').click();

// ❌ DON'T: Use text content that might change
await page.getByText('John Doe').click();
await page.locator('text=Profile').click();
```

### ✅ Use Specific Selectors
```typescript
// ✅ DO: Use role-based selectors for interactive elements
await page.getByRole('button', { name: 'Edit Person' }).click();
await page.getByRole('tab', { name: 'Summary' }).click();

// ✅ DO: Use label-based selectors for form elements
await page.getByLabel('Email').fill('test@example.com');
await page.getByLabel('First Name').fill('John');

// ✅ DO: Use heading selectors for titles
await page.getByRole('heading', { name: 'Person Profile' });
```

### Form Implementation Reality
- Person names in the directory are typically clickable links or buttons
- Tab interfaces use semantic tab components with proper ARIA attributes
- Profile information is displayed in structured layouts with clear labels

## Resilient Test Patterns: 
Complete code examples for conditional interactions

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

// Helper function for resilient interaction - handles missing elements gracefully
async function interactWithElementIfExists(page, elementTestId, actionName) {
  const element = page.getByTestId(elementTestId);
  const elementExists = await elementExistsSafely(element);

  if (elementExists) {
    await element.hover();
    const actionButton = page.getByRole('button', { name: `${actionName} ${elementTestId.replace('prefix-', '')}` });
    const buttonExists = await elementExistsSafely(actionButton);

    if (buttonExists) {
      if (actionName === 'Delete') {
        await expect(actionButton).toBeDisabled();
      } else {
        await actionButton.click();
      }
      return true;
    }
  }
  return false;
}
```

### Resilient Element Counting Pattern
```typescript
async function getElementsCount(page) {
  const elements = page.locator('[data-testid*="prefix-"]');
  return await elements.count();
}
```

### Conditional Feature Testing Pattern
```typescript
test('Scenario: Conditional feature interaction', async ({ page }) => {
  const elementCount = await getElementsCount(page);

  if (elementCount > 0) {
    // Interact with elements if they exist
    const firstElement = page.locator('[data-testid*="prefix-"]').first();
    const isVisible = await elementExistsSafely(firstElement);
    if (isVisible) {
      await firstElement.hover();
    }
  }

  // Always verify base state regardless of conditional outcomes
  await expect(page.getByText('Main Page')).toBeVisible();
});
```

## Locator Patterns specific to the feature

### Navigation Patterns
```typescript
// Navigate to People Directory
await page.goto('/people');

// Click on person's name to open profile
await page.getByRole('link', { name: 'John Doe' }).click();
// OR if implemented as button
await page.getByRole('button', { name: 'John Doe' }).click();
```

### Tab Interaction Patterns
```typescript
// Switch between profile tabs
await page.getByRole('tab', { name: 'Summary' }).click();
await page.getByRole('tab', { name: 'Roles' }).click();
await page.getByRole('tab', { name: 'Skills' }).click();
await page.getByRole('tab', { name: 'Security' }).click();
```

### Profile Content Verification
```typescript
// Verify profile header
await expect(page.getByRole('heading', { name: 'John Doe' })).toBeVisible();

// Verify tab content
await expect(page.getByRole('tabpanel')).toContainText('Basic details');
```

## Common Actions with helper functions

### Navigate to Person Profile
```typescript
async function navigateToPersonProfile(page, personName) {
  await page.goto('/people');
  await page.getByRole('link', { name: personName }).click();
  await page.waitForURL(/\/people\/\d+/);
}
```

### Verify Profile Tabs
```typescript
async function verifyProfileTabs(page) {
  const expectedTabs = ['Summary', 'Roles', 'Skills', 'Security'];
  
  for (const tabName of expectedTabs) {
    const tab = page.getByRole('tab', { name: tabName });
    const tabExists = await elementExistsSafely(tab);
    
    if (tabExists) {
      await expect(tab).toBeVisible();
    }
  }
}
```

### Verify Profile Content
```typescript
async function verifyProfileContent(page, personData) {
  // Verify basic information
  await expect(page.getByText(personData.firstName)).toBeVisible();
  await expect(page.getByText(personData.lastName)).toBeVisible();
  await expect(page.getByText(personData.email)).toBeVisible();
}
```

## Test Implementation Guidelines with conditional logic

### Person Profile Test Implementation
```typescript
test('Scenario: Implement Person\'s Profile', async ({ page }) => {
  // Given a user is on the People Directory page
  await page.goto('/people');
  await expect(page.getByRole('heading', { name: 'People Directory' })).toBeVisible();
  
  // When they click on a person's name
  const personName = 'John Doe';
  const personLink = page.getByRole('link', { name: personName });
  const linkExists = await elementExistsSafely(personLink);
  
  if (linkExists) {
    await personLink.click();
    
    // Then the Person's Profile for the selected person will open
    await page.waitForURL(/\/people\/\d+/);
    await expect(page.getByRole('heading', { name: personName })).toBeVisible();
    
    // And they should see the tabbed interface
    await verifyProfileTabs(page);
    
    // Verify Summary tab content
    await page.getByRole('tab', { name: 'Summary' }).click();
    await expect(page.getByRole('tabpanel')).toContainText('Basic details');
    
    // Verify other tabs exist (conditional on implementation)
    const rolesTab = page.getByRole('tab', { name: 'Roles' });
    if (await elementExistsSafely(rolesTab)) {
      await rolesTab.click();
      await expect(page.getByRole('tabpanel')).toBeVisible();
    }
    
    const skillsTab = page.getByRole('tab', { name: 'Skills' });
    if (await elementExistsSafely(skillsTab)) {
      await skillsTab.click();
      await expect(page.getByRole('tabpanel')).toBeVisible();
    }
    
    const securityTab = page.getByRole('tab', { name: 'Security' });
    if (await elementExistsSafely(securityTab)) {
      await securityTab.click();
      await expect(page.getByRole('tabpanel')).toBeVisible();
    }
  } else {
    // Handle case where person profile feature is not implemented
    console.log('Person profile navigation not implemented yet');
  }
  
  // Always verify we're still on a valid page
  await expect(page).toHaveURL(/\/people/);
});
```

## Character Limit Testing Pattern addressing browser behavior

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

## Practical Validation Testing with realistic expectations

### Realistic Profile Content Expectations
```typescript
// Test what's actually implemented vs assumed features
async function verifyProfileImplementation(page) {
  // Always verify basic navigation works
  await expect(page.getByRole('heading')).toBeVisible();
  
  // Conditionally test tabbed interface
  const tabsContainer = page.locator('[role="tablist"]');
  const tabsExist = await elementExistsSafely(tabsContainer);
  
  if (tabsExist) {
    await verifyProfileTabs(page);
  } else {
    // Fallback: verify basic profile content
    await expect(page.getByText('Profile')).toBeVisible();
  }
}
```

## Async Operations and Waiting: 
Proper handling with conditional logic

### Waiting Strategies for Profile Navigation
```typescript
async function waitForProfileLoad(page) {
  // Wait for profile content to load
  await page.waitForLoadState('networkidle');
  
  // Wait for profile header to be visible
  await page.waitForSelector('h1', { timeout: 5000 }).catch(() => {
    // Handle case where profile header doesn't exist
    console.log('Profile header not found');
  });
}
```

## Test Isolation: 
While maintaining resilience to missing features

### Test Data Management
```typescript
// Use test fixtures for consistent data
import { peopleTestData } from '../fixtures/peopleTestData';

test.beforeEach(async ({ page }) => {
  // Setup test data
  const testPerson = peopleTestData.createTestPerson();
  
  // Navigate to people directory
  await page.goto('/people');
});

test.afterEach(async ({ page }) => {
  // Clean up test data if needed
  // Most tests use read-only operations, so cleanup may not be necessary
});
```

## Success Criteria for the generated test file

### Success Criteria Checklist
1. ✅ Covers the Person Profile scenario using robust, unambiguous selectors
2. ✅ Uses accessibility-first, specific selectors with explicit guidance on avoiding ambiguity
3. ✅ Includes proper async handling and conditional logic for missing elements
4. ✅ Maintains test isolation while being resilient to missing or incomplete features
5. ✅ Follows established test patterns with comprehensive resilient patterns
6. ✅ Guides TDD implementation without breaking on incomplete features
7. ✅ Includes all Required Code Examples
8. ✅ Balances idealistic BDD scenarios with practical implementation considerations
9. ✅ Provides complete code examples for all recommended patterns
10. ✅ Handles navigation from People Directory to Person Profile gracefully

### Validation Process
- ✅ Each success criterion is explicitly confirmed as met
- ✅ The output passes validation step before finalizing
- ✅ All required testing patterns are included
- ✅ Conditional logic handles missing features gracefully
- ✅ Selector strategy provides clear guidance on best practices

## Required Code Examples

### 1. Helper Functions for Safe Element Interaction
```typescript
// Helper function to safely check if element exists without throwing errors
async function elementExistsSafely(locator) {
  try {
    return await locator.isVisible();
  } catch {
    return false;
  }
}

// Helper function for resilient interaction - handles missing elements gracefully
async function interactWithElementIfExists(page, elementTestId, actionName) {
  const element = page.getByTestId(elementTestId);
  const elementExists = await elementExistsSafely(element);

  if (elementExists) {
    await element.hover();
    const actionButton = page.getByRole('button', { name: `${actionName} ${elementTestId.replace('prefix-', '')}` });
    const buttonExists = await elementExistsSafely(actionButton);

    if (buttonExists) {
      if (actionName === 'Delete') {
        await expect(actionButton).toBeDisabled();
      } else {
        await actionButton.click();
      }
      return true;
    }
  }
  return false;
}
```

### 2. Resilient Element Counting Pattern
```typescript
async function getElementsCount(page) {
  const elements = page.locator('[data-testid*="prefix-"]');
  return await elements.count();
}
```

### 3. Character Limit Testing Pattern
```typescript
test('Scenario: Exceed character limits', async ({ page }) => {
  const longInput = 'A'.repeat(81);
  await page.getByLabel('Input Field').fill(longInput);

  // Browser enforces maxlength, no validation message needed
  const expectedValue = 'A'.repeat(80);
  await expect(page.getByLabel('Input Field')).toHaveValue(expectedValue);
});
```

### 4. Conditional Feature Testing Pattern
```typescript
test('Scenario: Conditional feature interaction', async ({ page }) => {
  const elementCount = await getElementsCount(page);

  if (elementCount > 0) {
    // Interact with elements if they exist
    const firstElement = page.locator('[data-testid*="prefix-"]').first();
    const isVisible = await elementExistsSafely(firstElement);
    if (isVisible) {
      await firstElement.hover();
    }
  }

  // Always verify base state regardless of conditional outcomes
  await expect(page.getByText('Main Page')).toBeVisible();
});
``` 