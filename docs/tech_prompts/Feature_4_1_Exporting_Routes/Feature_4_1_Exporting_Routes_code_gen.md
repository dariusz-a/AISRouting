# Working Code Generation Prompt: Feature 4.1: Exporting Routes

## Task: 
Generate working code for Feature 4.1: Exporting Routes, following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using C# and .NET 8 WPF.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing accessible, robust, and comprehensive tests following best practices.
- Ensuring all code aligns with project technical constraints, including MVVM architecture, dependency injection, and file-based storage.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in WPF, XAML, data binding, and robust desktop application engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.

## References
- BDD Scenarios: docs/spec_scenarios/export_route.md
- Test File: tests/export_route.spec.ts
- Feature Design Document: docs/tech_design/api_integration_patterns.md
- Application Architecture: docs/tech_design/overall_architecture.md
- Application Organization: docs/tech_design/application_organization.md
- Data Models: docs/tech_design/data_models.md
- Application Layout: docs/tech_design/application_layout.md

## Development Approach

This feature implements the route export functionality for the AISRouting application. It builds upon the track generation capabilities from Feature 3.1 and enables users to export optimized waypoints to XML format compatible with navigation systems.

The implementation follows a layered architecture:
- **Presentation Layer**: ViewModel commands and UI controls for export operations
- **Business Logic Layer**: RouteExporter service interface in Core
- **Data Access Layer**: XML generation and file I/O in Infrastructure

All file operations must be asynchronous to maintain UI responsiveness. The export process includes:
1. User selects output folder via dialog
2. User clicks Export button
3. System checks for file conflicts and prompts user (overwrite, append suffix, or cancel)
4. System generates XML with RouteTemplate structure and WayPoint elements
5. System writes XML to disk and displays success/error message

## Implementation Plan

### Scenario: Export generated track to XML with valid output path

**BDD Scenario:**
```gherkin
Given a generated track exists for ship "<mmsi>" and the user "<user_id>" is logged in.
When the user clicks the Export button, selects output folder "<output_path>" and confirms the export.
Then a file named "<mmsi>-<start>-<end>.xml" should be created at "<output_path>" containing a `<RouteTemplates>` root with a single `<RouteTemplate Name="<mmsi>">` element containing an ordered list of `<WayPoint/>` elements.

Examples:
| mmsi | start | end | user_id | output_path |
| mmsi-1 | ts_first | ts_last | scenario-user | export_tmp |
```

**Technical Design Details:**
- Service: `IRouteExporter` interface in `AISRouting.Core/Services/Interfaces/IRouteExporter.cs`
- Implementation: `RouteExporter` class in `AISRouting.Infrastructure/Persistence/RouteExporter.cs`
- ViewModel: `MainViewModel` in `AISRouting.App.WPF/ViewModels/MainViewModel.cs` (add export command and properties)
- XML structure: `<RouteTemplates>` root with `<RouteTemplate>` containing ordered `<WayPoint>` elements
- Filename format: `{MMSI}-{StartDateTime:yyyyMMddTHHmmss}-{EndDateTime:yyyyMMddTHHmmss}.xml`
- Each WayPoint has attributes: Name, Lat, Lon, Alt, Speed, ETA, Delay, Mode, TrackMode, Heading, PortXTE, StbdXTE, MinSpeed, MaxSpeed
- Mapping from RouteWaypoint model to XML attributes follows data_models.md specifications

**Tasks:**

1. Create `IRouteExporter` interface in `AISRouting.Core/Services/Interfaces/IRouteExporter.cs` with method signature:
   ```csharp
   Task ExportRouteAsync(
       IEnumerable<RouteWaypoint> waypoints,
       string outputFilePath,
       ExportOptions options = null,
       CancellationToken cancellationToken = default);
   ```

2. Create `ExportOptions` class in `AISRouting.Core/Models/ExportOptions.cs` to hold export configuration (e.g., conflict resolution strategy, formatting options)

3. Create `RouteExporter` class in `AISRouting.Infrastructure/Persistence/RouteExporter.cs` implementing `IRouteExporter` with:
   - Constructor injecting `ILogger<RouteExporter>` and `IPathValidator`
   - `ExportRouteAsync` method that validates inputs (waypoints not empty, output path valid)
   - Method to generate filename from MMSI and time range: `{MMSI}-{Start:yyyyMMddTHHmmss}-{End:yyyyMMddTHHmmss}.xml`
   - Private method `GenerateRouteXml(List<RouteWaypoint> waypoints)` that creates XDocument with structure:
     ```xml
     <?xml version="1.0" encoding="utf-8"?>
     <RouteTemplates>
       <RouteTemplate Name="{MMSI}" ColorR="1" ColorG="124" ColorB="139">
         <WayPoint Name="{MMSI}" Lat="{Latitude}" Lon="{Longitude}" Alt="0" ... />
         ...
       </RouteTemplate>
     </RouteTemplates>
     ```
   - Each `<WayPoint>` element must include all attributes per data_models.md RouteWaypoint mapping

