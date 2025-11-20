# API Integration Patterns

This document covers API design principles, integration patterns with external systems, data flow between services, error handling strategies, and performance considerations for the AISRouting application.

## Service Layer Architecture

### Service Interface Design

All services follow interface-based design to support testability and dependency injection:

```csharp
namespace AISRouting.Core.Services
{
    // File system scanning service
    public interface ISourceDataScanner
    {
        Task<IEnumerable<VesselInfo>> ScanInputFolderAsync(
            string inputFolderPath, 
            CancellationToken cancellationToken = default);
    }

    // CSV position data loading
    public interface IShipPositionLoader
    {
        Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(
            int mmsi, 
            DateTime startTime, 
            DateTime stopTime, 
            string inputFolderPath,
            IProgress<LoadProgress> progress = null,
            CancellationToken cancellationToken = default);
    }

    // JSON static data loading
    public interface IShipStaticDataLoader
    {
        Task<ShipStaticData> LoadStaticDataAsync(
            int mmsi, 
            string inputFolderPath,
            CancellationToken cancellationToken = default);
    }

    // Track optimization algorithm
    public interface ITrackOptimizer
    {
        Task<IEnumerable<RouteWaypoint>> OptimizeTrackAsync(
            IEnumerable<ShipDataOut> positions,
            OptimizationOptions options = null,
            IProgress<OptimizationProgress> progress = null,
            CancellationToken cancellationToken = default);
    }

    // XML route export
    public interface IRouteExporter
    {
        Task ExportRouteAsync(
            IEnumerable<RouteWaypoint> waypoints,
            string templatePath,
            string outputFilePath,
            ExportOptions options = null,
            CancellationToken cancellationToken = default);
    }

    // Dialog services
    public interface IFolderDialogService
    {
        string ShowFolderBrowserDialog(string initialPath = null);
    }

    public interface IFileConflictDialogService
    {
        FileConflictResolution ShowFileConflictDialog(string filePath);
    }
}
```

## Service Implementations

### ISourceDataScanner Implementation

**Purpose**: Scan input folder for MMSI subfolders and vessel metadata

**Implementation:**
```csharp
public class SourceDataScanner : ISourceDataScanner
{
    private readonly ILogger<SourceDataScanner> _logger;
    private readonly IPathValidator _pathValidator;

    public SourceDataScanner(ILogger<SourceDataScanner> logger, IPathValidator pathValidator)
    {
        _logger = logger;
        _pathValidator = pathValidator;
    }

    public async Task<IEnumerable<VesselInfo>> ScanInputFolderAsync(
        string inputFolderPath, 
        CancellationToken cancellationToken = default)
    {
        // Validate input path
        _pathValidator.ValidateInputFolderPath(inputFolderPath);

        var vesselInfos = new List<VesselInfo>();

        // Get all subdirectories (MMSI folders)
        var subdirs = Directory.GetDirectories(inputFolderPath);

        foreach (var subdir in subdirs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dirName = Path.GetFileName(subdir);

            // Check if directory name is valid MMSI
            if (!int.TryParse(dirName, out int mmsi) || !MmsiValidator.IsValid(mmsi))
            {
                _logger.LogWarning("Skipping invalid MMSI folder: {Folder}", dirName);
                continue;
            }

            // Check for CSV files
            var csvFiles = Directory.GetFiles(subdir, "*.csv");
            if (csvFiles.Length == 0)
            {
                _logger.LogWarning("No CSV files in MMSI folder: {MMSI}", mmsi);
                continue;
            }

            // Try loading static data
            var staticDataPath = Path.Combine(subdir, $"{mmsi}.json");
            ShipStaticData staticData = null;

            if (File.Exists(staticDataPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(staticDataPath, cancellationToken);
                    staticData = JsonSerializer.Deserialize<ShipStaticData>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load static data for MMSI {MMSI}", mmsi);
                }
            }

            // Extract date range from CSV filenames
            var (startDate, endDate) = ExtractDateRange(csvFiles);

            vesselInfos.Add(new VesselInfo
            {
                MMSI = mmsi,
                Name = staticData?.ShipName ?? $"Vessel {mmsi}",
                StaticData = staticData,
                AvailableStartDate = startDate,
                AvailableEndDate = endDate,
                PositionFileCount = csvFiles.Length
            });
        }

        _logger.LogInformation("Scanned {Count} vessels from {Path}", 
            vesselInfos.Count, inputFolderPath);

        return vesselInfos;
    }

    private (DateTime start, DateTime end) ExtractDateRange(string[] csvFiles)
    {
        var dates = new List<DateTime>();

        foreach (var file in csvFiles)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            // Filename format: 2025-03-15.csv
            if (DateTime.TryParseExact(filename, "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                dates.Add(date);
            }
        }

        if (dates.Count == 0)
            return (DateTime.MinValue, DateTime.MinValue);

        return (dates.Min(), dates.Max().AddDays(1).AddSeconds(-1));
    }
}
```

