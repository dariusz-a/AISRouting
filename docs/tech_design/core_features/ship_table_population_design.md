# Feature Design:: Ship Table Population

This document outlines the technical design for the Ship Table Population feature.

## Feature Overview
The Ship Table Population feature is responsible for transforming folder selections (input and output prerequisites) plus parsed ship metadata and CSV availability into a coherent, interactive tabular UI that enables users to:
- View a list of all discovered ships (identified by MMSI) after folder scanning
- See key metadata columns (e.g., MMSI, vessel name, type, available time span, CSV count)
- Automatically disable rows for ships that lack required CSV files (preventing invalid downstream processing)
- Apply default sorting (MMSI ascending) and allow user re-sorting by supported columns
- Trigger enabling of dependent UI controls (time pickers, process workflow prerequisites) once a valid ship row is selected

Business value: This feature establishes a reliable, testable, and user-friendly selection pivot for conversion workflow dependencies. It minimizes user error (e.g., selecting ships lacking data) and provides visual clarity and state-driven enablement for subsequent steps (time interval selection, track optimization, processing). It synthesizes underlying parsing and scanning results into actionable UI state.

Included BDD scenarios (from implementation plan):
1. "Ship selection enables time pickers and process prerequisites" (spec: ais_to_xml_route_convertor_summary.md)
2. "Populate ship table after selecting folders" (spec: getting_started.md)
3. "Ship row without CSV files is disabled" (spec: ais_to_xml_route_convertor_summary.md)
4. "Sorting ship table by MMSI ascending default" (spec: ais_to_xml_route_convertor_summary.md)

Note: This feature references scenarios distributed across two spec documents (ais_to_xml_route_convertor_summary.md and getting_started.md), which is a deviation from the stated rule that a Feature should map to a single BDD spec file. A consolidation into one spec file is recommended for consistency. Until consolidation, this design treats both as authoritative input.

## Architectural Approach
Pattern alignment: MVVM (Avalonia) with clear separation of view (XAML), view-model (reactive state & commands), domain models (ShipStaticData, ShipState), and service layer (FolderScanService, ShipAggregationService). State is centrally managed through a Feature Store abstraction implementing observable properties and exposing immutable read models to the view-model.

Key principles:
- Single Responsibility: Each component/store/service handles one concern (e.g., scanning, aggregation, presentation).
- Explicit State Derivation: Disabled rows, sort order, selection state, and enablement flags (time pickers/process button) are computed in selector-like methods, enabling deterministic testing.
- Dependency Inversion: View-model depends on interfaces (IShipTableStore, IShipSelectionService) allowing test doubles.
- Testability by Design: All derived states expose observable properties; UI elements include deterministic data-test attributes.

Data Flow Summary:
Input Folder & Parsing Completion → FolderScanService emits ship discovery records → ShipAggregationService merges static JSON + CSV presence → ShipTableStore normalizes data into ShipRowViewModel records → ShipTableViewModel exposes ObservableCollection for binding; selection changes propagate to TimeIntervalStore enabling controls.

User Experience Strategy:
- Fast initial render with placeholder rows if scanning is in progress.
- Immediate visual differentiation for disabled rows (grayed styling + tooltip explaining missing CSV).
- Default sort applied once population completes; user sort interactions update store sort state.
- Selecting a valid row fires a command that updates shared workflow context enabling downstream controls.