4. Add mapping logic in `GenerateRouteXml` to convert `RouteWaypoint` properties to XML attributes:
   - Name ← MMSI (from first waypoint)
   - Lat ← Latitude (formatted as "F6")
   - Lon ← Longitude (formatted as "F6")
   - Alt ← 0 (fixed value)
   - Speed ← SOG or 0 if null (formatted as "F2")
   - ETA ← EtaSecondsUntil or 0 if null
   - Delay ← 0 (fixed value)
   - Mode ← Result of SetWaypointMode() (to be implemented based on TODO in specs)
   - TrackMode ← "Track" (fixed value)
   - Heading ← Heading or 0 if null
   - PortXTE ← 20 (fixed value)
   - StbdXTE ← 20 (fixed value)
   - MinSpeed ← 0 (fixed value)
   - MaxSpeed ← maximum SOG across all waypoints in route (ignoring zeros), formatted as "F2"

5. Add `MaxSpeed` calculation logic in `RouteExporter.ExportRouteAsync`:
   - Before generating XML, compute `maxSpeed = waypoints.Where(w => w.Speed > 0).Max(w => w.Speed) ?? 0`
   - Store this value and use it for all WayPoint elements' MaxSpeed attribute

6. Add write operation in `RouteExporter.ExportRouteAsync`:
   - Use `await File.WriteAllTextAsync(outputFilePath, xml.ToString(), cancellationToken)`
   - Log success with waypoint count and output path

7. Add `OutputFolderPath` property to `MainViewModel` (string, initially null or empty)

8. Add `SelectOutputFolderCommand` to `MainViewModel`:
   - Use `IFolderDialogService.ShowFolderBrowserDialog()` to get folder path
   - Set `OutputFolderPath` property
   - Raise PropertyChanged notification

9. Add `ExportRouteCommand` to `MainViewModel`:
   - Check `CanExportRoute()`: returns true if `GeneratedWaypoints` has items and `OutputFolderPath` is not empty
   - In `ExecuteExportRouteAsync()`:
     - Construct output file path: `Path.Combine(OutputFolderPath, GenerateFilename())`
     - Call `_routeExporter.ExportRouteAsync(GeneratedWaypoints, outputFilePath, cancellationToken)`
     - Update `StatusMessage` to "Export successful" on success
     - Handle exceptions and update `StatusMessage` with error details

10. Add private method `GenerateFilename()` in `MainViewModel`:
    - Extract MMSI from first waypoint: `var mmsi = GeneratedWaypoints.First().Name`
    - Extract start time from first waypoint: `var start = GeneratedWaypoints.First().Time`
    - Extract end time from last waypoint: `var end = GeneratedWaypoints.Last().Time`
    - Return formatted string: `$"{mmsi}-{start:yyyyMMddTHHmmss}-{end:yyyyMMddTHHmmss}.xml"`

11. Update `MainWindow.xaml` to add output folder selection UI in Track Results panel:
    - Add Label "Output Folder:"
    - Add TextBox bound to `{Binding OutputFolderPath}` (IsReadOnly="True")
    - Add Button "Browse..." with Command `{Binding SelectOutputFolderCommand}`
    - Existing "Export to XML" button should be bound to `{Binding ExportRouteCommand}`

12. Register `IRouteExporter` and `RouteExporter` in DI container in `App.xaml.cs`:
    - Add `services.AddSingleton<IRouteExporter, RouteExporter>();`

13. Inject `IRouteExporter` into `MainViewModel` constructor and store in private field `_routeExporter`

14. Add `IPathValidator` to validate output file paths (if not already exists in Infrastructure layer)

15. Create unit tests in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
    - Test `ExportRouteAsync_WithValidWaypoints_CreatesXmlFile`
    - Test `ExportRouteAsync_WithEmptyWaypoints_ThrowsArgumentException`
    - Test `ExportRouteAsync_WithInvalidOutputPath_ThrowsException`

16. Create integration test in `AISRouting.Tests/IntegrationTests/XmlExportValidationTests.cs`:
    - Test end-to-end export with real RouteWaypoint data
    - Verify generated XML structure and attributes match expected format
    - Verify filename format matches specification

---

### Scenario: Prompt on filename conflict and overwrite chosen

**BDD Scenario:**
```gherkin
Given a generated track exists for ship "205196000" and an export file named "205196000-20250315T000000-20250316T000000.xml" already exists in "C:\\tmp\\exports" and the user "scenario-user" is logged in.
When the user initiates export and chooses the "Overwrite" option in the conflict prompt.
Then the existing file is replaced with the new XML and a confirmation message with text "Export successful" is shown.
```

