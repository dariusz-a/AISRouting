# Working Code Generation Prompt: Feature 7.3: Person Profile

## Task: 
Generate working code for Feature 7.3: Person Profile, following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using Vue, TypeScript, and Vite.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing accessible, resilient, and comprehensive Playwright tests following best practices.
- Ensuring all code aligns with project technical constraints, including RBAC, data relationships, and local storage via service layers.
- Practicing test-driven development (TDD) by writing and running tests before implementation.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in UI/UX best practices, accessibility, and resilient frontend engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.
- Ensuring all generated UI code uses semantic HTML elements and includes proper accessibility attributes (e.g., <label> tags, aria-label, aria-labelledby, role attributes) on interactive elements so that tests can reliably select them with getByRole, getByLabel, and other accessibility-first selectors.

## References
- BDD Scenarios: docs/spec_scenarios/manage_people_spec.md
- Test File: tests/manage_people.spec.ts
- Feature Design Document: docs/tech_design/core_features/people_management_design.md
- Application Architecture `docs/tech_design/overall_architecture.md`
- Application Organization: `docs/tech_design/application_organization.md`

## Development Approach
Follow Test-Driven Development (TDD) cycle:
1. For each task in the implementation plan, MUST do the following:
   a. Run only the relevant test cases associated with the current task.
   b. Analyze the test failures for this task.
   c. Make minimal changes to fix the failing test(s) for this task.
   d. Run the same test cases again to confirm success.
   e. Only proceed to the next task after the current task's tests pass.
2. Repeat until all tasks in the implementation plan are complete and all tests pass.

## Implementation Plan

### Scenario: Implement Person's Profile
**BDD Scenario:**
```gherkin
### Scenarion: Implement Person's Profile
  Given a user is on the People Directory page
  When they click on on person's name
  Then the Person's Profile for the selected person will open
  And they should see:
    | Tab                    | Content Description                          |
    | Summary                | Basic details like name, email.              |
    | Roles                  | List of assigned roles                       |
    | Skills.                | Skills associated with the person            |
    | Security.              | Current security group                       |
```

**Technical Design Details (inlined):**

The Person Profile feature provides a comprehensive view of a person's information through a tabbed interface. This feature addresses the need for users to access detailed information about individuals in the system, including their basic details, organizational roles, skill assessments, and security configuration. The design follows a modular approach where each tab represents a distinct aspect of the person's profile, allowing for organized and intuitive information presentation.

**Component Architecture:**
The Person Profile uses a **hierarchical component structure** where the main profile component serves as a coordinator for multiple specialized tab components. This approach allows for clean separation of concerns and makes the codebase more maintainable.

**Component Hierarchy:**
```
PersonProfile (Container)
├── PersonSummaryTab (Basic Information)
├── PersonRolesTab (Role Assignments)
├── PersonSkillsTab (Skill Assessments)
└── PersonSecurityTab (Security Information)
```

**Tabbed Interface Design:**
The tabbed interface uses **PrimeVue v4 Tabs components** to provide a modern, accessible user experience. The design emphasizes clarity and organization, with each tab representing a logical grouping of related information.

**Tab Organization Strategy:**
1. **Summary Tab**: Provides an overview of essential person information
2. **Roles Tab**: Shows organizational role assignments and their implications
3. **Skills Tab**: Displays skill assessments organized by categories
4. **Security Tab**: Presents security and access control information

**Data Integration Strategy:**
The Person Profile integrates with multiple services to gather comprehensive information about a person. This **service-oriented approach** ensures that data is consistently managed and that the profile can access all relevant information from across the system.

**Service Integration Pattern:**
The profile component coordinates with multiple services to build a complete picture of the person:
- **People Service**: Core person information and relationships
- **Teams Service**: Team membership and organizational structure
- **Roles Service**: Role assignments and associated skills
- **Skills Service**: Skill categories and assessment data
- **User Service**: Login access and account information
- **Security Groups Service**: Permissions and access control

**Navigation Integration:**
The Person Profile feature integrates seamlessly with the existing People Directory through **enhanced navigation patterns**. This integration follows the **drill-down pattern**, where users can navigate from a list view to detailed information about a specific item.

**Tasks:**

1. **Create PersonProfile component with tabbed interface structure**
   - File: `src/components/people/PersonProfile.vue`
   - Implement main container component with PrimeVue v4 Tabs
   - Add header with person name and edit button (for administrators)
   - Create tab structure with Summary, Roles, Skills, and Security tabs
   - Implement reactive state management for person data and active tab
   - Add loading and error states
   - Include proper accessibility attributes for tab navigation

