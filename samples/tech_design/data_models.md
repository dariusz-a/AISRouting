# Data Models and Storage Structure

## Local Storage Implementation

We will use `vuex-persist` library to handle local storage operations with TypeScript support. This provides a clean wrapper around localStorage with proper typing and CRUD operations.

## Data Models

### Authorization System
```typescript
/**
 * User model for authentication and authorization
 * Represents an account that can log into the system
 * Potentially managed by external identity provider (e.g., Keycloak)
 */
interface User {
  id: string;
  email: string;
  passwordHash?: string;   // Only stored for internal authentication
  externalId?: string;    // Optional reference to external identity provider
  securityGroupId: string; // Foreign key to SecurityGroup for permissions
  loginAttempts: number;
  isLocked: boolean;
  lastLoginAttempt: Date;
}

/**
 * Security group defines a set of permissions for users
 */
interface SecurityGroup {
  id: string;
  name: string;
  description: string;
  isBuiltIn: boolean; // True for system-defined groups like "Administrator"
  permissions: Permission[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Permission defines what actions can be performed on a resource
 */
interface Permission {
  resource: string; // e.g., 'people', 'teams', 'projects'
  action: 'create' | 'read' | 'update' | 'delete' | 'assess' | 'admin';
}

/**
 * Pre-defined security groups for the system
 */
const SECURITY_GROUPS = {
  ADMINISTRATOR: {
    id: 'admin',
    name: 'Administrator',
    description: 'Full system access',
    isBuiltIn: true,
    permissions: [
      { resource: '*', action: 'admin' } // Wildcard for all permissions
    ]
  },
  STANDARD_USER: {
    id: 'user',
    name: 'Standard User',
    description: 'Regular user access',
    isBuiltIn: true,
    permissions: [
      { resource: 'people', action: 'read' },
      { resource: 'skills', action: 'read' },
      // Other standard permissions
    ]
  },
  HR_MANAGER: {
    id: 'hr_manager',
    name: 'HR Manager',
    description: 'People management access',
    isBuiltIn: true,
    permissions: [
      { resource: 'people', action: 'create' },
      { resource: 'people', action: 'read' },
      { resource: 'people', action: 'update' },
      { resource: 'people', action: 'delete' },
      // Other HR permissions
    ]
  }
};
```

### Person
```typescript
/**
 * Person data model that represents an individual in the organization
 * Person records can exist with or without associated User accounts
 * Enhanced with comprehensive role and skill management capabilities
 */
interface Person {
  id: string;
  firstName: string;
  lastName: string;
  email: string;          // Business email address
  teamId: string;         // Foreign key to Team
  userId?: string;        // Optional reference to User email (if they have login access)
  roleIds: string[];      // Array of role IDs assigned to the person
  skills?: string[];      // Individual skill IDs assigned directly
  skillCategories?: string[]; // Category IDs assigned (explicit category tracking)
  externalId?: string;    // Optional reference to external HR system (e.g., Workday ID)
  skillAssessments?: { [skillId: string]: SkillAssessment }; // Skill assessments
  interestAreas?: { [skillId: string]: number }; // Interest levels in skills
  createdAt?: string;     // Creation timestamp
  updatedAt?: string;     // Last update timestamp
}

/**
 * Enhanced Person with full details for profile view
 */
interface PersonWithDetails extends Person {
  team?: Team;
  roles?: Role[];
  user?: User;
  skillAssessments?: SkillAssessment[];
}

/**
 * Data transfer object for creating a new person
 * Contains only the fields needed for initial creation
 */
interface PersonCreationDTO {
  firstName: string;
  lastName: string;
  email: string;
  teamId: string;         // Required - validates against Team existence
  createUserAccount: boolean; // Whether to create a User account for this Person
  externalId?: string;     // Optional reference to external HR system
  roleIds: string[];      // Optional on creation, can be empty array
  additionalSkills?: string[]; // Optional for manually adding skills
  additionalCategories?: string[]; // Optional for manually adding skill categories
}

/**
 * Data transfer object for creating a new user account
 * Used when adding a user or linking a user to an existing person
 */
interface UserCreationDTO {
  personId: string;       // Reference to the Person this User belongs to
  username: string;       // Usually the email address
  initialPassword?: string; // Required for internal authentication
  securityGroupId: string;  // Permission group assignment
  externalId?: string;      // Optional reference to external identity provider
}

/**
 * Enhanced data transfer object for person creation with user account
 */
interface PersonWithUserDTO {
  firstName: string;
  lastName: string;
  email: string;
  teamId: string;
  roles: string[];        // Role IDs to assign
  skills?: string[];      // Additional skills beyond role-based skills
  hasLoginAccess: boolean; // Whether to create user account
  password?: string;      // Initial password if login access enabled
  securityGroupId?: string; // Security group if login access enabled
}

/**
 * Skill assessment data for a person
 */
interface SkillAssessment {
  level: number; // 1-5 skill level
  assessorId: string; // Who assessed this skill
  assessmentType: 'self' | 'supervisor' | 'peer';
  lastUpdated: string;
}

/**
 * Validation result for person operations
 */
interface ValidationResult {
  isValid: boolean;
  message?: string;
  errors?: string[];
}
```