**Technical Design Details:**
- Service: `IFileConflictDialogService` interface in `AISRouting.Core/Services/Interfaces/IFileConflictDialogService.cs`
- Implementation: `FileConflictDialogService` in `AISRouting.App.WPF/Services/FileConflictDialogService.cs`
- Enum: `FileConflictResolution` with values: `Overwrite`, `AppendSuffix`, `Cancel`
- Integration: `RouteExporter.ExportRouteAsync` checks if file exists, calls dialog service if conflict detected
- Dialog shows message: "File {filename} already exists. Choose an option:" with three buttons
- If user chooses Overwrite, proceed with existing path
- If user chooses AppendSuffix, generate unique filename (handled in separate scenario)
- If user chooses Cancel, throw `OperationCanceledException`

**Tasks:**

1. Create `FileConflictResolution` enum in `AISRouting.Core/Models/FileConflictResolution.cs`:
   ```csharp
   public enum FileConflictResolution
   {
       Cancel,
       Overwrite,
       AppendSuffix
   }
   ```

2. Create `IFileConflictDialogService` interface in `AISRouting.Core/Services/Interfaces/IFileConflictDialogService.cs`:
   ```csharp
   public interface IFileConflictDialogService
   {
       FileConflictResolution ShowFileConflictDialog(string filePath);
   }
   ```

3. Create `FileConflictDialogService` class in `AISRouting.App.WPF/Services/FileConflictDialogService.cs` implementing `IFileConflictDialogService`:
   - Show WPF dialog with message: "File {Path.GetFileName(filePath)} already exists. Choose an option:"
   - Three buttons: "Overwrite", "Append Suffix", "Cancel"
   - Return corresponding `FileConflictResolution` enum value based on user selection

4. Update `RouteExporter.ExportRouteAsync` to check for file conflicts before writing:
   - Add check: `if (File.Exists(outputFilePath))`
   - Inject `IFileConflictDialogService` in constructor
   - Call `var resolution = _conflictDialog.ShowFileConflictDialog(outputFilePath)`
   - Handle resolution:
     - `Cancel`: throw `OperationCanceledException("Export cancelled by user")`
     - `Overwrite`: proceed with existing path (log warning about overwrite)
     - `AppendSuffix`: call `GenerateUniquePath(outputFilePath)` and use new path

5. Add logging in `RouteExporter` when file conflict is detected:
   - Log warning: "File conflict detected for {outputFilePath}, prompting user"

6. Add logging in `RouteExporter` when overwrite is chosen:
   - Log info: "User chose to overwrite existing file {outputFilePath}"

7. Update `MainViewModel.ExecuteExportRouteAsync()` to handle `OperationCanceledException`:
   - Catch specifically and set `StatusMessage = "Export cancelled"`
   - Do not log as error (user cancellation is normal flow)

8. Register `IFileConflictDialogService` in DI container in `App.xaml.cs`:
   - Add `services.AddSingleton<IFileConflictDialogService, FileConflictDialogService>();`

9. Update `RouteExporter` constructor to inject `IFileConflictDialogService` and store in private field `_conflictDialog`

10. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
    - Test `ExportRouteAsync_WithExistingFile_AndOverwriteChosen_OverwritesFile`
    - Mock `IFileConflictDialogService` to return `FileConflictResolution.Overwrite`
    - Verify file is written and success is logged

11. Create unit test in `AISRouting.Tests/UnitTests/ViewModels/MainViewModelTests.cs`:
    - Test `ExportRouteCommand_WithUserCancellation_SetsStatusMessageToCancelled`
    - Mock `IRouteExporter` to throw `OperationCanceledException`
    - Verify `StatusMessage` is set to "Export cancelled" and no error is logged

---

### Scenario: Fail export when output path not writable

**BDD Scenario:**
```gherkin
Given a generated track exists for ship "205196000" and the user "scenario-user" selects an output folder "C:\\protected\\exports" which is not writable.
When the user confirms export.
Then a visible error banner with text "Cannot write to output path: C:\\protected\\exports" is displayed and no file is created.
```

**Technical Design Details:**
- Validation: `IPathValidator.ValidateOutputFilePath(string path)` checks write permissions
- Exception: Throw `UnauthorizedAccessException` if path not writable
- ViewModel catches exception and displays error in `StatusMessage`
- Error message format: "Cannot write to output path: {path}"
- No file should be created if validation fails

**Tasks:**

1. Add method `ValidateOutputFilePath(string path)` to `IPathValidator` interface (if not already exists):
   - Signature: `void ValidateOutputFilePath(string path)`
   - Should throw `ArgumentException` if path is null or empty
   - Should throw `DirectoryNotFoundException` if directory does not exist
   - Should throw `UnauthorizedAccessException` if directory is not writable

