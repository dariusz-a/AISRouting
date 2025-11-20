# Feature 1.2: Geographic Math Utilities

Purpose
- Provide a small, well-tested collection of geographic math utilities used across the application (distance, bearing, coordinate validation, and optional helpers for interpolation).

Goals
- Accurate and well-documented Haversine distance calculation.
- Initial bearing (forward azimuth) between two coordinates.
- Utility functions that are deterministic, pure, and free of external dependencies.
- Comprehensive unit tests with edge-cases and known-reference values.

Scope
- Implementations:
  - haversineDistance(lat1, lon1, lat2, lon2, radius = 6371000): number (meters)
  - initialBearing(lat1, lon1, lat2, lon2): number (degrees, 0-360)
  - degToRad(deg), radToDeg(rad) helpers
  - validateLatLon(lat, lon): boolean (lightweight validation)
- Excluded (for now): great-circle path generation, rhumb-line utilities, map projections.

API Design
- Module: `src/utils/geoMath.ts`
- Exports:
  - `haversineDistance(a: Coordinate, b: Coordinate, radius?: number): number`
  - `initialBearing(a: Coordinate, b: Coordinate): number`
  - `degToRad(d: number): number`
  - `radToDeg(r: number): number`
  - `isValidLatLon(lat: number, lon: number): boolean`

Types
- Coordinate: { lat: number; lon: number }

Algorithm Notes
- Haversine distance: use numerically stable formula with sin^2 of half-angle and clamp to [-1,1] as needed.
- Initial bearing: compute with atan2 and convert to degrees, normalize to [0,360).
- Use Earth radius default of 6,371,000 meters; allow override for testing.

Precision & Testing
- Unit tests must cover:
  - identical points -> distance 0
  - known distance between reference coordinates (use well-known sample: London (51.5074N, 0.1278W) to Paris (48.8566N, 2.3522E) ~343,556 m)
  - known initial bearing for reference points
  - boundary lat/lon values and invalid inputs
- Acceptable tolerances: distances within 1 meter for short ranges and within 100 meters for long inter-city ranges in tests; bearings within 0.1 degrees.

Testing Strategy
- Tests live under `tests/geo_math.spec.ts` and use the project's test runner (Vitest/Jest compatible). Keep tests deterministic and include clear expected values and tolerances.

Examples
- Example usage:
```ts
import { haversineDistance, initialBearing } from '../../src/utils/geoMath'

const d = haversineDistance({lat:51.5074, lon:-0.1278}, {lat:48.8566, lon:2.3522})
const bearing = initialBearing({lat:51.5074, lon:-0.1278}, {lat:48.8566, lon:2.3522})
```

Acceptance Criteria
- Functions implemented in `src/utils/geoMath.ts`.
- Unit tests in `tests/geo_math.spec.ts` exist and pass locally.
- Documentation in this feature file describes API, algorithms and tolerances.

Milestones / Tasks
- Implement feature design doc (this file) — done
- Add unit test skeleton — done (placeholder tests added)
- Implement `src/utils/geoMath.ts` utility functions
- Run tests and adjust tolerances/implementation as necessary

Risks
- Small floating-point differences across platforms; mitigate with tolerances in tests.
- Incorrect latitude/longitude ordering — document and standardize on `(lat, lon)`.

Dependencies
- None external. Keep implementation dependency-free to aid portability and testing.

Next Steps
- Implement `src/utils/geoMath.ts` and run `tests/geo_math.spec.ts`.
