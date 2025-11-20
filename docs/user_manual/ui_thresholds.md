# Thresholds Panel and Filtering Algorithm

## Overview
This document specifies the adjustable thresholds used for track optimization and the exact filtering algorithm applied to time-series AIS records.

## Thresholds Panel
- **Panel Label**: `Thresholds`
- **Fields**:
  - `Time gap threshold (seconds)` — numeric edit box. Used to compare `dt` between `prev` and `prev-prev`.
  - `Distance tolerance (meters)` — numeric edit box. Tolerance used when comparing `prev` position to interpolated position.
- **Defaults**: Choose conservative defaults (e.g., `Time gap threshold = 30s`, `Distance tolerance = 10m`) but values are editable by QA engineers.

## Filtering Algorithm (applied in order)
1. When a **new** record arrives, examine the two previous records: **prev** (immediately previous) and **prev-prev**.
2. **new** is always added to the output sequence; the filter may remove **prev** according to rules below.
3. Calculate `dt = time(prev) - time(prev-prev)`.
4. If `dt < Time gap threshold` then consider removing **prev**:
   a. Use **prev-prev** and **new** to compute an `interpolated-prev` record at the timestamp of **prev** by linear interpolation (interpolate latitude and longitude and any other continuous fields using time as the parameter).
   b. Compute distance between position(prev) and position(interpolated-prev).
   c. If this distance < `Distance tolerance` then discard **prev** (do not include it in the optimized output).

## Notes
- The algorithm is simple and deterministic to keep the code easy to maintain.
- Expose thresholds in the UI to allow tuning by QA when testing different datasets.