2. Implement `ValidateOutputFilePath` in `PathValidator` class in `AISRouting.Infrastructure/Validation/PathValidator.cs`:
   - Check if path is null or whitespace: throw `ArgumentException("Output path cannot be empty")`
   - Check if directory exists: if not, throw `DirectoryNotFoundException($"Output directory not found: {path}")`
   - Check write permissions by attempting to create a temporary file:
     ```csharp
     var testFile = Path.Combine(path, $"_write_test_{Guid.NewGuid()}.tmp");
     try
     {
         File.WriteAllText(testFile, "test");
         File.Delete(testFile);
     }
     catch (UnauthorizedAccessException)
     {
         throw new UnauthorizedAccessException($"Cannot write to output path: {path}");
     }
     ```

3. Call `_pathValidator.ValidateOutputFilePath(outputFilePath)` in `RouteExporter.ExportRouteAsync` before checking file conflicts:
   - Place this validation at the start of the method
   - Let exceptions propagate to ViewModel

4. Update `MainViewModel.ExecuteExportRouteAsync()` to catch and handle validation exceptions:
   - Add catch block for `UnauthorizedAccessException`:
     ```csharp
     catch (UnauthorizedAccessException ex)
     {
         StatusMessage = $"Cannot write to output path: {OutputFolderPath}";
         _logger.LogError(ex, "Access denied to output path");
     }
     ```
   - Add catch block for `DirectoryNotFoundException`:
     ```csharp
     catch (DirectoryNotFoundException ex)
     {
         StatusMessage = $"Output folder not found: {OutputFolderPath}";
         _logger.LogError(ex, "Output directory not found");
     }
     ```

5. Add logging in `PathValidator.ValidateOutputFilePath` when validation fails:
   - Log error with details about which validation failed

6. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/PathValidatorTests.cs`:
   - Test `ValidateOutputFilePath_WithNullPath_ThrowsArgumentException`
   - Test `ValidateOutputFilePath_WithNonExistentDirectory_ThrowsDirectoryNotFoundException`
   - Test `ValidateOutputFilePath_WithReadOnlyDirectory_ThrowsUnauthorizedAccessException`

7. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
   - Test `ExportRouteAsync_WithUnwritablePath_ThrowsUnauthorizedAccessException`
   - Mock `IPathValidator` to throw `UnauthorizedAccessException`
   - Verify no file is created

8. Create unit test in `AISRouting.Tests/UnitTests/ViewModels/MainViewModelTests.cs`:
   - Test `ExportRouteCommand_WithUnwritablePath_DisplaysErrorMessage`
   - Mock `IRouteExporter` to throw `UnauthorizedAccessException`
   - Verify `StatusMessage` contains "Cannot write to output path"

---

### Scenario: Append numeric suffix on filename conflict

**BDD Scenario:**
```gherkin
Given a generated track exists and target filename "205196000-20250315T000000-20250316T000000.xml" already exists in "C:\\tmp\\exports" and the user "scenario-user" selects "Append numeric suffix" in the prompt.
When the user confirms export.
Then the application creates a new file such as "205196000-20250315T000000-20250316T000000 (1).xml" and no existing file is overwritten and a success message is shown.
```

**Technical Design Details:**
- Method: `GenerateUniquePath(string originalPath)` in `RouteExporter` class
- Algorithm: Append " (n)" before file extension, incrementing n until unique filename found
- Format: `{baseFilename} (1).xml`, `{baseFilename} (2).xml`, etc.
- Integration: Called from `RouteExporter.ExportRouteAsync` when user selects `AppendSuffix` resolution
- Success message should include the actual filename used

**Tasks:**

1. Add private method `GenerateUniquePath(string originalPath)` to `RouteExporter` class:
   ```csharp
   private string GenerateUniquePath(string originalPath)
   {
       var dir = Path.GetDirectoryName(originalPath);
       var filenameNoExt = Path.GetFileNameWithoutExtension(originalPath);
       var ext = Path.GetExtension(originalPath);
       
       int suffix = 1;
       string newPath;
       
       do
       {
           newPath = Path.Combine(dir, $"{filenameNoExt} ({suffix}){ext}");
           suffix++;
       }
       while (File.Exists(newPath));
       
       return newPath;
   }
   ```

2. Update `RouteExporter.ExportRouteAsync` to handle `AppendSuffix` resolution:
   - In the file conflict handling switch statement, add case for `AppendSuffix`:
     ```csharp
     case FileConflictResolution.AppendSuffix:
         outputFilePath = GenerateUniquePath(outputFilePath);
         _logger.LogInformation("Generated unique filename: {Path}", outputFilePath);
         break;
     ```

3. Update `MainViewModel.ExecuteExportRouteAsync()` to display the actual filename in success message:
   - Extract filename from path: `var filename = Path.GetFileName(outputFilePath)`
   - Update success message: `StatusMessage = $"Export successful: {filename}"`

4. Add logging in `GenerateUniquePath` when unique filename is generated:
   - Log info: "Generating unique filename, original: {originalPath}, new: {newPath}"

5. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
   - Test `GenerateUniquePath_WithExistingFile_ReturnsUniqueFilename`
   - Create test file, call method, verify format is "{base} (1).xml"

6. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
   - Test `GenerateUniquePath_WithMultipleConflicts_IncrementsCorrectly`
   - Create multiple test files with suffixes, verify next suffix is correct

7. Create integration test in `AISRouting.Tests/IntegrationTests/XmlExportValidationTests.cs`:
   - Test `ExportRouteAsync_WithAppendSuffix_CreatesUniqueFile`
   - Create initial file, mock dialog to return `AppendSuffix`, export again
   - Verify two files exist and neither is overwritten

8. Update unit test in `AISRouting.Tests/UnitTests/ViewModels/MainViewModelTests.cs`:
   - Test `ExportRouteCommand_WithAppendSuffix_DisplaysSuccessWithFilename`
   - Verify success message includes the modified filename

---

### Scenario: Export WayPoint attribute mapping

**BDD Scenario:**
```gherkin
Given a generated track for ship "205196000" contains AIS records with sample values and the user "scenario-user" initiates export to "C:\\tmp\\exports".
When the export completes and the XML is opened.
Then each `<WayPoint>` element includes attributes mapped as: Name=MMSI, Lat=CSV latitude, Lon=CSV longitude, Alt=0, Speed=SOG, ETA=EtaSecondsUntil or 0, Delay=0, Mode=computed via SetWaypointMode (TODO), TrackMode="Track", Heading=Heading or 0, PortXTE=20, StbdXTE=20, MinSpeed=0, MaxSpeed=maximum SOG observed in range.
```

**Technical Design Details:**
- Attribute mapping defined in data_models.md under RouteWaypoint section
- Mode calculation: Implement `SetWaypointMode()` logic (spec indicates TODO, use placeholder or simple logic for now)
- MaxSpeed: Must be computed across all waypoints before XML generation
- Formatting: Lat/Lon use "F6" format, Speed/MaxSpeed use "F2" format, integers as-is
- All fixed values (Alt=0, Delay=0, PortXTE=20, StbdXTE=20, MinSpeed=0, TrackMode="Track") must be included
- Ensure nullable values default correctly (Speed → 0 if null, Heading → 0 if null, ETA → 0 if null)

**Tasks:**

1. Add `Mode` property calculation logic in `RouteWaypoint` model or in `RouteExporter`:
   - For now, implement simple logic: Mode = "Waypoint" (placeholder until SetWaypointMode algorithm is specified)
   - Add TODO comment indicating this needs proper implementation per navigation requirements

2. Update `GenerateRouteXml` method in `RouteExporter` to compute MaxSpeed before generating XML:
   - Calculate: `var maxSpeed = waypoints.Where(w => w.Speed > 0).Max(w => w.Speed) ?? 0;`
   - Use this value for all WayPoint elements' MaxSpeed attribute

3. Ensure all attribute mappings in `GenerateRouteXml` use correct formatting and defaults:
   - Name: `wp.Name` (MMSI as string)
   - Lat: `wp.Lat.ToString("F6", CultureInfo.InvariantCulture)`
   - Lon: `wp.Lon.ToString("F6", CultureInfo.InvariantCulture)`
   - Alt: `wp.Alt` (always 0)
   - Speed: `(wp.Speed ?? 0).ToString("F2", CultureInfo.InvariantCulture)`
   - ETA: `wp.ETA ?? 0`
   - Delay: `wp.Delay` (always 0)
   - Mode: `wp.Mode ?? "Waypoint"`
   - TrackMode: `"Track"` (literal string)
   - Heading: `wp.Heading ?? 0`
   - PortXTE: `wp.PortXTE` (always 20)
   - StbdXTE: `wp.StbdXTE` (always 20)
   - MinSpeed: `wp.MinSpeed` (always 0)
   - MaxSpeed: `maxSpeed.ToString("F2", CultureInfo.InvariantCulture)` (computed value)

4. Add RouteTemplate attributes in `GenerateRouteXml`:
   - Name: `mmsi` (from first waypoint)
   - ColorR: `"1"` (fixed)
   - ColorG: `"124"` (fixed)
   - ColorB: `"139"` (fixed)

5. Ensure XML declaration is included:
   - `new XDeclaration("1.0", "utf-8", null)`

6. Ensure all numeric formatting uses `CultureInfo.InvariantCulture` to prevent locale-specific decimal separators

7. Create unit test in `AISRouting.Tests/UnitTests/Infrastructure/RouteExporterTests.cs`:
   - Test `GenerateRouteXml_WithSampleWaypoints_MapsAllAttributesCorrectly`
   - Create test waypoints with known values (including nulls)
   - Parse generated XML and verify all attributes match expected values and formats

8. Create unit test to verify MaxSpeed calculation:
   - Test `GenerateRouteXml_WithVariousSpeedValues_ComputesMaxSpeedCorrectly`
   - Create waypoints with speeds: 0, 12.5, 15.3, null, 18.7
   - Verify MaxSpeed attribute is "18.70" (formatted as F2)

9. Create unit test to verify null handling:
   - Test `GenerateRouteXml_WithNullValues_UsesDefaultsCorrectly`
   - Create waypoint with null Speed, Heading, ETA
   - Verify XML has Speed="0.00", Heading="0", ETA="0"

10. Create integration test in `AISRouting.Tests/IntegrationTests/XmlExportValidationTests.cs`:
    - Test `ExportedXml_HasValidStructureAndAttributes`
    - Export real track data, load XML, validate structure and all required attributes present

11. Add XML schema validation (optional enhancement):
    - Define XSD schema for RouteTemplates structure
    - Add validation in integration test to ensure exported XML conforms to schema

## Code Examples

### IRouteExporter Interface
```csharp
namespace AISRouting.Core.Services.Interfaces
{
    public interface IRouteExporter
    {
        Task ExportRouteAsync(
            IEnumerable<RouteWaypoint> waypoints,
            string outputFilePath,
            ExportOptions options = null,
            CancellationToken cancellationToken = default);
    }
}
```

### RouteExporter Implementation Skeleton
```csharp
namespace AISRouting.Infrastructure.Persistence
{
    public class RouteExporter : IRouteExporter
    {
        private readonly ILogger<RouteExporter> _logger;
        private readonly IPathValidator _pathValidator;
        private readonly IFileConflictDialogService _conflictDialog;

        public RouteExporter(
            ILogger<RouteExporter> logger,
            IPathValidator pathValidator,
            IFileConflictDialogService conflictDialog)
        {
            _logger = logger;
            _pathValidator = pathValidator;
            _conflictDialog = conflictDialog;
        }

        public async Task ExportRouteAsync(
            IEnumerable<RouteWaypoint> waypoints,
            string outputFilePath,
            ExportOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= ExportOptions.Default;

            // Validate inputs
            _pathValidator.ValidateOutputFilePath(Path.GetDirectoryName(outputFilePath));

            var waypointList = waypoints.ToList();
            if (waypointList.Count == 0)
                throw new ArgumentException("Waypoint list is empty");

            // Handle file conflicts
            if (File.Exists(outputFilePath))
            {
                var resolution = _conflictDialog.ShowFileConflictDialog(outputFilePath);

                switch (resolution)
                {
                    case FileConflictResolution.Cancel:
                        throw new OperationCanceledException("Export cancelled by user");

                    case FileConflictResolution.AppendSuffix:
                        outputFilePath = GenerateUniquePath(outputFilePath);
                        _logger.LogInformation("Generated unique filename: {Path}", outputFilePath);
                        break;

                    case FileConflictResolution.Overwrite:
                        _logger.LogInformation("User chose to overwrite existing file: {Path}", outputFilePath);
                        break;
                }
            }

            // Generate XML
            var xml = GenerateRouteXml(waypointList);

            // Write to file
            await File.WriteAllTextAsync(outputFilePath, xml.ToString(), cancellationToken);

            _logger.LogInformation(
                "Exported {Count} waypoints to {Path}",
                waypointList.Count, outputFilePath);
        }

