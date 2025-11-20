# Mock Data Catalog

Purpose: Canonical test entities for BDD scenarios. All scenario IDs MUST reference only values defined here. Do NOT invent new IDs, names, or MMSI values outside this file.

## Conventions
- IDs are lowercase, hyphen-delimited.
- Times use UTC in ISO format `YYYY-MM-DD HH:MM` unless specified.
- Permission flags are boolean; absence implies false.
- Security groups model coarse-grained access; roles model functional capabilities.

---
## Roles
| role_id        | label          | description                               | can_select_folders | can_generate_route | can_view_ship_static | can_view_errors |
|----------------|----------------|-------------------------------------------|--------------------|--------------------|----------------------|-----------------|
| role-admin     | Administrator  | Full system control including configuration| true               | true               | true                 | true            |
| role-operator  | Operator       | Standard route generation operations       | true               | true               | true                 | true            |
| role-qae       | QAEngineer     | Test validation & error observation        | true               | true               | true                 | true            |
| role-viewer    | Viewer         | Read-only viewing of ship static data      | true               | false              | true                 | true            |

---
## Security Groups
| security_group_id | label          | description                          |
|-------------------|----------------|--------------------------------------|
| sec-routing       | Routing        | Access to routing workflow features  |
| sec-admin         | Admin          | System-wide administrative rights    |
| sec-readonly      | ReadOnly       | Restricted, view-only access         |

---
## Teams
| team_id | name          | description                        |
|---------|---------------|------------------------------------|
| team-1  | Operations    | Daily route processing team         |
| team-2  | QA            | Quality assurance & test execution |
| team-3  | Observers     | Monitoring & reporting             |

---
## People
| person_id    | display_name | email                     | primary_role   | additional_roles        | teams            | security_groups                 |
|--------------|--------------|---------------------------|----------------|-------------------------|------------------|---------------------------------|
| person-alice | Alice Ops    | alice.ops@example.test    | role-operator  | role-qae                | team-1,team-2    | sec-routing,sec-readonly        |
| person-bob   | Bob Admin    | bob.admin@example.test    | role-admin     | role-operator           | team-1           | sec-routing,sec-admin           |
| person-cara  | Cara QA      | cara.qa@example.test      | role-qae       | role-viewer             | team-2           | sec-readonly,sec-routing        |
| person-dan   | Dan Viewer   | dan.viewer@example.test   | role-viewer    | (none)                  | team-3           | sec-readonly                    |
| person-erin  | Erin Ops     | erin.ops@example.test     | role-operator  | role-viewer             | team-1           | sec-routing                     |

---
## Ships (Static Test Set)
| mmsi      | imo      | name        | call_sign | vessel_type      | length_m | beam_m | draft_m | data_range_start      | data_range_end        |
|-----------|----------|-------------|----------|------------------|----------|--------|---------|-----------------------|-----------------------|
| 205196000 | 9301234  | TestVessel1 | ONAB1    | Cargo            | 180      | 28     | 9.5     | 2025-03-15 00:00      | 2025-03-16 00:00      |
| 205197000 | 9302235  | TestVessel2 | ONAB2    | Tanker           | 220      | 35     | 11.0    | 2025-03-14 12:00      | 2025-03-16 12:00      |
| 205198000 | 9303236  | TestVessel3 | ONAB3    | Passenger        | 250      | 38     | 8.0     | 2025-03-15 06:00      | 2025-03-15 22:00      |

---
## Folder Accessibility (Sample Paths)
| path                                  | writable | notes                               |
|---------------------------------------|----------|-------------------------------------|
| C:\\Data\\AIS_Input                  | true     | Default input root                  |
| C:\\Data\\AIS_Input_ReadOnly         | false    | Simulate permission denied          |
| C:\\Data\\AIS_Output                 | true     | Default output root                 |
| C:\\Data\\AIS_Output_Full            | true     | Large dataset target                |
| C:\\Data\\AIS_Output_NoWrite         | false    | Disk quota exceeded simulation      |

---
## Permission Matrix (Derived)
| person_id    | can_select_folders | can_generate_route | can_view_ship_static | can_view_errors |
|--------------|--------------------|--------------------|----------------------|-----------------|
| person-alice | true               | true               | true                 | true            |
| person-bob   | true               | true               | true                 | true            |
| person-cara  | true               | true               | true                 | true            |
| person-dan   | true               | false              | true                 | true            |
| person-erin  | true               | true               | true                 | true            |

Note: `person-dan` cannot generate routes (role-viewer). Use this for negative permission scenarios.

---
## Validation Messages (Canonical Copies)
| key                               | text                                      |
|-----------------------------------|-------------------------------------------|
| validation.select_ship            | Select a ship to enable processing        |
| validation.start_before_end       | Start time must be before End time        |
| validation.both_times_required    | Select both Start and End time            |
| validation.output_not_writable    | Output folder is not writable             |
| error.processing_failed_prefix    | Processing failed:                        |

---
## Filename Pattern Rules
- Success filename pattern: `<MMSI>_<YYYY-MM-DD>_<HHMM>_<HHMM>.xml`
- Temporary/incomplete pattern (must NOT remain): `<MMSI>_<YYYY-MM-DD>_<HHMM>_<HHMM>.xml.tmp`

---
## Test Selectors (data-testid registry)
| component                | data-testid            |
|--------------------------|------------------------|
| Main Window              | main-window            |
| Input Folder Picker      | input-folder-picker    |
| Output Folder Picker     | output-folder-picker   |
| Ship Table               | ship-table             |
| Ship Static Panel        | ship-static-panel      |
| Start Time Picker        | time-start             |
| End Time Picker          | time-end               |
| Process Button           | process-btn            |
| Output Path Field        | output-path            |
| Inline Validation        | validation-inline      |
| Result Modal             | result-modal           |
| Error Banner             | error-banner           |

---
## Usage Guidance
1. Always reference `person_id` + `role_id` for role-sensitive scenarios.
2. For time range tests, select MMSI `205196000` unless testing edge ranges.
3. Use negative folder paths for write-failure scenarios (`C:\\Data\\AIS_Output_NoWrite`).
4. Never fabricate new validation messages; pull from the Validation Messages table.
5. If adding entities, update this file first, then scenarios.

---
## Revision Log
| version | date       | author        | change                                      |
|---------|------------|---------------|---------------------------------------------|
| 1.0.0   | 2025-11-20 | ai-generator  | Initial mock data catalog creation          |
