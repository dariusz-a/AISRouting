# Mock Data Used For BDD Scenarios

This file provides canonical mock values used by the refined BDD scenarios under `docs/spec_scenarios/`.

Users:
- scenario-user: A simulator user with full create/export privileges
- user-no-export: A simulator user with read-only permissions (cannot export)
- user-no-create: A simulator user without create-track privileges

Vessels / IDs:
- mmsi-1: 205196000
- mmsi-2: 205196001

Paths:
- input_root_example: C:\\data\\ais_root
- empty_root_example: C:\\empty\\root
- export_tmp: C:\\tmp\\exports
- export_protected: C:\\protected\\exports

Timestamps:
- ts_first: 20250315T000000
- ts_last: 20250316T000000
- ts_last_plus_24h: 20250317T000000

Notes:
- Use these identifiers exactly in Examples tables or Scenario Outline parameters.
- If additional mock values are needed, add them here and I will update the scenarios to use them.
