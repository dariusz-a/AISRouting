# Working Code Generation Prompt: Feature 3.1: Create Track

## Task: 
Generate working code for Feature 3.1: Create Track, following the guidelines below.

## Role: Software Engineer

When executing this prompt, you MUST assume the role of a **Software Engineer** with the following responsibilities and expertise:

- Designing and implementing robust, maintainable, and scalable features using C# and .NET 8.
- Translating BDD scenarios into actionable technical designs and implementation plans.
- Applying service-based architecture patterns and ensuring proper separation of concerns.
- Writing comprehensive unit and integration tests following best practices with NUnit.
- Ensuring all code aligns with project technical constraints, including MVVM patterns, dependency injection, and async/await patterns.
- Collaborating with team members to review, refine, and document technical solutions.
- Maintaining high standards for code quality, documentation, and test coverage.
- Adapting to evolving requirements and integrating feedback into the design and implementation process.
- Demonstrating expertise in WPF UI/UX best practices and robust desktop application engineering.
- Communicating technical decisions clearly and providing practical guidance for future maintainers.
- Ensuring all generated UI code uses proper XAML data binding and MVVM command patterns.

## References
- BDD Scenarios: docs/spec_scenarios/create_track.md
- Test File: tests/create_track.spec.ts
- Feature Design Document: docs/tech_design/api_integration_patterns.md
- Application Architecture: docs/tech_design/overall_architecture.md
- Application Organization: docs/tech_design/application_organization.md
- Data Models: docs/tech_design/data_models.md

## Development Approach

This feature implements the core track generation capability by loading AIS position data from CSV files and applying optimization algorithms to generate waypoint tracks. The implementation follows a layered architecture with clear separation between presentation (WPF ViewModels), business logic (Core services), and data access (Infrastructure services).

## BDD Scenarios

### Scenario 1: Create track for selected ship and time range

**BDD Scenario:**
```gherkin
Given the application has an input root "<input_root>" containing a vessel folder named "<mmsi>" 
  and the simulator user "<user_id>" is logged in 
  and the UI shows available CSV timestamps from "<first_ts>" to "<last_ts>".
When the user selects vessel "<mmsi>" 
  and sets start "<start>" and stop "<end>" with second resolution 
  and clicks "Create Track".
Then the system processes AIS CSV rows in the selected interval using default optimization parameters 
  and an ordered list of track points is shown in the UI 
  and the track reflects expected vessel continuity.
```

**Technical Design Details:**

This scenario represents the happy path for track creation. The implementation requires:

1. **IShipPositionLoader Service** (Infrastructure Layer)
   - Location: `AISRouting.Infrastructure/IO/ShipPositionLoader.cs`
   - Implementation details from api_integration_patterns.md:
     - Validates MMSI and time range inputs
     - Determines which CSV files fall within the date range
     - Uses CsvHelper to parse CSV files in streaming mode
     - Filters position records by timestamp
     - Returns `IEnumerable<ShipDataOut>` ordered by timestamp
     - Supports progress reporting via `IProgress<LoadProgress>`
     - Supports cancellation via `CancellationToken`

2. **ITrackOptimizer Service** (Core Layer)
   - Location: `AISRouting.Core/Services/Implementations/TrackOptimizer.cs`
   - Implementation details from api_integration_patterns.md:
     - Applies Douglas-Peucker algorithm for track simplification
     - Uses tolerance threshold (default 50 meters)
     - Converts `ShipDataOut` positions to `RouteWaypoint` instances
     - Maintains first and last waypoints
     - Supports progress reporting via `IProgress<OptimizationProgress>`
     - Runs as async Task to keep UI responsive

3. **MainViewModel Command** (Presentation Layer)
   - Location: `AISRouting.App.WPF/ViewModels/MainViewModel.cs`
   - Command: `CreateTrackCommand` (AsyncRelayCommand)
   - Orchestrates the workflow:
     - Validates that SelectedVessel is not null
     - Validates TimeInterval (Start < Stop)
     - Calls IShipPositionLoader.LoadPositionsAsync()
     - Updates StatusMessage with progress
     - Calls ITrackOptimizer.OptimizeTrackAsync()
     - Populates GeneratedWaypoints ObservableCollection
     - Handles exceptions and displays user-friendly error messages
     - Supports cancellation via CancellationTokenSource

4. **Data Models** (from data_models.md):
   - **ShipDataOut**: Position record with Time, Lat, Lon, SOG, Heading, ROT, etc.
   - **RouteWaypoint**: Optimized waypoint with Name, Lat, Lon, Speed, Heading, Mode, etc.
   - **TimeInterval**: Start/Stop DateTime with validation
   - **LoadProgress**: ProcessedFiles, TotalFiles, RecordsLoaded
   - **OptimizationProgress**: ProcessedPoints, TotalPoints

**Implementation Tasks:**

1. Implement `ShipPositionLoader` class in `AISRouting.Infrastructure/IO/ShipPositionLoader.cs`
   - Add constructor accepting `ILogger<ShipPositionLoader>`, `IPathValidator`, and `ICsvParser<ShipDataOut>`
   - Implement `LoadPositionsAsync` method signature as defined in api_integration_patterns.md
   - Add input validation for MMSI, time range, and folder path
   
