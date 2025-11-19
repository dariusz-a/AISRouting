This file describes the project structure, component organization, and coding conventions for AISRouting.

Project structure

- /src
  - /client (frontend SPA)
    - /components
    - /pages
    - /services (API clients)
    - /stores (state management: Redux/Pinia/Vuex)
    - /routes (React Router / Vue Router)
  - /server
    - /services (RoutingService, VesselService, AuthService)
    - /controllers
    - /models (DB models / ORM)
    - /workers (background jobs)
    - /utils
  - /shared
    - /types (TypeScript interfaces shared between client & server)
    - /api (API contract definitions)

Component organization

- UI components are organized by feature domain. For example:
  - /components/route/
    - RouteEditor.tsx
    - WaypointList.tsx
    - RouteMap.tsx

- Services (client-side) encapsulate API calls and local caching patterns:
  - RouteService (getRoutes, createRoute, generateRoutePreview, exportRoute)
  - VesselService (subscribeToAIS, getVesselById)

Code organization

- Use TypeScript for type-safety across the stack.
- Follow a service-first pattern: UI components should delegate business logic to services and stores.
- Keep components small and presentational; container components handle orchestration.
- Tests are colocated with files under a __tests__ folder or a tests/ directory mirroring the src structure.

Examples and naming

- Files follow kebab-case or PascalCase for components (RouteEditor.tsx)
- Services: PascalCaseService (RouteService.ts)
- Stores: use domain-based naming (routeStore, vesselStore)

Cross references

- See data_models.md for persistent entities and how they map to models in /server/models.
- See application_layout.md for UI pages and navigation mapping.