## File Structure
```
src/
  features/
    shipTablePopulation/
      models/
        ShipRowModel.cs              // Normalized presentation model (pure data)
        ShipTableSortState.cs        // Enum + comparer logic for sort criteria
      services/
        IShipAggregationService.cs   // Interface for assembling ship rows
        ShipAggregationService.cs    // Implements data merge & row derivation
      store/
        IShipTableStore.cs           // Read-only interface for external consumers
        ShipTableStore.cs            // Internal mutable reactive state & derivations
      viewmodels/
        ShipTableViewModel.cs        // Exposes collection, selection command, sort commands
      views/
        ShipTableView.axaml          // Avalonia XAML table with bindings & test selectors
        ShipTableView.axaml.cs       // Code-behind minimal wiring
      converters/
        DisabledReasonConverter.cs   // Value converter for tooltip text
      accessibility/
        ShipTableA11yMap.cs          // Central mapping of automation IDs & roles
      tests/
        ShipTablePopulation.spec.ts  // BDD mapped test (Playwright/E2E)
        ShipTableStore.Tests.cs      // Unit tests for state derivations
        ShipAggregationService.Tests.cs // Unit tests merging logic
      mockdata/
        ShipStaticDataMocks.cs       // Centralized static data fixtures
        ShipStateCsvMocks.cs         // Sample CSV-derived time spans & counts
        ShipRowFactoryTestHelper.cs  // Helper building rows for tests
      selectors/
        ShipTableSelectors.cs        // Static methods deriving computed states (e.g., enabledRows)
      types/
        ShipTableEvents.cs           // Domain events (RowSelected, SortChanged)
```
Comments:
- `models` isolates pure POCOs from reactive constructs aiding serialization & test fixture reuse.
- `store` centralizes mutable state behind interface promoting isolation and mocking.
- `selectors` host pure derivation logic ensuring deterministic & unit-testable computation.
- `accessibility` ensures consistent AutomationProperties & data-test attributes.

## Component Architecture
Components & Responsibilities:
1. ShipTableView (View): Renders table (DataGrid) bound to `ObservableCollection<ShipRowModel>`; applies styles for disabled rows; exposes data-test attributes per row.
2. ShipTableViewModel: Holds reference to `IShipTableStore`; wraps Commands: SelectRow, SortByMmsi, SortByName. Handles mapping of store rows into observable collection and dispatches selection events.
3. ShipTableStore: Maintains internal list of ShipRowModel, current sort state, selected MMSI, and exposes reactive properties (INotifyPropertyChanged). Provides derived flags: `IsSelectionValid`, `EnabledRows`, `DefaultSortApplied`.
4. ShipAggregationService: Accepts results from scanning (list of MMSI folders) + parsed static JSON + CSV presence map; builds normalized ShipRowModel list including `IsDataComplete` boolean.
5. DisabledReasonConverter: UI converter returning reason string for disabled rows for tooltips.
6. ShipTableSelectors: Static pure functions used by store (e.g., `DeriveEnabledRows(rows)`, `ApplySort(rows, sortState)`).
7. TimeIntervalStore (existing feature dependency): Subscribes to `ShipTableStore.SelectedMmsiChanged` event enabling pickers when valid.

Communication Patterns:
- Services → Store: Push new data sets.
- Store → ViewModel: Reactive property changed events.
- ViewModel → Store: Method invocations for selection & sorting.
- Store → Other Stores: Event aggregator / direct event raising for selection enabling downstream controls.

Accessibility & Interaction:
- Each row: `data-test="ship-row-{MMSI}"`, AutomationId = `ShipRow_{MMSI}`.
- Disabled row styling: `class="ship-row--disabled"` plus tooltip.
- Sorting headers: `data-test="sort-mmsi"`, `data-test="sort-name"` enabling Playwright selectors.

End-to-End Testing Considerations:
- Deterministic ordering after sort command assures stable selectors.
- Selection triggers explicit event logged via diagnostic logger for test confirmation.
- Disabled rows expose `aria-disabled="true"` for accessibility & testing.

## Data Integration Strategy
Data Inputs:
- ShipStaticData (from JSON parser)
- ShipState aggregates (CSV parser output for time bounds & count)
- Folder scan results (list of discovered MMSI identifiers)

Integration Flow:
1. Folder scan completes → list of MMSI.
2. For each MMSI, attempt load of static JSON; map presence & parsed fields.
3. Gather CSV presence & derive temporal coverage (min/max timestamp) & row count.
4. Build `ShipRowModel`:
   - `Mmsi: int`
   - `Name: string?`
   - `VesselType: string?`
   - `CsvFileCount: int`
   - `AvailableFrom: DateTime?`
   - `AvailableTo: DateTime?`
   - `IsDataComplete: bool` (CsvFileCount > 0 && static JSON available)
   - `IsSelectable: bool` (equals IsDataComplete)
5. Store receives full collection; applies default sort (MMSI ascending) exactly once.

Error Handling & Edge Cases:
- Missing JSON: Row still appears but disabled; Name / VesselType fallback to "Unknown".
- No CSV files: `CsvFileCount = 0`; row disabled with reason "No CSV data".
- Partial CSV time span anomaly (e.g., min > max): Row flagged with warning tooltip; remains disabled.
- Duplicate MMSI discovered: Service deduplicates early and logs warning.

