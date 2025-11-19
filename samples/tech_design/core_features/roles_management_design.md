# Feature Design: Roles Management

This document outlines the technical design for the Roles Management feature.

## Feature Implementation Details: Roles Management

*   **Primary Components & Tests**:
    *   `src/views/RolesView.vue`
    *   `tests/manage_roles.spec.ts`

*   **Core Services**:
    *   `useRolesService`

*   **Data Models**:
    *   `Role`
    *   `SkillCategory` (referenced from skill management)

*   **Interaction Flow**:
    *   The `RolesView.vue` component will handle all UI for role management. It will use the `useRolesService` to fetch, create, update, and delete role data. When a user adds a new role, the component will call the `saveRole()` method on the service, which will handle validation and storage. The component's state will then be updated to reflect the changes in the UI.

## Additional Design Considerations for Roles Management

### UI Implementation Details (`RolesView.vue`)

*   The main UI will be encapsulated within `src/views/RolesView.vue`.
*   **Add Role Modal**: A modal will be used for creating and editing roles. Its visibility will be controlled by a reactive variable (e.g., `isAddRoleModalOpen`).
*   **Roles Display**: Roles will be displayed in a list or table within the main view.
*   **Data and Logic**: The component will use the Composition API's `setup()` function to manage state and logic via `useRolesService`.

### Test Data Management

*   Test data for roles will be managed in a dedicated fixture file.
*   **Example (`tests/fixtures/rolesTestData.ts`):**
    ```typescript
    import { Role } from '@/types';

    export const mockRoles: Role[] = [
      {
        id: 'role-1', 
        name: 'Backend Developer', 
        skillCategoryIds: [], 
        skillIds: ['lib3', 'lib7']  
      },
      { 
        id: 'role-2', 
        name: 'Frontend Developer', 
        skillCategoryIds: [], 
        skillIds: ['lib1', 'lib2'] 
      }
    ];
    ```

### Core Logic Implementation (Roles View)

*   The `RolesView.vue` component will fetch all roles from `useRolesService` and display them.
*   It will provide buttons for "Add new role", which will open the creation modal.
*   Each role in the list will have options to edit or delete.

### Business Logic (`useRolesService`)

*   The service will manage all business logic related to roles.
*   **`createRole(roleData)`**: Validates the role data (e.g., name is required) and saves it to storage. It must prevent creating roles with duplicate names.
*   **`assignRoleToPerson(roleId, personId)`**: Handles assigning a role to a person. It should check for duplicate assignments and potential conflicts.
*   **`deleteRole(roleId)`**: Before deletion, it must verify that the role is not actively assigned to any person. If it is, the deletion should be prevented, and an error should be thrown.
*   **`importRoles(rolesData)`**: Logic for bulk importing roles, including validation of each role in the dataset and handling of duplicates.
*   **`getSkillsForRole(roleId)`**: Retrieves all skills associated with the categories assigned to a role.
*   **`assignSkillCategoriesToRole(roleId, categoryIds)`**: Updates a role's skill categories, which determines the skills expected from people with this role.
*   **`getConflictingRoles(roleId, personId)`**: Identifies potential role conflicts when assigning multiple roles to a person (e.g., "Junior Developer" and "Senior Developer").
*   **`combineRoleSkillRequirements(roleIds)`**: When a person has multiple roles, this method combines their skill requirements, using the higher target level for overlapping skills.

## Frontend Layout Architecture

The Roles Management feature's layout is structured in two distinct architectural layers:

### 1. Application-Level Layout: DashboardLayout Integration

### Layout Integration

*   **Main Navigation**: The Roles Management view is accessible through the main navigation sidebar, which is always visible on the left side of the screen. This sidebar is implemented in `src/components/layout/DashboardLayout.vue`.

*   **Page Content Area**: When a user navigates to Roles Management, the `RolesView.vue` component fills the entire content area to the right of the main sidebar.

```
[Main Nav Sidebar] | [        RolesView Content Area         ]
```

`RolesView.vue` must be wrapped within the application's `DashboardLayout` component:

```vue
<template>
  <DashboardLayout>
    <!-- RolesView content -->
  </DashboardLayout>
</template>
```

This ensures:
* Consistent navigation and header across the application
* Authentication handling through layout guards
* Access to the main navigation sidebar for all authenticated routes
* Proper routing integration with other application modules

For detailed information about the `DashboardLayout.vue` component and its functionality, refer to the [Main Dashboard Design](./main_dashboard_design.md) document.