2. Implement CSV file discovery logic in `ShipPositionLoader`
   - Add `GetRelevantCsvFiles` method to identify CSV files within date range
   - Use filename parsing (format: YYYY-MM-DD.csv) to filter files
   - Handle missing files gracefully
   
3. Implement CSV parsing and filtering in `ShipPositionLoader`
   - Use ICsvParser to read CSV files
   - Filter records where BaseDateTime is between startTime and stopTime
   - Aggregate records from multiple files
   - Report progress using IProgress<LoadProgress>
   - Support cancellation using CancellationToken
   
4. Implement error handling in `ShipPositionLoader`
   - Log and skip individual file failures
   - Throw FileNotFoundException if no CSV files found
   - Throw DirectoryNotFoundException if MMSI folder missing
   - Continue processing other files if one fails
   
5. Implement sorting logic in `ShipPositionLoader`
   - Sort aggregated positions by BaseDateTime
   - Return as ordered IEnumerable<ShipDataOut>
   
6. Implement `TrackOptimizer` class in `AISRouting.Core/Services/Implementations/TrackOptimizer.cs`
   - Add constructor accepting `ILogger<TrackOptimizer>`
   - Implement `OptimizeTrackAsync` method signature as defined in api_integration_patterns.md
   - Validate input positions list is not empty
   
7. Implement Douglas-Peucker algorithm in `TrackOptimizer`
   - Add `DouglasPeuckerOptimize` method with recursive logic
   - Use bool[] array for keep flags to track retained points
   - Always keep first and last points
   - Calculate perpendicular distance using Haversine formula
   
8. Implement distance calculation in `TrackOptimizer`
   - Add `PerpendicularDistance` method using geographic coordinates
   - Use Haversine formula for accurate distance on sphere
   - Add `ToRadians` helper method
   
9. Implement waypoint mapping in `TrackOptimizer`
   - Convert ShipDataOut to RouteWaypoint
   - Map properties: Lat, Lon, Speed (from SOG), Heading
   - Set fixed values: Alt=0, Delay=0, TrackMode="Track", PortXTE=20, StbdXTE=20, MinSpeed=0
   - Calculate MaxSpeed from all waypoints in route
   - Re-index waypoints after optimization
   
10. Implement progress reporting in `TrackOptimizer`
    - Report progress periodically during recursive optimization
    - Use IProgress<OptimizationProgress> to update UI
    
11. Add `CreateTrackCommand` to `MainViewModel` in `AISRouting.App.WPF/ViewModels/MainViewModel.cs`
    - Define as AsyncRelayCommand with CanExecute check
    - Inject IShipPositionLoader and ITrackOptimizer in constructor
    - Add CancellationTokenSource field for operation cancellation
    
12. Implement `CanCreateTrack` method in `MainViewModel`
    - Check SelectedVessel is not null
    - Check TimeInterval is valid (Start < Stop)
    - Check InputFolderPath is not empty
    - Return true only if all conditions met
    
13. Implement `CreateTrackAsync` command execution in `MainViewModel`
    - Create new CancellationTokenSource
    - Update StatusMessage to "Loading position data..."
    - Call IShipPositionLoader.LoadPositionsAsync with progress callback
    - Update StatusMessage during loading with file count and record count
    
14. Implement optimization step in `CreateTrackAsync`
    - Update StatusMessage to "Optimizing track..."
    - Call ITrackOptimizer.OptimizeTrackAsync with progress callback
    - Update StatusMessage during optimization with point count
    
15. Implement results display in `CreateTrackAsync`
    - Create new ObservableCollection<RouteWaypoint> from results
    - Assign to GeneratedWaypoints property
    - Update StatusMessage with success: "Track created: X waypoints from Y records"
    
16. Implement error handling in `CreateTrackAsync`
    - Catch OperationCanceledException: Set StatusMessage to "Operation cancelled"
    - Catch FileNotFoundException: Set StatusMessage to "Error: No position data files found"
    - Catch DirectoryNotFoundException: Set StatusMessage to "Error: Vessel folder not found"
    - Catch ArgumentException: Set StatusMessage with validation error
    - Catch general Exception: Log error and set StatusMessage to "Error: {ex.Message}"
    
17. Implement cleanup in `CreateTrackAsync`
    - Dispose CancellationTokenSource in finally block
    - Set CancellationTokenSource to null
    
18. Add `CancelOperationCommand` to `MainViewModel`
    - Define as RelayCommand
    - Call Cancel() on CancellationTokenSource if not null
    - Update StatusMessage to indicate cancellation requested
    
19. Add `GeneratedWaypoints` property to `MainViewModel`
    - Define as ObservableCollection<RouteWaypoint>
    - Use [ObservableProperty] attribute for change notification
    - Initialize as empty collection in constructor
    
