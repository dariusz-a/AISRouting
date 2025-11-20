# API & Integration Patterns: Design principles, data flow, error handling, performance

This document covers internal service integration patterns (file system, parsing, optimization, export) and future external API considerations. It includes data flow diagrams, error handling strategy, performance approaches, and example interactions. Cross?references overall_architecture.md, data_models.md, and security_architecture.md.

## 1. Design Principles
- Interface-driven services (Core defines interfaces, Infrastructure implements).
- Streaming & lazy loading to reduce memory footprint.
- Pure functions for math helpers (distance, bearing) to simplify testing.
- Fail fast on critical I/O errors; degrade gracefully on partial data (skip malformed rows).
- Separation of parsing from business rules (CsvParser vs TrackOptimizer).

## 2. Internal Integration Flow (Happy Path)
Select Input Folder ? ISourceDataScanner.ScanAsync(path) ? For each vessel: IShipStaticDataLoader.LoadAsync(mmsi) + derive min/max bounds ? User selects vessel + interval ? IShipPositionLoader.LoadPositionsAsync(mmsi, interval) (aggregates multi-day CSV) ? ITrackOptimizer.OptimizeTrack(positions) ? IRouteExporter.ExportToXml(waypoints,...).

## 3. Service Interface Contracts
(See data_models.md for payload types.)

ISourceDataScanner
Task<IReadOnlyList<ShipStaticData>> ScanAsync(string rootPath, CancellationToken ct);
- Returns enriched ShipStaticData objects with Min/Max dates.

IShipStaticDataLoader
Task<ShipStaticData?> LoadAsync(long mmsi, string vesselFolder, CancellationToken ct);

IShipPositionLoader
Task<IReadOnlyList<ShipDataOut>> LoadPositionsAsync(long mmsi, TimeInterval interval, CancellationToken ct);
- Aggregates CSV days covering [StartUtc.Date … StopUtc.Date].

ITrackOptimizer
IReadOnlyList<RouteWaypoint> OptimizeTrack(IReadOnlyList<ShipDataOut> orderedPositions, OptimizationParameters parameters);

IRouteExporter
Task<string> ExportToXmlAsync(long mmsi, TimeInterval interval, IReadOnlyList<RouteWaypoint> waypoints, string outputFolder, CancellationToken ct);
- Returns absolute output file path.

## 4. Data Flow Nuances & Edge Cases
- Multi-day CSV: Loader calculates inclusive date range; loads only days touched by interval.
- Time filtering: After loading rows, filter by AbsoluteTimestamp in [StartUtc, StopUtc].
- Sorting: Ensure orderedPositions ascending by timestamp prior to optimization.
- Duplicate timestamps: Keep first occurrence; subsequent duplicates ignored unless different lat/lon.
- Missing Heading/SOG: Default 0 in RouteWaypoint mapping.

## 5. Error Handling Strategy
Categories:
- Input Validation Errors (invalid path, MMSI) ? surface to UI early.
- Parsing Errors (malformed CSV row) ? row skipped; count aggregated.
- Critical Failures (file access denied) ? abort operation; user message + log.
Patterns:
- Use custom exceptions (DataLoadException, ExportException) with context fields (MMSI, FilePath).
- Wrap low-level IOException; enrich message.

## 6. Retry & Resilience (Future)
Current MVP: No automatic retries (local file system assumed stable).
Future external API integration:
- Token refresh on 401.
- Exponential backoff for transient network failures.
- Circuit breaker for persistent HTTP 5xx.

## 7. Performance Considerations
- CsvParser yields rows; loader only materializes filtered list.
- Optimization is O(n); thresholds applied inline.
- XML export uses streaming write (XmlWriter) avoiding large string concatenations.
- Use Span<char> where beneficial in filename parsing (date extraction) (future micro-optimization).

## 8. Example Interaction
Alice selects vessel 205196000 for interval 2024-01-01 00:00–2024-01-02 00:00.
Loader loads 2024-01-01.csv and 2024-01-02.csv (only early hours for stop boundary). After filter: 642 position rows.
Optimizer retains 118 waypoints (81.6% reduction). Exporter writes file: 205196000-20240101T000000-20240102T000000.xml.

## 9. XML Export Mapping (RouteWaypoint ? XML)
Attributes written per waypoint:
- Name = MMSI
- Lat, Lon
- Alt = 0
- Speed = Speed (double, format "F2")
- ETA = EtaSecondsUntil or 0
- Delay = 0
- Mode = SetWaypointMode(SOG, Heading) (implementation-specific stub)
- TrackMode = "Track"
- Heading = Heading
- PortXTE = 20; StbdXTE = 20
- MinSpeed = 0
- MaxSpeed = global max speed observed (passed externally)
Edge: If waypoints empty ? Export aborted with user message.

## 10. Filename Generation & Conflict Handling
Pattern: {MMSI}-{StartUtc:yyyyMMddTHHmmss}-{StopUtc:yyyyMMddTHHmmss}.xml
If exists:
- Overwrite (user choice) ? replace file.
- Append suffix “_1”, “_2”… until free.
- Cancel ? no write.

## 11. Security Integration Points (Cross-Reference security_architecture.md)
- Path validation before read/write.
- Logging omitted for raw row data (only counts).

## 12. Testing Integration Patterns
Unit Tests stub interfaces using Moq; supply synthetic sequences to Optimizer.
Integration Tests use temporary folders with sample CSV/JSON fixtures; assert XML output contents & filename pattern.

## 13. Future External API Pattern (Placeholder)
External Enrichment Service (e.g., vessel registry API):
- Service: IVesselEnrichmentService.FetchAsync(mmsi) returns AdditionalStaticInfo.
- Decorator pattern wraps ShipStaticDataLoader to merge enrichment fields if available.

## 14. Edge Scenario Narratives
Scenario: Missing CSV file for expected date (filesystem gap) ? Loader logs warning; continues with existing files; interval filter may yield fewer points.
Scenario: Optimization returns only 2 waypoints (start & end) ? Export still valid; UI warns about sparse track.
Scenario: Export folder path becomes unavailable mid-write ? ExportException; partial file cleaned up.

## 15. Cross-References
- overall_architecture.md (Iteration plan & thresholds)
- data_models.md (Model fields)
- security_architecture.md (Validation & logging constraints)
- application_organization.md (Project/service layout)
