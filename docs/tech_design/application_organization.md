# Application Organization: Project, component, and code structure

This document covers the organizational blueprint for AISRouting: solution layout, project responsibilities, folder conventions, naming standards, dependency flow, and edge case considerations for maintainability. Cross?references overall_architecture.md and data_models.md.

## 1. Solution Structure
Root: AISRouting/
- src/
  - AISRouting.App.WPF/ (Presentation Layer)
  - AISRouting.Core/ (Domain + Service Interfaces + Optimization)
  - AISRouting.Infrastructure/ (I/O Implementations: CSV, JSON, XML)
  - AISRouting.Tests/ (Unit/Integration tests)
- docs/ (Design & workflow docs)

## 2. Project Responsibilities
- AISRouting.App.WPF: Views (XAML), ViewModels (MVVM Toolkit), DI bootstrap, application configuration.
- AISRouting.Core: Domain models, interfaces (ISourceDataScanner, IShipStaticDataLoader, IShipPositionLoader, ITrackOptimizer, IRouteExporter), algorithm helpers (distance, bearing), optimization logic.
- AISRouting.Infrastructure: Concrete service implementations (SourceDataReader, CsvParser, JsonParser, XmlRouteWriter). Handles filesystem path validation, streaming parse, serialization concerns.
- AISRouting.Tests: NUnit test projects with folder-per-concern (UnitTests/Services, UnitTests/ViewModels, IntegrationTests/Workflow).

## 3. Folder Conventions (Per Project)
App.WPF:
- Views/ (MainWindow.xaml, dialogs)
- ViewModels/ (MainWindowViewModel, VesselSelectionViewModel, TrackGenerationViewModel)
- Resources/ (Styles.xaml, DataTemplates.xaml)
- Startup/ (ServiceRegistration.cs) optional.
Core:
- Models/ (ShipStaticData.cs, ShipDataOut.cs, TimeInterval.cs, RouteWaypoint.cs, Route.cs)
- Services/ (interfaces + core logic: TrackOptimizer.cs)
- Helpers/ (GeoMath.cs for Haversine/Bearing)
Infrastructure:
- IO/ (SourceDataScanner.cs, ShipStaticDataLoader.cs, ShipPositionLoader.cs)
- Parsing/ (CsvParser.cs, JsonParser.cs)
- Export/ (RouteExporter.cs, XmlRouteWriter.cs)
Tests:
- UnitTests/ (ModelTests, OptimizerTests, LoaderTests)
- IntegrationTests/ (EndToEndTrackTests)
- TestData/ (Sample JSON/CSV fixtures)

## 4. Dependency Direction
App.WPF ? Core & Infrastructure.
Core ? Infrastructure only via interfaces defined in Core; Infrastructure implements them (no cyclic dependency).
Tests project references all others but produces no exports consumed by runtime.

## 5. Naming Standards
- Interfaces: I + PascalCase (ITrackOptimizer).
- Classes: PascalCase (TrackOptimizer).
- Private fields: _camelCase.
- Async methods: VerbNounAsync (LoadPositionsAsync).
- Commands: Verb (CreateTrackCommand).
- XAML Elements: Descriptive names (VesselComboBox) to aid future UI automation.

## 6. ViewModel Patterns
RelayCommand for commands. Properties raise change notifications via ObservableObject. Long-running operations executed async with cancellation support and UI state flags (IsBusy, ProgressValue). Error messages exposed via string? ErrorMessage; UI binds with visibility converters.

Edge Cases:
- Multiple overlapping commands ? Disable UI while IsBusy.
- Cancellation requested mid-optimization ? Clear partial results, reset progress, log event.

## 7. Service Composition & DI
ServiceRegistration.cs configures:
services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
services.AddSingleton<IShipStaticDataLoader, ShipStaticDataLoader>();
services.AddTransient<IShipPositionLoader, ShipPositionLoader>();
services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
services.AddTransient<IRouteExporter, RouteExporter>();
services.AddSingleton<IFolderDialogService, FolderDialogService>();
Logging: services.AddLogging(builder => builder.AddFile("logs/app.log"));
ViewModels resolved either via constructor injection or a simple ViewModelFactory.

## 8. Code Organization Principles
- Separation of concerns: Parsing vs business logic vs UI orchestration.
- Interface-driven design for testability.
- Small methods (single responsibility) in optimizer; math extracted.
- Avoid static state; prefer DI singletons where caching is safe (scanner). No ambient global variables.
- Null handling: Domain models prefer nullable types; mapping layer handles defaults.

## 9. Testing Organization
Unit tests mirror folder structure:
- OptimizerTests cover threshold logic (heading, ROT, SOG, distance).
- LoaderTests simulate multiple day range, malformed rows.
- ExporterTests ensure XML schema correctness.
Integration tests simulate full flow.
Fixtures stored in Tests/TestData (e.g., 205196000.json, 2024-01-01.csv). Test utilities for building in-memory CSV lines.

## 10. Extensibility Guidelines
Add new algorithm (DouglasPeuckerOptimizer) implementing ITrackOptimizer; swap in DI via config flag. Introduce mapping UI: new project AISRouting.App.Mapping referencing Core & Infrastructure; keep original App as host.
If SQLite introduced: New Infrastructure subfolder Persistence/ with repositories (PositionReportRepository). Maintain interface boundary in Core.

## 11. Error Handling Strategy
- Services bubble domain exceptions wrapped in custom DataLoadException, ExportException.
- ViewModels catch exceptions, set ErrorMessage, log details.
- Non-fatal issues (missing fields) logged as warnings; never crash UI.

## 12. Example Interaction Flow (Cross-Reference overall_architecture.md)
MainWindowViewModel.Initialize() ? Prompt folder selection ? SourceDataScanner.ScanAsync(path) ? Populate Vessels ObservableCollection. User selects vessel ? Load static data + date bounds ? User sets interval ? CreateTrackCommand executes optimization ? Waypoints displayed.

## 13. Edge Scenario Narratives
Scenario: User selects folder with no valid MMSI subfolders ? Vessels list empty; informational message.
Scenario: Two simultaneous exports attempted (double-click) ? Command disabled after first invocation.
Scenario: JSON parse exception ? ShipStaticData created with MMSI only; Name fallback; warning logged.

## 14. Cross-References
- overall_architecture.md (Iterations, algorithms)
- data_models.md (Field definitions)
- application_layout.md (UI hierarchy mapping to ViewModels)
- security_architecture.md (Validation & path safety)
