<!-- Source: /docs/user_manual/generating_waypoint_mappings.md -->
# Feature: Generating Waypoint Mappings
This feature defines how AIS CSV fields map to WayPoint attributes in the generated XML and how missing values are handled.

## Positive Scenarios
### Scenario: Map AIS fields to WayPoint attributes
  Given an AIS record with MMSI "555444333" and values for Latitude, Longitude, SOG, EtaSecondsUntil, and Heading
  When the system generates a WayPoint
  Then the WayPoint should have Name set to "555444333"
  And Lat and Lon set to the provided coordinates
  And Speed set to the SOG value
  And ETA set to EtaSecondsUntil or 0 if missing
  And Heading set to the provided Heading or 0 if missing

### Scenario: Compute MaxSpeed from observed SOG
  Given multiple AIS records with SOG values [2.5, 5.0, 3.0]
  When GetMaxShipSpeed is executed for the selected range
  Then MaxSpeed should be 5.0

## Negative Scenarios
### Scenario: Missing SOG results in Speed=0
  Given an AIS record has an empty SOG field
  When the WayPoint is generated
  Then Speed should be set to 0

### Scenario: Invalid numeric formats are skipped
  Given an AIS record has a non-numeric latitude
  When parsing the CSV line
  Then the line should be skipped and a parse error logged

## Edge & Permission Scenarios
### Scenario: Default XTE and MinSpeed values
  Given an AIS record is valid
  When generating WayPoint
  Then PortXTE and StbdXTE should be set to 20
  And MinSpeed should be set to 0

### Cross-Feature Reference
  (Saving generated WayPoints as XML depends on Feature: Creating Routes and Output XML)