### IShipPositionLoader Implementation

**Purpose**: Load and aggregate position records from multiple CSV files

**Implementation:**
```csharp
public class ShipPositionLoader : IShipPositionLoader
{
    private readonly ILogger<ShipPositionLoader> _logger;
    private readonly IPathValidator _pathValidator;
    private readonly ICsvParser<ShipDataOut> _csvParser;

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
```

### ITrackOptimizer Implementation

**Purpose**: Apply Douglas-Peucker algorithm to reduce waypoint count

**Implementation:**
```csharp
public class TrackOptimizer : ITrackOptimizer
{
    private readonly ILogger<TrackOptimizer> _logger;

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
            Lat = p.Lat,
            Lon = p.Lon,
            Speed = p.SOG,
            Heading = p.Heading
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

        // ... (Haversine distance calculation implementation)

        return distance;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
```

### IRouteExporter Implementation

**Purpose**: Export waypoints to XML using template

**Implementation:**
```csharp
public class RouteExporter : IRouteExporter
{
    private readonly ILogger<RouteExporter> _logger;
    private readonly IPathValidator _pathValidator;
    private readonly IFileConflictDialogService _conflictDialog;

    public async Task ExportRouteAsync(
        IEnumerable<RouteWaypoint> waypoints,
        string templatePath,
        string outputFilePath,
        ExportOptions options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= ExportOptions.Default;

        // Validate inputs
        _pathValidator.ValidateTemplatePath(templatePath);
        _pathValidator.ValidateOutputFilePath(outputFilePath);

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
                    break;

                case FileConflictResolution.Overwrite:
                    // Proceed with existing path
                    break;
            }
        }

        // Load template
        var templateXml = await LoadTemplateAsync(templatePath, cancellationToken);

        // Generate WayPoint elements
        var waypointElements = GenerateWaypointElements(waypointList);

        // Inject waypoints into template
        var finalXml = InjectWaypointsIntoTemplate(templateXml, waypointElements);

        // Write to file
        await File.WriteAllTextAsync(outputFilePath, finalXml, cancellationToken);

        _logger.LogInformation(
            "Exported {Count} waypoints to {Path}",
            waypointList.Count, outputFilePath);
    }

    private async Task<XDocument> LoadTemplateAsync(string templatePath, 
        CancellationToken cancellationToken)
    {
        try
        {
            var xml = await File.ReadAllTextAsync(templatePath, cancellationToken);
            return XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("Template XML is invalid", ex);
        }
    }

    private List<XElement> GenerateWaypointElements(List<RouteWaypoint> waypoints)
    {
        var elements = new List<XElement>();

        foreach (var wp in waypoints)
        {
            var element = new XElement("WayPoint",
                new XElement("Time", wp.Time.ToString("o")), // ISO 8601
                new XElement("Lat", wp.Lat.ToString("F6", CultureInfo.InvariantCulture)),
                new XElement("Lon", wp.Lon.ToString("F6", CultureInfo.InvariantCulture)),
                new XElement("Speed", wp.Speed.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement("Heading", wp.Heading.ToString())
            );

            elements.Add(element);
        }

        return elements;
    }

    private XDocument InjectWaypointsIntoTemplate(XDocument template, 
        List<XElement> waypointElements)
    {
        // Find <Route> element and inject waypoints
        var routeElement = template.Descendants("Route").FirstOrDefault();
        if (routeElement == null)
            throw new InvalidOperationException("Template missing <Route> element");

        // Remove existing WayPoint elements (if any)
        routeElement.Elements("WayPoint").Remove();

        // Add new waypoints
        routeElement.Add(waypointElements);

        return template;
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
            newPath = Path.Combine(dir, $"{filenameNoExt}_{suffix}{ext}");
            suffix++;
        }
        while (File.Exists(newPath));

        return newPath;
    }
}
```

## Data Flow Patterns

### Typical Operation Flow

