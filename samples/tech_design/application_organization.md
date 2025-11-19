# Application Organization

## Project Structure

```
src/
├── assets/                 # Static assets
│   ├── styles/            # Global styles (with Primer CSS integration)
│   └── images/            # Image assets
│
├── components/            # Reusable UI components
│   ├── auth/             # Authentication related components
│   ├── common/           # Shared components (buttons, inputs, etc.)
│   ├── dashboard/        # Dashboard components
│   ├── skills/          # Skills management components
│   ├── teams/           # Team management components
│   ├── people/          # People management components
│   ├── projects/        # Project management components
│   └── reports/         # Reporting components
│
├── views/                # Page components
│   ├── auth/            # Authentication pages
│   ├── dashboard/       # Dashboard pages
│   ├── skills/         # Skills management pages
│   ├── teams/          # Team management pages
│   ├── people/         # People management pages
│   ├── projects/       # Project management pages
│   └── reports/        # Reporting pages
│
├── router/              # Vue Router configuration
│   ├── index.ts        # Router setup
│   ├── guards.ts       # Navigation guards
│   └── routes/         # Route definitions
│
├── store/              # Pinia stores
│   ├── users.ts       # User management store
│   ├── teams.ts       # Team management store
│   ├── skills.ts      # Skills management store
│   ├── projects.ts    # Project management store
│   └── reports.ts     # Reporting store
│
├── services/           # Business logic
│   ├── auth.ts        # Authentication service
│   ├── storage.ts     # Local storage service
│   ├── users.ts       # User management service
│   ├── teams.ts       # Team management service
│   ├── skills.ts      # Skills management service
│   └── projects.ts    # Project management service
│

├── utils/            # Helper functions
│   ├── validation.ts # Form validation
│   ├── formatting.ts # Data formatting
│   ├── security.ts   # Security utilities
│   └── testing.ts    # Test helpers
│
├── types/            # TypeScript type definitions
│   ├── user.ts       # User types  
│   ├── team.ts       # Team types
│   ├── skill.ts      # Skill types
│   └── project.ts    # Project types
│
└── tests/                    # Test files
    ├── e2e/                 # Playwright tests (legacy location)
    ├── fixtures/            # Centralized test data and prototype data
    │   └── skillsTestData.ts # Skills test fixtures (single source of truth)
    └── *.spec.ts            # Feature test files
```

## Component Organization

### Common Components
```typescript
// Button Component
interface ButtonProps {
  variant: 'primary' | 'secondary' | 'danger';
  size: 'small' | 'medium' | 'large';
  disabled?: boolean;
  loading?: boolean;
}

// Input Component
interface InputProps {
  type: 'text' | 'email' | 'password';
  label: string;
  error?: string;
  required?: boolean;
}

// Table Component
interface TableProps {
  columns: Column[];
  data: any[];
  sortable?: boolean;
  filterable?: boolean;
}
```

### Feature Components

1. **Authentication**
```typescript
// LoginForm.vue
interface LoginFormProps {
  onSuccess: () => void;
  onError: (error: Error) => void;
}

// PasswordStrength.vue
interface PasswordStrengthProps {
  password: string;
  showRequirements?: boolean;
}
```

2. **Dashboard**
```typescript
// QuickAccessTile.vue
interface QuickAccessTileProps {
  icon: string;
  title: string;
  route: string;
}

// NotificationsPanel.vue
interface NotificationsPanelProps {
  maxItems?: number;
  showAll?: boolean;
}
```

3. **Skills Management**
```typescript
// SkillTree.vue
interface SkillTreeProps {
  categories: SkillCategory[];
  skills: Skill[];
  selectable?: boolean;
}

// SkillAssessment.vue
interface SkillAssessmentProps {
  userId: string;
  skillId: string;
  currentLevel?: number;
}
```

## View Organization

### Layout Structure

The application's layout is managed by `DashboardLayout.vue`, which provides a clean, GitHub-style interface using Primer CSS utilities. This component creates a consistent application shell for all authenticated pages.

#### Layout Hierarchy

```
Main Container (d-flex flex-column, height: 100vh)
├── Top Header (full width)
│   ├── KnowCount (left aligned)
│   └── User Info & Logout (right aligned)
└── Content Container (d-flex flex-1)
    ├── Navigation Sidebar (280px, fixed width)
    │   ├── Directories (collapsible)
    │   ├── Projects (collapsible)
    │   ├── Reports (collapsible)
    │   └── Administration (collapsible)
    └── Main Content Area (flex-1)
        └── Page Content (via slot)
            └── Home.vue / SkillsView.vue / etc.
```

