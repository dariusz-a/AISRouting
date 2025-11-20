# Feature Design:: ShipStaticData Panel

This document outlines the technical design for the ShipStaticData Panel feature.

## Feature Overview
The ShipStaticData Panel presents a complete, read-only view of the selected vessel's static metadata (`ShipStaticData`) and supports exporting that data to a JSON file on demand. It activates after a user selects a ship (MMSI) in the ship selection workflow (Feature 5.1 / 5.2 prerequisites) and displays all available fields (Mmsi, Name, Length, Beam, Draught, CallSign, ImoNumber, MinDateUtc, MaxDateUtc) with appropriate units and null-safe formatting (e.g., "N/A"). The panel strictly forbids editing—attempts to modify content are blocked visually (read-only controls) and logically (no setters or commands enabling mutation). An explicit "Export JSON" action opens a save dialog and persists the current static record to a user-chosen path.

Business value:
- Gives users confidence that correct vessel metadata has loaded before time interval selection and processing.
- Reduces conversion errors by making date range (Min/Max) visible for validating time interval boundaries.
- Enables audit/traceability by exporting the static JSON snapshot exactly as loaded.
- Reinforces simplicity principle: passive visualization with a single explicit action (export) rather than a complex editable form.

Included BDD Scenarios (spec: `docs/spec_scenarios/ui_ship_data.md`):
1. Show full static data for selected ship
2. Export JSON of static data
3. Panel read-only and does not allow edits

Scope Constraints:
- No inline editing, copying individual fields, or contextual validation messages beyond null display handling.
- No refresh/reload button (static data only changes if a different ship is selected).
- Export requires explicit user interaction each time (no auto-export).

## Architectural Approach
Pattern Alignment: Consistent with overall architecture (single-project, MVVM, static helpers). The panel integrates non-invasively into `MainWindow.axaml` as a dedicated region (e.g., right-hand detail panel or collapsible section). All domain retrieval remains delegated to existing static helper (`Helper.LoadShipStatic`). The panel does not own data loading logic; it reacts to selection changes published by `MainViewModel`.

Key Principles Applied:
- Single Responsibility: Panel displays—does not load or mutate—static data.
- Separation of Concerns: View (XAML) handles layout; ViewModel exposes formatted properties; Helper performs file I/O for export.
- Immutable Data: `ShipStaticData` is an immutable record; defensive copying unnecessary.
- Testability by Design: Export logic isolated in a small static method; formatting functions pure; read-only enforcement verifiable via properties and control attributes.
- Accessibility & Observability: Data-test selectors and AutomationIds applied to each field for E2E validation.

Data Flow:
1. User selects ship (MMSI) via Ship Selection feature.
2. `MainViewModel` invokes `Helper.LoadShipStatic(mmsiFolder)`; result stored in a property `SelectedShipStaticData`.
3. `ShipStaticDataPanelViewModel` (bound to panel view) receives change notification (property injection or event) and maps raw record to formatted display properties.
4. User clicks Export → ViewModel calls `ShipStaticDataExportHelper.Export(ShipStaticData, targetPath)`.
5. Success/failure surfaced via status banner/logging.

User Experience Strategy:
- Clear two-column layout: Label / Value rows with subtle grouping (Identification, Dimensions, Operational Range).
- All values appear instantly upon selection; blank panel placeholder when no selection.
- Export button disabled if no static data.
- Tooltips for fields with potential ambiguity (e.g., Draught: "Current draught in meters (may differ from max).")
- Read-only visual cues: Non-editable TextBlocks; copy-to-clipboard enhancement deferred (future extension).

## File Structure
Following `application_organization.md` conventions (single project, no feature sub-project). New/affected files:
```
src/AisToXmlRouteConvertor/
  ViewModels/
    ShipStaticDataPanelViewModel.cs      // MVVM ViewModel for panel
  Views/
    ShipStaticDataPanel.axaml            // Panel XAML view (added to MainWindow or included as UserControl)
    ShipStaticDataPanel.axaml.cs         // Code-behind wiring (minimal)
  Services/
    ShipStaticDataExportHelper.cs        // Static export helper (JSON serialization)
  Mappers/
    ShipStaticDataFormat.cs              // Pure static formatting utilities (null handling, unit strings)
  Accessibility/
    ShipStaticDataA11y.cs                // Central constants for AutomationIds & data-test attributes
  Tests/
    UnitTests/
      ShipStaticDataFormatTests.cs       // Formatting & null-handling unit tests
      ShipStaticDataExportHelperTests.cs // Export success/failure tests
    IntegrationTests/
      ShipStaticDataPanelTests.cs        // Panel rendering & export workflow
    TestData/
      static_ship_valid.json             // Fixture for export/format tests
      static_ship_missing_fields.json    // Fixture with nulls
  mocks/
    mockData.ts                          // (Centralized mock data file; add ship static mock generator)
```
Notes:
- `ShipStaticDataFormat.cs` keeps ViewModel trim (SRP, pure functions easily unit-tested).
- `Accessibility` centralizes selectors (`data-test` keys) for consistent E2E usage.
- Reuse existing `JsonParser.ParseShipStatic` for load; export uses `System.Text.Json` with identical naming to ensure fidelity.

