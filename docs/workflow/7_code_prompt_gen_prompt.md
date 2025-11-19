# Prompt for generating the code-gen prompt for one Iteration Feature

## How to Use This Prompt

Extract the following information:
   - `bdd_spec_file` (one or more BDD spec files containing scenarios for the feature; a feature may reference scenarios from multiple BDD spec files)
   - `tech_design_file` (technical design file for the feature)
   - `test_file` (derived from the  BDD spec file, with the suffix `_spec` removed)
    - `feature_name` (derived from the feature section header)
       - Replace spaces and dots with underscores
       - Remove colons and hyphens
       - Keep only letters, numbers, and underscores; collapse multiple underscores
       - Example: `## === Feature 7.1: User Creation ===` -> `Feature_7_1_User_Creation`

You have **three options** for selecting the feature for the Code Generation prompt:

### Summary of Options

**Check for Existing File First**
   - Look for existing file: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`
   - If file exists, proceed with Option 3
   - If file does NOT exist, use Option 2 instead

| Option | Purpose                                         | Trigger                                           | Action                             |
|--------|-------------------------------------------------|---------------------------------------------------|------------------------------------|---------------|
| 1      | **New Code Generation prompt (auto)**          | First `not started` item                          | Create new Code Generation prompt |
| 2      | **New Code Generation prompt (explicit)**      | User specifies feature                            | Create new Code Generation prompt |
| 3      | **Update existing Code Generation prompt**     | "new", "changed", or "removed" BDD scenarios      | **UPDATE existing file**           |

---

### Option 1: Automatic Selection (New Code Generation prompt)

**When to use**: When the user DID NOT explicitly specify a feature and wants to automatically create a new Code generation prompt for the next feature to implement.

1. Read the implementation plan [implementation_plan.md](../tech_design/implementation_plan.md).
2. Find the first **NEXT FEATURE TO IMPLEMENT** in the implementation plan (first feature with `not started` scenarios).
3. Select the entire feature for the design document, which includes ALL scenarios within that feature section.
4. Use all scenarios within that feature (both `not started` and any other statuses).
5. Use this entire feature as the target for the design document.
6. Reference the corresponding BDD file and all relevant scenarios within that feature.
7. **Create a NEW file** at: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`
8. Follow the output requirements as below.

**Example**: If Feature 7.1: User Creation is the next feature, create `docs/tech_prompts/Feature_7_1_User_Creation/Feature_7_1_User_Creation_code_gen.md`

---

### Option 2: Explicit Selection (New Code Generation prompt)

**When to use**: When the user explicitly specifies a feature and want to create a new Code generation prompt for it.

**Steps**:
- If a feature is specified, extract the feature section (including all scenarios, BDD spec file, tech design file, and test file) directly from `implementation_plan.md`.
- Do not search the codebase for the feature definition; use only the information in `implementation_plan.md` to determine the feature's boundaries and scenarios.
- Only after extracting the feature definition from `implementation_plan.md`, proceed to inline the referenced BDD scenarios, technical design, and data models from their respective files.
- **Create a NEW file** at: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`

**Example**: If user specifies "Feature 8.2: Project Staffing", create `docs/tech_prompts/Feature_8_2_Project_Staffing/Feature_8_2_Project_Staffing_code_gen.md`

---

### Option 3: Update Existing Code Generation prompt

**When to use**: When the user explicitly specifies a feature and a code generation prompt file already exists for that feature and there are **"new", "changed", or "removed" BDD scenarios**.

**IMPORTANT**: This option **UPDATES** an existing file, does NOT create a new one.

#### Steps for Option 3:

1. **Identify the Target Feature**
   - The user provides `feature_name` or points to changed scenarios.
   - Locate the corresponding feature in the `implementation_plan`.
   - Identify scenarios with status "new", "changed", or "removed".

2. **Locate the Existing File**
   - Find the existing file: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`
   - **DO NOT create a new file** - update the existing one.