### Person-User Relationship

The system maintains a clear relationship between Person and User entities using a **composition pattern**:

#### Connection Model
- **Person**: Represents an individual in the organization (can exist without login access)
- **User**: Represents an account that can log into the system
- **Connection**: `Person.userId` field contains the email address of the associated User account

#### Examples
```typescript
// Person with User account (can log in)
{
  id: "person-alice",
  firstName: "Alice",
  lastName: "Smith", 
  email: "alice.smith@company.com",
  teamId: "team-1",
  roleIds: ["manager", "admin"],
  skills: ["lib8", "lib9"],
  skillCategories: ["cat-management"],
  userId: "alice.smith@company.com" // References user account
}

// Person without User account (cannot log in)
{
  id: "person-1",
  firstName: "John",
  lastName: "Doe",
  email: "dev1@test.com",
  teamId: "team-1", 
  roleIds: ["role-1"],
  skills: ["javascript", "vue"],
  skillCategories: ["cat-web-development"]
  // No userId - this person doesn't have a user account
}
```

#### Benefits of Person-User Composition Approach

This design approach offers several key advantages:

1. **Clear Separation of Concerns**:
   - Person records focus on organizational identity and skills
   - User records focus on authentication and authorization
   - Each entity can evolve independently

2. **Flexible Authentication Options**:
   - Supports both internal authentication and external identity providers
   - Enables gradual migration to external providers like Keycloak
   - Maintains compatibility with existing authentication system

3. **Improved HR Integration**:
   - Person records can be synchronized with HR systems like Workday
   - External IDs allow for mapping between systems
   - HR data changes don't affect authentication

4. **Enhanced Security**:
   - Authentication-sensitive data is isolated from general person information
   - Authentication can be delegated to specialized security systems
   - Password resets don't require modifying core person data

5. **Better User Experience**:
   - Clear distinction between people who can and cannot log in
   - Simple process for granting login access to existing people
   - Consistent user interface for managing both aspects

6. **Maintainability**:
   - Services focus on specific domain concerns
   - Changes to authentication don't affect people management features
   - Easier to test each system in isolation

7. **Future-Proofing**:
   - Simplifies adoption of Single Sign-On (SSO) in the future
   - Makes it easier to implement MFA and other security features
   - Supports potential migration to different identity management solutions

### Enhanced Role Management

```typescript
/**
 * Enhanced Role model with comprehensive skill management
 */
interface Role {
  id: string;
  name: string;
  description: string;
  skillIds?: string[];        // Individual skill IDs associated with role
  skillCategoryIds?: string[]; // Category IDs associated with role
  skillCategories: {
    categoryId: string;
    targetLevel: number;
  }[];
  createdAt: string;
  updatedAt: string;
}
```

### Skill
```typescript
interface Skill {
  id: string;
  name: string;
  description: string;
  categoryId: string;
  isFromLibrary: boolean;
  createdAt: string;
  updatedAt: string;
}

interface SkillCategory {
  id: string;
  name: string;
  parentCategoryId: string | null;
  createdAt: string;
  updatedAt: string;
}
```

### Team
```typescript
interface Team {
  id: string;
  name: string;
  parentTeamId: string | null;
  supervisorId: string;
  supervisionScope: 'everyone' | 'team_members';
  skillCategories: string[];
  createdAt: string;
  updatedAt: string;
}
```

### Project
```typescript
interface Project {
  id: string;
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  projectManagerId: string;
  roles: ProjectRole[];
  createdAt: string;
  updatedAt: string;
}

interface ProjectRole {
  id: string;
  projectId: string;
  roleId: string;
  skillRequirements: {
    skillId: string;
    targetLevel: number;
  }[];
}

interface ProjectAssignment {
  id: string;
  projectId: string;
  personId: string;
  projectRoleId: string;
  allocationPercentage: number;
  startDate: string;
  endDate: string | null;
}
```

