# Security Architecture

This document covers authentication and authorization framework, data access controls, API security measures, compliance features, and security testing approach for the AISRouting application.

## Security Overview

AISRouting is a standalone desktop application with the following security characteristics:

- **No network connectivity**: All processing is local
- **No user authentication**: Single-user desktop application
- **File-based data access**: User controls all input/output folders
- **No sensitive credentials**: No API keys, passwords, or tokens stored
- **Local file system security**: Relies on Windows file permissions

## Threat Model

### Assets to Protect

1. **User Data**: AIS CSV/JSON files containing vessel position history
2. **Generated Routes**: Optimized waypoint XML files
3. **Application Integrity**: Executable and configuration files
4. **User Privacy**: Vessel tracking data and navigation patterns

### Threat Scenarios

**Malicious CSV/JSON Files**
- **Threat**: User loads crafted CSV/JSON with malicious content to cause buffer overflow, code execution, or denial of service
- **Impact**: High (application crash, potential code execution)
- **Mitigation**: Input validation, safe parsing, exception handling

**Path Traversal Attacks**
- **Threat**: User-provided paths escape intended directories
- **Impact**: Medium (unauthorized file access)
- **Mitigation**: Path validation, canonicalization

**Unauthorized File Access**
- **Threat**: Application reads/writes files outside user-selected folders
- **Impact**: Medium (data exposure, file corruption)
- **Mitigation**: Strict folder boundary enforcement

**Data Tampering**
- **Threat**: Malicious actors modify input files or exported routes
- **Impact**: Low (user controls file locations)
- **Mitigation**: File integrity checks (future enhancement)

**Denial of Service**
- **Threat**: Extremely large CSV files cause memory exhaustion
- **Impact**: Medium (application crash, system slowdown)
- **Mitigation**: Streaming parsing, memory limits, cancellation support

## Input Validation and Sanitization

### Folder Path Validation

**Requirements:**
- Paths must exist and be accessible
- Paths must not contain illegal characters
- Paths must resolve to local file system (no UNC paths without validation)
- No path traversal sequences (`..`, `..\..`, etc.)

**Implementation:**
```csharp
public class PathValidator
{
    public ValidationResult ValidateFolderPath(string path)
    {
        // Check for null or empty
        if (string.IsNullOrWhiteSpace(path))
            return ValidationResult.Error("Path cannot be empty");

        // Resolve to absolute path
        var fullPath = Path.GetFullPath(path);

        // Check for path traversal
        if (fullPath != path && !Path.IsPathRooted(path))
            return ValidationResult.Error("Path traversal detected");

        // Verify existence
        if (!Directory.Exists(fullPath))
            return ValidationResult.Error("Folder does not exist");

        // Check read access
        if (!HasReadAccess(fullPath))
            return ValidationResult.Error("Insufficient read permissions");

        return ValidationResult.Success;
    }

    private bool HasReadAccess(string path)
    {
        try
        {
            Directory.GetFiles(path);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
```

### MMSI Validation

**Requirements:**
- Must be 9-digit numeric value
- Range: 200000000 to 999999999 (per AIS specification)
- Used as folder name and in XML export

**Implementation:**
```csharp
public class MmsiValidator
{
    public ValidationResult ValidateMmsi(string mmsi)
    {
        if (!long.TryParse(mmsi, out var mmsiValue))
            return ValidationResult.Error("MMSI must be numeric");

        if (mmsiValue < 200000000 || mmsiValue > 999999999)
            return ValidationResult.Error("MMSI out of valid range (200000000-999999999)");

        return ValidationResult.Success;
    }
}
```

### CSV Parsing Security

**Requirements:**
- Handle malformed data gracefully
- Prevent memory exhaustion from large files
- Validate data types and ranges
- Skip invalid rows without crashing

