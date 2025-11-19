# Overall Architecture and Technology Stack

## References

### QA testing documentation
- Location:  `docs/tech_design/testing/QA_testing.md`
   - Testing Strategy
   - Tests Structure
   - Test Data Management
   - Testing Patterns and Best Practices
   - Accessibility Testing Requirements

## Project Structure
```
./                          # Project root (where this architecture.md is located)
  ├── src/               # Source code
  │   ├── components/    # Reusable UI components
  │   ├── views/        # Page components
  │   ├── types/        # Data Models
  │   ├── router/       # Route definitions
  │   ├── store/        # Pinia stores
  │   ├── services/     # Business logic and data access
  │   ├── models/       # TypeScript interfaces and types
  │   ├── utils/        # Helper functions and utilities
  │   └── assets/       # Static assets
  ├── public/          # Static public assets
  └── dist/           # Build output (generated)
```

## Technology Stack

**Package Name**: knowledge-accounting

### Frontend
- **Framework**: Vue 3 with Composition API
  - Vue.js version: ^3.4.0 || ^3.5.0
  - @vue/compiler-sfc: (same version as Vue.js)
- **Build Tool**: Vite
  - vite version: ^4.5.0 <6.0.0
  - @vitejs/plugin-vue: ^4.5.0
  - esbuild: ^0.19.0 (for security compliance)
- **Language**: TypeScript
  - typescript version: ^5.0.0
  - @types/node: ^18.0.0
- **Router**: Vue Router (history mode)
  - vue-router version: ^4.2.0
- **State Management**: Pinia
  - pinia version: ^2.1.0
- **UI Components**: PrimeVue for advanced components
  - primevue version: ^4.0.0
  - @primevue/themes version: ^4.0.0