3. **Compare and Analyze Changes**
   - Open and read:
     - The current design: `docs/tech_design/core_features/[file]_design.md`
     - The updated BDD spec: `docs/spec_scenarios/[file]_spec.md`
   - **The existing Code Generation prompt**: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`

4. **Update the Existing Code Generation prompt**
   - **MODIFY the existing file** - do not create a new one
   - Detect impacted code based on changed scenarios
   - Modify only affected code and sections
   - Retain existing structure and code where still valid
   - Add new code and other files as needed
   - Update any outdated references or examples

5. **Add a Changelog Section**
   - At the top of the existing file, add or update:
     ```markdown
     > **Changelog**
     > Updated on: YYYY-MM-DD
   > - Added code for scenario: "Prevent duplicate email registration"
   > - Enhanced role-based skill assignment code
     ```

6. **Save Changes to Existing File**
   - **Save to the SAME file**: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`
   - **DO NOT create a new file**

#### Common Scenarios for Option 3:
- BDD scenarios have been modified or added
- Technical design has been updated
- Code patterns need to be enhanced
- Existing code needs to be updated for new functionality

**Example**: If `docs/tech_prompts/Feature_7_1_User_Creation/Feature_7_1_User_Creation_code_gen.md` already exists and requirements have changed, update that existing file instead of creating a new one.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using TypeScript.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing accessible, robust, and comprehensive Playwright tests following best practices.
- Ensuring all code aligns with project technical constraints, including RBAC, data relationships, and local storage via service layers.
- Practicing test-driven development (TDD) by writing and running tests before implementation.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in UI/UX best practices, accessibility, and robust frontend engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.

## Input Sources

Read the below documents in full.

### Architecture Documents
- Location: `docs/tech_design/overall_architecture.md`

### Application Organization 

- Location: `docs/tech_design/application_organization.md`

### Application Layout 

- Location: `docs/tech_design/application_layout.md`

### Application Data Models 

- Location: `docs/tech_design/data_models.md`

### Feature Specification

- Location: docs/spec_scenarios/[bdd_spec_file]
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)

### Implementation Plan
- Location: `docs/tech_design/implementation_plan.md`
- Contains: List of features with their BDD spec files, tech design files, and test files

### Feature Design Document
- Location: docs/tech_design/core_features/[tech_design_file]
   - Feature implementation
   - Business logic
   - Service patterns

## Output Requirements

You MUST create or update a single file named `[feature_name]_code_gen.md` inside a feature-specific folder: `docs/tech_prompts/[feature_name]/`.

1. Determine `feature_name` exactly as defined earlier (normalize to letters, numbers, underscores).
2. If missing, create the directory: `docs/tech_prompts/[feature_name]/`
3. Write (or update in place) the code generation prompt at: `docs/tech_prompts/[feature_name]/[feature_name]_code_gen.md`
4. If the file already exists at that exact path, modify it—do NOT create duplicates elsewhere.

- If source material is ambiguous, ask a single clear follow-up question and wait.

See the dedicated **Feature Code Generation Prompt Structure** section below for the required structure and formatting of the implementation tasks.

### Explicit Input Extraction

1. You MUST extract and include the full text of:
   - The relevant BDD scenarios (not just references).
   - The technical design for the feature (not just references) from the Feature Design Document.
   - Any relevant data model definitions.
   - Any navigation decisions and designs described in the Feature Design Document, including routing logic, navigation flows, and any UI transitions between pages or views.
   - Any UI Layouts, including wireframes, layout diagrams, component hierarchies, and detailed descriptions of page or view structure, as described in the Feature Design Document, including all relevant layout decisions and design rationale.
   - Any relevant architectural or organizational constraints.
   - All referenced content (BDD scenarios, Feature Design, data models) MUST be inlined in the output prompt, not just linked.

  2. For each task in the Implementation Plan, you MUST extract and inline all relevant technical details from the Feature Design Document and related sources. This includes, but is not limited to:
   - File names and paths (classes, components, services, stores, types, views, routes, etc.)
   - Service and store names, composable functions, and their usage patterns
   - Component names and their relationships/hierarchy
   - Code snippets or patterns for integration, validation, and state management
   - Any architectural or organizational conventions referenced in the design
   - Example code for critical interactions (e.g., how to call a service, how to use a store, how to structure a component, etc.)
   - All relevant configuration or integration details

### Feature Code Generation Prompt Structure
The feature Code Generation prompt MUST follow this structure:

```markdown
# Working Code Generation Prompt: [Feature Name]

## Task: 
Generate working code for [Feature Name], following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using TypeScript.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing accessible, robust, and comprehensive Playwright tests following best practices.
- Ensuring all code aligns with project technical constraints, including RBAC, data relationships, and local storage via service layers.
- Practicing test-driven development (TDD) by running tests before implementation to see the current implementation state.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in UI/UX best practices, accessibility, and robust frontend engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.
- Ensuring all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, and other accessibility-first selectors.

## References
- BDD Scenarios: docs/spec_scenarios/[bdd_spec_file]
- Test File: tests/[test_file]
- Feature Design Document: docs/tech_design/core_features/[tech_design_file]
- Application Architecture `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`

## Development Approach

Follow Test-Driven Development (TDD) cycle:
1. After the WHOLE feature is implemented you MUST:
   a. Run only the relevant test cases associated with the current feature.
   b. Analyze the test failures for this feature.
   c. Make minimal changes to fix the failing test(s) for this feature.
   d. Run the same test cases again to confirm success.
   e. Only proceed to the next feature after the current feature's tests pass.
2. Repeat until all tests pass.

## Implementation Plan

### Scenario: [Scenario Name]
**BDD Scenario:**
```gherkin
<Full Gherkin scenario here>
```
**Technical Design Details (inlined):**
<Inline all relevant design details from the Feature Design Document>

**Tasks:**
1. <Atomic task 1 with file(s), components, services>
2. <Atomic task 2>
...

## Code Examples
...

## Success Criteria
- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.


## Technical Requirements
[specific technical requirements from the **Feature Design Document** that apply to this feature]
```

### Implementation Plan, 
  1. Break the feature down into small, iterative tasks that build on each other. 
  - Look at these tasks and then go another round to break it into small tasks. 
  - Review the results and make sure that the tasks are small enough to be implemented safely with test-first development approach, but big enough to move the project forward. 
  - Iterate until you feel that the tasks are right sized for this feature.

  2. For each task:
   - Include all relevant technical details, such as the exact file(s) to be created or modified, the names of services, stores, and components involved, and any code patterns or integration points described in the Feature Design Document.
   - Reference and inline any code snippets, function signatures, or configuration details necessary to implement the task as described in the technical design.
   - Ensure that each task is actionable and includes enough technical context for a developer to implement it without needing to refer back to the original design document.

  3.Tasks MUST be grouped by BDD scenario:
    - For each scenario in the selected feature, create a section titled with the scenario name
    - Under each scenario, list atomic, numbered tasks covering all user interactions, validations, error states, UI changes, service integrations, and test hooks needed to satisfy that scenario
  - Each scenario section must contain detailed, explicit tasks, with each interaction or requirement as a separate, actionable item.

### Technical Detail Extraction
  1. For every task in the Implementation Plan, you MUST explicitly list:
   - The file(s) to be created or modified (with full or relative paths)
   - The service(s), store(s), and component(s) involved (with their names)
   - Any relevant code patterns, function names, or integration points
   - Example code snippets where appropriate
   - Any navigation logic, route definitions, or UI flow details relevant to the task, as described in the Feature Design Document.
   - Example code snippets for navigation (e.g., router.push, navigation guards, etc.) where appropriate.

  2. Do not summarize or generalize; copy the relevant technical details and code patterns directly from the Feature Design Document and related sources.

## Validation Step (Mandatory)

Before finalizing the generated code generation prompt content in the file named `[feature_name]_code_gen.md` in the `docs/tech_prompts/[feature_name]/` folder, you MUST validate the output against the following success criteria:

### Success Criteria Checklist
1. Covers all feature-related positive scenarios from the BDD file.
2. Covers all feature-related negative scenarios from the BDD file.
3. Includes all Required Code Examples.
4. Provides complete code examples for all recommended patterns.
5. **MANDATORY:** Each user interaction, validation, and error state is a separate, explicit task.
6. **MANDATORY:** No combined or high-level tasks remain.
7. **MANDATORY:** Each task must be so granular that it can be implemented and tested in isolation, ideally in less than a couple of hours.
8. **MANDATORY:** If any task is not atomic, or if any user interaction, validation, or error state is not listed as a separate list item, the output is invalid and must be regenerated.
9. **MANDATORY:** After initial breakdown, review and further split any task that could be divided into smaller, more focused tasks.

### Validation Process
- You must explicitly confirm that each success criterion is met.
- If any criterion is not met, you must revise the output to address the issue before finalizing it.
- The output will not be considered complete until it passes this validation step.