        private XDocument GenerateRouteXml(List<RouteWaypoint> waypoints)
        {
            var mmsi = waypoints.FirstOrDefault()?.Name ?? "Unknown";
            var maxSpeed = waypoints.Where(w => w.Speed > 0).Max(w => w.Speed) ?? 0;

            var routeTemplate = new XElement("RouteTemplate",
                new XAttribute("Name", mmsi),
                new XAttribute("ColorR", "1"),
                new XAttribute("ColorG", "124"),
                new XAttribute("ColorB", "139"));

            foreach (var wp in waypoints)
            {
                var waypointElement = new XElement("WayPoint",
                    new XAttribute("Name", wp.Name),
                    new XAttribute("Lat", wp.Lat.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("Lon", wp.Lon.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("Alt", wp.Alt),
                    new XAttribute("Speed", (wp.Speed ?? 0).ToString("F2", CultureInfo.InvariantCulture)),
                    new XAttribute("ETA", wp.ETA ?? 0),
                    new XAttribute("Delay", wp.Delay),
                    new XAttribute("Mode", wp.Mode ?? "Waypoint"),
                    new XAttribute("TrackMode", "Track"),
                    new XAttribute("Heading", wp.Heading ?? 0),
                    new XAttribute("PortXTE", wp.PortXTE),
                    new XAttribute("StbdXTE", wp.StbdXTE),
                    new XAttribute("MinSpeed", wp.MinSpeed),
                    new XAttribute("MaxSpeed", maxSpeed.ToString("F2", CultureInfo.InvariantCulture))
                );

                routeTemplate.Add(waypointElement);
            }

            var rootElement = new XElement("RouteTemplates", routeTemplate);
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                rootElement);
        }

        private string GenerateUniquePath(string originalPath)
        {
            var dir = Path.GetDirectoryName(originalPath);
            var filenameNoExt = Path.GetFileNameWithoutExtension(originalPath);
            var ext = Path.GetExtension(originalPath);

            int suffix = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(dir, $"{filenameNoExt} ({suffix}){ext}");
                suffix++;
            }
            while (File.Exists(newPath));

            return newPath;
        }
    }
}
```

### MainViewModel Export Command Implementation
```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IRouteExporter _routeExporter;
    private readonly IFolderDialogService _folderDialog;

    [ObservableProperty]
    private string _outputFolderPath;

    [ObservableProperty]
    private ObservableCollection<RouteWaypoint> _generatedWaypoints = new();

    [RelayCommand]
    private void SelectOutputFolder()
    {
        var path = _folderDialog.ShowFolderBrowserDialog(OutputFolderPath);
        if (!string.IsNullOrEmpty(path))
        {
            OutputFolderPath = path;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExportRoute))]
    private async Task ExportRouteAsync()
    {
        try
        {
            StatusMessage = "Exporting route...";

            var filename = GenerateFilename();
            var outputFilePath = Path.Combine(OutputFolderPath, filename);

            await _routeExporter.ExportRouteAsync(
                GeneratedWaypoints,
                outputFilePath,
                cancellationToken: _cancellationTokenSource.Token);

            var actualFilename = Path.GetFileName(outputFilePath);
            StatusMessage = $"Export successful: {actualFilename}";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Export cancelled";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = $"Cannot write to output path: {OutputFolderPath}";
            _logger.LogError("Access denied to output path: {Path}", OutputFolderPath);
        }
        catch (DirectoryNotFoundException)
        {
            StatusMessage = $"Output folder not found: {OutputFolderPath}";
            _logger.LogError("Output directory not found: {Path}", OutputFolderPath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            _logger.LogError(ex, "Failed to export route");
        }
    }

    private bool CanExportRoute()
    {
        return GeneratedWaypoints?.Count > 0 && !string.IsNullOrWhiteSpace(OutputFolderPath);
    }

    private string GenerateFilename()
    {
        var mmsi = GeneratedWaypoints.First().Name;
        var start = GeneratedWaypoints.First().Time;
        var end = GeneratedWaypoints.Last().Time;
        return $"{mmsi}-{start:yyyyMMddTHHmmss}-{end:yyyyMMddTHHmmss}.xml";
    }
}
```

### FileConflictDialogService Implementation
```csharp
namespace AISRouting.App.WPF.Services
{
    public class FileConflictDialogService : IFileConflictDialogService
    {
        public FileConflictResolution ShowFileConflictDialog(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            var messageBoxText = $"File {filename} already exists. Choose an option:";
            var caption = "File Conflict";

            var dialog = new FileConflictDialog
            {
                Message = messageBoxText,
                Owner = Application.Current.MainWindow
            };

            var result = dialog.ShowDialog();

            return result switch
            {
                true when dialog.SelectedOption == "Overwrite" => FileConflictResolution.Overwrite,
                true when dialog.SelectedOption == "AppendSuffix" => FileConflictResolution.AppendSuffix,
                _ => FileConflictResolution.Cancel
            };
        }
    }
}
```

### PathValidator Output Path Validation
```csharp
public class PathValidator : IPathValidator
{
    private readonly ILogger<PathValidator> _logger;