#### Implementation Details

**Header Component**:
- Full-width header spanning entire application
- KnowCount branding positioned on the left
- Welcome message and logout button aligned to the right
- Uses Primer CSS `Header` component with `d-flex flex-justify-between`

**Navigation Sidebar**:
- Fixed width (280px) with proper borders (`border-right color-border-muted`)
- PrimeVue PanelMenu for hierarchical navigation
- Minimal Primer CSS styling overrides for GitHub-style appearance
- Collapsible menu items for organized navigation

**Main Content Area**:
- Uses `flex-1` to fill remaining horizontal space
- Proper padding (`p-4`) and overflow handling (`overflow-y-auto`)
- Contains page-specific content via Vue slot system

#### Usage Pattern

```typescript
// src/components/layout/DashboardLayout.vue
// Provides full application layout with header, sidebar, and content area

// Example usage in a View component:
<template>
  <DashboardLayout>
    <!-- Page content renders in main content area -->
    <h1 class="f1 color-fg-default mb-4">Page Title</h1>
    <div class="d-grid gap-4">
      <!-- Page-specific content goes here -->
    </div>
  </DashboardLayout>
</template>
```

#### Layout Benefits

- **Consistent Navigation**: Persistent sidebar available on all pages
- **Clean Header**: Professional top header with branding and user controls
- **Flexible Content**: Pages can be simple or complex without affecting global layout
- **Responsive Design**: Built with Primer CSS utilities for proper scaling
- **GitHub-Style UX**: Familiar interface patterns for developer-friendly experience

### Route Organization
```typescript
const routes = [
  {
    path: '/auth',
    component: AuthLayout,
    children: [
      { path: 'login', component: LoginView },
      { path: 'logout', component: LogoutView }
    ]
  },
  {
    path: '/',
    component: DashboardLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', component: DashboardView },
      {
        path: 'skills',
        component: SkillsLayout,
        children: [
          { path: '', component: SkillsListView },
          { path: 'new', component: SkillCreateView },
          { path: ':id', component: SkillDetailView }
        ]
      }
      // ... other feature routes
    ]
  }
];
```

## Reusable Components

### SkillSelectionTree Component

The `SkillSelectionTree.vue` component is a central reusable element that provides consistent skill selection functionality across the application:

* **File Location**: `src/components/SkillSelectionTree.vue`

* **Purpose**: 
  * Provides a standardized interface for selecting skills and skill categories
  * Ensures consistent behavior for skill selection across different contexts (roles management, project roles, skill gap analysis)
  * Centralizes skill selection validation logic

* **Key Features**:
  * Tree-based hierarchical selection interface using PrimeVue's Tree component
  * Support for both category-level and individual skill selection
  * Two-way data binding via v-model
  * Pre-selection capabilities for edit operations
  * Validation for skill conflicts and existing assignments
  * Consistent styling and behavior across the application

* **Interface**:
  * **Props**:
    * `modelValue`: Object containing the selection state (`{[key: string]: boolean}`)
    * `preSelectedCategoryIds`: Array of category IDs to pre-select
    * `preSelectedSkillIds`: Array of skill IDs to pre-select
    * `validateSelection`: Optional validation function
    * `helperText`: Optional guidance text
    * `roleId`: Optional role ID for validation against existing assignments

  * **Events**:
    * `update:modelValue`: Emitted when selection changes
    * `selection-change`: Detailed selection change information
    * `validation-error`: Error messages from validation

* **Integration**:
  * Used in `RolesList.vue` for role skill assignment
  * Used in project role components for project-specific skill assignments
  * Reused consistently across features requiring skill selection

* **Benefits**:
  * Reduces code duplication
  * Ensures consistent UX across the application
  * Centralizes changes to skill selection behavior
  * Simplifies the implementation of skill selection in new features

#### Usage

**Usage in Project Management:**
- Used in `RoleForm.vue` for defining project role skill requirements
- Automatically handles adding new skills to the global directory
- Supports both creating new roles and editing existing ones
- Integrates seamlessly with form validation

#### Conditional Rendering via Prop

To keep the `SkillSelectionTree.vue` component reusable across different contexts (project roles vs. general roles), a prop is introduced:

- **Prop Name:** `showSkillLevelSelector` (default: `false`)
- **Behavior:**
  - If `showSkillLevelSelector` is `true`, the skill level dropdown/stepper is rendered next to each selected skill.
  - If `false`, the selector is omitted and only the skill/category checkboxes are shown.
- **Usage:**
  - In project role assignment (e.g., in `ProjectRoles.vue` or project role modals):
    ```vue
    <SkillSelectionTree :showSkillLevelSelector="true" ... />
    ```
  - In general role management (e.g., in `RolesList.vue`):
    ```vue
    <SkillSelectionTree :showSkillLevelSelector="false" ... />
    ```

**Data Model Impact:**
- When emitting selected skills, the component includes the level only if the selector is enabled.
- Validation for skill levels is only enforced if the prop is enabled.

**User Experience:**
- For project roles, users can set and review skill levels directly in the tree view, making the process fast and intuitive.
- For general roles, the UI remains uncluttered, focusing only on skill/category selection.

#### Custom Tree Node Templates: Using PrimeVue Tree's Slot System for Custom Rendering

To support advanced UI requirements—such as inline skill level selectors, custom icons, or additional controls next to each skill node—the `SkillSelectionTree.vue` component leverages PrimeVue Tree's slot system for custom node rendering.

**Key Implementation Details:**
- The component uses the `:template` or `v-slot` feature of PrimeVue's Tree to define how each node is rendered.
- This allows for:
  - Displaying a dropdown (or other input) inline with the skill name when a skill is selected (e.g., for target level selection)
  - Showing custom icons, badges, or action buttons next to nodes
  - Applying conditional styling based on node type (category vs. skill) or selection state
- The slot receives the node data, selection state, and any additional context needed for rendering.
- Event handling (e.g., for dropdowns or buttons) is managed within the slot template, using `@click.stop` or similar modifiers to prevent event conflicts with the tree's own selection logic.

**Example Usage:**
```vue
<template #default="{ node, selected }">
  <div class="tree-node-content">
    <span>{{ node.label }}</span>
    <Select
      v-if="showSkillLevelSelector && selected && node.type === 'skill'"
      class="skill-level-selector"
      :options="levelOptions"
      v-model="skillLevels[node.key]"
      @click.stop
      @mousedown.stop
    />
  </div>
</template>
```

**Benefits:**
- Enables highly flexible, context-aware UI for skill selection and configuration
- Keeps the component reusable for both simple and advanced use cases
- Ensures accessibility and a consistent user experience

This approach is essential for implementing features like inline skill level selection, as required by project role management, while maintaining a clean and maintainable codebase.

## Store Organization

The application leverages the Pinia store for state management, ensuring a centralized and reactive approach to managing application state. This guarantees consistency and predictability across components.

### Usage in the Application
Each feature module defines its own store (e.g., `peopleStore`, `teamsStore`) with the following structure:
- **State**: Defines the data structure and initial values.
- **Getters**: Provides derived or computed data based on the state.
- **Actions**: Encapsulates business logic and state mutations.
- **Reactivity**: Automatically updates components when the state changes.
- **Robust Unique ID Generation**: All new entities (e.g., projects, roles, assignments) are assigned unique IDs using a robust generator to prevent duplicate keys and ensure data integrity.

- **File Location:** Place each store in `src/stores/` and name it according to its domain (e.g., `projectsStore.ts`, `rolesStore.ts`).
- **Store Name:** Use a clear, domain-specific name (e.g., `'projects'`, `'roles'`).
- **State Structure:** Use a flat object for state, with one array per main entity (e.g., `projects`, `projectRoles`, etc.), plus `isLoading` and `error` fields.
- **Initialization:** All entity arrays must be initialized with mock data from `src/mocks/mockData.ts` if empty. This ensures that the application starts with a default dataset, preventing issues caused by an uninitialized or empty state..
- **Persistence:** Use a shared `StorageService` (e.g., from `../services/base/StorageService`) for local storage persistence, as in `rolesStore.ts`. **All entity arrays must be persisted to local storage using `StorageService` on every mutation (add, update, delete, etc.), not just on initialization.**
- **Store Type:** Implement “fat” stores—place all business logic, validation, and CRUD operations for the store’s entities directly in the store’s actions.
- **Error Handling:** All actions must both throw errors and set the `error` field in the store’s state, so consumers can handle errors reactively or via try/catch. **Consumers must use try/catch for all actions.**
- **API Exposure:** Expose full CRUD actions and relevant getters for all managed entities in the store.
- **Robust Unique ID Generation:** All new entities must use a robust unique ID generator (e.g., `generateUniqueId`) to prevent duplicate keys and ensure data integrity, especially under rapid creation or concurrency.
- **Service Layer:** Each store should be accessed via a thin, stable service (e.g., `useProjectsService.ts`) that delegates to the store. This abstraction provides a stable API for views and allows for future changes without affecting consumers.