```
MainViewModel (Presentation)
    ↓ SelectInputFolderCommand
ISourceDataScanner.ScanInputFolderAsync()
    → Returns: IEnumerable<VesselInfo>
    ↓ Populate AvailableVessels ObservableCollection
User Selects Vessel
    ↓ SelectedVessel property changed
IShipStaticDataLoader.LoadStaticDataAsync()
    → Returns: ShipStaticData
    ↓ Display in StaticDataDisplay property
User Adjusts Time Interval
    ↓ CreateTrackCommand
IShipPositionLoader.LoadPositionsAsync()
    → Returns: IEnumerable<ShipDataOut>
    ↓ Pass to optimizer
ITrackOptimizer.OptimizeTrackAsync()
    → Returns: IEnumerable<RouteWaypoint>
    ↓ Populate GeneratedWaypoints ObservableCollection
User Selects Output Folder
    ↓ ExportRouteCommand
IRouteExporter.ExportRouteAsync()
    → File written to disk
    ↓ Success message in StatusMessage
```

### Asynchronous Patterns

**Async/Await Throughout:**
```csharp
public async Task ExecuteCreateTrackAsync()
{
    try
    {
        StatusMessage = "Loading position data...";

        var positions = await _positionLoader.LoadPositionsAsync(
            SelectedVessel.MMSI,
            TimeInterval.StartTime,
            TimeInterval.StopTime,
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

        GeneratedWaypoints = new ObservableCollection<RouteWaypoint>(waypoints);
        StatusMessage = $"Track created: {waypoints.Count()} waypoints from {positions.Count()} records";
    }
    catch (OperationCanceledException)
    {
        StatusMessage = "Operation cancelled";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create track");
        StatusMessage = $"Error: {ex.Message}";
    }
}
```

### Progress Reporting Pattern

**Progress Classes:**
```csharp
public class LoadProgress
{
    public int ProcessedFiles { get; set; }
    public int TotalFiles { get; set; }
    public int RecordsLoaded { get; set; }
}

public class OptimizationProgress
{
    public int ProcessedPoints { get; set; }
    public int TotalPoints { get; set; }
}
```

**Usage in ViewModel:**
```csharp
var progress = new Progress<LoadProgress>(p => 
{
    // Update UI on UI thread automatically
    StatusMessage = $"Loading: {p.ProcessedFiles}/{p.TotalFiles} files";
});

await _positionLoader.LoadPositionsAsync(..., progress, cancellationToken);
```

### Cancellation Pattern

**ViewModel Management:**
```csharp
public class MainViewModel : ObservableObject
{
    private CancellationTokenSource _cancellationTokenSource;

    [RelayCommand]
    private async Task CreateTrackAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await ExecuteCreateTrackAsync();
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    private void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
    }
}
```

**Service Support:**
```csharp
public async Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(
    ..., 
    CancellationToken cancellationToken = default)
{
    foreach (var csvFile in csvFiles)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Process file
    }
}
```

## Error Handling Strategies

### Layered Error Handling

**Service Layer:**
- Throws specific exceptions (FileNotFoundException, InvalidOperationException, etc.)
- Logs errors with context
- Does not catch general exceptions

**ViewModel Layer:**
- Catches exceptions from services
- Translates to user-friendly messages
- Updates UI state (StatusMessage, error dialogs)

**Infrastructure Layer:**
- Validates inputs (PathValidator, MmsiValidator)
- Throws ArgumentException for invalid inputs
- Wraps external library exceptions

### Exception Types

```csharp
// Custom exceptions
public class CsvParseException : Exception
{
    public string FilePath { get; }
    public int LineNumber { get; }

    public CsvParseException(string message, string filePath, int lineNumber, Exception inner = null)
        : base(message, inner)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
    }
}

public class XmlTemplateException : Exception
{
    public string TemplatePath { get; }

    public XmlTemplateException(string message, string templatePath, Exception inner = null)
        : base(message, inner)
    {
        TemplatePath = templatePath;
    }
}
```

### Error Handling Examples

**File Not Found:**
```csharp
try
{
    var positions = await _positionLoader.LoadPositionsAsync(...);
}
catch (FileNotFoundException ex)
{
    StatusMessage = "Error: No position data files found for selected time range";
    _logger.LogWarning(ex, "CSV files not found for MMSI {MMSI}", mmsi);
}
```

**CSV Parse Error:**
```csharp
catch (CsvParseException ex)
{
    StatusMessage = $"Error: Invalid CSV format in {Path.GetFileName(ex.FilePath)} at line {ex.LineNumber}";
    _logger.LogError(ex, "CSV parse error");
}
```

**Permission Denied:**
```csharp
catch (UnauthorizedAccessException ex)
{
    StatusMessage = "Error: Permission denied. Check folder access rights.";
    _logger.LogError(ex, "Access denied to {Path}", path);
}
```

**General Fallback:**
```csharp
catch (Exception ex)
{
    StatusMessage = $"Unexpected error: {ex.Message}";
    _logger.LogError(ex, "Unexpected error in operation");
}
```

