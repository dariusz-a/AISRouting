# **GenAI-Assisted TDD for Developers**

| Context | Audience: Mid-level & Senior Developers (3–4 per team) Environment: Local (VS Code \+ Git \+ GitHub Copilot) Workflow: 7-Step Iterative TDD Chain (Spec → Design → Test → Code) Mode: 70% Hands-on / 30% Reasoning  |
| :---: | :---: |

| Section | Module Title | Objectives | Deliverables (with tooling references) |
| ----- | ----- | ----- | ----- |
| **1** | **Framing: GenAI-Assisted TDD Workflow** | Understand the GenAI-augmented development lifecycle and team roles. Align on iterative reasoning → generation → validation flow. | Shared understanding of workflow; local repo cloned and branch created (`feature/<topic>`). |
| **2** | **User Manual Generation** | Translate a vague business goal into a concise user-centric brief to guide all next steps. | `/docs/user_manual/user_manual.md` generated in VS Code with Copilot prompt; committed to Git. |
| **3** | **Specification (BDD Scenarios \+ NHPs)** | Write clear Given/When/Then scenarios including non-happy paths (NHPs) for validation. | `/docs/spec_scenarios/feature_specification.md` written via Copilot prompts; committed to repo. |
| **4** | **Technical Design** | Produce a concise system architecture (C4 Context) aligned with requirements and constraints. | `/docs/tech_design/overall_architecture.md` generated via Copilot prompt; includes systems, APIs, quality attributes. |
| **5** | **Implementation Plan (Iteration/Feature/Scenario Tracker)** | Establish a hierarchical, test-first execution plan: Iteration → Feature → Scenarios; codify NEXT FEATURE selection rule (pick the first Feature with all scenarios status: not started); record per-scenario links to bdd\_spec\_file, optional tech\_design\_file and test\_file; track status across not started / completed / deterred; capture dependencies and working strategy. | `/docs/tech_design/implementation_plan.md` in the exact structure provided (with YAML blocks); includes “NEXT FEATURE TO IMPLEMENT” section, Iterations/Features with scenario rows and statuses, dependency notes, and the Test-First \+ Iterative strategies. Created/edited in VS Code with Copilot prompt assist; committed to Git. (Use Angular \+ .NET paths, e.g., tests/\<feature\>.spec.ts, docs/tech\_design/....) |
| **6** | **Feature Design (Prompt Refinement)** | Structured Copilot prompts for each functional unit and integrates design constraints. | `/docs/tech_design/core_features/feature_design.md`; prompt templates embedded; local test runs simulated via Copilot. |
| **7** | **Test Generation & Validation (TDD Phase 1\)** | Produce Playwright UI and API tests before code based on BDD scenarios. Ensure tests reflect expected behaviour and fail initially against stubs/mocks. | `/tests/<feature>/*.spec.ts`; runnable via Playwright; executed locally to validate coverage and naming; committed to Git.. |
| **8** | **Code Generation & Integration (TDD Phase 2\)** | Generate working features (UI \+ API) from prompts, make previously failing tests pass, validate compilation & local run. | `/src/<feature>/` folder with working code; Playwright tests green; committed and pushed to branch.. |
| **9** | **Showcase & Reflection** | Teams demo their feature and artifact chain; discuss workflow insights & next steps. | Short presentation \+ reflection notes; all artifacts (1–7) committed and pushed to feature branch. |