20. Add `StatusMessage` property to `MainViewModel`
    - Define as string with [ObservableProperty]
    - Initialize to empty string
    - Update throughout operation lifecycle

### Scenario 2: Create track with noisy data and narrowed time window

**BDD Scenario:**
```gherkin
Given the input root "C:\\data\\ais_root" contains noisy AIS CSV rows for vessel "205196000" 
  and the simulator user "scenario-user" is logged in.
When the user selects vessel "205196000" 
  and sets a narrow start/stop window within noisy interval 
  and clicks "Create Track".
Then the generated track contains fewer spurious points due to the narrowed window 
  and processing completes without errors 
  and a completion status is displayed.
```

**Technical Design Details:**

This scenario tests the system's robustness when dealing with noisy AIS data. The narrow time window helps filter out spurious position reports that would otherwise create waypoints far from the vessel's actual path. The implementation uses the same services as Scenario 1 but emphasizes:

1. **Time-based filtering effectiveness**
   - ShipPositionLoader filters by exact timestamp range
   - Reduces input data volume before optimization
   - Noisy points outside the window are excluded

2. **Error resilience**
   - System should handle anomalous position data gracefully
   - No crashes or unhandled exceptions
   - Complete with success status even with imperfect data

**Implementation Tasks:**

21. Validate time filtering in `ShipPositionLoader`
    - Ensure BaseDateTime comparison is exact (>=, <=)
    - Test with narrow time windows (e.g., 1 hour)
    - Verify no off-by-one errors in timestamp comparison
    
22. Add data quality logging in `ShipPositionLoader`
    - Log record count before and after time filtering
    - Log warning if filtered count is very low relative to time span
    - Help diagnose data quality issues
    
23. Handle anomalous coordinates in `TrackOptimizer`
    - Skip waypoints with invalid Lat/Lon values
    - Log warning when skipping invalid positions
    - Continue processing remaining valid positions
    
24. Update success message in `CreateTrackAsync`
    - Include reduction ratio: "Track created: X waypoints from Y records (Z% reduction)"
    - Display completion time
    - Indicate successful completion even with noisy data

### Scenario 3: Reject track creation when no ship selected

**BDD Scenario:**
```gherkin
Given the input root "C:\\data\\ais_root" is selected 
  and the UI has no vessel selected 
  and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then an inline error message with text "No ship selected" should be visible 
  and track creation does not start.
```

**Technical Design Details:**

This negative scenario ensures the UI properly validates preconditions before executing the track creation workflow. The implementation relies on the Command's CanExecute pattern:

1. **Command CanExecute validation**
   - CreateTrackCommand.CanExecute returns false when SelectedVessel is null
   - Button is disabled in UI when command cannot execute
   - No service calls are made

2. **User feedback**
   - Display inline error message near Create Track button
   - Use data binding to show/hide error based on validation state
   - Clear error when vessel is selected

**Implementation Tasks:**

25. Implement vessel selection validation in `CanCreateTrack`
    - Check if SelectedVessel is null
    - Return false if null
    - Trigger CommandManager.RequerySuggested when SelectedVessel changes
    
26. Add validation error message property to `MainViewModel`
    - Define `TrackCreationError` as string with [ObservableProperty]
    - Set to "No ship selected" when SelectedVessel is null and user attempts action
    - Clear when SelectedVessel is not null
    
27. Update SelectedVessel property setter in `MainViewModel`
    - When SelectedVessel changes, clear TrackCreationError
    - Call CreateTrackCommand.NotifyCanExecuteChanged()
    - Update UI bindings
    
28. Add XAML validation UI in MainWindow
    - Add TextBlock bound to TrackCreationError
    - Use Visibility converter (Collapsed when empty)
    - Style with error color (red text)
    - Position near Create Track button
    
29. Test command disabling
    - Verify CreateTrackCommand is disabled when no vessel selected
    - Verify button is visually disabled in UI
    - Verify clicking disabled button has no effect

### Scenario 4: Fail gracefully on malformed CSV rows

**BDD Scenario:**
```gherkin
Given the selected time range includes CSV rows with missing latitude/longitude or required columns 
  and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then the system skips malformed rows, processing continues for valid rows, 
  and a warning banner with text "Some rows were ignored due to invalid format" is displayed.
```

**Technical Design Details:**

This scenario tests CSV parsing robustness. The implementation must handle:

1. **CSV parsing with CsvHelper**
   - Configure CsvHelper to skip bad rows rather than throw
   - Log each malformed row with row number
   - Continue parsing remaining rows
   - Use MissingFieldFound = null and BadDataFound callbacks

2. **Position validation**
   - Check ShipDataOut.Latitude and Longitude are not null
   - Skip records with null coordinates
   - Count and report skipped records

3. **User notification**
   - Display warning message if any rows were skipped
   - Don't treat as fatal error
   - Include count of skipped rows in message

**Implementation Tasks:**

30. Configure CsvHelper for error tolerance in `ICsvParser` implementation
    - Set MissingFieldFound = null to ignore missing fields
    - Set BadDataFound callback to log warnings
    - Use HeaderValidated callback to verify required columns
    - Continue parsing after encountering bad data
    
