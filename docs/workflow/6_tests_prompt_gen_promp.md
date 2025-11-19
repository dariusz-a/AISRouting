## Role
You are a senior Automation QA (Quality Assurance) Engineer.  

Your job is to produce a single Tests Generation prompt.

## How to Use This Prompt

Extract the following information:
   - `bdd_spec_files` (one or more BDD spec files containing scenarios for the feature; a feature may reference scenarios from multiple BDD spec files)
   - `tech_design_file` (technical design file for the feature)
   - `test_file` (derived from the BDD spec file(s), with the suffix `_spec` removed; if multiple BDD spec files, clarify naming)
   - `feature_name` (derived from the feature section header, with all spaces, colons and dots replaced by underscores; for example, for a section header `## === Feature 7.1: User Creation ===`, the feature_name should be `Feature_7_1_User_Creation` )

You have **three options** for selecting the feature for the Tests Generation prompt:

### Summary of Options

| Option | Purpose                                         | Trigger                                           | Action                             |
|--------|-------------------------------------------------|---------------------------------------------------|------------------------------------|---------------|
| 1      | **New Tests Generation prompt (auto)**          | First `not started` item                          | Create new Tests Generation prompt |
| 2      | **New Tests Generation prompt (explicit)**      | User specifies feature                            | Create new Tests Generation prompt |
| 3      | **Update existing Tests Generation prompt**     | "new", "changed", or "removed" BDD scenarios      | **UPDATE existing file**           |

---

## Option 1: Automatic Selection (New Tests Generation prompt)

**When to use**: When you want to automatically create a new test generation prompt for the next feature to implement.

**Steps**:
1. Read the implementation plan [implementation_plan.md](../tech_design/implementation_plan.md).
2. Find the first **NEXT FEATURE TO IMPLEMENT** in the implementation plan (first feature with `not started` scenarios).
3. Select the entire feature for the design document, which includes ALL scenarios within that feature section.
4. Use all scenarios within that feature (both `not started` and any other statuses).
5. Use this entire feature as the target for the design document.
6. Reference the corresponding BDD file and all relevant scenarios within that feature.
7. **Create a NEW file** at: `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`
8. Follow the output requirements as below.

**Example**: If Feature 7.1: User Creation is the next feature, create `docs/tech_prompts/Feature_7_1_User_Creation/Feature_7_1_User_Creation_test_files_gen.md`

---

## Option 2: Explicit Selection (New Tests Generation prompt)

**When to use**: When a user explicitly specifies a feature and you want to create a new test generation prompt for it.

**Steps**:
- If a feature is specified, extract the feature section (including all scenarios, BDD spec file, tech design file, and test file) directly from `implementation_plan.md`.
- Do not search the codebase for the feature definition; use only the information in `implementation_plan.md` to determine the feature's boundaries and scenarios.
- Only after extracting the feature definition from `implementation_plan.md`, proceed to inline the referenced BDD scenarios, technical design, and data models from their respective files.
- **Create a NEW file** at: `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`

**Example**: If user specifies "Feature 8.2: Project Staffing", create `docs/tech_prompts/Feature_8_2_Project_Staffing/Feature_8_2_Project_Staffing_test_files_gen.md`

---

## Option 3: Update Existing Tests Generation prompt

**When to use**: When a test generation prompt file already exists for a feature but **BDD scenarios have changed**.

**IMPORTANT**: This option **UPDATES** an existing file, does NOT create a new one.

### Steps for Option 3:

1. **Identify the Target Feature**
   - The user provides `feature_name` or points to changed scenarios.
   - Locate the corresponding feature in the `implementation_plan`.
   - Identify scenarios with status "new", "changed", or "removed".

2. **Locate the Existing File**
   - Find the existing file: `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`
   - **DO NOT create a new file** - update the existing one.

3. **Compare and Analyze Changes**
   - Open and read:
     - The current design: `docs/tech_design/core_features/[file]_design.md`
     - The updated BDD spec: `docs/spec_scenarios/[file]_spec.md`
   - **The existing Tests Generation prompt**: `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`

4. **Update the Existing Tests Generation prompt**
   - **MODIFY the existing file** - do not create a new one
   - Detect impacted tests based on changed scenarios
   - Modify only affected tests and sections
   - Retain existing structure and content where still valid
   - Add new tests and validations as needed
   - Update any outdated references or examples

5. **Add a Changelog Section**
   - At the top of the existing file, add or update:
     ```markdown
     > **Changelog**
     > Updated on: YYYY-MM-DD
     > - Added test design for scenario: "Prevent duplicate email registration"
     > - Updated validation testing for email format requirements
     > - Enhanced role-based skill assignment test patterns
     > - Added new helper functions for robust element interaction
     ```

6. **Save Changes to Existing File**
   - **Save to the SAME file**: `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`
   - **DO NOT create a new file**

### Common Scenarios for Option 3:
- BDD scenarios have been modified or added
- Technical design has been updated
- New validation requirements have been added
- Test patterns need to be enhanced
- Existing tests need to be updated for new functionality

**Example**: If `docs/tech_prompts/Feature_7_1_User_Creation/Feature_7_1_User_Creation_test_files_gen.md` already exists and requirements have changed, update that existing file instead of creating a new one.

---

## Role: Automation QA (Quality Assurance) Engineer

When executing this prompt, you MUST assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, TypeScript, and robust test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.

**Selector Requirement Update:**
When working with elements that contain a `data-testid` attribute, you MUST use `.getByTestId()` instead of `waitForSelector` or any other selector method. Do NOT use `waitForSelector` for elements with `data-testid`. Always prefer `.getByTestId()` for these elements to ensure selector reliability and maintainability.

Assume all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, getByTestId, and other accessibility-first selectors.

## Input Sources

Read the below documents in full.

### Application Architecture 

- Location: `docs/tech_design/overall_architecture.md`

### Playwright Testing Authentication Process ** READ THIS FILE IN FULL AND FOLLOW THE INSTRUCTIONS**
- Location: `docs/tech_design/testing/QA_playwright_authentication.md`

  - Contains the required authentication process and credentials for automated Playwright tests. All test scripts must follow the login flow and use the specified credentials when accessing the application.

### QA ISTQB Standard Best Practices (Labels, IDs, Accessibility)
- Location: `docs/tech_design/testing/QA_ISTQB_standard_best_practices.md`
  - Contains best practices for using labels, aria-labels, IDs, and data-testid attributes in UI development and test automation, aligned with ISTQB standards for quality assurance and accessibility. All test generation prompts and test code must follow these conventions for selector strategy, accessibility, and traceability.
  
### QA Test Locators Instructions
- Location: `docs/tech_design/testing/QA_test_locators_instructions.md`
- Contains detailed instructions and best practices for adding locators in application code and generating Playwright tests, including selector priority, ARIA rules, naming conventions for ARIA labels and data-testid, and robust locator strategies. All test generation prompts and test code must follow these conventions for selector strategy, accessibility, and traceability.

### QA testing documentation
- Location:  `docs/tech_design/testing/QA_testing.md`
   - Testing Strategy
   - Tests Structure
   - Test Data Management
   - Testing Patterns and Best Practices
   - Accessibility Testing Requirements


### Application Layout 

- Location: `docs/tech_design/application_layout.md`

### Application Organization 

- Location: `docs/tech_design/application_organization.md`

### Application Data Models 

- Location: `docs/tech_design/data_models.md`

### Security Architecture and Test Credentials

- Location: `docs/tech_design/security_architecture.md`
- Contains automated testing credentials that MUST be used for all Playwright tests

### Feature Specification

- Location: docs/spec_scenarios/[bdd_spec_file]
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

### Feature Design Document
- Location: docs/tech_design/core_features/[tech_design_file]
   - Feature implementation
   - Business logic
   - Service patterns

## Output Requirements


You MUST create a new file named ([feature_name]_test_files_gen.md) inside the `docs/tech_prompts/[feature_name]/` folder and write the full code generation prompt content to that file, following all the rules below.

If the `docs/tech_prompts/[feature_name]/` folder does not exist, you MUST create it before writing the output file.

You MUST update any existing files with the same name in that folder.

**Additional Requirements:**
- When generating tests, you MUST always check if suitable test data (fixtures) already exist in the project. If compatible test data is available, you MUST reuse it for your tests. Only create new test data if no existing data meets the requirements of the test scenario.
- For testids and aria-labels, you MUST verify whether the related components already define these attributes. If they exist, you MUST use the existing testids and aria-labels in your tests. Do NOT create new testids or aria-labels for elements that already have them defined in the codebase. Only introduce new ones if none exist for the required UI elements.

- If source material is ambiguous, ask a single clear follow-up question and wait.

See the dedicated **Test File Generation Prompt Structure** section below for the required structure and formatting.

### Test File Generation Prompt Structure

Each feature implementation prompt must follow this structure:

```markdown
# Test File Generation Prompt: [Feature Name]

## Task: 
Generate a new file named ([test_file]) or update if it is existing, following the guidelines below. 

The document is purely about test generation, not feature implementation. It's designed to create tests that will guide TDD (Test-Driven Development) implementation.

The application code (components, stores, services) MUST NOT be implemented, not as part of this test generation task.

If the guidelines are ambiguous you MUST ask a single clear follow-up question and wait.

## Role: 
When executing this prompt, you MUST assume the role of an Automation QA (Quality Assurance) Engineer with expertise in Playwright, TypeScript, and robust test design. You are responsible for translating BDD scenarios into robust, maintainable Playwright tests, applying accessibility-first selector strategies, and ensuring all code aligns with project technical constraints and best practices.
Assume all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, and other accessibility-first selectors.

**Selector Requirement Update:**
Follow the detailed selector requirements and testing framework specifications in the `docs/tech_design/overall_architecture.md` and the `docs/tech_design/testing/` directory. **THESE FILES MUST ALWAYS BE USED AS A MANDATORY INPUT REQUIREMENT. Read all the files in full and always follow the exact instructions in them.**
**Always check the application code for existing selectors before writing or updating tests. If you find test ids of the elements needed for the test, reuse them and do not create new.**


## References
- BDD Scenarios: docs/spec_scenarios/[bdd_spec_file]
- Test File: tests/[test_file]
- Technical Design Document: docs/tech_design/core_features/[tech_design_file]
- Implementation Plan: docs/tech_prompts/[feature_name]/[feature_name]_implementation_plan.md
- Application Architecture `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`

## Output Requirements

The output document must be placed in a feature-specific folder. If that folder does not exist, it must be created.

1. Determine `feature_name` exactly as defined earlier in this document.
   - Normalize `feature_name` for filesystem use (letters, numbers, underscores only).

2. Create the output directory (if missing):
   - `docs/tech_prompts/[feature_name]/`

3. Write the Test File Generation Prompt (create or update in place) at:
   - `docs/tech_prompts/[feature_name]/[feature_name]_test_files_gen.md`

4. If the file already exists at that exact path, update it in place. Do not create duplicates elsewhere.


## Test File Structure with authentication setup

## Tests Structure

## Test Data Management 
Follow the detailed requirements and code examples in the "Test Data Management" section of `docs/tech_design/testing/QA_testing.md`. 
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
**THESE FILES MUST ALWAYS BE USED AS A MANDATORY INPUT REQUIREMENT.**

## Critical Selector Strategy Updates with ❌/✅ examples

## Test Patterns with complete code examples according to project architecture and testing documentation

## Locator Patterns specific to the feature as defined in project architecture and testing documentation
...

## Common Actions with helper functions

## Test Implementation Guidelines with conditional logic

## Character Limit Testing Pattern addressing browser behavior

## Practical Validation Testing with realistic expectations

## Success Criteria for the generated test file

## Critical Selector Strategy Updates: 
Show what to avoid and what to use instead

## Test Patterns: 
Complete code examples for conditional interactions

## Practical Validation Testing: 
Realistic expectations vs assumed features

## Character Limit Testing Pattern: 
Browser behavior vs validation messages

## Async Operations and Waiting: 
Proper handling with conditional logic

## Test Isolation

## Required Content

```

### Required Content Extraction for Test Generation Prompts

The test generation prompt must include these specific sections with extracted content:



#### 1. BDD Scenarios (Full Text)

Extract and include the complete BDD scenarios for the selected feature ONLY, as obtained from the implementation plan. Even though the `[bdd_spec_file]` file may contain additional scenarios, those are for future features and MUST NOT be included. Only scenarios that are part of the current implementation plan for the selected feature should be used.

- ONLY feature-related positive scenarios with Given/When/Then steps related to the selected feature ONLY (from the implementation plan)
- ONLY feature-related negative scenarios with validation and error cases for the selected feature ONLY (from the implementation plan)
- Complete scenario context including data tables and examples for the selected feature ONLY (from the implementation plan)
- Do NOT just reference the file – include the full scenario text for the selected feature ONLY

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
   - Follow the test structure and patterns defined in `docs/tech_design/overall_architecture.md` and the `docs/tech_design/testing/` directory
   - Include proper authentication setup as specified in the project's architecture documentation
   - Use the test framework and methods specified in the project's architecture documentation

2. Critical Selector Strategy Guidelines - `QA_testing.md` file contains the most important selector requirements.
   - **You MUST always check the application code for existing selectors before writing or updating tests.**

3. Test Patterns
   - **Element Counting and Conditional Logic**: Provide helper functions for safe element interaction
   - **Error Handling Patterns**: Show how to use proper error handling
   - Include complete code examples of interaction patterns

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


6. Test Implementation Guidelines
   Convert the specific BDD scenarios to tests using robust patterns as defined in the project's architecture and testing documentation. The test file should:
   - Update already written tests related to the functionality if any exist
   - Always check the application code for existing selectors before writing or updating tests
   - If compatible test data is available, you MUST reuse it for your tests. **DO NOT create a new test data file if an appropriate one already exists**
   - **ALWAYS check if suitable test data files (fixtures) already exist in the project before creating new ones**
   - Focus only on the single test file for this feature
   - Maintain test isolation
   - Handle async operations properly
   - Include proper waiting strategies as specified in the project's architecture documentation
   - Provide helper functions for safe element interaction
   - Use the Feature: line as the outer describe() block
   - Convert each Scenario: into a separate test() block
   - Implement Given/When/Then steps using the testing framework's logic
   - Use standard commands from the testing framework as defined in the project documentation
   - Simulate realistic user behavior
   - Assume UI elements will be built to support these tests

## Validation Step (Mandatory)

Before finalizing the generated test file prompt file named ([feature_name]_test_files_gen.md) in the `docs/tech_prompts/[feature_name]/` folder, you MUST validate the output against the following success criteria:

### Success Criteria Checklist
1. Covers ONLY feature-related positive scenarios from the BDD file using robust, unambiguous selectors as defined in the project's architecture and testing documentation.
2. Covers ONLY feature-related negative/validation scenarios with realistic expectations about what's implemented.
3. Uses accessibility-first, specific selectors with explicit guidance on avoiding ambiguity, following the project's architecture and testing documentation.
4. Includes proper async handling as specified in the project's architecture documentation.
5. Maintains test isolation.
6. Follows established test patterns with comprehensive examples as defined in the project's architecture and testing documentation.
7. Guides TDD implementation.
8. Includes all Required Code Examples in alignment with the project's testing framework.
9. Provides practical implementation considerations.
10. Provides complete code examples for all recommended patterns.

### Validation Process
- You must explicitly confirm that each success criterion is met.
- If any criterion is not met, you must revise the output to address the issue before finalizing it.
- The output will not be considered complete until it passes this validation step.