### Assessment
```typescript
interface SkillAssessment {
  id: string;
  personId: string;
  skillId: string;
  assessorId: string;
  level: number;
  assessmentType: 'self' | 'supervisor';
  assessmentDate: string;
}
```
## Data Relationships

1. **Person - User**
   - One-to-one (optional): A Person may have an associated User account (for login access), referenced by `userId` in the Person model
   - Not all Persons require a User account (e.g., external or inactive staff)
   - User accounts reference permissions and security groups, while Person records store organizational and skill data

2. **Person - Team**
   - One-to-many: Each person belongs to one team
   - Team can have multiple persons
   - Historical team membership tracked separately

3. **Team - Team**
   - Self-referential: Teams can have parent teams
   - Teams inherit skill categories from parent
   - No circular relationships allowed

4. **Person - Role**
   - Many-to-many: Persons can have multiple roles
   - Roles can be assigned to multiple persons
   - Role assignments tracked with timestamps
   - **Enhanced**: Support for multiple role assignment with automatic skill inheritance

5. **Person - Skill**
   - Many-to-many through skill assignments
   - Skills can be assigned individually or through roles/categories
   - **Enhanced**: Skill independence rule - skills don't change when roles change
   - **Enhanced**: Support for both individual skills and category-based assignments

6. **Role - Skill**
   - Many-to-many: Roles can have multiple skills
   - Skills can be associated with multiple roles
   - **Enhanced**: Support for both individual skills and skill categories

7. **Project - Person**
   - Many-to-many through ProjectAssignment
   - Persons can be assigned to multiple projects
   - Projects can have multiple persons
   - Assignments include role and allocation percentage

8. **Skill - SkillCategory**
   - Many-to-one: Skills belong to one category
   - Categories can have multiple skills
   - Categories can have parent categories

   

## Data Validation Rules

1. **User**
   - Email must be valid format
   - Password must meet security requirements
   - Must be assigned to a team
   - Cannot be assigned conflicting roles

2. **Person**
   - Email must be unique across all persons
   - Team must exist before assignment
   - Role IDs must reference valid roles
   - Skill IDs must reference valid skills
   - **Enhanced**: Support for multiple role validation
   - **Enhanced**: Duplicate role prevention
   - **Enhanced**: Role-based skill assignment validation

3. **Team**
   - Name must be unique
   - Supervisor must have assess permission
   - Cannot create circular hierarchies
   - Must have at least one skill category

3. **Project**
   - End date must be after start date
   - Must have a project manager
   - Person allocations cannot exceed 100%
   - Role skill requirements must be 1-3

5. **Skills**
   - Name must be unique within category
   - Must belong to a category
   - Cannot be deleted if in use
   - Description max 1000 chars

6. **Role Assignment**
   - **Enhanced**: Cannot assign duplicate roles to same person
   - **Enhanced**: Role must exist before assignment
   - **Enhanced**: Skill inheritance must be valid
   - **Enhanced**: Category assignments must be valid

## Local Storage Structure

```typescript
interface LocalStorageSchema {
  users: Record<string, User>;
  persons: Record<String, Person>;
  teams: Record<string, Team>;
  skills: Record<string, Skill>;
  skillCategories: Record<string, SkillCategory>;
  roles: Record<string, Role>;
  projects: Record<string, Project>;
  projectRoles: Record<string, ProjectRole>;
  projectAssignments: Record<string, ProjectAssignment>;
  skillAssessments: Record<string, SkillAssessment>;
  securityGroups: Record<string, SecurityGroup>;
  roleAssignments: Record<string, RoleAssignment>; // Enhanced: Track role assignments
  skillAssignments: Record<string, SkillAssignment>; // Enhanced: Track skill assignments
}
```

## Enhanced Storage Implementation


### Role-Based Skill Assignment

The system implements automatic skill assignment based on roles, as specified in the BDD scenarios. Skills are stored in the `Person.skills?: string[]` array, which contains skill IDs assigned to the person.

#### Automatic Skill Assignment
When a person is assigned a role, the system automatically assigns all skills and skill categories associated with that role to the person's skill set.

#### Skill Independence Rule
**A person's skills are independent of their roles - they don't change when roles change.** This means:
- Skills are only automatically assigned during initial person creation
- When roles are updated later, existing skills remain unchanged
- Manual skill assignments persist regardless of role changes
- Role changes do not trigger automatic skill modifications