#### Unique ID Generation Pattern
To guarantee uniqueness (especially when creating multiple entities rapidly), stores use a robust ID generator defined directly inside the store (local utility):

```typescript
function generateUniqueId(prefix = ''): string {
  return `${prefix}${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}
```

This function is used for all new entities in store actions, e.g.:

```typescript
const newRole: Role = {
  id: generateUniqueId('role-'),
  ...roleData
};
```

**Rationale:**
- Prevents duplicate key warnings in Vue lists and DataTables
- Ensures data integrity across all CRUD operations
- Works reliably even under rapid creation or in concurrent environments


### Store Modules

```typescript

// userStore.ts
interface UsersState {
  users: Record<string, User>;
  loading: boolean;
  error: Error | null;
}

// peopleStore.ts
export interface PeopleState {
  people: Person[];
  filteredPeople: Person[];
  filters: {
    teamId?: string;
    roleId?: string;
    skillId?: string;
    securityGroupId?: string;
    searchText?: string;
  };
  isLoading: boolean;
  error: string | null;
}

// rolesStore.ts
interface RolesState {
  roles: Role[];
  isLoading: boolean;
  error: string | null;
}

// teams.ts
interface TeamsState {
  selectedTeam: Team | null;
  loading: boolean;
  error: string | null;
  filter: TeamFilter;
}
```

## Service Organization

### Base Service Pattern
```typescript
// Base service pattern
abstract class BaseService<T> {
  abstract getAll(): Promise<T[]>;
  abstract getById(id: string): Promise<T>;
  abstract create(data: Partial<T>): Promise<T>;
  abstract update(id: string, data: Partial<T>): Promise<T>;
  abstract delete(id: string): Promise<void>;
}

// Implementation example
class UserService extends BaseService<User> {
  private storage: LocalStorageService;

  constructor() {
    super();
    this.storage = new LocalStorageService('users');
  }

  // Implementation of abstract methods
}
```

## Styling Organization

### CSS Architecture

The application uses a **hybrid styling approach** combining:

1. **Primer CSS (GitHub Design System)**
   - Utility-first CSS framework providing consistent design tokens
   - Layout utilities: `d-flex`, `d-grid`, `p-4`, `mb-3`, etc.
   - Typography utilities: `f1`, `f3`, `f4`, `lh-condensed`, etc.
   - Color utilities: `color-fg-default`, `color-bg-subtle`, `color-accent-fg`, etc.
   - Component utilities: `Box`, `rounded-2`, `hover-grow`, etc.

2. **PrimeVue Components**
   - Advanced UI components (Tree, DataTable, Dialog, etc.)
   - Integrated with Primer CSS color scheme
   - Aura theme preset for consistent theming

3. **Minimal Custom CSS**
   - Only for features not covered by Primer CSS
   - Hover effects, transitions, and component-specific styling
   - Responsive adjustments when needed

### Styling Guidelines

```typescript
// Example component using Primer CSS utilities
<template>
  <div class="Box p-4 color-shadow-medium">
    <h3 class="f3 color-fg-default mb-2 lh-condensed">Card Title</h3>
    <p class="f4 color-fg-muted mb-3 lh-default">Card description</p>
    <div class="d-flex flex-items-center">
      <i class="pi pi-folder f3 color-accent-fg mr-3"></i>
      <span class="f4 color-fg-default">Action Item</span>
    </div>
  </div>
</template>