    public void ValidateOutputFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Output path cannot be empty", nameof(path));
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Output directory not found: {path}");
        }

        // Test write permissions
        var testFile = Path.Combine(path, $"_write_test_{Guid.NewGuid()}.tmp");
        try
        {
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Cannot write to output path: {Path}", path);
            throw new UnauthorizedAccessException($"Cannot write to output path: {path}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate output path: {Path}", path);
            throw;
        }
    }
}
```

### Example Generated XML Output
```xml
<?xml version="1.0" encoding="utf-8"?>
<RouteTemplates>
  <RouteTemplate Name="205196000" ColorR="1" ColorG="124" ColorB="139">
    <WayPoint Name="205196000" Lat="55.123456" Lon="12.345678" Alt="0" Speed="12.50" 
              ETA="0" Delay="0" Mode="Waypoint" TrackMode="Track" Heading="180" 
              PortXTE="20" StbdXTE="20" MinSpeed="0" MaxSpeed="18.70" />
    <WayPoint Name="205196000" Lat="55.234567" Lon="12.456789" Alt="0" Speed="15.30" 
              ETA="0" Delay="0" Mode="Waypoint" TrackMode="Track" Heading="185" 
              PortXTE="20" StbdXTE="20" MinSpeed="0" MaxSpeed="18.70" />
    <WayPoint Name="205196000" Lat="55.345678" Lon="12.567890" Alt="0" Speed="18.70" 
              ETA="0" Delay="0" Mode="Waypoint" TrackMode="Track" Heading="190" 
              PortXTE="20" StbdXTE="20" MinSpeed="0" MaxSpeed="18.70" />
  </RouteTemplate>
</RouteTemplates>
```

## Success Criteria
- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.
- All positive and negative scenarios from BDD specification are covered by implementation.
- Export functionality creates valid XML files with correct structure and attribute mappings.
- File conflict handling works correctly for all three resolution options (Overwrite, AppendSuffix, Cancel).
- Output path validation properly detects and reports permission issues.
- Unique filename generation appends numeric suffixes correctly.
- All WayPoint attributes are mapped according to data_models.md specifications with correct formatting.
- ViewModel commands integrate smoothly with service layer.
- Error messages are user-friendly and displayed in the UI status bar.
- All operations are asynchronous to maintain UI responsiveness.
- Proper logging is implemented at all levels (info, warning, error).
- Unit tests cover all service methods and edge cases.
- Integration tests verify end-to-end export workflow and XML validity.

## Technical Requirements

### Architecture Constraints
- Follow three-layer architecture: Presentation (WPF), Business Logic (Core), Data Access (Infrastructure)
- Core layer contains only interfaces and domain models, no dependencies on other layers
- Infrastructure layer implements Core interfaces and handles all file I/O
- Presentation layer uses MVVM pattern with ViewModels orchestrating service calls
- All dependencies injected via constructor injection using Microsoft.Extensions.DependencyInjection

### MVVM Pattern
- ViewModels extend `ObservableObject` from CommunityToolkit.Mvvm
- Commands use `RelayCommand` or `AsyncRelayCommand` attributes
- Properties use `[ObservableProperty]` attribute for automatic INotifyPropertyChanged implementation
- View bindings use `{Binding PropertyName}` syntax in XAML
- Minimal code-behind in views (only view-specific logic like focus management)

### Asynchronous Operations
- All file I/O operations must be async (File.WriteAllTextAsync, etc.)
- Long-running operations use async/await pattern to prevent UI blocking
- Commands that perform async work use `AsyncRelayCommand`
- Support cancellation via `CancellationToken` parameters
- Progress reporting via `IProgress<T>` where appropriate

### Error Handling
- Service layer throws specific exceptions (ArgumentException, FileNotFoundException, UnauthorizedAccessException, etc.)
- ViewModel layer catches exceptions and displays user-friendly messages in StatusMessage property
- All exceptions logged with context (filename, MMSI, operation type)
- Distinguish between expected errors (user cancellation, validation failures) and unexpected errors
- Never swallow exceptions without logging

### File Operations
- All file paths validated before use (existence, permissions)
- File conflicts handled gracefully with user prompts
- Use Path.Combine for all path concatenation (cross-platform compatibility)
- Delete temporary files in finally blocks or using statements
- Use CultureInfo.InvariantCulture for all numeric formatting to avoid locale issues

### XML Generation
- Use System.Xml.Linq (XDocument, XElement) for XML generation
- Include XML declaration with UTF-8 encoding
- Format numeric values consistently (F6 for coordinates, F2 for speeds)
- Ensure all required attributes are present on every element
- Validate structure matches expected schema (root, template, waypoints hierarchy)

### Logging
- Use Microsoft.Extensions.Logging.ILogger<T> throughout
- Log levels:
  - Info: Operation start/complete, waypoint count, file paths
  - Warning: File conflicts, permission checks
  - Error: Exceptions, validation failures
- Include relevant context in log messages (MMSI, filename, waypoint count, etc.)
- Do not log sensitive data

### Testing
- Unit tests use NUnit framework
- Mock dependencies with Moq or NSubstitute
- Integration tests use real file system with test data in TestData folder
- Clean up test files in teardown methods
- Use FluentAssertions for readable assertions
- Test both happy path and error scenarios
- Verify logging calls using mock verification