31. Add row validation in CSV parser
    - Check each ShipDataOut record for null Lat/Lon
    - Track count of invalid records
    - Log each invalid record with row number
    
32. Return parsing statistics from `ShipPositionLoader`
    - Track total rows read vs. valid rows returned
    - Include skipped count in LoadProgress
    - Pass statistics to caller
    
33. Display warning banner in `MainViewModel`
    - Add `WarningMessage` property with [ObservableProperty]
    - Set to "Some rows were ignored due to invalid format (X rows skipped)" if any skipped
    - Clear warning on next successful operation
    
34. Add warning UI to MainWindow XAML
    - Add Border/TextBlock for warning banner
    - Style with warning color (yellow/orange background)
    - Bind Visibility to WarningMessage (visible when not empty)
    - Position above track results area
    
35. Test CSV parsing resilience
    - Create test CSV with missing Lat/Lon values
    - Create test CSV with missing required columns
    - Verify operation completes successfully
    - Verify warning message displays correct count

### Scenario 5: Handle missing Heading or SOG values in records

**BDD Scenario:**
```gherkin
Given CSV records in the selected range contain missing Heading or SOG values 
  and the simulator user "scenario-user" is logged in.
When the user clicks "Create Track".
Then missing Heading or SOG fields default to 0 for WayPoint mapping 
  and points are generated where possible 
  and a data-quality note is shown.
```

**Technical Design Details:**

This scenario addresses nullable fields in AIS data. Per the data model, Heading and SOG are nullable int and double respectively. The implementation must:

1. **Nullable field handling in ShipDataOut**
   - Heading is int? (nullable)
   - SOG is double? (nullable)
   - Both defined in data_models.md as optional

2. **Waypoint mapping with defaults**
   - When mapping ShipDataOut → RouteWaypoint
   - Use Heading ?? 0 for waypoint Heading
   - Use SOG ?? 0.0 for waypoint Speed
   - Documented in data_models.md

3. **Data quality notification**
   - Count records with missing Heading or SOG
   - Display data quality note if any defaults were applied
   - Include percentage of records affected

**Implementation Tasks:**

36. Update waypoint mapping in `TrackOptimizer`
    - In conversion from ShipDataOut to RouteWaypoint
    - Use null-coalescing: `Heading = position.Heading ?? 0`
    - Use null-coalescing: `Speed = position.SOG ?? 0.0`
    - Track count of records where defaults were applied
    
37. Add data quality tracking to optimization
    - Count records with null Heading
    - Count records with null SOG
    - Return statistics in OptimizationProgress or result
    
38. Display data quality note in `MainViewModel`
    - Add `DataQualityMessage` property with [ObservableProperty]
    - Set message if any defaults applied: "Data quality note: X records had missing Heading/SOG (defaulted to 0)"
    - Display alongside or below track results
    
39. Add data quality UI to MainWindow XAML
    - Add TextBlock for data quality notes
    - Style with informational color (blue/gray)
    - Bind to DataQualityMessage property
    - Position below track results or in status area
    
40. Test with missing Heading/SOG values
    - Create test CSV with null Heading values
    - Create test CSV with null SOG values
    - Verify waypoints are created with 0 defaults
    - Verify data quality message displays

### Scenario 6: Prevent track creation when input root empty

**BDD Scenario:**
```gherkin
Given the selected input root "C:\\empty\\root" contains no vessel subfolders 
  and the simulator user "scenario-user" is logged in.
When the user opens the ship selection combo box and attempts to click "Create Track".
Then the ship combo shows an empty list 
  and an inline warning "No vessels found in input root" is displayed 
  and the Create Track action is disabled.
```

**Technical Design Details:**

This scenario ensures the application handles empty input folders gracefully. The implementation requires:

1. **Folder scanning results**
   - ISourceDataScanner returns empty collection if no MMSI folders found
   - AvailableVessels collection is empty
   - Ship selection combo shows empty

2. **Command state management**
   - CreateTrackCommand.CanExecute returns false when AvailableVessels is empty
   - Button is disabled
   - Validation message explains why

3. **User guidance**
   - Display warning message about empty input root
   - Suggest selecting a different folder
   - Clear warning when vessels are found

**Implementation Tasks:**

41. Handle empty scan results in `MainViewModel`
    - After calling ISourceDataScanner.ScanInputFolderAsync
    - Check if result collection is empty
    - Set AvailableVessels to empty ObservableCollection
    
42. Display empty folder warning in `MainViewModel`
    - Set ValidationMessage to "No vessels found in input root" if AvailableVessels is empty
    - Position message near folder selection UI
    - Style as warning (orange/yellow)
    
43. Update `CanCreateTrack` validation
    - Add check: AvailableVessels.Count > 0
    - Return false if empty
    - Combine with existing null checks
    
44. Add empty state UI to combo box
    - Set ComboBox.IsEnabled to false when AvailableVessels is empty
    - Add TextBlock showing "No vessels available" in disabled state
    - Bind visibility to AvailableVessels.Count
    