2. **Create PersonSummaryTab component for basic information display**
   - File: `src/components/people/PersonSummaryTab.vue`
   - Display person's full name, email, team, roles, and login access status
   - Use grid layout for organized information display
   - Integrate with TeamsService and RolesService for data resolution
   - Add computed properties for team name and roles display
   - Include proper semantic HTML with labels and values
   - Add responsive design with CSS Grid

3. **Create PersonRolesTab component for role assignments display**
   - File: `src/components/people/PersonRolesTab.vue`
   - Display assigned roles with associated skill categories
   - Show role count and empty state when no roles assigned
   - Use card-based layout for each role with skill implications
   - Integrate with RolesService and SkillsService for data resolution
   - Add helper function for skill category name resolution
   - Include proper visual hierarchy with role headers and skill lists

4. **Create PersonSkillsTab component for skill assessments display**
   - File: `src/components/people/PersonSkillsTab.vue`
   - Display skills organized by categories with assessment details
   - Show skill level, assessment type, and assessment date for each skill
   - Use grid layout for skills within each category
   - Integrate with SkillsService for skill and category data
   - Add computed properties for skill count and category organization
   - Include helper functions for skill level, assessment type, and date formatting

5. **Create PersonSecurityTab component for security information display**
   - File: `src/components/people/PersonSecurityTab.vue`
   - Display login access status, security group, and permissions
   - Show account status information for users with login access
   - Use conditional display for account-specific information
   - Integrate with UserService and SecurityGroupsService
   - Add status indicators with color coding for security states
   - Include permission tags display for security group permissions

6. **Add PersonProfileView page component for routing**
   - File: `src/views/people/PersonProfileView.vue`
   - Create view component that wraps PersonProfile component
   - Extract personId from route parameters
   - Pass personId to PersonProfile component
   - Add proper page layout and styling

7. **Update router configuration to add Person Profile route**
   - File: `src/router/index.ts`
   - Add route for `/people/:personId` path
   - Configure route to use PersonProfileView component
   - Add meta information for authentication and title
   - Ensure route is properly integrated with existing navigation

8. **Enhance PeopleDirectory component to support navigation to Person Profile**
   - File: `src/components/people/PeopleDirectory.vue`
   - Add clickable person names that navigate to profile view
   - Implement navigation handler using Vue Router
   - Add proper styling for clickable person name links
   - Include accessibility attributes for navigation links
   - Add hover effects and focus states for better UX

9. **Enhance usePeopleService to support Person Profile functionality**
   - File: `src/services/usePeopleService.ts`
   - Add getPersonWithDetails method for comprehensive person data
   - Add getPersonSkillAssessments method for skill assessment data
   - Integrate with related services (Teams, Roles, Skills, Users, SecurityGroups)
   - Add proper error handling and type safety
   - Ensure service methods return properly typed data

10. **Add PersonWithDetails interface to type definitions**
    - File: `src/types/person.ts`
    - Create PersonWithDetails interface extending Person
    - Add optional team, roles, user, and skillAssessments properties
    - Add PersonProfileData interface for comprehensive profile data
    - Ensure proper TypeScript typing for all profile-related data

11. **Add Person Profile E2E tests to manage_people.spec.ts**
    - File: `tests/manage_people.spec.ts`
    - Add test for displaying person profile with tabbed interface
    - Add test for navigating from People Directory to Person Profile
    - Add test for displaying person roles in Roles tab
    - Add test for displaying person skills in Skills tab
    - Add test for displaying security information in Security tab
    - Include proper test data setup and assertions