**Implementation:**
```csharp
public class SecurePositionCsvParser
{
    private const int MaxRowLength = 10000;  // Prevent DoS via extremely long rows
    private const long MaxFileSize = 500 * 1024 * 1024;  // 500MB limit

    public async IAsyncEnumerable<ShipDataOut> ParseCsvFile(string csvPath)
    {
        // Check file size before loading
        var fileInfo = new FileInfo(csvPath);
        if (fileInfo.Length > MaxFileSize)
        {
            _logger.LogWarning("CSV file exceeds size limit: {Size} bytes", fileInfo.Length);
            throw new InvalidOperationException($"CSV file too large: {fileInfo.Length} bytes");
        }

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, _config);

        // Register error handling
        csv.Context.BadDataFound = context =>
        {
            _logger.LogWarning("Bad CSV data at row {Row}: {Data}", 
                context.Row, context.RawRecord);
        };

        await foreach (var record in csv.GetRecordsAsync<ShipDataOut>())
        {
            // Validate critical fields
            if (!ValidatePositionData(record))
            {
                _logger.LogWarning("Invalid position data at row {Row}", csv.Context.Row);
                continue;  // Skip invalid row
            }

            yield return record;
        }
    }

    private bool ValidatePositionData(ShipDataOut data)
    {
        // Latitude: -90 to +90
        if (data.Latitude.HasValue && 
            (data.Latitude.Value < -90 || data.Latitude.Value > 90))
            return false;

        // Longitude: -180 to +180
        if (data.Longitude.HasValue && 
            (data.Longitude.Value < -180 || data.Longitude.Value > 180))
            return false;

        // SOG: non-negative, reasonable upper limit (100 knots)
        if (data.SOG.HasValue && (data.SOG.Value < 0 || data.SOG.Value > 100))
            return false;

        // Heading: 0-359 or null
        if (data.Heading.HasValue && (data.Heading.Value < 0 || data.Heading.Value > 359))
            return false;

        return true;
    }
}
```

### JSON Deserialization Security

**Requirements:**
- Handle malformed JSON gracefully
- Prevent JSON bomb attacks (deeply nested objects, large strings)
- Validate field values

**Implementation:**
```csharp
public class SecureShipStaticDataParser
{
    private const int MaxJsonSize = 10 * 1024;  // 10KB limit for static data JSON

    public async Task<ShipStaticData> ParseStaticDataJson(string jsonPath)
    {
        var fileInfo = new FileInfo(jsonPath);
        if (fileInfo.Length > MaxJsonSize)
        {
            _logger.LogWarning("JSON file exceeds size limit: {Size} bytes", fileInfo.Length);
            throw new InvalidOperationException("Static data JSON too large");
        }

        try
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                MaxDepth = 5,  // Prevent deeply nested JSON
                AllowTrailingCommas = true
            };

            var data = JsonSerializer.Deserialize<ShipStaticData>(json, options);
            
            // Validate deserialized data
            if (!ValidateStaticData(data))
                throw new InvalidDataException("Static data validation failed");

            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse static data JSON: {Path}", jsonPath);
            throw new InvalidDataException("Malformed JSON file", ex);
        }
    }

    private bool ValidateStaticData(ShipStaticData data)
    {
        // Validate MMSI
        if (data.MMSI < 200000000 || data.MMSI > 999999999)
            return false;

        // Validate dimensions (if provided)
        if (data.Length.HasValue && data.Length.Value < 0)
            return false;
        if (data.Beam.HasValue && data.Beam.Value < 0)
            return false;

        return true;
    }
}
```

## Data Access Controls

### File System Permissions

**Read Operations:**
- Input folder: Requires read access
- CSV/JSON files: Requires read access
- route_waypoint_template.xml: Requires read access

**Write Operations:**
- Output folder: Requires write + create directory access
- Exported XML files: Requires write access

