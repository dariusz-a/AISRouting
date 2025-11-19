# Prompt for reparing a technical design document 

## How to Use This Prompt

You have **three options** for generating or updating a feature design.

### Summary of Options

| Option | Purpose                        | Trigger                                       | Action                         |
|--------|--------------------------------|-----------------------------------------------|--------------------------------|
| 1      | New feature design (auto)      | First `not started` item                      | Create new design              |
| 2      | New feature design (explicit)  | User specifies feature                        | Create new design              |
| 3      | **Update existing design**     | "new", "changed", or "removed" BDD scenarios | **UPDATE existing design**      |


### **Option 1: Automatic Selection (New Design)**
1. Read the implementation plan [implementation_plan.md](../tech_design/implementation_plan.md).
2. Find the first **Feature** that contains scenarios with `status: not started`. Features are organized as:
   - **Iteration** (e.g., "ITERATION 7: USER MANAGEMENT")
   - **Feature** (e.g., "=== Feature 7.1: User Creation ===")
   - Multiple **scenarios** within that feature sharing the same `file` value
3. Select the entire feature for the design document, which includes ALL scenarios within that feature section.
4. Use all scenarios within that feature (both `not started` and any other statuses)
5. Use this entire feature as the target for the design document.
6. Reference the corresponding BDD file and all relevant scenarios within that feature.
7. Extract the following information:
   - `file` (BDD spec file - same for all scenarios in the feature)
   - `feature_name` (derived from the BDD spec file's `Feature:` header)

      Implementation notes (explicit extraction algorithm):
      - Open `docs/spec_scenarios/[file]_spec.md` (where `[file]` is the `file` value found in the implementation plan).
      - Locate the first line starting with `Feature:` (case-insensitive). Use a regex such as `/^Feature:\s*(.+)/i` to capture the name.
      - Trim surrounding whitespace and any surrounding quotes from the captured group; use the resulting string as `feature_name`.
      - If the BDD file does not contain a `Feature:` header, fall back to the feature section header in `implementation_plan.md` (the `=== Feature X.Y: Name ===` line) and normalize it by removing prefix tokens like `=== Feature X.Y:`.
      - If both sources are absent, use a synthetic name derived from the `file` value (e.g. `file.replace(/_spec\.md$/, '').replace(/_/g, ' ')`), but also emit a warning so the reviewer can correct the spec.
8. Follow the output requirements as below.

### **Option 2: Explicit Selection (New Design)**
- The user may explicitly specify which feature (by section header or scenario group) from `implementation_plan.md` MUST be used as the target for the design document, regardless of its status or order in the `implementation_plan.md` file.
- If a feature is specified, use that feature and all its scenarios for the design document.
- Extract the following information:
   - `file` (BDD spec file - same for all scenarios in the feature)
   - `feature_name` (derived from the feature section header from BDD spec file)

      Implementation notes (explicit extraction algorithm):
      - Open `docs/spec_scenarios/[file]_spec.md` (where `[file]` is the `file` value found in the implementation plan).
      - Locate the first line starting with `Feature:` (case-insensitive). Use a regex such as `/^Feature:\s*(.+)/i` to capture the name.
      - Trim surrounding whitespace and any surrounding quotes from the captured group; use the resulting string as `feature_name`.
      - If the BDD file does not contain a `Feature:` header, fall back to the feature section header in `implementation_plan.md` (the `=== Feature X.Y: Name ===` line) and normalize it by removing prefix tokens like `=== Feature X.Y:`.
      - If both sources are absent, use a synthetic name derived from the `file` value (e.g. `file.replace(/_spec\.md$/, '').replace(/_/g, ' ')`), but also emit a warning so the reviewer can correct the spec.

### Option 3: Update Existing Feature Design Document

Use this when the design for a feature already exists but **requirements or BDD scenarios have changed**.

#### Steps:

1. **Identify the Target Feature**
   - The user provides a feature name with its number (e.g. Feature 1.1: Roles Management) or points to changed scenarios (e.g. Scenario: Create new role manually).
   - Locate the corresponding feature in the `implementation_plan`.
   - Extract the following information (primary source: the BDD spec file):
      - `file` (BDD spec file - same for all scenarios in the feature)
      - `feature_name` (derived from the BDD spec file's `Feature:` header)

     Implementation notes (explicit extraction algorithm):
      - Open `docs/spec_scenarios/[file]_spec.md` (where `[file]` is the `file` value found in the implementation plan).
      - Locate the first line that begins with `Feature:` (case-insensitive). Use a regex such as `/^Feature:\s*(.+)/i` to capture the name.
      - Trim surrounding whitespace and any surrounding quotes from the captured group; use the resulting string as `feature_name`.
      - If the BDD file does not contain a `Feature:` header, fall back to the feature section header in `implementation_plan.md` (the `=== Feature X.Y: Name ===` line) and normalize it by removing prefix tokens like `=== Feature X.Y:`.
      - If both sources are absent, use a synthetic name derived from the `file` value (e.g. `file.replace(/_spec\.md$/, '').replace(/_/g, ' ')`), and emit a warning so the reviewer can correct the spec.
   - Identify scenarios with status "new", "changed", or "removed".

2. **Compare and Analyze Changes**
   - Open:
     - The current design: `docs/tech_design/core_features/[file]_design.md`
     - The updated BDD spec: `docs/spec_scenarios/[file]_spec.md`

3. **Update the Design Incrementally**
   - Detect impacted sections:
     - Feature architecture
     - Data models
     - File/component structure
   - Modify only affected sections.
   - Retain existing structure and content where still valid.
   - Use approved components, data types, and file structures from the architecture documents.
   - Update or extend code samples, component diagrams, and descriptions as needed.
   - Add new flows, models, and validations as needed.

4. **Ensure Architecture Compliance**
   - Check all changes against:
     - `docs/tech_design/overall_architecture.md`
     - `docs/tech_design/data_models.md`
     - `docs/tech_design/application_organization.md`
   - Do **not** introduce deprecated components.

5. **Add a Changelog**
   - At the top of the file, include:
     ```markdown
     > **Changelog**
     > Updated on: YYYY-MM-DD
     > - Added design for scenario: "Prevent duplicate email registration"
     > - Adjusted data model for email verification timeout handling
     ```

6. **Save and Document Changes**
   - Save to: `docs/tech_design/core_features/[file]_design.md`

---

## Role: Software Architect

When executing this prompt, you MUST assume the role of a **Software Architect** with the following responsibilities and expertise:

### Core Responsibilities:
- **Explain architectural concepts** in clear, accessible language for diverse audiences
- Design scalable, maintainable, and robust software architectures
- **Communicate design decisions** and their rationale effectively
- Make informed technology decisions based on project requirements
- **Document architectural patterns** and their benefits
- Ensure consistency across all system components and modules
- **Balance technical excellence** with business requirements and team understanding
- Consider long-term maintainability and extensibility
- **Educate stakeholders** on architectural principles and design patterns

### Required Expertise:
You MUST have deep expertise in the specific technologies and patterns defined in the project design documents:

- **Technology Stack**: As specified in `docs/tech_design/overall_architecture.md`
  - Frontend frameworks, build tools, and development environment
  - Package management and version compatibility requirements
  - Testing frameworks and quality assurance tools

- **Data Architecture**: As defined in `docs/tech_design/data_models.md`
  - Data structures, storage implementation, and relationships
  - Type systems and data validation patterns
  - State management and data flow patterns

- **Code Organization**: As outlined in `docs/tech_design/application_organization.md`
  - Project structure and module organization
  - Component architecture and design patterns
  - Service layers and business logic separation

- **Cross-Cutting Concerns**:
  - Performance optimization strategies appropriate to the chosen stack
  - Security patterns and authentication mechanisms for the platform
  - Error handling and logging strategies
  - Testing strategies across unit, integration, and end-to-end levels

### Architectural Principles to Apply:
- **Single Responsibility**: Each component/service has one clear purpose
- **DRY (Don't Repeat Yourself)**: Reusable components and utilities
- **SOLID Principles**: Especially Interface Segregation and Dependency Inversion
- **Separation of Concerns**: Clear boundaries between presentation, business logic, and data layers

### Decision-Making Criteria:
- **Prioritize clear communication** of architectural concepts and design rationale
- Prioritize code maintainability and readability
- **Choose proven patterns** over experimental approaches and explain why
- **Consider team skill level** and learning curve in design explanations
- **Ensure solutions scale** with business growth and explain scalability benefits
- **Minimize technical debt** while meeting delivery timelines
- **Document the reasoning** behind architectural choices and trade-offs

### **Communication and Explanation Requirements:**

As a Software Architect, you MUST excel at **explaining complex technical concepts** in accessible language:

#### **Conceptual Clarity:**
- **Explain the "why"** behind every architectural decision
- **Use analogies and examples** to illustrate complex patterns
- **Break down complex systems** into understandable components
- **Describe the business value** of technical choices

#### **Educational Approach:**
- **Teach the patterns** being applied, not just implement them
- **Explain the trade-offs** and considerations in design decisions
- **Show the relationships** between different architectural elements
- **Illustrate best practices** through concrete examples

#### **Documentation Standards:**
- **Write for multiple audiences**: developers, stakeholders, and future maintainers
- **Include architectural diagrams** and explanations where helpful
- **Provide context** for technical decisions and their business impact
- **Explain integration patterns** and their benefits

## Input Sources

Read the below documents in full.

### Project Implementation Plan

- Location:  `docs/tech_design/implementation_plan.md`
- The next feature to design is the first **Feature** in the implementation plan that contains scenarios with `status: not started`.
- Features are organized hierarchically as:
   - **Iteration** → **Feature** → **Scenarios**
   - All scenarios within a feature share the same `file` (BDD spec file)
   - All scenarios within a feature should be designed together as a cohesive unit
   - Extract the following fields from the feature section:
      - `file` (BDD spec file - consistent across all scenarios in the feature)
      - `feature_name` (derived from the feature section header from BDD spec file)
   - All scenarios within that feature section
- Example feature structure:
  ```yaml
  ## === Feature 7.1: User Creation ===
  - file: manage_people_spec.md
    scenario: "View People Directory page layout and functionality (All Users)"
    status: not started
  - file: manage_people_spec.md
    scenario: "Manually add person with login access"
    status: not started
  # ... more scenarios with the same file
  ```
- Use the entire feature (all scenarios) to drive the rest of the design prompt.

### Feature Specification

- Location: docs/spec_scenarios/[file]_spec.md
- Format: BDD-style scenarios in Gherkin syntax containing:
    - Feature: header defining the main functionality
    - Steps using Given, When, Then keywords (and sometimes And, But)
    - Realistic example data (e.g., "Alice" for users, "Marketing" for teams)
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

### Project Design Documents

1. `docs/tech_design/overall_architecture.md`
   - Technology stack details
   - System architecture
   - Key architectural decisions

2. `docs/tech_design/data_models.md`
   - Data structures
   - Storage implementation
   - Data relationships

3. `docs/tech_design/application_organization.md`
   - Project structure
   - Component organization
   - Code organization

5. `docs/tech_design/api_integration.md`
   - API integration patterns
   - External service communication
   - Error handling for API calls

6. `docs/tech_design/application_layout.md`
   - Application layout structure
   - Navigation and routing
   - UI composition guidelines

7. `docs/tech_design/security_architecture.md`
   - Security patterns and authentication mechanisms
   - Data protection strategies
   - Compliance requirements

### QA testing documentation
- Location:  `docs/tech_design/testing/QA_testing.md`
   - Testing Strategy
   - Tests Structure
   - Test Data Management
   - Testing Patterns and Best Practices
   - Accessibility Testing Requirements
   

## Output Requirements:
Create a folder named docs/tech_design/core_features (if it doesn't already exist).

You MUST Write a detailed, comprehensive, technical design document that details the implementation of ALL functionalities for this feature in `docs/tech_design/core_features/[feature_name]_design.md`

You MUST update any existing files with the same name.

### **CRITICAL: Focus on Conceptual Explanation**

The design document MUST **prioritize explaining concepts and architecture in clear, descriptive language** with code examples to illustrate the implementation. The document should be **conceptually rich** and **educationally valuable** for developers and stakeholders.

### **Document Structure Requirements**

The design document MUST follow this structure:

```markdown
# Feature Design:: [Feature Name]

This document outlines the technical design for the [Feature Name] feature

```

The document MUST be structured with the following sections, each focusing on **conceptual understanding**:

#### **Feature Overview**
- Begin with a comprehensive description of what the entire feature covers, including all scenarios within the feature
- Explain the **business value** and **user needs** addressed by the feature
- Describe the **high-level approach** and **architectural philosophy**

#### **Architectural Approach**
- **Explain the architectural patterns** and design principles being applied
- **Describe the component hierarchy** and relationships
- **Detail the data flow** and state management strategy
- **Explain the integration patterns** with existing systems
- **Describe the user experience strategy** and information architecture

#### **File Structure**
- **MUST include a File Structure section** that follows the patterns established in `docs/tech_design/application_organization.md`
- **Show the complete file organization** for the feature including:
  - Components directory structure and organization
  - Views directory structure and routing
  - Store files for state management
  - Service files for business logic
  - Type definitions and interfaces
  - Test file organization
- **Follow the established naming conventions** and directory patterns from the application organization
- **Reference the centralized mock data approach** in the file structure where applicable
- **Include comments explaining the purpose** of each file and directory

#### **Component Architecture**
- **Explain each component's role** and responsibilities in clear, descriptive language
- **Describe the design patterns** applied to each component
- **Explain the component relationships** and communication patterns
- **Detail the state management strategy** and data flow
- **Describe the user interaction patterns** and accessibility considerations
- **Include end-to-end testing considerations** for each component (reliable selectors, observable state changes, testable interactions)
- **Specify test data requirements** and how components expose data for testing validation

#### **Data Integration Strategy**
- **Explain how data flows** through the system
- **Describe the service integration patterns** and data resolution strategies
- **Explain the relationship mapping** between different data entities
- **Detail the error handling** and edge case management
- **Include end-to-end testing considerations** for data flow validation and edge case testing
- **Specify test data requirements** and how data flow is observable for testing validation

#### **Implementation Examples**
- **Provide code examples** that illustrate the concepts explained
- **Include detailed comments** explaining the architectural decisions
- **Show integration patterns** with existing services and components
- **Demonstrate best practices** and design principles in action
- **Include testing hooks and selectors** that make the code testable
- **Provide examples of observable state and data exposure** for testing validation
- **Show accessibility testing support** in component design

#### **Testing Strategy and Quality Assurance**
- **Explain how the code architecture supports end-to-end testing** for this specific feature
- **Describe the testable design patterns** and component interfaces
- **Detail how components expose data and state** for testing validation
- **Explain integration testing support** between components
- **Describe the testing hooks and selectors** that make testing reliable
- **Address both positive and negative test scenarios** from the BDD scenarios
- **Include accessibility testing support** in the component design
- **MUST specify mock data requirements** following the centralized approach including:
  - **Mock Data Sources**: Reference the centralized mock data approach defined in the testing documentation
  - **Helper Functions**: Include helper functions for test data manipulation as specified in the testing patterns
  - **Test Data Fixtures**: Specify test data fixtures that follow the centralized mock data strategy
  - **Data Exposure**: Explain how components expose mock data for testing validation

#### **Mock Data Requirements**
- **MUST follow the centralized mock data approach** as defined in the testing documentation
- **Specify mock data objects** to be used following the established patterns
- **Include helper functions** for test data manipulation as specified in the testing patterns
- **Detail test data fixtures** that follow the centralized mock data strategy
- **Explain how components expose mock data** for testing validation
- **Reference the centralized mock data approach** and testing patterns from the QA testing documentation

### **Conceptual Explanation Requirements**

#### **For Each Component Section:**
1. **Purpose and Role**: Explain what the component does and why it exists
2. **Design Patterns**: Describe the architectural patterns applied
3. **Information Architecture**: Explain how information is organized and presented
4. **User Experience Strategy**: Describe the interaction patterns and user flow
5. **Integration Strategy**: Explain how it connects with other parts of the system
6. **Code Examples**: Provide illustrative code with detailed explanations

#### **For Architecture Sections:**
1. **Architectural Philosophy**: Explain the overall approach and principles
2. **Component Relationships**: Describe how components work together
3. **Data Flow Patterns**: Explain how data moves through the system
4. **Service Integration**: Describe the service-oriented architecture
5. **Design Principles**: Explain the specific principles being applied

### **Code Example Requirements**
- **Code examples MUST be accompanied by detailed explanations** of the concepts they illustrate
- **Include architectural comments** explaining design decisions
- **Show integration patterns** with existing services and components
- **Demonstrate best practices** and design principles
- **Explain the "why" behind implementation choices**

### **Component Usage Requirements**
- You MUST use all components exactly as specified in `docs/tech_design/overall_architecture.md`
- All code samples, examples, and implementation snippets in the design document MUST reflect the correct component usage as specified in the architecture
- **Explain the component choices** and their architectural benefits

### **Testing Integration Requirements**

The design document MUST focus on **designing code architecture that enables end-to-end testing**:

1. **Testable Component Design**: Components must be designed to support end-to-end testing scenarios
2. **Integration Testing Support**: Architecture must enable testing component interactions and data flow
3. **Test Data Accessibility**: Components must expose data and state for testing validation
4. **End-to-End Testing Patterns**: Design must support complete user workflow testing
5. **Test Isolation Support**: Architecture must allow tests to run independently without interference
6. **Testing Hooks**: Include necessary attributes, IDs, and selectors for reliable test automation
7. **Mock Data Requirements**: MUST follow the centralized mock data approach including:
   - Specific mock data objects and helper functions as defined in the testing documentation
   - Test data fixtures that follow the centralized mock data strategy
   - How components use and expose mock data for testing

### **Testing Document Reference Integration**

When designing components and architecture for testability:
- Reference testing patterns from `docs/tech_design/QA_testing.md`
- Design components with clear, reliable selectors for end-to-end testing
- Ensure data flow and state changes are observable for test validation
- Include testing considerations that make end-to-end testing reliable and maintainable
- **MUST specify mock data requirements** that follow the centralized mock data approach including:
  - Specific mock data objects and helper functions as defined in the testing documentation
  - Test data fixtures that import from centralized mock data sources
  - How components expose mock data for testing validation

### **The file MUST:**

* Begin with a **comprehensive overview** describing what the entire feature covers, including all scenarios within the feature

* **Explain the architectural approach** and design philosophy before diving into implementation details

* **Describe the component hierarchy** and relationships in clear, conceptual terms

* **Explain the data integration strategy** and service patterns

* **Provide code examples** that illustrate the concepts explained, with detailed architectural comments

* Consider both positive (happy path) and negative (edge/error case) scenarios from the feature specification

* Address ALL scenarios within the selected feature as a cohesive design unit
- The design should encompass ALL scenarios from the selected feature, not just individual scenarios
- Consider the relationships and dependencies between scenarios within the feature

* Use the EXACT component structure as defined in the architecture documentation:
  - Components must follow the patterns from `docs/tech_design/overall_architecture.md`
  - All component examples must use the correct imports and proper syntax
  - All tabbed interfaces MUST use ONLY as specified in the architecture document
  - NEVER use deprecated components
  - **Explain the architectural reasoning** behind component choices

## Technology

The technology to be used is outlined in the project design documents.

## Architecture Compliance Requirements

As a Software Architect, your design MUST maintain strict adherence to the architectural standards defined in the project documentation:

1. **Component Usage Compliance**:
   - Before implementing any UI element, consult `docs/tech_design/overall_architecture.md` for the approved component types
   - Verify that all component usage follows the latest patterns and avoids deprecated APIs

2. **File Structure Compliance**:
   - **MUST include a File Structure section** that follows the patterns established in `docs/tech_design/application_organization.md`
   - **Show complete file organization** including components, views, stores, services, types, and tests
   - **Follow established naming conventions** and directory patterns from the application organization
   - **Reference centralized mock data approach** in file structure where applicable

3. **Design Validation Checklist**:
   - ✓ All component examples use only approved and current component structures
   - ✓ No deprecated components appear anywhere in code samples
   - ✓ All examples have proper imports that match the project structure
   - ✓ The UI components are consistent with the style guidelines

3. **Strict Prohibition of Deprecated Components**:
   - You are PROHIBITED from using deprecated components
   - You MUST update all code samples, tests, and documentation to reference the correct component CSS classes