Observability:
- Store emits `RowsPopulated` event after initial population & sort.
- Diagnostic logger records counts: total discovered, selectable, disabled reasons summary.

## Implementation Examples
### ShipRowModel
```csharp
public sealed record ShipRowModel(
    int Mmsi,
    string? Name,
    string? VesselType,
    int CsvFileCount,
    DateTime? AvailableFrom,
    DateTime? AvailableTo,
    bool IsDataComplete,
    bool IsSelectable);
```

### ShipTableStore (excerpt)
```csharp
public sealed class ShipTableStore : IShipTableStore, INotifyPropertyChanged {
    private readonly List<ShipRowModel> _rows = new();
    private ShipTableSortState _sortState = ShipTableSortState.MmsiAsc;
    private int? _selectedMmsi;

    public IReadOnlyList<ShipRowModel> Rows => _rows;
    public int? SelectedMmsi => _selectedMmsi;
    public bool IsSelectionValid => _selectedMmsi.HasValue && _rows.Any(r => r.Mmsi == _selectedMmsi && r.IsSelectable);

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<int>? SelectedMmsiChanged;

    public void Populate(IEnumerable<ShipRowModel> incoming) {
        _rows.Clear();
        _rows.AddRange(ShipTableSelectors.ApplySort(incoming, _sortState));
        OnPropertyChanged(nameof(Rows));
        // fire populated diagnostic event
    }

    public void Select(int mmsi) {
        var row = _rows.FirstOrDefault(r => r.Mmsi == mmsi);
        if (row is null || !row.IsSelectable) return; // selection guard
        _selectedMmsi = mmsi;
        OnPropertyChanged(nameof(SelectedMmsi));
        OnPropertyChanged(nameof(IsSelectionValid));
        SelectedMmsiChanged?.Invoke(this, mmsi);
    }

    public void Sort(ShipTableSortState state) {
        _sortState = state;
        var sorted = ShipTableSelectors.ApplySort(_rows, state);
        _rows.Clear();
        _rows.AddRange(sorted);
        OnPropertyChanged(nameof(Rows));
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### Selector Example
```csharp
public static class ShipTableSelectors {
    public static IEnumerable<ShipRowModel> ApplySort(IEnumerable<ShipRowModel> rows, ShipTableSortState state) =>
        state switch {
            ShipTableSortState.MmsiAsc => rows.OrderBy(r => r.Mmsi),
            ShipTableSortState.NameAsc => rows.OrderBy(r => r.Name).ThenBy(r => r.Mmsi),
            _ => rows
        };
}
```

### ViewModel (selection & sort commands)
```csharp
public sealed class ShipTableViewModel {
    private readonly IShipTableStore _store;
    public ObservableCollection<ShipRowModel> Rows { get; } = new();

    public ICommand SelectRowCommand { get; }
    public ICommand SortMmsiCommand { get; }
    public ICommand SortNameCommand { get; }

    public ShipTableViewModel(IShipTableStore store) {
        _store = store;
        SelectRowCommand = ReactiveCommand.Create<int>(mmsi => _store.Select(mmsi));
        SortMmsiCommand  = ReactiveCommand.Create(() => _store.Sort(ShipTableSortState.MmsiAsc));
        SortNameCommand  = ReactiveCommand.Create(() => _store.Sort(ShipTableSortState.NameAsc));
        // subscribe to store changes & sync Rows
    }
}
```

### Avalonia View (snippet)
```xml
<DataGrid Items="{Binding Rows}" AutoGenerateColumns="False" x:Name="ShipTable" data-test="ship-table">
  <DataGrid.Columns>
    <DataGridTextColumn Header="MMSI" Binding="{Binding Mmsi}" data-test="col-mmsi"/>
    <DataGridTextColumn Header="Name" Binding="{Binding Name}" data-test="col-name"/>
    <DataGridTextColumn Header="Type" Binding="{Binding VesselType}"/>
    <DataGridTextColumn Header="CSV" Binding="{Binding CsvFileCount}"/>
  </DataGrid.Columns>
  <DataGrid.RowStyle>
    <Style Selector="DataGridRow">
      <Setter Property="IsEnabled" Value="{Binding IsSelectable}" />
      <Setter Property="ToolTip.Tip" Value="{Binding IsSelectable, Converter={StaticResource DisabledReasonConverter}}" />
      <Setter Property="Classes" Value="{Binding IsSelectable, Converter={StaticResource DisabledRowClassConverter}}" />
    </Style>
  </DataGrid.RowStyle>