## Component Architecture
Components & Responsibilities:
1. ShipStaticDataPanel (View): Presents structured, read-only fields. Contains Export button. Applies data-test attributes on container and each field.
2. ShipStaticDataPanelViewModel: Transforms `ShipStaticData` into formatted string properties (e.g., length "285.0 m" or "N/A"). Exposes `IsDataAvailable`, `ExportCommand`. No parsing or file IO besides invoking export helper.
3. ShipStaticDataExportHelper: Static method performing SaveFileDialog interaction (invoked by ViewModel or MainViewModel) then writes JSON. Ensures atomic write (write temp + move) for robustness.
4. ShipStaticDataFormat: Pure static functions for unit conversion & null substitution.
5. MainViewModel (existing): Publishes selected ship static data; triggers PanelViewModel update.

Communication Patterns:
- MainViewModel → PanelViewModel: Property setter or event `OnShipStaticDataChanged(ShipStaticData?)`.
- PanelViewModel → ExportHelper: Command executes; returns path or error message.
- ExportHelper → MainViewModel (optional): Could raise a status event for user notifications.

Accessibility & Testing:
- Panel root: `data-test="ship-static-panel"`, AutomationId="ShipStaticPanel".
- Field labels: `data-test="ship-static-label-{field}"`.
- Field values: `data-test="ship-static-value-{field}"`.
- Export button: `data-test="ship-static-export"`, disabled state verifiable.

Read-Only Enforcement:
- Use `TextBlock` for all values (not `TextBox`).
- No focus adorners; tab order places Export button after last value.
- Attempted programmatic edits (simulated in E2E) confirm absence of writable controls.

## Data Integration Strategy
Inputs:
- `ShipStaticData` record from JSON parsing (provided by existing helpers).
Processing:
- Formatting layer maps each nullable double to string with unit and precision `#.##` (or integer for MMSI), fallback "N/A".
- Date range formatting uses `yyyy-MM-dd HH:mm:ss 'UTC'` when present.
Outputs:
- Display-only strings; exported JSON identical to original (no formatting) to maintain fidelity.

Error & Edge Handling:
- Null record (no selection): Panel shows placeholder text "Select a ship to view static data"; Export disabled.
- Malformed JSON (parser returned null): Panel shows "Static data unavailable" with subtle warning style.
- Missing optional fields: Each independently rendered as "N/A".
- Invalid date range (Min > Max) if present: Show both values and inline warning indicator `data-test="ship-static-date-warning"`.

Observability:
- Export operation logs info: `Exported static data for MMSI {mmsi} -> {file}`.
- Failure logs error with exception message and surfaces user dialog.

## Implementation Examples
### ViewModel Skeleton (`ShipStaticDataPanelViewModel.cs`)
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AisToXmlRouteConvertor.Models;

namespace AisToXmlRouteConvertor.ViewModels;

public partial class ShipStaticDataPanelViewModel : ObservableObject
{
    private ShipStaticData? _staticData;

    [ObservableProperty] private string? mmsi;
    [ObservableProperty] private string? name;
    [ObservableProperty] private string? length;
    [ObservableProperty] private string? beam;
    [ObservableProperty] private string? draught;
    [ObservableProperty] private string? callSign;
    [ObservableProperty] private string? imoNumber;
    [ObservableProperty] private string? minDateUtc;
    [ObservableProperty] private string? maxDateUtc;
    [ObservableProperty] private bool isDataAvailable;
    [ObservableProperty] private bool hasDateRangeWarning;

    public void Update(ShipStaticData? data)
    {
        _staticData = data;
        if (data is null)
        {
            IsDataAvailable = false;
            Clear();
            return;
        }
        IsDataAvailable = true;
        Mmsi = data.Mmsi.ToString();
        Name = ShipStaticDataFormat.Display(data.Name);
        Length = ShipStaticDataFormat.Meters(data.Length);
        Beam = ShipStaticDataFormat.Meters(data.Beam);
        Draught = ShipStaticDataFormat.Meters(data.Draught);
        CallSign = ShipStaticDataFormat.Display(data.CallSign);
        ImoNumber = ShipStaticDataFormat.Display(data.ImoNumber);
        MinDateUtc = ShipStaticDataFormat.Date(data.MinDateUtc);
        MaxDateUtc = ShipStaticDataFormat.Date(data.MaxDateUtc);
        HasDateRangeWarning = ShipStaticDataFormat.HasInvalidRange(data.MinDateUtc, data.MaxDateUtc);
    }

