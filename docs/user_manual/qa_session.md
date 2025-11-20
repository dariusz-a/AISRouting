# QA Session

## Q1: What is the primary purpose of the AISRouting application and who are its main target users (roles and their technical proficiency)?

**A1:** It creates a route which is optimized and filtered and exports a file in xml format

## Q2: Which user roles will use AISRouting (e.g., Analyst, Operator, Administrator) and what actions should each role be able to perform?

**A2:** simulator users, maritime data analysts, ship operators, researchers

## Q3: For each role you listed (simulator users, maritime data analysts, ship operators, researchers), please specify typical tasks they need in AISRouting and their expected technical proficiency (novice, intermediate, expert).

**A3:** Novice

## Q4: List the specific tasks each role needs in AISRouting. Provide one role per answer. Which role should I start with?

**A4:** Simulator user

## Q5: For the Simulator user role (novice), list the specific tasks they need in AISRouting.

**A5:** select input folder, choose ship, set time interval, create track, export route

## Q6: What should happen when the simulator user selects the input folder? Specify accepted folder structure, default path behavior, validation rules, and any error messages or permissions the UI must show.

**A6:** User must see a Combo box with the names of subfolders. No validation rules and Error messages in case the root folder is empty and no permissions needed.

## Q7: Describe the expected behavior and UI elements for the ship selection combo box (what ship names show, how to display ship static data, how min/max dates appear, and how start/stop time pickers should behave).

**A7:** Ship names should be obtained from Static file or if name is missing from static file, use folder name. Static data - It should be displayed in the large TextBox widget, Min/Max dates - Please use date picker widget

## Q8: For the date picker and time selection, should the UI allow selecting exact seconds within the date range (seconds resolution), or only dates and minutes? Also specify any default start/stop values when a ship is selected.

**A8:** Use Seconds resolution...StartValue: First fileName in the vessel folder,   StopValue: Last filename + 24 hrs

## Q9: What default optimization parameters and thresholds should the "create track" process use (e.g., minimum heading change in degrees, minimum distance between points in meters, SOG change threshold, ROT threshold, time gap handling)? Please provide numeric defaults if possible.

**A9:** heading change degrees - 0.2 degree, min distance meters - 5 meters, SOG - 0.2 knots , ROT - 0.2 deg/s, max allowed time gap - not needed

## Q10: What filename pattern and XML metadata should exported route files use (e.g., filename format, RouteTemplate Name, color attributes, required WayPoint attributes, any additional metadata)?

**A10:** filename pattern - MMSINumber-StartDate-EndDate.xml and metadata from route_waypoint_template.xml

## Q11: Should the exported XML include only generated WayPoint elements inside a single RouteTemplate, or support multiple RouteTemplate entries per file? Also specify whether RouteTemplate Name should use MMSI or a user-supplied name.

**A11:** Support the Single Route template & Route template name can be MMSI number of the vessel

## Q12: Which WayPoint attributes should be included and how should they be mapped from AIS records?

**A12:** Apply the following mapping for each AIS record from ShipDataOut included in the selected time range:

- Name: set to MMSI (string)
- Lat: use CSV Latitude value
- Lon: use CSV Longitude value
- Alt: set to 0 (no mapping from AIS)
- Speed: use SOG from CSV (no unit conversion in this release)
- ETA: use EtaSecondsUntil from CSV if provided; otherwise set to 0
- Delay: set to 0
- Mode: computed via SetWaypointMode (defined later)
- TrackMode: set to "Track"
- Heading: use Heading from CSV or 0 if missing
- PortXTE: set to 20
- StbdXTE: set to 20
- MinSpeed: set to 0
- MaxSpeed: computed by GetMaxShipSpeed (maximum SOG observed in selected range)

## Q13: Define the rules for SetWaypointMode and GetMaxShipSpeed. For SetWaypointMode, specify how to derive Mode from AIS fields (e.g., NavigationalStatusIndex, SOG thresholds, ROT) and a short list of possible Mode values. For GetMaxShipSpeed, specify whether to ignore zero SOG records and how to handle missing SOG values.

**A13:** This will be explained later

## Q14: When exporting the XML file, if a file with the same name already exists in the output folder, should AISRouting overwrite it, append a numeric suffix, or prompt the user to choose?

**A14:** Prompt the user to choose

## Q15: Do you want an option to preview the generated route (display waypoints list and basic stats) before exporting the XML? (yes/no)

**A15:** No

## Q16: Should AISRouting remember the last used input and output folders between sessions? (yes/no)

**A16:** No

## Q17: What should happen when the user selects the output folder? Specify default path behavior, whether the application should create the folder if it doesn't exist, any validation rules or error messages, and whether the output folder path should be displayed in the UI.

**A17:** If the selected output path does not exist, create it. For a given path, use that path to store the generated results. Validate that the path is writable; if creation or write fails show an error message. Display the selected output folder path in the UI.
