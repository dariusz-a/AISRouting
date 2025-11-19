<!-- Source: /docs/user_manual/generating_waypoint_mappings.md -->
# Feature: Generating Waypoint Mappings
This feature defines how AIS CSV fields map to WayPoint attributes in the generated XML and how missing values are handled.

## Positive Scenarios
### Scenario: Map AIS CSV record to WayPoint attributes with all fields present
  Given a prepared AIS CSV file containing a single valid record with MMSI "555444333" and Latitude 58.12345 and Longitude 6.54321 and SOG 4.2 and EtaSecondsUntil 3600 and Heading 90.
  When the waypoint generation process is executed for the selected vessel and time range and the CSV line is processed by the AIS parser.
  Then a WayPoint object is produced with Name set to "555444333" and Lat set to 58.12345 and Lon set to 6.54321 and Speed set to 4.2 and ETA set to 3600 and Heading set to 90 and Alt set to 0 and Delay set to 0 and TrackMode set to "Track" and PortXTE set to 20 and StbdXTE set to 20 and MinSpeed set to 0 and MaxSpeed set to 4.2.

### Scenario: Compute MaxSpeed as maximum observed SOG in selected range
  Given a prepared AIS CSV file containing three valid records for the selected vessel with SOG values 2.5 and 5.0 and 3.0 and all other numeric fields valid.
  When GetMaxShipSpeed is executed over the selected time range as part of waypoint generation.
  Then MaxSpeed is computed as 5.0 and the generated WayPoints for the range include MaxSpeed=5.0 persisted in the generated route metadata.

## Negative Scenarios
### Scenario: Missing SOG maps Speed to 0 for that WayPoint
  Given a prepared AIS CSV file containing a valid record with MMSI "555444333" and an empty SOG field and valid Latitude and Longitude values.
  When the waypoint generation process is executed and the CSV row is mapped to a WayPoint.
  Then the produced WayPoint has Speed set to 0 and ETA set according to EtaSecondsUntil or 0 and the WayPoint is included in the output list (not skipped).

### Scenario: Invalid numeric format in Latitude causes row to be skipped and parse error recorded
  Given a prepared AIS CSV file containing a record with MMSI "555444333" and Latitude "not-a-number" and valid Longitude.
  When the CSV parser processes the file line-by-line and encounters the invalid Latitude value.
  Then the parser skips that CSV row and a parse error is recorded in the processing log with an entry describing the row number and field (TODO: confirm exact log message text), and the skipped row does not produce a WayPoint.

## Edge & Permission Scenarios
### Scenario: Default XTE and MinSpeed applied when fields are not provided
  Given a prepared AIS CSV file containing a valid record with MMSI "555444333" and missing explicit XTE and MinSpeed fields.
  When the waypoint generation process maps the record to a WayPoint.
  Then the resulting WayPoint has PortXTE set to 20 and StbdXTE set to 20 and MinSpeed set to 0 and these values are present in the generated XML output.

### Scenario: Empty CSV selection results in no WayPoints and visible user feedback
  Given the user selects an AIS CSV file that contains zero valid records for the chosen vessel and time range.
  When the waypoint generation is started for that selection.
  Then no WayPoint elements are generated and the UI shows a visible banner or modal indicating "No valid AIS records found" (TODO: confirm exact UI message text) and the generation process completes without creating output XML.

### Scenario: Unauthenticated API call for waypoint generation is rejected with 401
  Given an unauthenticated client calls the waypoint generation HTTP endpoint for a prepared CSV file.
  When the server-side generation endpoint validates the request authorization.
  Then the request is rejected with HTTP status 401 Unauthorized and no WayPoints are produced and an audit log entry is created for the unauthorized attempt.

### Cross-Feature Note
  (Saving generated WayPoints as XML depends on Feature: Creating Routes and Output XML — tests that verify persistency of saved XML should be placed in that feature's spec.)