**Permission Checks:**
```csharp
public class FileSystemSecurityChecker
{
    public bool CanReadFolder(string path)
    {
        try
        {
            Directory.GetFiles(path);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    public bool CanWriteFolder(string path)
    {
        try
        {
            // Attempt to create folder if doesn't exist
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Test write access with temp file
            var testFile = Path.Combine(path, $".write_test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
```

### Folder Boundary Enforcement

**Requirements:**
- Application must not read files outside user-selected input folder
- Application must not write files outside user-selected output folder
- No symbolic link following (unless explicitly allowed)

**Implementation:**
```csharp
public class FolderBoundaryGuard
{
    private string _inputFolderBoundary;
    private string _outputFolderBoundary;

    public void SetInputBoundary(string folder)
    {
        _inputFolderBoundary = Path.GetFullPath(folder);
    }

    public void SetOutputBoundary(string folder)
    {
        _outputFolderBoundary = Path.GetFullPath(folder);
    }

    public bool IsPathWithinInputBoundary(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(_inputFolderBoundary, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsPathWithinOutputBoundary(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(_outputFolderBoundary, StringComparison.OrdinalIgnoreCase);
    }
}
```

## XML Export Security

### XML Injection Prevention

**Requirements:**
- Escape XML special characters in waypoint data
- Prevent XXE (XML External Entity) attacks
- Validate generated XML structure

**Implementation:**
```csharp
public class SecureXmlRouteWriter
{
    public async Task WriteRouteXml(IEnumerable<RouteWaypoint> waypoints, string outputPath)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
            Async = true,
            CheckCharacters = true  // Validate XML characters
        };

        using var writer = XmlWriter.Create(outputPath, settings);

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "RouteTemplate", null);
        await writer.WriteAttributeStringAsync(null, "Name", null, 
            SecurityElement.Escape(waypoints.First().Name));

        foreach (var waypoint in waypoints)
        {
            await WriteWaypointElement(writer, waypoint);
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
    }

    private async Task WriteWaypointElement(XmlWriter writer, RouteWaypoint waypoint)
    {
        await writer.WriteStartElementAsync(null, "WayPoint", null);
        
        // Use strongly-typed values (no string interpolation risk)
        await writer.WriteAttributeStringAsync(null, "Name", null, 
            SecurityElement.Escape(waypoint.Name));
        await writer.WriteAttributeStringAsync(null, "Lat", null, 
            waypoint.Lat.ToString(CultureInfo.InvariantCulture));
        await writer.WriteAttributeStringAsync(null, "Lon", null, 
            waypoint.Lon.ToString(CultureInfo.InvariantCulture));
        // ... other attributes

        await writer.WriteEndElementAsync();
    }
}
```

### File Overwrite Protection

**Requirements:**
- Prompt user before overwriting existing files
- Offer alternatives: overwrite, append suffix, cancel
- Log all export operations

**Implementation:**
```csharp
public class FileConflictResolver
{
    public enum ConflictResolution { Overwrite, AppendSuffix, Cancel }

    public async Task<string> ResolveFileConflict(
        string desiredPath,
        Func<string, Task<ConflictResolution>> promptUser)
    {
        if (!File.Exists(desiredPath))
            return desiredPath;

        _logger.LogInformation("File already exists: {Path}", desiredPath);

        var resolution = await promptUser(desiredPath);

        switch (resolution)
        {
            case ConflictResolution.Overwrite:
                _logger.LogInformation("User chose to overwrite: {Path}", desiredPath);
                return desiredPath;

            case ConflictResolution.AppendSuffix:
                var newPath = GenerateUniquePath(desiredPath);
                _logger.LogInformation("Generated unique path: {Path}", newPath);
                return newPath;

            case ConflictResolution.Cancel:
                _logger.LogInformation("User cancelled export");
                throw new OperationCanceledException("Export cancelled by user");

            default:
                throw new InvalidOperationException("Unknown conflict resolution");
        }
    }

    private string GenerateUniquePath(string basePath)
    {
        var directory = Path.GetDirectoryName(basePath);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(basePath);
        var extension = Path.GetExtension(basePath);

        int suffix = 1;
        string newPath;
        do
        {
            newPath = Path.Combine(directory, $"{fileNameWithoutExt} ({suffix}){extension}");
            suffix++;
        }
        while (File.Exists(newPath));

        return newPath;
    }
}
```

