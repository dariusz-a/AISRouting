# QA Session for User Manual Generation

## Q1: What is the primary purpose of the application?

**Answer:**

The primary purpose of the application is to create route for selected a ship from AIS data.

---

## Q2: Who are the primary users of the application and what user roles & permissions exist?

**Answer:**

Users of the application will be instructor of the K-Sim application. No extra permission needed.

---

## Q3: How do users access and authenticate to the application?

**Answer:**

The application is installed locally on the instructor's PC. No online authentication is required; users launch the application from the desktop/start menu. Local OS user account provides access control.

---

## Q4: What input data formats does the application accept and how should files be prepared?

**Answer:**

The application accepts AIS CSV files with the following characteristics:

- Location & naming:
  - Files are placed under: outputFolder/<MMSI>/<MMSI>-yyyy-MM-dd.csv
  - No header row (headerless)
  - Encoding: UTF-8
  - Line endings: CRLF on Windows

- Columns (order, headerless). Null/empty cells map to nullable types:
  1. Time: seconds (signed 64-bit) from T0
  2. Latitude: decimal (use invariant '.' as decimal separator)
  3. Longitude: decimal (use invariant '.')
  4. NavigationalStatusIndex: int (dictionary index)
  5. ROT: decimal or empty
  6. SOG: decimal or empty
  7. COG: decimal or empty
  8. Heading: int or empty
  9. Draught: decimal or empty
  10. DestinationIndex: int (dictionary index) or empty
  11. EtaSecondsUntil: signed 64-bit seconds or empty

- Notes:
  - Example single line (11 fields, headerless):
    0,55.884242,12.759092,0,,,,,,,
  - Use the invariant dot (.) for decimals. Empty fields (e.g., ,,) should be treated as null values.
  - Ensure MMSI directory and file naming match the specified pattern so the application locates files correctly.
  - Keep file sizes within expected processing limits on the instructor's PC (recommendations/limits can be documented later if needed).

---

## Q5: How does a user select a ship from the AIS data and initiate route creation?

**Answer:**

<!-- Please describe the UI steps or menu actions: how the user picks a vessel (by MMSI, name, or map click), what information is shown before routing, and how to start route creation (buttons, dialogs, parameters). -->

---

*Next step*: After you reply with the answer above, I will append your answer here and proceed with the next question (one question at a time) as specified in the workflow.
