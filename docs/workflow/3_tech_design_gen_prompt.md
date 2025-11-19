# Prompt for preparing a set of technical design documents 

You are tasked with preparing a set of technical design documents for this project that outline the below documents:

* The overall architecture and technology stack
* Data models and storage structure
* Application organization
* Security architecture
* Application layout and UI components
* API integration patterns
* Core features
* Implementation plan broken into iterations

## Input Source

### Technical design documents
Location: docs/input_tech_docs/*.md

## Output Requirements:
Create a folder named docs/tech_design (if it doesn't already exist).

Overwrite any existing files with the same names.

You MUST create ALL of the following files in the docs/tech_design/ folder:

1. overall_architecture.md
   - Technology stack details
   - System architecture
   - Key architectural decisions

2. data_models.md
   - Data structures
   - Storage implementation
   - Data relationships

3. application_organization.md
   - Project structure
   - Component organization
   - Code organization

4. security_architecture.md
   - Authentication and authorization framework
   - Data access controls
   - API security measures
   - Compliance features
   - Security testing approach

5. application_layout.md
   - UI components hierarchy
   - Layout structure
   - Navigation flow
   - Responsive design approach
   - Reusable component documentation

6. api_integration_patterns.md
   - API design principles
   - Integration patterns with external systems
   - Data flow between services
   - Error handling strategies
   - Performance considerations

### Each file must:

* Begin with a line describing what the file covers.

* Consider both positive (happy path) and negative (edge/error case) scenarios from the product specification.

* Use realistic example data (e.g., names like "Alice", teams like "Marketing", skills like "Python").

* Cross-reference other sections if necessary.