45. Test empty folder handling
    - Create test with empty input folder
    - Verify ISourceDataScanner returns empty collection
    - Verify UI shows warning message
    - Verify Create Track button is disabled
    - Verify combo box is disabled or shows empty state

### Scenario 7: Create track unavailable for user without permission

**BDD Scenario:**
```gherkin
Given a logged-in user "user-no-create" without create-track privileges 
  and an available vessel "205196000".
When the user views the UI controls for creating a track.
Then the "Create Track" button is disabled 
  and a tooltip with text "Insufficient privileges" is shown.
```

**Technical Design Details:**

This scenario implements permission-based UI controls. The current implementation is a desktop application without user authentication, so this is a future-proofing scenario. The implementation approach:

1. **Permission service abstraction**
   - Define IPermissionService interface
   - Method: `bool CanCreateTrack()`
   - Inject into MainViewModel

2. **Default implementation**
   - AlwaysAllowPermissionService: returns true for all operations
   - For future: replace with actual authentication/authorization

3. **Command integration**
   - Add permission check to CanCreateTrack()
   - Update tooltip based on permission state

**Implementation Tasks:**

46. Define `IPermissionService` interface in `AISRouting.Core/Services/Interfaces/`
    - Add method: `bool CanCreateTrack()`
    - Add method: `string GetPermissionDeniedReason(string operation)`
    
47. Implement `AlwaysAllowPermissionService` in `AISRouting.Core/Services/Implementations/`
    - Implement IPermissionService
    - Return true for CanCreateTrack()
    - Return empty string for GetPermissionDeniedReason()
    
48. Inject IPermissionService into `MainViewModel` constructor
    - Add parameter: `IPermissionService permissionService`
    - Store as private field
    - Add to DI registration in App.xaml.cs
    
49. Update `CanCreateTrack` to check permissions
    - Call `_permissionService.CanCreateTrack()`
    - Return false if permission denied
    - Combine with existing validation checks
    
50. Add permission tooltip to `MainViewModel`
    - Add `CreateTrackTooltip` property with [ObservableProperty]
    - Set to "Insufficient privileges" if permission denied
    - Set to "Create optimized track from position data" if allowed
    - Update when CanCreateTrack state changes
    
51. Bind tooltip in MainWindow XAML
    - Set Button.ToolTip to CreateTrackTooltip property
    - Update tooltip dynamically based on permission state
    
52. Register permission service in DI container
    - In App.xaml.cs ConfigureServices
    - Register: `services.AddSingleton<IPermissionService, AlwaysAllowPermissionService>()`
    
53. Test permission denial (future enhancement)
    - Create mock IPermissionService returning false
    - Verify button is disabled
    - Verify tooltip shows "Insufficient privileges"

## Code Examples

### ShipPositionLoader Implementation

```csharp
namespace AISRouting.Infrastructure.IO
{
    public class ShipPositionLoader : IShipPositionLoader
    {
        private readonly ILogger<ShipPositionLoader> _logger;
        private readonly IPathValidator _pathValidator;
        private readonly ICsvParser<ShipDataOut> _csvParser;

        public ShipPositionLoader(
            ILogger<ShipPositionLoader> logger,
            IPathValidator pathValidator,
            ICsvParser<ShipDataOut> csvParser)
        {
            _logger = logger;
            _pathValidator = pathValidator;
            _csvParser = csvParser;
        }

        public async Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(
            int mmsi,
            DateTime startTime,
            DateTime stopTime,
            string inputFolderPath,
            IProgress<LoadProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            _pathValidator.ValidateInputFolderPath(inputFolderPath);
            MmsiValidator.Validate(mmsi);

            if (startTime >= stopTime)
                throw new ArgumentException("Start time must be before stop time");

            var mmsiFolder = Path.Combine(inputFolderPath, mmsi.ToString());
            if (!Directory.Exists(mmsiFolder))
                throw new DirectoryNotFoundException($"MMSI folder not found: {mmsi}");

            // Determine which CSV files to load
            var csvFiles = GetRelevantCsvFiles(mmsiFolder, startTime, stopTime);
            if (csvFiles.Count == 0)
                throw new FileNotFoundException($"No CSV files found for MMSI {mmsi} in date range");

            var allPositions = new List<ShipDataOut>();
            int processedFiles = 0;

            foreach (var csvFile in csvFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var positions = await _csvParser.ParseFileAsync(csvFile, cancellationToken);
                    
                    // Filter by time range
                    var filtered = positions.Where(p => 
                        p.BaseDateTime >= startTime && p.BaseDateTime <= stopTime);

                    allPositions.AddRange(filtered);

                    processedFiles++;
                    progress?.Report(new LoadProgress
                    {
                        ProcessedFiles = processedFiles,
                        TotalFiles = csvFiles.Count,
                        RecordsLoaded = allPositions.Count
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load CSV file: {File}", csvFile);
                    // Continue processing other files
                }
            }

            // Sort by timestamp
            var sorted = allPositions.OrderBy(p => p.BaseDateTime).ToList();

            _logger.LogInformation(
                "Loaded {Count} position records for MMSI {MMSI} from {Start} to {Stop}",
                sorted.Count, mmsi, startTime, stopTime);

            return sorted;
        }

        private List<string> GetRelevantCsvFiles(string mmsiFolder, DateTime start, DateTime stop)
        {
            var allCsvFiles = Directory.GetFiles(mmsiFolder, "*.csv");
            var relevantFiles = new List<string>();

            var currentDate = start.Date;
            while (currentDate <= stop.Date)
            {
                var expectedFilename = $"{currentDate:yyyy-MM-dd}.csv";
                var filePath = Path.Combine(mmsiFolder, expectedFilename);

                if (File.Exists(filePath))
                    relevantFiles.Add(filePath);

                currentDate = currentDate.AddDays(1);
            }

            return relevantFiles;
        }
    }
}
```

