This file describes the data models, storage choices, and relationships used by AISRouting.

Data structures

- Vessel
  - id: uuid
  - name: string (e.g., "MV Aurora")
  - mmsi: string
  - last_seen_at: timestamp
  - last_position: geography(Point)
  - status: enum (Active, Inactive)

- Route
  - id: uuid
  - name: string (e.g., "Morning Transit")
  - owner_id: uuid (references User)
  - vessel_id: uuid (references Vessel)
  - created_at: timestamp
  - updated_at: timestamp
  - time_window_start: timestamp
  - time_window_end: timestamp
  - waypoints: geometry(LineString) or JSONB array of waypoint objects
  - status: enum (Draft, Generated, Exported)

- Waypoint
  - id: uuid
  - route_id: uuid
  - sequence: integer
  - name: string (e.g., "Waypoint 1 - Buoy A")
  - position: geography(Point)
  - eta: timestamp

- User
  - id: uuid
  - username: string (e.g., "alice")
  - display_name: string (e.g., "Alice Johnson")
  - email: string
  - roles: JSONB (array of role ids)

- Team
  - id: uuid
  - name: string (e.g., "Marketing")
  - members: JSONB (array of user ids)

Storage implementation

- Primary DB: PostgreSQL with PostGIS extension
  - Use geography(Point) for coordinates and geometry(LineString) for routes.
  - Use GIST indexes on spatial columns for fast queries.
  - Use JSONB for flexible metadata (role lists, preferences).

- Cache: Redis
  - Store computed route previews, session data, and rate-limited caches.

- Object Storage: S3-compatible
  - Store exported files (GPX, KML), large log artifacts, and map tiles if needed.

Data relationships

- User 1..* Team membership via Team.members (JSONB) or normalized join table (preferred)
- User 1..* Route (owner_id)
- Vessel 1..* Route (vessel_id)
- Route 1..* Waypoint (route_id, sequence)

Example data

- User: { id: "1111-2222", username: "alice", display_name: "Alice Johnson", email: "alice@example.com", roles: ["planner"] }
- Team: { id: "t1", name: "Marketing", members: ["1111-2222"] }
- Vessel: { id: "v1", name: "MV Aurora", mmsi: "123456789", last_seen_at: "2025-01-15T08:00:00Z", last_position: { type: "Point", coordinates: [10.0, 59.9] } }
- Route: { id: "r1", name: "Morning Transit", owner_id: "1111-2222", vessel_id: "v1", time_window_start: "2025-01-15T08:00:00Z", time_window_end: "2025-01-15T12:00:00Z", waypoints: [{sequence:1, name:"Start - Port", position:{type:"Point",coordinates:[10.0,59.9]}}, {sequence:2, name:"Waypoint Alpha", position:{type:"Point",coordinates:[10.5,60.1]}}] }

Cross references

- See overall_architecture.md for system architecture and service responsibilities.
- See application_organization.md for code modules that map to these models.