    [RelayCommand]
    private void Export()
    {
        if (_staticData is null) return;
        ShipStaticDataExportHelper.TryExport(_staticData);
    }

    private void Clear()
    {
        Mmsi = Name = Length = Beam = Draught = CallSign = ImoNumber = MinDateUtc = MaxDateUtc = null;
        HasDateRangeWarning = false;
    }
}
```

### Formatting Utilities (`ShipStaticDataFormat.cs`)
```csharp
namespace AisToXmlRouteConvertor.Mappers;

public static class ShipStaticDataFormat
{
    public static string Display(string? value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value!;
    public static string Meters(double? value) => value is null ? "N/A" : $"{value:0.##} m";
    public static string Date(DateTime? dt) => dt is null ? "N/A" : dt.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
    public static bool HasInvalidRange(DateTime? min, DateTime? max) => min.HasValue && max.HasValue && min > max;
}
```

### Export Helper (`ShipStaticDataExportHelper.cs`)
```csharp
using System.Text.Json;
using AisToXmlRouteConvertor.Models;

namespace AisToXmlRouteConvertor.Services;

public static class ShipStaticDataExportHelper
{
    public static void TryExport(ShipStaticData data)
    {
        // 1. Prompt user for save location (SaveFileDialog inside MainWindow or injected delegate).
        var targetPath = PromptForPath(data.Mmsi);
        if (string.IsNullOrWhiteSpace(targetPath)) return;
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var temp = targetPath + ".tmp";
        File.WriteAllText(temp, json);
        File.Copy(temp, targetPath, overwrite: true);
        File.Delete(temp);
        // Logging + status update handled externally.
    }

