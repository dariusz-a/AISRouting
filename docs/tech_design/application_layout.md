# Application Layout: UI hierarchy, layout structure, navigation flow, responsive design, reusable components

This document covers the WPF UI composition for AISRouting: component hierarchy, layout containers, navigation flow (single-window interactions), styling and accessibility considerations, happy path & edge cases. Cross?references application_organization.md and overall_architecture.md.

## 1. UI Paradigm
Single primary window (MainWindow) with stacked functional sections. No multi-page navigation in MVP; state transitions occur via visibility and data-bound collections. Future expansion could add tabbed or dialog-based views.

## 2. Hierarchy Overview
MainWindow
- HeaderBar (Title + app version)
- InputFolderSection
  - FolderPathTextBox (readonly)
  - SelectInputFolderButton
  - VesselCountLabel
- VesselSelectionSection
  - VesselComboBox (ItemsSource: ObservableCollection<ShipStaticData>)
  - StaticDataDisplay (ScrollViewer + TextBlock)
  - DateRangePanel (MinDateLabel, MaxDateLabel)
- IntervalSelectionSection
  - StartDateTimePicker
  - StopDateTimePicker
  - ValidationMessageTextBlock
- TrackActionsSection
  - CreateTrackButton
  - CancelTrackButton
  - ProgressBar
- WaypointResultsSection
  - WaypointsDataGrid
  - StatisticsPanel (#input, #retained, reduction %)
- ExportSection
  - OutputFolderPathTextBox
  - SelectOutputFolderButton
  - ExportButton
  - ExportStatusTextBlock
- StatusBar
  - GlobalStatusMessage
  - LoggerViewToggle (optional future)

## 3. Layout Structure
Overall container: Grid with row definitions per major section. Use shared styles for spacing (Uniform margin thickness 8). DataGrid uses virtualization for performance with large waypoint sets.
Responsive Behavior (Desktop):
- Window resizable; ScrollViewer wraps WaypointResultsSection for small screens.
- Minimum window size enforced (e.g., 1024x640) for readability.
- Dynamic column widths: DataGrid auto-size, but lat/lon columns fixed width.

## 4. Styling & Resources
Resources/Styles.xaml defines:
- ButtonStyle (consistent padding, focus visual).
- LabelStyle (FontWeight=SemiBold).
- ErrorTextStyle (Foreground=Red).
- NeutralTextStyle (Foreground=Gray).
Theme considerations: Light theme default; future dark theme extension via ResourceDictionary override.

## 5. Accessibility Considerations
- Keyboard navigation: Tab order logical top-to-bottom.
- Automation Properties: x:Name + AutomationProperties.Name on interactive elements (e.g., SelectInputFolderButton). Enables UI automation (FlaUI).
- High Contrast compatibility tested by system theme switching.

## 6. Navigation Flow
Sequence (Happy Path):
1. User clicks SelectInputFolderButton ? Dialog returns path ? FolderPathTextBox updated ? Vessel list populates.
2. User selects vessel in VesselComboBox ? StaticDataDisplay updates, date range shown.
3. User sets Start/Stop in DateTimePickers ? Live validation message clears if valid.
4. User clicks CreateTrackButton ? ProgressBar active; WaypointsDataGrid populates when complete.
5. User reviews waypoints ? Clicks SelectOutputFolderButton then ExportButton ? ExportStatusTextBlock shows success.

Edge Cases:
- Input folder invalid ? VesselComboBox disabled; ErrorTextStyle message displayed.
- CreateTrackButton disabled if interval invalid or no vessel selected.
- CancelTrackButton enabled during optimization; on cancel resets ProgressBar to 0 and clears WaypointsDataGrid.
- ExportButton disabled until waypoints exist + valid output path.

## 7. Reusable Components
Potential user controls for encapsulation:
- FolderPickerControl (Label + TextBox + Button) used for Input and Output paths.
- DateTimeIntervalControl (Start/Stop pickers + validation message).
- StatisticsPanelControl (InputCount, OutputCount, ReductionPercent bindings).
Benefits: Simplified MainWindow XAML; easier future reuse if multi-window introduced.

## 8. Data Binding Strategy
DataContext: MainWindowViewModel, which exposes nested observable properties or lightweight child ViewModels (e.g., IntervalViewModel). Two-way binding on StartUtc and StopUtc. Commands: CreateTrackCommand, CancelTrackCommand, ExportRouteCommand, SelectInputFolderCommand, SelectOutputFolderCommand.

## 9. Error & Status Messaging
Priority order: Critical error (red) > warning (orange) > info (gray). ExportStatusTextBlock resets on new optimization. GlobalStatusMessage displays ephemeral notifications (e.g., “Optimization cancelled.”).

## 10. Example Data Visualization
WaypointResultsSection DataGrid columns:
- Index (#)
- Lat
- Lon
- Speed (knots)
- Heading (deg)
- ETA (seconds)
- ROT (deg/s) (optional debug column hidden by default)
Context menu (future): Copy waypoint row, Export selected subset.

## 11. Performance UI Considerations
- VirtualizingStackPanel for DataGrid ItemsPanel.
- Bind ProgressBar to OptimizationProgress.PercentComplete.
- Disable layout-heavy updates (e.g., statistics recalculation) until optimization completes.

## 12. Edge Scenario Narratives
Scenario: Alice chooses wide interval (2 days) generating 20k input points; DataGrid remains responsive due to virtualization; reduction percent displayed (e.g., 85%).
Scenario: Bob cancels mid-way; partial waypoints discarded; GlobalStatusMessage: “Track generation cancelled by user.”
Scenario: Empty optimization result (e.g., identical points) ? DataGrid shows minimal rows; ExportButton still enabled if >0.

## 13. Future Enhancements
- MapPreviewControl: Show polyline track with waypoints; color-coded significant deviations.
- ThresholdAdjustPanel: UI sliders for heading/ROT/distance thresholds; triggers re-optimization.
- Multi-Tab UI: Tab per vessel for batch operations.

## 14. Cross-References
- application_organization.md (ViewModels & folder patterns)
- overall_architecture.md (Optimization workflow)
- data_models.md (Waypoint fields) 
- security_architecture.md (Validation messaging) 