<style scoped>
/* Minimal custom styles only when needed */
.hover-grow {
  transition: transform 0.2s ease-in-out;
}
.hover-grow:hover {
  transform: translateY(-2px);
}
</style>
```

### Benefits of This Approach

- **Consistency**: Primer CSS provides GitHub's proven design system
- **Maintainability**: Reduced custom CSS from ~220 lines to ~30 lines per component
- **Performance**: Utility classes are highly optimized and reusable
- **Accessibility**: Built-in accessibility features from Primer CSS
- **Theme Support**: Automatic light/dark theme compatibility
- **Developer Experience**: Predictable utility classes with clear naming

## Component Configuration Standards

This section defines standard configuration patterns for UI components used throughout the application. Following these standards ensures visual consistency, improves developer experience, and simplifies maintenance.

### 1. DataTable Component

All data tables in the application should follow a standardized configuration pattern based on the PrimeVue DataTable component as implemented in TeamList.vue. This ensures consistent appearance and behavior across all table views.

#### DataTable Standard Configuration

```vue
<DataTable 
  :value="items"                      <!-- Data array to display -->
  v-model:selection="selectedItem"    <!-- Selected item binding -->
  selectionMode="single"              <!-- Allow single row selection -->
  dataKey="id"                        <!-- Unique identifier field -->
  :paginator="items.length > 10"      <!-- Conditional pagination -->
  :rows="10"                          <!-- Items per page -->
  :filters="filters"                  <!-- Filter model -->
  filterDisplay="menu"                <!-- Filter UI style -->
  :globalFilterFields="['name', 'description']" <!-- Global search fields -->
  stripedRows                         <!-- Alternate row styling -->
  responsiveLayout="scroll"           <!-- Mobile behavior -->
  :rowClass="(data) => `item-row item-row-${data.id}`" <!-- Dynamic row classes -->
  @rowClick="$emit('rowClick', $event.data.id)"> <!-- Row click handling -->
  <!-- Column definitions go here -->
</DataTable>
```

#### Key Configuration Parameters

| Parameter | Description | Standard Value |
|-----------|-------------|----------------|
| `:value` | Array of items to display | Component-specific data array |
| `v-model:selection` | Binding for selected row(s) | Component-specific ref |
| `selectionMode` | Row selection mode | `"single"` for most tables |
| `dataKey` | Unique identifier field | `"id"` |
| `:paginator` | Enable pagination | Conditional based on row count |
| `:rows` | Items per page | `10` |
| `filterDisplay` | Filter UI style | `"menu"` for dropdown filters, `"row"` for inline filters |
| `stripedRows` | Alternate row styling | `true` |
| `responsiveLayout` | Mobile behavior | `"scroll"` |
| `:rowClass` | Dynamic row classes | Used for styling and testing |
| `@rowClick` | Row click handler | Emit event with row id |

#### Standard Column Structure

Each DataTable should include columns appropriate to its data model, but follow these patterns:

```vue
<!-- Standard column with sorting -->
<Column field="name" header="Name" sortable>
  <!-- Optional custom body template -->
  <template #body="slotProps">
    <div class="name-cell">{{ slotProps.data.name }}</div>
  </template>
</Column>

<!-- Column with custom filtering -->
<Column field="category" header="Category" sortable>
  <template #filter="{ filterModel, filterCallback }">
    <Select 
      v-model="filterModel.value" 
      :options="categoryOptions" 
      optionLabel="label" 
      optionValue="value"
      placeholder="All Categories" 
      class="p-column-filter" 
      @change="filterCallback()"
    />
  </template>
</Column>

<!-- Actions column -->
<Column header="Actions" :exportable="false">
  <template #body="slotProps">
    <div class="action-buttons">
      <Button icon="pi pi-eye" @click.stop="viewItem(slotProps.data)" />
      <Button icon="pi pi-pencil" @click.stop="editItem(slotProps.data)" />
      <Button icon="pi pi-trash" @click.stop="confirmDelete(slotProps.data)" />
    </div>
  </template>
</Column>
```

#### Data Management Pattern

Tables should follow this pattern for data management:

1. **Props**: Accept data and filters from parent components
2. **Events**: Emit actions to be handled by parent components
3. **Local State**: Maintain minimal local state for UI concerns only
4. **Store Integration**: Use Pinia stores for shared state management

#### Accessibility Considerations

- Include proper `aria-label` attributes on action buttons
- Use `data-testid` attributes for important elements to support testing
- Ensure color contrast meets WCAG standards
- Provide meaningful labels for filter controls

### 2. Tree Component

Tree views in the application (such as skill selection, organizational hierarchies) should follow a standardized configuration based on the PrimeVue Tree component. The main implementation reference is the `SkillSelectionTree.vue` component.

#### Tree Standard Configuration

```vue
<Tree
  :value="treeNodes"
  :expandedKeys="expandedKeys"
  :selectionKeys="selectedKeys"
  :selectionMode="selectionMode"
  dataKey="key"
  @node-select="onNodeSelect"
  @node-unselect="onNodeUnselect"
  @node-expand="onNodeExpand"
  @node-collapse="onNodeCollapse"
  class="skill-tree">
  <!-- Custom node template -->
  <template #default="{ node }">
    <div class="tree-node-content">
      <span class="node-label">{{ node.label }}</span>
      <!-- Optional additional controls based on node type -->
    </div>
  </template>