    private static string PromptForPath(long mmsi)
    {
        // Placeholder for dialog interaction (not implemented in design spec).
        // Return synthesized path or integrate with UI later.
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{mmsi}_static.json");
    }
}
```

### Panel XAML Snippet (`ShipStaticDataPanel.axaml`)
```xml
<UserControl x:Class="AisToXmlRouteConvertor.Views.ShipStaticDataPanel"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Name="ShipStaticPanel"
             data-test="ship-static-panel">
  <StackPanel Margin="8" Spacing="4">
    <TextBlock Text="Ship Static Data" FontWeight="Bold" />
    <ItemsControl>
      <ItemsControl.Resources>
        <DataTemplate x:Key="FieldTemplate">
          <DockPanel>
            <TextBlock Width="140" Text="{Binding Label}" FontWeight="SemiBold" data-test="ship-static-label-{Binding Key}" />
            <TextBlock Text="{Binding Value}" data-test="ship-static-value-{Binding Key}" />
          </DockPanel>
        </DataTemplate>
      </ItemsControl.Resources>
      <ItemsControl.Items>
        <!-- Bound through code-behind or generated collection; design-level placeholder -->
      </ItemsControl.Items>
    </ItemsControl>
    <Button Content="Export JSON"
            Command="{Binding ExportCommand}"
            IsEnabled="{Binding IsDataAvailable}"
            data-test="ship-static-export" />
    <Border Background="#FFEFD5" Padding="4" IsVisible="{Binding HasDateRangeWarning}" data-test="ship-static-date-warning">
      <TextBlock Text="Warning: MinDateUtc is after MaxDateUtc" Foreground="DarkRed" />
    </Border>
    <TextBlock Text="Select a ship to view static data" IsVisible="{Binding IsDataAvailable, Converter={StaticResource InvertBool}}" />
  </StackPanel>
</UserControl>
```

Testing Hooks:
- Each value field given deterministic selector pattern.
- Warning border has dedicated selector for negative scenario validation.
- Export button state validated through `IsEnabled` attribute.

## Testing Strategy and Quality Assurance
Layers:
- Unit Tests: Formatting & export helper behaviors (null handling, date formatting, invalid date range detection, atomic write).
- Integration: PanelViewModel update when MainViewModel changes selection; verify all properties populated for mock record and cleared when selection cleared.
- E2E (Playwright):
  1. Select ship → assert panel appears & fields populated.
  2. Attempt field edit (no editable control present) → assert failure / absence of TextBox.
  3. Click Export JSON → verify file existence (test harness uses temporary folder override).
  4. Ship with missing optional fields → verify "N/A" displays.
  5. Invalid date range mock → panel shows warning element.

Positive Scenarios Covered:
- Full static data display.
- Successful export to chosen path.

Negative / Edge Scenarios:
- Read-only enforcement (no focusable editable fields).
- Missing fields show "N/A".
- Invalid date range triggers warning.
- Export with no data (button disabled, no action).

Selectors & Assertions:
- `ship-static-value-mmsi` text equals selected MMSI.
- `ship-static-export` enabled only when data loaded.
- Warning presence toggled by invalid range fixture.

Mock Data Requirements (centralized approach per `QA_testing.md`):
- Extend `src/mocks/mockData.ts` with:
  - `export function getMockShipStaticData(overrides?: Partial<ShipStaticDataJson>): ShipStaticDataJson` returning a JSON-like object.
  - Predefined constants: `mockShipStaticFull`, `mockShipStaticMissingFields`, `mockShipStaticInvalidRange`.
- Fixtures in tests import from centralized mocks rather than duplicating JSON strings.
- Helper in test project `TestDataBuilder.cs` (C#) to construct `ShipStaticData` from JSON stub for unit tests.
- Ensure consistent timestamp formatting in mocks for deterministic output.

Data Exposure for Tests:
- ViewModel public properties directly assertable (no private transformation).
- Export helper returns path (could be refactored to return for assertion—design notes future improvement).
- Logging captured via test logger (if integrated) for export success messages.

Accessibility Testing:
- Verify screen reader can enumerate fields (TextBlocks inside structured container).
- Ensure Export button has accessible name matching visible text.
- Warning region has role=alert (can be added in implementation) for proper announcement.

## Mock Data Requirements
Centralized Mock Objects (TypeScript example additions):
```typescript
// src/mocks/mockData.ts
export interface ShipStaticDataJson {
  Mmsi: number; Name?: string; Length?: number; Beam?: number; Draught?: number;
  CallSign?: string; ImoNumber?: string; MinDateUtc?: string; MaxDateUtc?: string;
}

export const mockShipStaticFull: ShipStaticDataJson = {
  Mmsi: 205196000,
  Name: "Alice's Container Ship",
  Length: 285.0,
  Beam: 40.0,
  Draught: 14.5,
  CallSign: "ONBZ",
  ImoNumber: "IMO9234567",
  MinDateUtc: "2025-03-10T00:00:00Z",
  MaxDateUtc: "2025-03-20T23:59:59Z"
};

export const mockShipStaticMissingFields: ShipStaticDataJson = {
  Mmsi: 205196000,
  Name: "Test Vessel"
};

export const mockShipStaticInvalidRange: ShipStaticDataJson = {
  Mmsi: 205196000,
  MinDateUtc: "2025-03-21T00:00:00Z",
  MaxDateUtc: "2025-03-20T00:00:00Z"
};

export function getMockShipStaticData(overrides: Partial<ShipStaticDataJson> = {}): ShipStaticDataJson {
  return { ...mockShipStaticFull, ...overrides };
}
```
Usage:
- E2E tests import mocks → drive UI selection sequence → assert panel render.
- Unit tests deserialize JSON mocks to `ShipStaticData` to exercise formatting logic.

## Design Validation Checklist
- ✓ Uses MVVM with ViewModel & static helper separation.
- ✓ No mutable editing controls present.
- ✓ Export operation isolated & testable.
- ✓ Pure formatting functions unit-tested.
- ✓ Centralized mock data strategy integrated.
- ✓ Data-test selectors comprehensive & deterministic.
- ✓ Null handling uniform ("N/A").
- ✓ Warning mechanism for invalid date range.

## Trade-offs & Rationale
- Separate formatting utility vs inline ViewModel logic: Improves test isolation and keeps ViewModel lean.
- Static export helper (rather than service DI): Aligns with simplicity principle in overall architecture; reduces boilerplate.
- No clipboard/copy enhancements initially: Avoids scope creep; can be added later without refactoring core design.
- Atomic export write (temp file pattern): Slight complexity increase but prevents partial/ corrupt files on failure.

## Future Extensions
- Add copy-to-clipboard icon per field.
- Add collapsible advanced metadata section if schema extended.
- Integrate JSON schema validation before export (ensure consistency post-transformation).
- Provide alternative export formats (YAML, plain text summary).
- Add diff view when selecting a new ship vs prior selection (history panel).

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Users expect editable fields | Clear label "Read-Only" and tooltip clarifications |
| Export path overwrite issues | Future confirmation dialog enhancement |
| Large numeric precision confusion | Format to 2 decimal places with unit suffix |
| Invalid date range causes downstream confusion | Early visual warning indicator & test coverage |
| Missing JSON returns null silently | Display explicit placeholder "Static data unavailable" |

---
This design adheres to architectural standards emphasizing simplicity, immutability, and testability while enabling comprehensive, read-only vessel metadata visualization and export.
