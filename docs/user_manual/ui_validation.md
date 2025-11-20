# XML Validation

## Overview
Decision: No runtime XML schema validation is required by default. The application generates XML using a deterministic algorithm that follows `route_waypoint_template.xml`.

## Rationale
- Since the XML is produced by a deterministic, well-tested algorithm and follows the provided template, runtime validation is optional and not necessary for the initial internal QA tool.

## Optional Extension
- If desired later, add a CLI flag or an optional setting to validate generated XML against a schema and surface errors for debugging.