12. **Add permission checking for Person Profile access**
    - File: `src/utils/permissions.ts` (create if doesn't exist)
    - Add canViewPersonProfile function for basic profile access
    - Add canEditPersonFromProfile function for edit permissions
    - Add canViewSecurityInfo function for security tab access
    - Implement role-based access control for different profile features
    - Ensure proper security validation for sensitive information

## Code Examples

### Main PersonProfile Component Structure
```typescript
// src/components/people/PersonProfile.vue
<template>
  <div class="person-profile">
    <!-- Profile Header with Person Name and Actions -->
    <div class="profile-header">
      <h1>{{ person?.firstName }} {{ person?.lastName }}</h1>
      <div class="profile-actions" v-if="canEditPerson">
        <Button 
          label="Edit Person" 
          icon="pi pi-pencil" 
          @click="openEditDialog"
          variant="secondary"
        />
      </div>
    </div>

    <!-- Tabbed Interface for Organized Information Display -->
    <Tabs v-model:activeIndex="activeTabIndex">
      <TabList>
        <Tab>Summary</Tab>
        <Tab>Roles</Tab>
        <Tab>Skills</Tab>
        <Tab>Security</Tab>
      </TabList>
      
      <TabPanels>
        <TabPanel>
          <PersonSummaryTab :person="person" />
        </TabPanel>
        <TabPanel>
          <PersonRolesTab :person="person" />
        </TabPanel>
        <TabPanel>
          <PersonSkillsTab :person="person" />
        </TabPanel>
        <TabPanel>
          <PersonSecurityTab :person="person" />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>
```

### Enhanced PeopleService Methods
```typescript
// src/services/usePeopleService.ts
export const usePeopleService = () => {
  const peopleStore = usePeopleStore();
  
  return {
    // ... existing methods
    
    // Get person with full details for profile view
    getPersonWithDetails: async (personId: string): Promise<PersonWithDetails | null> => {
      try {
        const person = await peopleStore.getPersonById(personId);
        if (!person) return null;
        
        // Load related data
        const team = teamsStore.getTeamById(person.teamId);
        const roles = person.roleIds.map(id => rolesStore.getRoleById(id)).filter(Boolean);
        const user = person.userId ? userStore.getUserById(person.userId) : null;
        
        return {
          ...person,
          team,
          roles,
          user
        };
      } catch (error) {
        console.error('Error loading person details:', error);
        throw error;
      }
    },
    
    // Get person's skill assessments
    getPersonSkillAssessments: async (personId: string): Promise<SkillAssessment[]> => {
      try {
        const person = await peopleStore.getPersonById(personId);
        if (!person?.skillAssessments) return [];
        
        return Object.values(person.skillAssessments);
      } catch (error) {
        console.error('Error loading skill assessments:', error);
        throw error;
      }
    }
  };
};
```

### Router Configuration
```typescript
// src/router/index.ts - Add Person Profile route
const routes: RouteRecordRaw[] = [
  // ... existing routes
  {
    path: '/people/:personId',
    name: 'PersonProfile',
    component: () => import('@/views/people/PersonProfileView.vue'),
    meta: {
      requiresAuth: true,
      title: 'Person Profile'
    }
  }
];
```

### Enhanced PeopleDirectory Navigation
```typescript
// src/components/people/PeopleDirectory.vue
<template>
  <div class="people-directory">
    <!-- Existing directory content with enhanced navigation -->
    <DataTable 
      :value="filteredPeople" 
      :loading="loading"
      @row-click="onPersonClick"
      class="people-table"
    >
      <Column field="name" header="Name" sortable>
        <template #body="{ data }">
          <!-- Clickable Person Name for Profile Navigation -->
          <a 
            href="#" 
            @click.prevent="navigateToProfile(data.id)"
            class="person-name-link"
            :aria-label="`View profile for ${data.firstName} ${data.lastName}`"
          >
            {{ data.firstName }} {{ data.lastName }}
          </a>
        </template>
      </Column>
      <!-- Other columns remain the same -->
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();

// Navigation Handler for Profile View
const navigateToProfile = (personId: string) => {
  router.push(`/people/${personId}`);
};
</script>
```

## Success Criteria
- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.
- Person Profile displays comprehensive information through tabbed interface
- Navigation from People Directory to Person Profile works seamlessly
- All tab components display appropriate information with proper styling
- Service integration provides complete person data with related information
- E2E tests validate all profile functionality and navigation
- Proper accessibility attributes are included for all interactive elements
- Type safety is maintained throughout the implementation

## Technical Requirements

### Component Architecture Requirements
- Use PrimeVue v4 Tabs components (Tabs, TabList, Tab, TabPanels, TabPanel)
- Follow container/presenter pattern for main profile component
- Implement proper separation of concerns for each tab component
- Use reactive state management with Vue 3 Composition API
- Include proper loading and error states

### Service Integration Requirements
- Enhance usePeopleService with profile-specific methods
- Integrate with TeamsService, RolesService, SkillsService, UserService, and SecurityGroupsService
- Implement proper error handling and type safety
- Use facade pattern for complex service interactions

### Navigation Requirements
- Add route for `/people/:personId` path
- Implement drill-down navigation from People Directory
- Include proper route guards and meta information
- Ensure seamless navigation experience

### Accessibility Requirements
- Use semantic HTML elements throughout
- Include proper ARIA labels and roles
- Ensure keyboard navigation support
- Add focus management for tab navigation
- Include screen reader support for all interactive elements

### Testing Requirements
- Add comprehensive E2E tests for profile functionality
- Test navigation from directory to profile
- Validate all tab content and interactions
- Include proper test data setup and cleanup
- Ensure tests use accessibility-first selectors

### Type Safety Requirements
- Create PersonWithDetails interface extending Person
- Add proper TypeScript interfaces for all profile data
- Ensure type safety for service method returns
- Include proper type guards for optional data

### Security Requirements
- Implement role-based access control for profile features
- Add permission checking for edit functionality
- Ensure sensitive information is properly protected
- Include proper authorization for security tab access 