### 2. Component-Level Layout: DataTable Structure

Within `RolesView.vue`, implement a modern data table structure using PrimeVue DataTable:

```
[DataTable with Roles] 
[Modal Dialogs for Create/Edit Operations]
```

The implementation uses the following key components:

* `RolesList.vue` - Main component for roles display and management
* `SkillSelectionTree.vue` - Reusable component for hierarchical skill selection

#### DataTable Implementation
* Use PrimeVue's DataTable component for displaying roles in a tabular format
* Features to implement:
  * Sortable columns (by role name)
  * Search/filter capability for quick role lookup
  * Row selection to view/edit role details
  * Action buttons integrated as table columns
  * Pagination for large role collections
  * Responsive design that adapts to different screen sizes

#### DataTable Structure
* Key columns to include:
  * Role Name - primary information, sortable
  * Associated Skill Categories (when implemented) - showing count or preview
  * Actions Column - containing Edit and Delete buttons
* Table header with:
  * "Roles Directory" heading
  * Global search/filter input
  * "Add new role" button

#### Adding New Roles
* The "Add new role" functionality will be implemented through:
  * A prominent "Add new role" button in the DataTable header section
  * When clicked, it will open a modal dialog using PrimeVue Dialog component:
    * PrimeVue Dialog is suitable for complex content including hierarchical components
    * Dialog size will be configurable to accommodate the skill selection component
    * Should support responsive sizing for different screen dimensions
  * The modal will contain:
    * Input field for role name with validation
    * Option "Define skill categories? → Yes/No" as a toggle or radio buttons
    * When "Yes" is selected:
      * Use the reusable `SkillSelectionTree` component for skill selection
      * `SkillSelectionTree` provides:
        * A hierarchical tree-based UI for skill selection
        * Support for both category-level and individual skill-level selection
        * Visual indication of selected categories and skills
        * Ability to expand/collapse categories to select individual skills
        * Consistent selection behavior across the application
        * Validation and error messaging for skill assignments
        * Event emissions for selection changes and validation errors
        * **Skill level selector (dropdown) is supported via a prop, but is not shown in Roles Management.**
      * Configuration via props:
        * `v-model` for two-way binding of selection state
        * `preSelectedCategoryIds` and `preSelectedSkillIds` for editing scenarios
        * `helperText` for contextual guidance
        * `roleId` for validation against existing role assignments
        * `showSkillLevelSelector` (boolean, default: false) — controls whether a skill level dropdown is shown next to each selected skill. In Roles Management, this is set to `false` so only skill/category selection is available.
    * Save and Cancel buttons with appropriate validation
  * Upon successful creation:
    * The dialog will close
    * The DataTable will refresh to display the newly created role
    * A success message will be shown to confirm the operation
  * Preserves and extends the existing creation logic from the current implementation in `RolesView.vue`

#### Modal Dialogs for CRUD Operations
* Create/Edit operations performed through modal dialogs:
  * Form fields for role properties including:
    * Role name field with validation
    * `SkillSelectionTree` component for skill selection with the following workflow:
      * Radio button option to enable/disable skill definition
      * When enabled, displays the hierarchical skill tree through the reusable component
      * For edit operations, pre-populates with existing selections using `preSelectedCategoryIds` and `preSelectedSkillIds` props
  * The edit dialog will mirror the create dialog but be pre-populated with existing role data
  * Both dialogs will support the complex skill selection requirements from the scenarios:
    * Assigning entire skill categories
    * Expanding categories to select individual skills
    * Combining both category and individual skill selections
  * Validation feedback for all user interactions through:
    * Direct form validation for role name
    * Component-emitted validation events from `SkillSelectionTree` for skill-related validations
  * Save/Cancel actions with appropriate state management
* Delete confirmation handled through a separate confirmation dialog component (PrimeVue ConfirmDialog)
  * Will include checks for active role assignments as specified in the negative scenarios
  * Display appropriate error messages when deletion is not allowed

#### Selected Role Details
* Implement as:
  * A modal dialog showing complete role details
  * Alternative: expandable row details within the DataTable

This DataTable-based layout provides a modern, efficient interface for role management with improved usability features:
* Better visibility of all roles at once
* Advanced filtering and sorting capabilities
* Consistent with other data management interfaces
* More efficient use of screen space

This layout ensures that:
1. The main navigation remains accessible at all times.
2. The Roles Management interface maintains consistency with other parts of the application.
3. Users can efficiently navigate between roles while viewing or editing a specific role.

