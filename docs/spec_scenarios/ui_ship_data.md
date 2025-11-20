<!-- Source: /docs/user_manual/ui_ship_data.md -->
# Feature: ShipStaticData Panel

Short description: Read-only panel showing full static data for the selected ship with copy/export options.

## Positive Scenarios
### Scenario: Show full static data for selected ship
  Given the ship "205196000" is selected
  When the ShipStaticData panel is displayed
  Then all fields from the `ShipStaticData` record should be visible with proper units

### Scenario: Export JSON of static data
  Given the ShipStaticData panel is displayed for ship "205196000"
  When the user clicks `Export JSON`
  Then a JSON file containing the ship's static record should be saved to a user-chosen location

## Negative Scenarios
### Scenario: Panel read-only and does not allow edits
  Given ShipStaticData panel is open
  When the user attempts to edit a field
  Then the field should be read-only and not editable