- **CSS Framework**: Primer CSS (GitHub's design system)
  - @primer/css version: latest
  - Utility-first CSS classes for consistent styling
  - Integrated with PrimeVue components
- **Storage**: Local Storage with TypeScript wrapper
  - Custom implementation
- **Testing**: Playwright for E2E testing
  - @playwright/test: ^1.40.0
  - @types/playwright: ^1.40.0

### Version Compatibility Notes
1. **Vue Ecosystem**:
   - Vue.js and @vue/compiler-sfc MUST always be at the same version
   - When updating Vue, all related packages (@vitejs/plugin-vue, vue-router, primevue) MUST be checked for compatibility
   - PrimeVue version MUST be compatible with Vue 3 composition API
2. **Build Tools**:
   - Vite version MUST be kept below 6.0.0 to maintain stability
   - esbuild version MUST be ≥0.19.0 for security compliance
3. **UI Components**:
   - PrimeVue and @primevue/themes MUST be at the same version
   - PrimeVue theme compatibility MUST be verified when updating
   - Primer CSS MUST be compatible with PrimeVue components
   - Primer CSS utility classes should be used for layout, spacing, and typography
4. **Version Updates**:
   - All version updates MUST be tested in development environment before deployment
   - Security audits MUST be run after dependency updates
   - Breaking changes in major versions MUST be reviewed and documented

### Development Environment Requirements
- Node.js: ≥18.x
- npm: ≥9.x
- Git: ≥2.x
- Operating System: Cross-platform compatible
- IDE: VS Code recommended with extensions:
  - Vue Language Features (Volar)
  - TypeScript Vue Plugin (Volar)
  - ESLint
  - Prettier

### All configuration files MUST be placed in the package directory (e.g. `./knowledge-accounting/`):
1. **package.json** - Package root (`./knowledge-accounting/package.json`)
2. **tsconfig.json** - TypeScript config (`./knowledge-accounting/tsconfig.json`)
3. **vite.config.ts** - Vite config (`./knowledge-accounting/vite.config.ts`)
4. **playwright.config.ts** - Playwright config (`./knowledge-accounting/playwright.config.ts`)
5. **.gitignore** - Git ignore rules (`./knowledge-accounting/.gitignore`)

### Configuration Files
1. **package.json**
```json
{
  "name": "knowledge-accounting",
  "private": true,
  "version": "0.1.0",
  "type": "module",
  "engines": {
    "node": ">=18.0.0",
    "npm": ">=9.0.0"
  },
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc && vite build",
    "test": "playwright test",
    "lint": "eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --fix --ignore-path .gitignore",
    "format": "prettier --write src/"
  },
  "dependencies": {
    "vue": "^3.5.13",
    "vue-router": "^4.5.0",
    "pinia": "^2.1.0",
    "primevue": "^4.0.0",
    "@primevue/themes": "^4.0.0",
    "@primer/css": "latest",
    "typescript": "^5.8.3"
  },
  "devDependencies": {
    "@playwright/test": "^1.52.0",
    "@vitejs/plugin-vue": "^5.2.3",
    "@vue/tsconfig": "^0.5.1",
    "vite": "^6.3.1",
    "vue-tsc": "^2.2.8"
  }
}
```

2. **tsconfig.json**
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "module": "ESNext",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "preserve",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src/**/*.ts", "src/**/*.d.ts", "src/**/*.tsx", "src/**/*.vue"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

3. **vite.config.ts**
```typescript
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
```

4. **playwright.config.ts**
```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    port: 5173,
    reuseExistingServer: !process.env.CI,
  },
})
```

5. **main.ts** (Application setup with PrimeVue and Primer CSS)
```typescript
import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import App from './App.vue'
import router from './router'
import '@primer/css/dist/primer.css'
import './style.css'

const app = createApp(App)
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura
  }
})
app.mount('#app')
```

6. **.gitignore**
```
# Logs
logs
*.log
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*
lerna-debug.log*

node_modules
dist
dist-ssr
*.local
/test-results/
/playwright-report/
/playwright/.cache/

# Editor directories and files
.vscode/*
!.vscode/extensions.json
.idea
.DS_Store
*.suo
*.ntvs*
*.njsproj
*.sln
*.sw?
```

### Build and Test Requirements
1. **Build Process**
   - Development server MUST start on port 5173
   - Production build MUST generate static assets in 'dist' directory
   - TypeScript compilation MUST have zero errors
   - ESLint MUST report zero errors
   - Prettier MUST format all files

2. **Test Requirements**
   - Playwright tests MUST run in Chromium
   - Tests MUST run in CI environment
   - Test coverage MUST include critical paths
   - E2E tests MUST verify core functionality

3. **Project Structure Validation**
   - All required directories MUST exist
   - All configuration files MUST be present
   - All required dependencies MUST be installed
   - Git repository MUST be initialized

## Architecture Overview

### Component Architecture
```
src/
├── components/         # Reusable UI components
├── views/             # Page components
├── types/             # Data Models
├── router/            # Route definitions
├── store/             # Pinia stores
├── services/          # Business logic and data access
├── models/            # TypeScript interfaces and types
├── utils/            # Helper functions and utilities
└── assets/           # Static assets
```

### Key Architectural Decisions

1. **Component Design**
   - Custom-built UI components with PrimeVue for advanced functionality
   - PrimeVue Tree component for hierarchical data display
   - **UI Component Requirements** (MUST be adhered to strictly):
     - PrimeVue v4 Tabs components MUST be used for tabbed interfaces:
       - `Tabs` - the main container component
       - `TabList` - contains the tab headers/buttons
       - `Tab` - individual tab header/button
       - `TabPanels` - contains the tab content panels
       - `TabPanel` - individual tab content panel
     - Do NOT use deprecated components such as `TabView`    
   - Composition API for component logic
   - TypeScript for type safety and better IDE support

2. **State Management**
   - Pinia stores for global state management
   - Separate stores for:
     - Authentication state
     - User data
     - Teams
     - Skills
     - Projects
     - Roles
     - Notifications
   - **Mock Data Support**: All stores support population with mock data for testing
   - **Test Isolation**: Stores support reset and re-population for test isolation
   - **Data Consistency**: Mock data maintains referential integrity across stores

3. **Data Storage**
   - Local Storage wrapper for data persistence
   - TypeScript interfaces for type-safe data access
   - CRUD operations implementation
   - Data relationship maintenance
   - Centralized test fixtures for prototype and testing data
     

4. **Authentication & Authorization**
   - Simulated authentication flow
   - Full RBAC (Role-Based Access Control) implementation
   - Session management using local storage
   - Permission-based component rendering

5. **Routing**
   - History mode for clean URLs
   - Route guards for authentication
   - Nested routes for hierarchical views
   - Dynamic route loading

### Data Flow Architecture

The application follows a clear data flow architecture that connects views, services, and stores:

#### 1. View → Service → Store Pattern

This is the standard pattern for data flow in the application:

```
View Component → Service Layer → Store → Local Storage
```

In this pattern:
- **Views** interact only with services, never directly with storage
- **Services** use stores for state management, data manipulation, and persistence
- **Stores** handle the actual state manipulation and storage interactions

**Example Flow:**
1. A view calls `rolesService.createRole('Developer')`
2. The service delegates to `rolesStore.createRole('Developer')`
3. The store updates its state and persists to local storage
4. Reactive updates flow back to all components using the store

#### 2. Store Structure and Organization

Each store follows a consistent structure:

```typescript
export const useRolesStore = defineStore('roles', {
  // 1. State definition with initial values
  state: (): RolesState => ({
    roles: storage.get<Role[]>(ROLES_KEY) || [],
    isLoading: false,
    error: null as string | null,
  }),
  getters: {
    getItems: (state) => state.items,
    getItemById: (state) => (id: string) => state.items.find(item => item.id === id),
  },
  actions: {
    createItem(data: Omit<Item, 'id'>) {
      try {
        const newItem: Item = { id: generateUniqueId('item-'), ...data };
        this.items.push(newItem);
        storage.set(ITEMS_KEY, this.items);
        return newItem;
      } catch (err) {
        this.error = (err as Error).message;
        throw err;
      }
    },
    deleteRole(roleId: string) { /* ... */ },
    // Other CRUD operations and specialized actions
  }
});
```
#### 3. Store Persistence Requirements

All stores MUST:
- Use `StorageService` for local storage persistence
- Persist data on every mutation (add, update, delete operations)
- Initialize with mock data if storage is empty
- Handle storage errors gracefully
- Support mock data population for testing purposes
- Maintain data consistency and referential integrity when populated with mock data

#### 4. Mock Data Integration for Testing

The architecture supports comprehensive testing through mock data integration that follows the centralized approach defined in the QA testing documentation:

**Mock Data Population Strategy:**
- **Centralized Source**: All mock data originates from centralized mock data sources
- **Store Population**: Stores can be populated with mock data for testing scenarios
- **Data Consistency**: Mock data maintains referential integrity across all stores
- **Test Isolation**: Stores support reset and re-population for test isolation

**Store Mock Data Support:**
- **Initialization**: Stores can be initialized with mock data during testing setup
- **State Population**: Store state can be populated with mock data objects
- **Reset Capability**: Stores support reset functionality for test cleanup
- **Validation**: Store state can be validated against expected mock data

**Service Layer Mock Data Integration:**
- **Mock Data Usage**: Services can use populated store data for business logic testing
- **RBAC Testing**: Mock user data enables testing of role-based access control
- **Data Flow Testing**: Mock data supports testing of complete data flow patterns
- **Integration Testing**: Services can test interactions with populated stores

**Testing Patterns:**
- **Store Initialization**: `beforeEach` hooks populate stores with mock data
- **Store Reset**: `afterEach` hooks reset stores for test isolation
- **State Validation**: Store state can be validated against mock data expectations
- **Data Flow Verification**: Complete View → Service → Store → Local Storage flows can be tested

For detailed testing patterns, mock data structures, and implementation examples, refer to the QA testing documentation.

#### 5. Service Layer as Abstraction

Services act as an abstraction layer between views and stores. They:
- Provide a stable API for views that won't change even if the underlying store implementation changes
- Handle cross-cutting concerns like logging, analytics, or error handling
- Can orchestrate operations across multiple stores when needed
- Integrate with mock data for testing purposes through centralized mock data sources

**Example Service Implementation:**
```typescript
export function useRolesService() {
  const rolesStore = useRolesStore();

  function getRoles(): Role[] {
    return rolesStore.getRoles;
  }
  
  function createRole(roleName: string, skillCategoryIds: string[] = [], skillIds: string[] = []): Role {
    return rolesStore.createRole(roleName, skillCategoryIds, skillIds);
  }
  
  // Additional methods that use the store
}
```

#### 5. View Integration

Views typically integrate with services rather than directly with stores:

```typescript
// In a Vue component
import { useRolesService } from '../services/useRolesService';

const rolesService = useRolesService();
const roles = ref<Role[]>([]);

onMounted(() => {
  roles.value = rolesService.getRoles();
});
```

For more reactive applications, views can directly leverage store reactivity while still using services for actions:

```typescript
// Using store for reactive state while using service for actions
import { useRolesStore } from '../stores/rolesStore';
import { useRolesService } from '../services/useRolesService';

const rolesStore = useRolesStore();
const rolesService = useRolesService();

// Reactive roles from store
const roles = computed(() => rolesStore.roles);

function handleCreateRole(roleName: string) {
  // Actions still go through the service
  rolesService.createRole(roleName);
}
```

**Mock Data Integration in Views:**
- **Testing Support**: Views can be tested with mock data populated stores
- **State Validation**: View state can be validated against mock data expectations
- **User Interaction Testing**: Views support testing of user interactions with mock data
- **Component Testing**: Individual components can be tested with mock data scenarios

This architecture provides a clean separation of concerns while ensuring that state is managed consistently across the application and supports comprehensive testing through mock data integration.


### Testing Architecture

The application architecture is designed to support comprehensive testing through mock data integration and store population strategies:

**Mock Data Architecture:**
- **Centralized Approach**: All mock data follows the centralized approach defined in the QA testing documentation
- **Store Population**: Stores support population with mock data for testing scenarios
- **Data Consistency**: Mock data maintains referential integrity across all stores
- **Test Isolation**: Stores support reset and re-population for test isolation

**Testing Data Flow:**
- **Complete Flow Testing**: The View → Service → Store → Local Storage flow can be fully tested with mock data
- **Store State Testing**: Store state can be validated against mock data expectations
- **Service Integration Testing**: Services can be tested with populated stores
- **Component Testing**: Components can be tested with mock data scenarios

**Testing Patterns Support:**
- **Store Initialization**: Stores support initialization with mock data during test setup
- **Store Reset**: Stores support reset functionality for test cleanup
- **State Validation**: Store state can be validated against expected mock data
- **Integration Testing**: Complete application flows can be tested with mock data

**Mock Data Integration Points:**
- **Store Level**: Stores can be populated with mock data objects
- **Service Level**: Services can use populated store data for business logic testing
- **View Level**: Views can be tested with mock data populated stores
- **Component Level**: Individual components can be tested with mock data scenarios

For detailed testing patterns, mock data structures, and implementation examples, refer to the QA testing documentation.

### Security Considerations

1. **Authentication**
   - Password strength validation
   - Account lockout after failed attempts
   - Secure session storage

2. **Authorization**
   - Role-based access control
   - Component-level permissions
   - Route protection

3. **Data Security**
   - Input validation
   - XSS prevention
   - CSRF protection

### Performance Considerations

1. **Code Splitting**
   - Route-based code splitting
   - Lazy loading of components
   - Dynamic imports for large features

2. **Caching**
   - Local storage data caching
   - Component caching where appropriate
   - Static asset caching

3. **State Management**
   - Efficient store updates
   - Computed properties for derived data
   - Watchers for reactive updates

### Error Handling

1. **Global Error Handling**
   - Vue error boundary implementation
   - Error logging system
   - User-friendly error messages

2. **Form Validation**
   - Client-side validation
   - Custom validation rules
   - Error message display

3. **API Error Handling**
   - Local storage operation error handling
   - Retry mechanisms
   - Fallback strategies


## Frontend Layout Architecture

The application employs a consistent, two-part layout structure for all authenticated views to ensure a predictable and scalable user experience, implemented through component composition rather than route nesting.

### Core Principles

1.  **Component-Based Layout**: The `DashboardLayout.vue` component (`src/components/layout/DashboardLayout.vue`) serves as a wrapper for all authenticated feature views. Each feature view explicitly includes this layout component rather than relying on route nesting.

2.  **Header and Navigation**: The layout includes a persistent top header with the application title and user information, as well as a main navigation sidebar that is always visible on the left side of the screen.

3.  **Content Projection**: Feature-specific content is injected into the layout using Vue's slot system, allowing each view to maintain its own internal structure while inheriting the consistent navigation framework.

### Layout Implementation

**Composition Pattern:**

```vue
<!-- Feature view (e.g., RolesView.vue) -->
<template>
  <DashboardLayout>
    <!-- Feature-specific content -->
  </DashboardLayout>
</template>
```

This component composition approach offers several advantages:
- Clear separation of layout and feature concerns
- Consistent user experience across the application
- Individual features can focus on their specific UI requirements

### Layout Structure

**Complete Layout Structure:**

```
[     Header with App Title and User Info     ]
[Main Nav Sidebar] | [  Feature Content Area  ]
```

### Sub-Layout Patterns

Feature views can implement their own internal layouts within the content area provided by the DashboardLayout:

**Example with a Two-Column Feature (`/roles`):**

```
[     Header with App Title and User Info     ]
[Main Nav Sidebar] | [Role List | Role Details]
```

**Example with a Complex Feature (`/skills`):**

```
[     Header with App Title and User Info     ]
[Main Nav Sidebar] | [Skills Sidebar | Skills Content]
```

This pattern ensures consistent navigation while allowing features to implement specialized layouts tailored to their specific requirements.