## Performance Considerations

### Async I/O Operations

**File Reading:**
- Use `File.ReadAllTextAsync()` for small files (JSON static data, XML template)
- Use streaming for large files (CSV position data)

```csharp
public async IAsyncEnumerable<ShipDataOut> StreamPositionsAsync(
    string csvPath,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    using var reader = new StreamReader(csvPath);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    await foreach (var record in csv.GetRecordsAsync<ShipDataOut>(cancellationToken))
    {
        yield return record;
    }
}
```

### Parallel Processing

**Multiple CSV Files:**
```csharp
var tasks = csvFiles.Select(file => 
    _csvParser.ParseFileAsync(file, cancellationToken));

var results = await Task.WhenAll(tasks);
var allPositions = results.SelectMany(r => r).ToList();
```

**Caution**: Limit parallelism to avoid I/O contention:
```csharp
var semaphore = new SemaphoreSlim(4); // Max 4 concurrent reads

var tasks = csvFiles.Select(async file =>
{
    await semaphore.WaitAsync(cancellationToken);
    try
    {
        return await _csvParser.ParseFileAsync(file, cancellationToken);
    }
    finally
    {
        semaphore.Release();
    }
});

var results = await Task.WhenAll(tasks);
```

### Memory Optimization

**Streaming vs. Buffering:**
- For position loading: Buffer in memory (typically < 10K records per vessel-day)
- For large datasets: Use `IAsyncEnumerable` to stream records

**Douglas-Peucker Optimization:**
- Use `bool[]` for keep flags instead of list operations
- Process in-place to avoid allocations

### Caching Strategies

**Static Data Caching:**
```csharp
public class CachedShipStaticDataLoader : IShipStaticDataLoader
{
    private readonly IShipStaticDataLoader _inner;
    private readonly ConcurrentDictionary<int, ShipStaticData> _cache = new();

    public async Task<ShipStaticData> LoadStaticDataAsync(int mmsi, string inputFolderPath, 
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(mmsi, out var cached))
            return cached;

        var data = await _inner.LoadStaticDataAsync(mmsi, inputFolderPath, cancellationToken);
        _cache.TryAdd(mmsi, data);
        return data;
    }
}
```

**Registration:**
```csharp
services.AddSingleton<IShipStaticDataLoader, CachedShipStaticDataLoader>();
```

### UI Responsiveness

**Long-Running Operations:**
- All service calls return `Task` for async execution
- Use `Progress<T>` to update UI during operations
- Support cancellation via `CancellationToken`

**Background Thread Work:**
```csharp
// CPU-bound optimization runs on thread pool
return await Task.Run(() => 
{
    return DouglasPeuckerOptimize(positions, tolerance);
}, cancellationToken);
```

## Testing Integration Patterns

### Service Mocking

**Using Moq:**
```csharp
[Test]
public async Task CreateTrack_WithValidData_GeneratesWaypoints()
{
    // Arrange
    var mockLoader = new Mock<IShipPositionLoader>();
    mockLoader.Setup(m => m.LoadPositionsAsync(
        It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), 
        It.IsAny<string>(), null, default))
        .ReturnsAsync(CreateTestPositions());

    var mockOptimizer = new Mock<ITrackOptimizer>();
    mockOptimizer.Setup(m => m.OptimizeTrackAsync(
        It.IsAny<IEnumerable<ShipDataOut>>(), null, null, default))
        .ReturnsAsync(CreateTestWaypoints());

    var viewModel = new MainViewModel(mockLoader.Object, mockOptimizer.Object, ...);

    // Act
    await viewModel.CreateTrackCommand.ExecuteAsync(null);

    // Assert
    Assert.That(viewModel.GeneratedWaypoints.Count, Is.EqualTo(50));
    mockLoader.Verify(m => m.LoadPositionsAsync(...), Times.Once);
    mockOptimizer.Verify(m => m.OptimizeTrackAsync(...), Times.Once);
}
```

### Integration Testing

