This file describes the UI layout, component hierarchy, navigation flow, responsive approach, and reusable components for AISRouting.

UI components hierarchy

- App Shell
  - TopNav (user menu, team selector)
  - SideNav (routes, vessels, exports)
  - Main Content
    - Pages
      - DashboardPage
      - RouteEditorPage
      - VesselDetailPage
      - ExportsPage

- RouteEditorPage
  - RouteForm (name, time range, vessel selector)
  - RouteMap (interactive map with waypoints)
  - WaypointList (editable list of waypoints)
  - ValidationBanner (shows errors like "AIS feed stale")

Layout structure and navigation flow

- SPA with client-side routing:
  - / -> Dashboard
  - /routes -> Routes list
  - /routes/new -> Route editor (create)
  - /routes/:id -> Route editor (edit)
  - /vessels/:id -> Vessel detail
  - /exports -> Exports

- Navigation flow example (happy path): Alice selects "Routes" from SideNav -> clicks "Create Route" -> fills RouteForm (Route name: "Morning Transit", Vessel: "MV Aurora") -> clicks "Generate" -> sees preview on RouteMap -> clicks "Export" -> sees success notification and download link.

Responsive design approach

- Use CSS Grid + Flexbox; breakpoints: mobile (<600px), tablet (600-900px), desktop (>900px).
- On mobile, SideNav collapses to hamburger menu; RouteMap prioritizes vertical stacking with collapsible WaypointList.

Reusable component documentation

- Map component (RouteMap)
  - Props: center, zoom, waypoints, onWaypointAdd, onWaypointMove
  - Emits events for drag/drop of waypoints and selection
  - Accessibility: provide aria-labels for map controls and buttons

- Form controls
  - Select (vessel selector) must support keyboard navigation and have aria-label
  - DateTimePicker for time window with validation messages

Cross references

- See application_organization.md for file locations of the components described above.
- See data_models.md for the data structure used by forms and map components.
