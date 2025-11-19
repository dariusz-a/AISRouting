# Manage Roles

How to use Roles in KnowCount to represent job titles, assign standardized skill sets, and support people and project planning.

---

## 1. What Are Roles?

**Roles** represent position titles in your organization (e.g., "Project Manager," "Accountant").  
They can be used in two ways:

- As **labels for organizational reporting**
- To **define expected skill sets** for anyone assigned to that role

### Key Principles

1. **Skills are Universal & Unique**: Skills are identified by name and there's only one instance of each skill globally. This is elegant - no duplicate "JavaScript" skills floating around.

2. **Roles are Independent Containers**: HR roles and project roles are completely separate entities that just happen to use the same skill references. No inheritance, no linking.

3. **Role Names Must Be Unique**: This is crucial for avoiding conflicts. Use a prefixing strategy ("HR Senior Java Developer" vs "X Senior Java Developer") as a good pattern.

### Implications for Usage

This means:
- When creating a project role "based on" an HR role, you're really just **copying** the skill assignments, not creating any ongoing relationship
- The flow should be: Pick an existing role → Copy its skills → Customize as needed → Save as new independent role
- New skills added during role creation go into the global skill catalog immediately
- Role uniqueness validation happens at save time

---

## 2. Creating Roles

You can add Roles in two ways:

### Option 1: Add a Role Manually

1. From the main sidebar, expand **Directories** and click **Roles**.
2. Click **Add new role**  
3. Enter a **Role name**  
4. Click **Save**

---

### Option 2: Import Roles *(Licensed Feature)*

Use the **Data Importer** tool if your KnowCount license includes bulk import features.  
This is helpful for setting up many roles at once.

---

## 3. Assigning Skills to a Role

You can assign skills to a Role either on a per-skill basis or by selecting entire Skill Categories. This allows you to tailor each Role's skill requirements precisely.

### Assign Skills Using the Tree View

The assignment interface uses a tree view similar to the Skills Directory:

![Skills Directory Tree View](../user_manual/images/skills_tree_view.png)

#### Assigning Skills by Category

1. Go to the **Roles** table and select the Role you want to edit.
2. Click the **Edit Role** button. The Role modal window will open.
3. Click **Assign Skills** for the selected Role.
4. In the tree view, locate the Skill Categories you wish to assign.
5. Select the checkbox next to a category to assign all skills within that category to the Role.
6. Click **Save** to apply the changes.

#### Assigning Individual Skills

1. In the same tree view, expand a Skill Category to see its skills.
2. Select the checkbox next to individual skills you want to assign to the Role.
3. You can mix category and individual skill selection as needed.
4. Click **Save** to confirm your assignments.

> **Note:**
> - Assigning a category will automatically include all current and future skills in that category for the Role.
> - Assigning individual skills allows for more granular control.
> - The tree view for assignment matches the layout and icons of the Skills Directory for consistency.

---

## 3a. Editing an Existing Role: Viewing Current Assignments

When you edit an existing role, the Assign Skills tree view will automatically show all skill categories and individual skills that are already assigned to the role as pre-selected (checked). This allows you to immediately see which skills and categories are currently assigned before making any changes.

**How to Edit a Role and View Current Assignments:**

1. Go to the **Roles** table and select the Role you want to edit.
2. Click the **Edit Role** button. The Role modal window will open.
3. Click **Define skill categories?** for the selected Role.
4. In the tree view, all categories and skills already assigned to the role will be checked.
5. You can add or remove skill categories or individual skills as needed.
6. Click **Save** to apply your changes.

> **Tip:** This feature helps you avoid accidentally removing existing assignments and makes it easy to review a role's current skill requirements at a glance.

---

## 4. Viewing Role Dashboard

Each role has its own **dashboard** that shows:

- **Skill distribution** across everyone assigned to that role  
- **Skills from all other roles** assigned to each person (if applicable)

> 🛈 *If a person has both "Multi Role 1" and "Multi Role 2," the dashboard for either role will display the **combined skill view**.*