**Real Services with Test Data:**
```csharp
[Test]
public async Task EndToEnd_LoadOptimizeExport_ProducesValidXml()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
    services.AddSingleton<IShipPositionLoader, ShipPositionLoader>();
    services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
    services.AddSingleton<IRouteExporter, RouteExporter>();

    var provider = services.BuildServiceProvider();

    var testInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
    var testOutputPath = Path.Combine(Path.GetTempPath(), "test_route.xml");

    // Act
    var scanner = provider.GetRequiredService<ISourceDataScanner>();
    var vessels = await scanner.ScanInputFolderAsync(testInputPath);

    var loader = provider.GetRequiredService<IShipPositionLoader>();
    var positions = await loader.LoadPositionsAsync(
        205196000, 
        new DateTime(2025, 3, 15, 6, 0, 0),
        new DateTime(2025, 3, 15, 18, 0, 0),
        testInputPath);

    var optimizer = provider.GetRequiredService<ITrackOptimizer>();
    var waypoints = await optimizer.OptimizeTrackAsync(positions);

    var exporter = provider.GetRequiredService<IRouteExporter>();
    await exporter.ExportRouteAsync(
        waypoints,
        Path.Combine(testInputPath, "route_waypoint_template.xml"),
        testOutputPath);

    // Assert
    Assert.That(File.Exists(testOutputPath), Is.True);

    var xml = XDocument.Load(testOutputPath);
    var waypointElements = xml.Descendants("WayPoint").ToList();
    Assert.That(waypointElements.Count, Is.GreaterThan(0));
}
```

## Dependency Injection Setup

### Service Registration

**Program.cs / App.xaml.cs:**
```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core Services
        services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
        services.AddSingleton<IShipPositionLoader, ShipPositionLoader>();
        services.AddSingleton<IShipStaticDataLoader, CachedShipStaticDataLoader>();
        services.AddSingleton<ITrackOptimizer, TrackOptimizer>();
        services.AddSingleton<IRouteExporter, RouteExporter>();

        // Infrastructure
        services.AddSingleton<IPathValidator, PathValidator>();
        services.AddSingleton<ICsvParser<ShipDataOut>, SecurePositionCsvParser>();

        // UI Services
        services.AddSingleton<IFolderDialogService, FolderDialogService>();
        services.AddSingleton<IFileConflictDialogService, FileConflictDialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
```

### Constructor Injection

**ViewModel Example:**
```csharp
public class MainViewModel : ObservableObject
{
    private readonly ISourceDataScanner _scanner;
    private readonly IShipPositionLoader _positionLoader;
    private readonly ITrackOptimizer _trackOptimizer;
    private readonly IRouteExporter _routeExporter;
    private readonly IFolderDialogService _folderDialog;
    private readonly ILogger<MainViewModel> _logger;

    public MainViewModel(
        ISourceDataScanner scanner,
        IShipPositionLoader positionLoader,
        ITrackOptimizer trackOptimizer,
        IRouteExporter routeExporter,
        IFolderDialogService folderDialog,
        ILogger<MainViewModel> logger)
    {
        _scanner = scanner;
        _positionLoader = positionLoader;
        _trackOptimizer = trackOptimizer;
        _routeExporter = routeExporter;
        _folderDialog = folderDialog;
        _logger = logger;

        // Initialize commands
        SelectInputFolderCommand = new AsyncRelayCommand(SelectInputFolderAsync);
        CreateTrackCommand = new AsyncRelayCommand(CreateTrackAsync, CanCreateTrack);
        ExportRouteCommand = new AsyncRelayCommand(ExportRouteAsync, CanExportRoute);
    }

    // ... command implementations
}
```

## External System Integration (Future)

### Potential External APIs

**AIS Data Providers:**
- MarineTraffic API
- VesselFinder API
- AISHub API

**Pattern for External API Integration:**
```csharp
public interface IExternalAisDataProvider
{
    Task<IEnumerable<ShipDataOut>> FetchPositionsAsync(
        int mmsi, 
        DateTime startTime, 
        DateTime stopTime,
        CancellationToken cancellationToken = default);
}

public class MarineTrafficApiProvider : IExternalAisDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public async Task<IEnumerable<ShipDataOut>> FetchPositionsAsync(...)
    {
        var url = $"https://api.marinetraffic.com/positions/{mmsi}?start={startTime}&end={stopTime}&apikey={_apiKey}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var positions = JsonSerializer.Deserialize<IEnumerable<ShipDataOut>>(json);

        return positions;
    }
}
```

**Composite Data Source:**
```csharp
public class CompositeShipPositionLoader : IShipPositionLoader
{
    private readonly IShipPositionLoader _localLoader;
    private readonly IExternalAisDataProvider _externalProvider;

    public async Task<IEnumerable<ShipDataOut>> LoadPositionsAsync(...)
    {
        // Try local files first
        try
        {
            return await _localLoader.LoadPositionsAsync(...);
        }
        catch (FileNotFoundException)
        {
            // Fallback to external API
            return await _externalProvider.FetchPositionsAsync(...);
        }
    }
}
```

## References

- Async/Await Best Practices: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
- Dependency Injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
- IProgress<T>: https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1
- CancellationToken: https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken
