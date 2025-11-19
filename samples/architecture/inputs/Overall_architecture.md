# Overall Architecture and Technology Stack

## System Landscape
The eCommerce platform consists of:
- API Gateway → Order Service → Payment Service → Notification Service
- Asynchronous events managed via Kafka topics (`orders`, `payments`, `notifications`)
- Data stored in PostgreSQL (Orders, Payments) and Redis (Cache)

## Integration Points
- Retry Service will subscribe to Kafka `order_failures` topic.
- It will communicate with Order Service via REST API and publish to `orders_retry` topic.

## Observability & Policies
- Distributed tracing via OpenTelemetry.
- Security policies: JWT-based service-to-service auth.
- Deployment environment: Kubernetes with autoscaling enabled.

## Technology Stack

### Frontend
- **Framework**: Vue 3 with Composition API
  - Vue.js version: ^3.4.0 || ^3.5.0
  - @vue/compiler-sfc: (same version as Vue.js)
- **Build Tool**: Vite
  - vite version: ^4.5.0 <6.0.0
  - @vitejs/plugin-vue: ^4.5.0
  - esbuild: ^0.19.0 (for security compliance)
- **Language**: TypeScript
  - typescript version: ^5.0.0
  - @types/node: ^18.0.0
- **Router**: Vue Router (history mode)
  - vue-router version: ^4.2.0
- **State Management**: Pinia
  - pinia version: ^2.1.0
- **UI Components**: PrimeVue for advanced components
  - primevue version: ^4.0.0
  - @primevue/themes version: ^4.0.0
- **CSS Framework**: Primer CSS (GitHub's design system)
  - @primer/css version: latest
  - Utility-first CSS classes for consistent styling
  - Integrated with PrimeVue components
- **Storage**: Local Storage with TypeScript wrapper
  - Custom implementation
- **Testing**: Playwright for E2E testing
  - @playwright/test: ^1.40.0
  - @types/playwright: ^1.40.0

### Version Compatibility Notes
1. **Vue Ecosystem**:
   - Vue.js and @vue/compiler-sfc MUST always be at the same version
   - When updating Vue, all related packages (@vitejs/plugin-vue, vue-router, primevue) MUST be checked for compatibility
   - PrimeVue version MUST be compatible with Vue 3 composition API
2. **Build Tools**:
   - Vite version MUST be kept below 6.0.0 to maintain stability
   - esbuild version MUST be ≥0.19.0 for security compliance
3. **UI Components**:
   - PrimeVue and @primevue/themes MUST be at the same version
   - PrimeVue theme compatibility MUST be verified when updating
   - Primer CSS MUST be compatible with PrimeVue components
   - Primer CSS utility classes should be used for layout, spacing, and typography
4. **Version Updates**:
   - All version updates MUST be tested in development environment before deployment
   - Security audits MUST be run after dependency updates
   - Breaking changes in major versions MUST be reviewed and documented

### Development Environment Requirements
- Node.js: ≥18.x
- npm: ≥9.x
- Git: ≥2.x
- Operating System: Cross-platform compatible
- IDE: VS Code recommended with extensions:
  - Vue Language Features (Volar)
  - TypeScript Vue Plugin (Volar)
  - ESLint
  - Prettier