### TrackOptimizer Implementation

```csharp
namespace AISRouting.Core.Services.Implementations
{
    public class TrackOptimizer : ITrackOptimizer
    {
        private readonly ILogger<TrackOptimizer> _logger;

        public TrackOptimizer(ILogger<TrackOptimizer> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<RouteWaypoint>> OptimizeTrackAsync(
            IEnumerable<ShipDataOut> positions,
            OptimizationOptions options = null,
            IProgress<OptimizationProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            options ??= OptimizationOptions.Default;

            var positionList = positions.ToList();
            if (positionList.Count == 0)
                throw new ArgumentException("Position list is empty");

            _logger.LogInformation(
                "Optimizing track with {Count} positions using tolerance {Tolerance} meters",
                positionList.Count, options.ToleranceMeters);

            // Run optimization asynchronously (CPU-bound work)
            return await Task.Run(() =>
            {
                var optimized = DouglasPeuckerOptimize(positionList, options.ToleranceMeters, 
                    progress, cancellationToken);

                _logger.LogInformation(
                    "Optimization complete: {Original} positions → {Optimized} waypoints",
                    positionList.Count, optimized.Count);

                return optimized;
            }, cancellationToken);
        }

        private List<RouteWaypoint> DouglasPeuckerOptimize(
            List<ShipDataOut> positions,
            double tolerance,
            IProgress<OptimizationProgress> progress,
            CancellationToken cancellationToken)
        {
            // Convert positions to waypoints
            var waypoints = positions.Select((p, i) => new RouteWaypoint
            {
                Index = i + 1,
                Time = p.BaseDateTime,
                Lat = p.Lat ?? 0.0,
                Lon = p.Lon ?? 0.0,
                Speed = p.SOG ?? 0.0,
                Heading = p.Heading ?? 0
            }).ToList();

            // Apply Douglas-Peucker algorithm
            var keepFlags = new bool[waypoints.Count];
            keepFlags[0] = true;
            keepFlags[waypoints.Count - 1] = true;

            DouglasPeuckerRecursive(waypoints, keepFlags, 0, waypoints.Count - 1,
                tolerance, progress, cancellationToken);

            // Extract kept waypoints
            var result = new List<RouteWaypoint>();
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (keepFlags[i])
                {
                    var wp = waypoints[i];
                    wp.Index = result.Count + 1; // Re-index
                    result.Add(wp);
                }
            }

            return result;
        }

        private void DouglasPeuckerRecursive(
            List<RouteWaypoint> waypoints,
            bool[] keepFlags,
            int startIndex,
            int endIndex,
            double tolerance,
            IProgress<OptimizationProgress> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (endIndex - startIndex <= 1)
                return;

            // Find point with maximum distance from line segment
            double maxDistance = 0;
            int maxIndex = startIndex;

            for (int i = startIndex + 1; i < endIndex; i++)
            {
                double distance = PerpendicularDistance(
                    waypoints[i], waypoints[startIndex], waypoints[endIndex]);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = i;
                }
            }

            // If max distance exceeds tolerance, keep the point and recurse
            if (maxDistance > tolerance)
            {
                keepFlags[maxIndex] = true;

                DouglasPeuckerRecursive(waypoints, keepFlags, startIndex, maxIndex,
                    tolerance, progress, cancellationToken);
                DouglasPeuckerRecursive(waypoints, keepFlags, maxIndex, endIndex,
                    tolerance, progress, cancellationToken);
            }

            // Report progress periodically
            progress?.Report(new OptimizationProgress
            {
                ProcessedPoints = endIndex,
                TotalPoints = waypoints.Count
            });
        }

        private double PerpendicularDistance(RouteWaypoint point,
            RouteWaypoint lineStart, RouteWaypoint lineEnd)
        {
            // Calculate perpendicular distance from point to line segment
            // Using Haversine formula for geographic coordinates

            const double earthRadius = 6371000; // meters

            var lat1 = ToRadians(lineStart.Lat);
            var lon1 = ToRadians(lineStart.Lon);
            var lat2 = ToRadians(lineEnd.Lat);
            var lon2 = ToRadians(lineEnd.Lon);
            var latP = ToRadians(point.Lat);
            var lonP = ToRadians(point.Lon);

            // Cross-track distance formula
            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d13 = earthRadius * c;

            var bearing13 = Math.Atan2(
                Math.Sin(lonP - lon1) * Math.Cos(latP),
                Math.Cos(lat1) * Math.Sin(latP) - Math.Sin(lat1) * Math.Cos(latP) * Math.Cos(lonP - lon1));
            
            var bearing12 = Math.Atan2(
                Math.Sin(lon2 - lon1) * Math.Cos(lat2),
                Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1));

            var crossTrack = Math.Abs(Math.Asin(Math.Sin(d13 / earthRadius) * Math.Sin(bearing13 - bearing12)) * earthRadius);

            return crossTrack;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
```