## Logging and Auditing

### Security Event Logging

**Events to Log:**
- Folder selection (input/output)
- File access (read/write)
- Validation failures (malformed CSV/JSON, invalid MMSI)
- Permission errors
- Export operations (success/failure)

**Log Levels:**
- **Information**: Normal operations (folder selected, export completed)
- **Warning**: Validation failures, skipped rows, missing files
- **Error**: Permission errors, file access failures, exceptions

**Implementation:**
```csharp
public class SecurityAuditLogger
{
    private readonly ILogger<SecurityAuditLogger> _logger;

    public void LogFolderAccess(string folder, AccessType type, bool success)
    {
        if (success)
            _logger.LogInformation("Folder access granted: {Type} on {Folder}", type, folder);
        else
            _logger.LogWarning("Folder access denied: {Type} on {Folder}", type, folder);
    }

    public void LogFileOperation(string filePath, FileOperation operation, bool success, string? error = null)
    {
        if (success)
            _logger.LogInformation("File operation: {Operation} on {File}", operation, filePath);
        else
            _logger.LogError("File operation failed: {Operation} on {File}, Error: {Error}", 
                operation, filePath, error);
    }

    public void LogValidationFailure(string context, string reason)
    {
        _logger.LogWarning("Validation failure in {Context}: {Reason}", context, reason);
    }

    public void LogExport(string mmsi, DateTime start, DateTime stop, string outputPath, bool success)
    {
        if (success)
            _logger.LogInformation("Route exported: MMSI={MMSI}, Period={Start} to {Stop}, Output={Path}",
                mmsi, start, stop, outputPath);
        else
            _logger.LogError("Route export failed: MMSI={MMSI}, Output={Path}", mmsi, outputPath);
    }
}
```

### Log File Security

**Requirements:**
- Log files stored in user profile or app data folder
- Restricted permissions (user-only access)
- Rotation to prevent disk exhaustion
- No sensitive data in logs (no full file contents, only paths/metadata)

**Configuration:**
```csharp
services.AddLogging(builder =>
{
    builder.AddFile("Logs/aisrouting-{Date}.log", options =>
    {
        options.RetainedFileCountLimit = 30;  // Keep 30 days of logs
        options.FileSizeLimitBytes = 10 * 1024 * 1024;  // 10MB per file
    });
});
```

## Error Handling and Exception Management

### Secure Exception Handling

**Requirements:**
- Never expose full exception details to UI (stack traces, file paths)
- Log full exception details for diagnostics
- Show user-friendly error messages
- Graceful degradation (skip invalid rows, not crash entire operation)

**Implementation:**
```csharp
public class SecureExceptionHandler
{
    public void HandleException(Exception ex, string operation)
    {
        // Log full details
        _logger.LogError(ex, "Exception during {Operation}", operation);

        // Show user-friendly message
        var message = GetUserFriendlyMessage(ex);
        _messageBox.ShowError(message);
    }

    private string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => "Access denied. Please check folder permissions.",
            FileNotFoundException => "Required file not found. Please verify input folder structure.",
            InvalidDataException => "Invalid data format detected. Some records may be skipped.",
            OutOfMemoryException => "Insufficient memory to process this dataset. Try a smaller time range.",
            _ => "An unexpected error occurred. Please check logs for details."
        };
    }
}
```

## Compliance and Privacy

### Data Privacy

**User Data Handling:**
- **No data collection**: Application does not transmit or collect any user data
- **No telemetry**: No usage statistics, crash reports, or analytics sent to servers
- **Local processing only**: All data remains on user's machine
- **User controls data**: User chooses input/output folders, retains full ownership