#### Skill Storage Mechanism
- **Storage Location**: Skills and categories are stored in separate arrays:
  - `Person.skills?: string[]` - Individual skill IDs assigned directly
  - `Person.skillCategories?: string[]` - Category IDs assigned (explicit category tracking)
- **Content**: 
  - Skills array contains skill IDs (strings) representing individual skills assigned to the person
  - Categories array contains category IDs (strings) representing skill categories assigned to the person
- **Assignment Types**: 
  - Role-based skills: Automatically added when roles are assigned
  - Individual skills: Manually selected skills beyond role-based skills
  - Category assignments: Entire skill categories assigned (includes all current and future skills in category)
- **Deduplication**: The system prevents duplicate skill IDs and category IDs in their respective arrays
- **Persistence**: Skills and categories are stored in local storage via the people store
- **Independence**: Skills and categories remain unchanged when roles are modified
- **Category Benefits**: 
  - Automatic inclusion of future skills added to assigned categories
  - Easier category-level operations (add/remove entire categories)
  - Clear audit trail of category vs. individual skill assignments

#### Additional Skill Assignment
Beyond role-based skills, administrators can manually assign additional skills using the skill tree view:

1. **Role-Based Skills**: Automatically assigned during initial person creation
2. **Additional Skills**: Manually selected from the skill tree view using the "Add/Remove" checkbox
3. **Combined Skill Set**: Person's total skills include both role-based and additional skills

### Multiple Role Assignment Support

The enhanced system supports assigning multiple roles to a single person, with automatic inheritance of all skills associated with those roles.

#### Multiple Role Data Structure
```typescript
// Enhanced Person model with multiple role support
interface Person {
  // ... existing fields ...
  roleIds: string[];      // Array of role IDs - supports multiple roles
  roleDisplay?: string;    // Display string for UI (e.g., "2 roles", "Software Architect")
}

// Enhanced role assignment tracking
interface RoleAssignment {
  personId: string;
  roleId: string;
  assignedDate: string;
  assignedBy: string;
  skillInheritance: {
    skillId: string;
    inheritedFrom: string; // Role ID that provided this skill
  }[];
}
```

### Composition Pattern for Person and User Models

This design uses a composition pattern where:

- **Person** represents an individual in your organization (from Workday or another HR source).
- **User** represents authentication information (from Keycloak or your internal auth system).
- A **Person** can have an optional reference to a **User** via a `userId` field.

This implementation creates a clean composition pattern where:

- The **Person** model represents an individual in your organization.
- The **User** model contains authentication and authorization data.
- A **Person** can optionally reference a **User** through the `userId` field.
- A **Person** without a **User** cannot log in.

The composition approach provides the ideal balance of separation and integration, while maintaining compatibility with both the existing authentication system and future integration possibilities with external systems like Keycloak and Workday.

### Helper Functions and Type Guards

```typescript
/**
 * Type guard to check if a Person can log in (has associated User account)
 */
function canLogIn(person: Person): boolean {
  return !!person.userId;
}

/**
 * Helper function to get the User associated with a Person
 */
async function getUserForPerson(person: Person): Promise<User | null> {
  if (!person.userId) {
    return null;
  }
  
  // Fetch from user service
  const userService = useUserService();
  return await userService.getUser(person.userId);
}

/**
 * Link a new or existing User to a Person
 */
async function linkUserToPerson(personId: string, userCreationDTO: UserCreationDTO): Promise<{person: Person, user: User} | null> {
  const peopleService = usePeopleService();
  const userService = useUserService();
  
  try {
    // Get the person
    const person = await peopleService.getPerson(personId);
    if (!person) {
      throw new Error('Person not found');
    }
    
    // Check if person already has a user
    if (person.userId) {
      throw new Error('Person already has an associated user account');
    }
    
    // Create or get the user
    const user = await userService.createUser(userCreationDTO);
    if (!user) {
      throw new Error('Failed to create user account');
    }
    
    // Update the person with user reference
    const updatedPerson = await peopleService.updatePerson(personId, {
      userId: user.id,
      updatedAt: new Date().toISOString()
    });
    
    return {
      person: updatedPerson!,
      user
    };
  } catch (error) {
    console.error('Failed to link user to person:', error);
    return null;
  }
}
```

## Data Migration Strategy

1. **Version Control**
   - Schema version tracking
   - Migration scripts for schema updates
   - Data validation during migration

2. **Backend Preparation**
   - Consistent data structures
   - Relationship integrity
   - Clean separation of concerns