### MainViewModel CreateTrackCommand Implementation

```csharp
namespace AISRouting.App.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IShipPositionLoader _positionLoader;
        private readonly ITrackOptimizer _trackOptimizer;
        private readonly ILogger<MainViewModel> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        [ObservableProperty]
        private ShipStaticData _selectedVessel;

        [ObservableProperty]
        private TimeInterval _timeInterval;

        [ObservableProperty]
        private ObservableCollection<RouteWaypoint> _generatedWaypoints;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private string _warningMessage;

        [ObservableProperty]
        private string _trackCreationError;

        public MainViewModel(
            IShipPositionLoader positionLoader,
            ITrackOptimizer trackOptimizer,
            ILogger<MainViewModel> logger)
        {
            _positionLoader = positionLoader;
            _trackOptimizer = trackOptimizer;
            _logger = logger;

            GeneratedWaypoints = new ObservableCollection<RouteWaypoint>();
            TimeInterval = new TimeInterval();

            CreateTrackCommand = new AsyncRelayCommand(CreateTrackAsync, CanCreateTrack);
        }

        public IAsyncRelayCommand CreateTrackCommand { get; }

        private bool CanCreateTrack()
        {
            if (SelectedVessel == null)
            {
                TrackCreationError = "No ship selected";
                return false;
            }

            if (!TimeInterval.IsValid)
            {
                TrackCreationError = "Invalid time interval";
                return false;
            }

            if (string.IsNullOrEmpty(InputFolderPath))
            {
                TrackCreationError = "No input folder selected";
                return false;
            }

            TrackCreationError = string.Empty;
            return true;
        }

        private async Task CreateTrackAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                StatusMessage = "Loading position data...";
                WarningMessage = string.Empty;

                var positions = await _positionLoader.LoadPositionsAsync(
                    SelectedVessel.MMSI,
                    TimeInterval.Start,
                    TimeInterval.Stop,
                    InputFolderPath,
                    progress: new Progress<LoadProgress>(p =>
                    {
                        StatusMessage = $"Loading: {p.ProcessedFiles}/{p.TotalFiles} files, {p.RecordsLoaded} records";
                    }),
                    cancellationToken: _cancellationTokenSource.Token
                );

                StatusMessage = "Optimizing track...";

                var waypoints = await _trackOptimizer.OptimizeTrackAsync(
                    positions,
                    options: new OptimizationOptions { ToleranceMeters = 50 },
                    progress: new Progress<OptimizationProgress>(p =>
                    {
                        StatusMessage = $"Optimizing: {p.ProcessedPoints}/{p.TotalPoints} points";
                    }),
                    cancellationToken: _cancellationTokenSource.Token
                );

                var waypointList = waypoints.ToList();
                GeneratedWaypoints = new ObservableCollection<RouteWaypoint>(waypointList);

                var positionCount = positions.Count();
                var reductionPercent = positionCount > 0 
                    ? (100.0 - (waypointList.Count * 100.0 / positionCount)) 
                    : 0;

                StatusMessage = $"Track created: {waypointList.Count} waypoints from {positionCount} records ({reductionPercent:F1}% reduction)";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled";
            }
            catch (FileNotFoundException ex)
            {
                StatusMessage = "Error: No position data files found for selected time range";
                _logger.LogWarning(ex, "CSV files not found for MMSI {MMSI}", SelectedVessel?.MMSI);
            }
            catch (DirectoryNotFoundException ex)
            {
                StatusMessage = "Error: Vessel folder not found";
                _logger.LogError(ex, "MMSI folder not found");
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _logger.LogWarning(ex, "Validation error in track creation");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, "Unexpected error in track creation");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        partial void OnSelectedVesselChanged(ShipStaticData value)
        {
            TrackCreationError = string.Empty;
            CreateTrackCommand.NotifyCanExecuteChanged();
        }
    }
}
```

### XAML Data Binding Example