</Tree>
```

#### Key Configuration Parameters

| Parameter | Description | Standard Value |
|-----------|-------------|----------------|
| `:value` | Array of tree nodes | Component-specific hierarchy |
| `:expandedKeys` | Object with expanded node keys | Component-specific state |
| `:selectionKeys` | Object with selected node keys | Component-specific state |
| `:selectionMode` | Selection behavior | `"checkbox"` for multi-select, `"single"` for single-select |
| `dataKey` | Node identifier property | `"key"` |

#### Data Structure

Tree nodes should follow this standard structure:

```typescript
interface TreeNode {
  key: string;          // Unique identifier
  label: string;        // Display text
  type: string;         // Node type (e.g., 'category', 'skill')
  children?: TreeNode[]; // Child nodes
  selectable?: boolean;  // Whether node can be selected
  data?: any;           // Additional data
}
```

#### Accessibility Considerations

- Implement proper keyboard navigation
- Use ARIA attributes for tree role and relationships
- Ensure focus states are visible
- Provide meaningful labels for interactive elements

### 3. Form Components

Forms throughout the application should follow standardized patterns for consistency in validation, layout, and user interaction. The application uses PrimeVue form components styled with Primer CSS utility classes.

#### Standard Form Layout

```vue
<form @submit.prevent="onSubmit" class="form-container" novalidate>
  <div class="form-group">
    <label for="fieldId" class="required">Field Label</label>
    <InputText 
      id="fieldId"
      v-model="formData.fieldName" 
      :class="{'p-invalid': hasError('fieldName')}"
      aria-describedby="fieldId-help" />
    <small id="fieldId-help" class="p-error">{{ getErrorMessage('fieldName') }}</small>
  </div>
  
  <div class="form-actions">
    <Button type="button" label="Cancel" class="p-button-secondary" @click="onCancel" />
    <Button type="submit" label="Submit" class="p-button-primary" :loading="isSubmitting" />
  </div>
</form>
```

#### Validation Pattern

Form validation should use a consistent pattern:

```typescript
// Form validation setup
const { value: formData, errorMessage, validate } = useVuelidate(rules, initialData);

// Submit handler with validation
const onSubmit = async () => {
  const isValid = await validate();
  if (!isValid) return;
  
  // Form submission logic
};

// Helper methods
const hasError = (field) => !!errorMessage.value[field];
const getErrorMessage = (field) => errorMessage.value[field] || '';
```

### 4. Modal Dialogs

Modal dialogs should follow a consistent pattern based on PrimeVue's Dialog component:

```vue
<Dialog
  v-model:visible="dialogVisible"
  :modal="true"
  :closable="true"
  :header="dialogTitle"
  :style="{width: '500px'}"
  :dismissableMask="true"
  :closeOnEscape="true">
  
  <div class="dialog-content">
    <!-- Dialog content goes here -->
  </div>
  
  <template #footer>
    <Button label="Cancel" icon="pi pi-times" class="p-button-text" @click="closeDialog" />
    <Button label="Save" icon="pi pi-check" @click="saveAndClose" autofocus />
  </template>
</Dialog>
```

#### Key Configuration Parameters

| Parameter | Description | Standard Value |
|-----------|-------------|----------------|
| `v-model:visible` | Dialog visibility state | Component-specific ref |
| `:modal` | Whether background is blocked | `true` |
| `:closable` | Allow closing via X button | `true` |
| `:dismissableMask` | Close when clicking outside | `true` |
| `:closeOnEscape` | Close when pressing escape | `true` |
| `:header` | Dialog title | Context-specific |
| `:style` | Dialog size | Width between 400-800px depending on content |

#### Accessibility Considerations

- Return focus to trigger element when closed
- Trap focus inside modal when open
- Support keyboard navigation for all actions
- Provide descriptive ARIA labels

