# Create Track

## Overview
This section covers generating an optimized track from AIS CSV records for the selected ship and time range. It describes default optimization thresholds and how the track is prepared for export.

## Prerequisites
- Input root and ship selected (see Getting Started and Ship Selection)
- Time range selected with second resolution

## Step-by-Step Instructions
### Start Track Creation
1. Click "Create Track" after selecting ship and time range.
2. The system will process AIS CSV rows in the selected interval and perform filtering and optimization using default parameters.

### Default Optimization Parameters
- Minimum heading change: 0.2 degrees
- Minimum distance between points: 5 meters
- SOG change threshold: 0.2 knots
- ROT threshold: 0.2 deg/s
- Max allowed time gap: not enforced in this release

### Results and Validation
1. After processing, the generated track appears in the UI as a list of points.
2. Review points for continuity and expected vessel behavior.

## Tips and Best Practices
- For noisy AIS data, consider narrowing the time window to improve results.
- Preview the results (if enabled) before exporting.

## Related Sections
- [Ship Selection and Static Data](ship_selection.md)
- [Exporting Routes](export_route.md)