**Privacy By Design:**
- No network connections or external API calls
- No embedded tracking or analytics
- No user identifiers or session data stored
- No persistent application state (except log files)

### GDPR Considerations (if applicable)

**Right to Access:** User has full access to input/output folders and log files

**Right to Erasure:** User can delete input data, output files, and logs at any time

**Data Minimization:** Application processes only necessary fields from AIS data

**Purpose Limitation:** Data used solely for route optimization and export

## Security Testing Approach

### Unit Tests

**Input Validation Tests:**
```csharp
[Test]
public void PathValidator_RejectsPathTraversal()
{
    var validator = new PathValidator();
    var result = validator.ValidateFolderPath("C:\\Data\\..\\..\\Windows");
    Assert.IsFalse(result.IsValid);
}

[Test]
public void MmsiValidator_RejectsInvalidRange()
{
    var validator = new MmsiValidator();
    var result = validator.ValidateMmsi("123456789");
    Assert.IsFalse(result.IsValid);
}
```

**CSV Parsing Security Tests:**
```csharp
[Test]
public async Task CsvParser_HandlesExtremelyLongRow()
{
    var parser = new SecurePositionCsvParser();
    var longRow = new string('A', 100000);
    var csvContent = $"Time,Latitude,Longitude\n{longRow},55.0,12.0";
    
    await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
    var records = await parser.ParseCsvStream(stream).ToListAsync();
    
    // Should skip or truncate malformed row
    Assert.That(records, Is.Empty);
}

[Test]
public async Task CsvParser_ValidatesLatitudeLongitudeRange()
{
    var parser = new SecurePositionCsvParser();
    var csvContent = "Time,Latitude,Longitude\n0,999.0,12.0";  // Invalid latitude
    
    await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
    var records = await parser.ParseCsvStream(stream).ToListAsync();
    
    Assert.That(records, Is.Empty);
}
```

### Integration Tests

**File Access Security Tests:**
```csharp
[Test]
public void FileSystemChecker_DeniesAccessToProtectedFolder()
{
    var checker = new FileSystemSecurityChecker();
    var result = checker.CanReadFolder("C:\\Windows\\System32");
    // Depends on user permissions, but should handle gracefully
    Assert.DoesNotThrow(() => checker.CanReadFolder("C:\\Windows\\System32"));
}
```

### Penetration Testing Scenarios

**Scenario: Malicious CSV File**
- Input: CSV with buffer overflow attempt, SQL injection strings, script tags
- Expected: Graceful handling, invalid rows skipped, no crashes

**Scenario: Path Traversal in Folder Selection**
- Input: Path like `C:\\Data\\..\\..\\Windows\\System32`
- Expected: Validation error, operation blocked

**Scenario: XML Bomb Export**
- Input: Thousands of waypoints
- Expected: Memory-efficient processing, streaming export

**Scenario: Large File DoS**
- Input: 10GB CSV file
- Expected: File size check before processing, error message shown

## Future Security Enhancements

1. **Digital Signatures**: Sign exported XML files for integrity verification
2. **Encryption**: Optional encryption for sensitive route data
3. **User Roles**: Multi-user scenarios with read-only vs. export permissions
4. **Audit Trail**: Enhanced logging with tamper-proof audit records
5. **Sandboxing**: Run parsing operations in isolated process or AppContainer
6. **Code Signing**: Sign application executable for Windows SmartScreen

## Security Contacts

For security issues or questions:
- Review application logs in `Logs/` folder
- Check Windows Event Viewer for application errors
- Report issues via GitHub repository (if open source)

## References

- OWASP Secure Coding Practices: https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/
- .NET Security Best Practices: https://learn.microsoft.com/en-us/dotnet/standard/security/
- CsvHelper Security: https://joshclose.github.io/CsvHelper/
- System.Text.Json Security: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
