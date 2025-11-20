```gherkin
# BDD scenarios for Feature 2.1: JSON Parser

Feature: JSON Parser
  As a developer
  I want the JSON Parser to robustly parse `<MMSI>.json` ship static files
  So that the application can display ship metadata and include it in exports

  Scenario: Parse valid ship static JSON file
    Given a file `205196000.json` containing valid ship static JSON
    When the parser reads the file
    Then it returns a `ShipStaticData` object with `mmsi` equal to "205196000"
      And `sourcePath` is set to the file's absolute path

  Scenario: Handle missing optional fields in JSON
    Given a file `205196000.json` missing optional fields like `callsign`
    When the parser reads the file
    Then it returns a `ShipStaticData` object
      And optional fields are `null` or absent

  Scenario: Return null for malformed JSON file
    Given a file `205196000.json` containing malformed JSON
    When the parser reads the file
    Then it returns `null`
      And a log entry is made indicating malformed JSON

  Scenario: Reject non-object JSON root
    Given a file `205196000.json` whose top-level JSON is an array
    When the parser reads the file
    Then it returns `null`

```
