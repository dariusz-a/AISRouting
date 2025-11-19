### Scenario: Assign skill categories to a role
  Given an administrator is editing the role "Data Analyst"
  When they click the Edit Role button
  And the Role modal window opens
  And they click "Assign Skills"
  And they select the category "Programming Languages"
  And they click "Save"
  Then the role "Data Analyst" should have all skills from the "Programming Languages" category assigned