```xml
<Window x:Class="AISRouting.App.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <StackPanel Margin="20">
            <!-- Ship Selection -->
            <ComboBox ItemsSource="{Binding AvailableVessels}"
                      SelectedItem="{Binding SelectedVessel}"
                      DisplayMemberPath="Name"
                      Width="300"
                      Margin="0,0,0,10"/>

            <!-- Time Interval -->
            <StackPanel Orientation="Horizontal" Margin="0,0,0,10">
                <DatePicker SelectedDate="{Binding TimeInterval.Start}" Width="150"/>
                <TextBlock Text=" to " VerticalAlignment="Center" Margin="5,0"/>
                <DatePicker SelectedDate="{Binding TimeInterval.Stop}" Width="150"/>
            </StackPanel>

            <!-- Create Track Button -->
            <Button Command="{Binding CreateTrackCommand}"
                    Content="Create Track"
                    Width="150"
                    Height="30"
                    Margin="0,0,0,10"/>

            <!-- Error Message -->
            <TextBlock Text="{Binding TrackCreationError}"
                       Foreground="Red"
                       Visibility="{Binding TrackCreationError, Converter={StaticResource StringToVisibilityConverter}}"
                       Margin="0,0,0,10"/>

            <!-- Warning Message -->
            <Border Background="Orange"
                    Padding="10"
                    Visibility="{Binding WarningMessage, Converter={StaticResource StringToVisibilityConverter}}"
                    Margin="0,0,0,10">
                <TextBlock Text="{Binding WarningMessage}"
                           Foreground="White"/>
            </Border>

            <!-- Status Message -->
            <TextBlock Text="{Binding StatusMessage}"
                       Foreground="Blue"
                       Margin="0,0,0,10"/>

            <!-- Track Results -->
            <DataGrid ItemsSource="{Binding GeneratedWaypoints}"
                      AutoGenerateColumns="False"
                      Height="300"
                      IsReadOnly="True">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Index" Binding="{Binding Index}"/>
                    <DataGridTextColumn Header="Latitude" Binding="{Binding Lat, StringFormat=F6}"/>
                    <DataGridTextColumn Header="Longitude" Binding="{Binding Lon, StringFormat=F6}"/>
                    <DataGridTextColumn Header="Speed (kn)" Binding="{Binding Speed, StringFormat=F2}"/>
                    <DataGridTextColumn Header="Heading" Binding="{Binding Heading}"/>
                </DataGrid.Columns>
            </DataGrid>
        </StackPanel>
    </Grid>
</Window>
```

## Success Criteria

- All implemented code, including new files and modifications, must remain as a permanent part of the codebase upon completion. Do not delete or revert the changes.
- All tasks above are implemented and tested in isolation.
- ShipPositionLoader correctly loads and filters CSV position data by time range.
- TrackOptimizer successfully applies Douglas-Peucker algorithm to reduce waypoint count.
- MainViewModel CreateTrackCommand orchestrates the workflow with proper error handling.
- UI displays progress updates during loading and optimization.
- Command CanExecute properly validates preconditions (vessel selected, valid time interval).
- Malformed CSV rows are skipped gracefully with warnings logged.
- Missing Heading/SOG values default to 0 in waypoint mapping.
- Empty input folders are handled with appropriate user messages.
- All exceptions are caught and translated to user-friendly status messages.
- Cancellation is supported via CancellationToken throughout the workflow.
- Progress reporting keeps UI responsive during long operations.
- Generated waypoints are displayed in ObservableCollection bound to DataGrid.
- All services follow dependency injection patterns defined in overall_architecture.md.
- All async operations use async/await pattern correctly.

## Technical Requirements

1. **Service Layer Architecture**: All business logic must be implemented in service classes with interface-based design for testability and dependency injection.

2. **MVVM Pattern**: ViewModels must use CommunityToolkit.Mvvm with [ObservableProperty] attributes and RelayCommand for commands. All UI state is exposed through data-bindable properties.

3. **Async/Await Pattern**: All I/O operations (file loading, parsing, optimization) must be async to keep UI responsive. Use Task.Run for CPU-bound work.

4. **Error Handling Strategy**: Services throw specific exceptions; ViewModels catch and translate to user messages; all errors logged with context.

5. **Progress Reporting**: Long-running operations must report progress via IProgress<T> to update UI during execution.

6. **Cancellation Support**: All async operations must accept CancellationToken and check for cancellation periodically.

7. **Data Validation**: Validate inputs at service boundaries (MMSI format, time range, folder paths). Use dedicated validator classes.

8. **Logging**: Use ILogger throughout with appropriate log levels (Info for milestones, Warning for skipped data, Error for failures).

9. **File Handling**: Use streaming for CSV parsing to handle large files. Use CsvHelper with error-tolerant configuration.

10. **Dependency Injection**: All services registered in App.xaml.cs and injected via constructors. Follow dependency rules from application_organization.md.

11. **Data Models**: Follow data model definitions from data_models.md. Handle nullable fields appropriately.

12. **Geodesic Calculations**: Use Haversine formula for distance calculations between geographic coordinates. Include earth radius constant (6371000 meters).

13. **Command State Management**: Commands must implement CanExecute to enable/disable based on validation state. Call NotifyCanExecuteChanged when state changes.

14. **Observable Collections**: Use ObservableCollection<T> for collections bound to UI. Update on UI thread.

15. **WPF Data Binding**: Use TwoWay binding for user inputs, OneWay for display-only data. Use value converters for type conversions (e.g., bool to Visibility).
