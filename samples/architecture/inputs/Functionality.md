# Feature: Roles Management

## Positive Scenarios

### Scenario: Create new role manually
  Given an administrator is in the Roles Directory
  When they click "Add new role"
  And they enter "Senior DevOps Engineer" as the Role name
  And they select "Define skill categories? → Yes"
  And they assign skill categories:
    | Category           |
    | Cloud Architecture |
    | CI/CD             |
    | Infrastructure    |
    | Containerization  |
  And they click "Save"
  Then the role "Senior DevOps Engineer" should appear in the Roles Directory
  And it should have the assigned skill categories

### Scenario: Assign skill categories to a role
  Given an administrator is editing the role "Data Analyst"
  When they click the Edit Role button
  And the Role modal window opens
  And they click "Assign Skills"
  And they select the category "Programming Languages"
  And they click "Save"
  Then the role "Data Analyst" should have all skills from the "Programming Languages" category assigned

### Scenario: Assign individual skills to a role
  Given an administrator is editing the role "Marketing Specialist"
  When they click the Edit Role button
  And the Role modal window opens
  And they click "Assign Skills"
  And they expand the category "Digital Marketing"
  And they select the skills:
    | Skill           |
    | SEO             |
    | Google Analytics|
  And they click "Save"
  Then the role "Marketing Specialist" should have the selected skills assigned

### Scenario: Assign both categories and individual skills to a role
  Given an administrator is editing the role "Backend Developer"
  When they click the Edit Role button
  And the Role modal window opens
  And they click "Assign Skills"
  And they select the category "Databases"
  And they expand the category "Programming Languages"
  And they select the skill "Python"
  And they click "Save"
  Then the role "Backend Developer" should have all skills from "Databases" and the skill "Python" assigned

### Scenario: Edit role shows current skills and categories as selected
  Given an administrator is editing the role "Frontend Developer" with the following assigned:
    | Category         | Skills           |
    | UI Frameworks    | React, Vue       |
    | Programming     | JavaScript       |
  When they click the Edit Role button
  And the Role modal window opens
  Then the categories and skills already assigned to the role should be visible and pre-selected in the Assign Skills section
  And the administrator should be able to see which skills and categories are currently assigned before making changes

## Negative Scenarios

### Scenario: Create role without name
  Given an administrator is adding a new role
  When they leave the Role name empty
  And click "Save"
  Then they should see an error message "Role name is required"
  And the role should not be created

### Scenario: Import invalid role data
  Given an administrator is using the Data Importer
  When they upload a CSV file with invalid data:
    | Role Name | Skill Categories |
    |          | Python, Java     |
  Then they should see an error message about missing required fields
  And no roles should be imported

### Scenario: Remove role with active assignments
  Given a role "Backend Developer" is assigned to multiple people
  When an administrator attempts to delete the role
  Then they should see an error message "Cannot delete role with active assignments"
  And they should be prompted to reassign or remove the role from all users first

### Scenario: Create role with duplicate name
  Given the role "Solution Architect" already exists
  When an administrator attempts to create the same role name
  Then they should see a warning about duplicate role names
  And they should be prompted to skip or update the existing role

### Scenario: Attempt to assign skills without selecting any
  Given an administrator is editing the role "QA Engineer"
  When they click the Edit Role button
  And the Role modal window opens
  And they click "Assign Skills"
  And they do not select any categories or skills
  And they click "Save"
  Then they should see an error message "Please select at least one skill or category to assign"
  And no changes should be made to the role

### Scenario: Assign a category already fully assigned
  Given the role "Project Manager" already has all skills from the category "Project Management" assigned
  When an administrator attempts to assign the same category again
  Then they should see a warning "All skills from this category are already assigned to this role"
  And no duplicate assignments should occur

### Scenario: Assign a skill already assigned individually and via category
  Given the role "DevOps Engineer" has the skill "AWS" assigned individually and via the category "Cloud Computing"
  When an administrator attempts to assign "AWS" again
  Then they should see a message "Skill already assigned via category"
  And the assignment should not be duplicated