</DataGrid>
```

## Testing Strategy and Quality Assurance
Test Layers:
- Unit: `ShipAggregationService.Tests`, `ShipTableStore.Tests` verifying row creation, sorting, selection guards.
- Integration: Simulated scan + parse feeding store ensuring combined behavior (population triggers correct disabled states).
- E2E (Playwright): `ShipTablePopulation.spec.ts` exercises full user flow: folder selection → table population → row selection enabling time pickers.
- Accessibility: Validate roles, `aria-disabled`, and keyboard navigation (arrow keys selecting enabled rows only).

Positive Scenarios:
- Valid ship with CSV & static JSON selectable and enables time pickers.
- Table defaults sorted by MMSI ascending.

Negative / Edge Scenarios:
- Attempt to select disabled row (assert no state change).
- Missing CSV leads to disabled row with tooltip.
- Sorting toggled retains disabled states.

Selectors:
- Table: `data-test="ship-table"`
- Row: `data-test="ship-row-{MMSI}"`
- Sort buttons/headers: `data-test="sort-mmsi"`, `data-test="sort-name"`

Mock Data Requirements:
- Centralized fixtures: `ShipStaticDataMocks` provides sample ships (with & without names).
- CSV presence: `ShipStateCsvMocks` exposes methods `CreateStateSet(mmsi, count, from, to)`.
- Helper Factory: `ShipRowFactoryTestHelper.Build(mmsi, hasCsv, hasJson)` for rapid scenario composition.
- Edge fixtures: Missing JSON, zero CSV, anomalous time bounds.
- Exposure: Store exposes `Rows` allowing direct inspection and row filtering in tests.

Test Data Management:
- Reuse domain-level mocks across features (avoid duplication) leveraging single source-of-truth pattern from QA guidelines.
- All timestamps use UTC ISO-8601; deterministic values for reproducibility.

## Mock Data Requirements
Objects:
```csharp
public static class ShipStaticDataMocks {
  public static ShipStaticData Create(int mmsi, string? name = null, string? type = null) =>
    new(mmsi, name ?? $"Vessel-{mmsi}", type ?? "Cargo", ...);
}
```
```csharp
public static class ShipStateCsvMocks {
  public static (DateTime from, DateTime to, int count) Span(int count) => (DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(count), count);
}
```
Factories ensure consistent generation while enabling edge case overrides.

Usage in Tests:
1. Build aggregated input lists.
2. Invoke `ShipAggregationService.BuildRows(...)`.
3. Populate store; assert disabled/enabled states & sorting.
4. Perform selection; assert enabling of time interval controls (mock subscriber verifying event).

## Design Validation Checklist
- ✓ Approved components only (DataGrid, Commands, reactive properties)
- ✓ Separation of concerns (aggregation vs. presentation vs. interaction)
- ✓ Deterministic sorting & selection logic
- ✓ Observability for E2E (events, data-test attributes)
- ✓ Edge cases documented & testable
- ✓ Accessibility attributes included (`aria-disabled` via Avalonia mapping)

## Trade-offs & Rationale
- Central Store vs. direct ViewModel logic: Store isolates mutation & derivations improving unit test clarity.
- Selectors static class: Pure functions simplify reasoning & allow reuse across other features (e.g., future filtering).
- Multiple spec file mapping: Temporary pragmatic approach to avoid blocking progress; flagged for consolidation.

## Future Extensions
- Column filtering (search by name/MMSI) using same selector pattern.
- Persist user sort preference.
- Virtualization for large fleets (>10k rows).

## Risks & Mitigations
- Inconsistent spec file usage → Mitigation: plan consolidation ticket.
- Large row counts could degrade performance → Mitigation: introduce virtualization lazily.
- Race between scan completion and user interaction → Mitigation: disable table until `RowsPopulated` event.

---
This design adheres to project architectural standards and emphasizes clarity, testability, and maintainability for Ship Table Population.
