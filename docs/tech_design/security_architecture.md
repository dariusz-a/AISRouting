# Security Architecture

This document covers authentication and authorization framework, data access controls, API security measures, compliance features, and security testing approach for the AisToXmlRouteConvertor application.

## 1. Overview

AisToXmlRouteConvertor is a local desktop application that processes AIS data stored on the user's file system. The application does not include traditional authentication, authorization, or network-based API security since it operates entirely offline with local file access. This document addresses the security considerations relevant to a desktop application handling sensitive maritime navigation data.

## 2. Security Context and Threat Model

### 2.1 Application Profile

- **Deployment**: Local desktop installation on user's workstation
- **Data Location**: Local file system (user-controlled folders)
- **Network Activity**: None (no remote APIs, cloud services, or telemetry)
- **User Model**: Single user per installation, no multi-user support
- **Data Sensitivity**: Maritime vessel positions and routes (potentially sensitive for commercial vessels)

### 2.2 Threat Model

**In-Scope Threats**:
1. **Unauthorized file access**: Malicious users accessing AIS data files or exported XML routes
2. **Data tampering**: Modification of input CSV/JSON files or output XML files
3. **Information disclosure**: Exposure of vessel positions through insecure file permissions
4. **Input validation vulnerabilities**: Malformed input files causing crashes or unexpected behavior
5. **Path traversal**: User-provided folder paths accessing unintended file system locations
6. **Denial of service**: Maliciously large files consuming excessive memory/CPU

**Out-of-Scope Threats** (Not Applicable to Offline Desktop App):
1. Network-based attacks (no network communication)
2. SQL injection (no database)
3. Cross-site scripting (no web interface)
4. Authentication bypass (no authentication system)
5. Session hijacking (no sessions or remote access)

### 2.3 Security Assumptions

- User's operating system provides baseline file system security
- Antivirus/endpoint protection handles malware threats
- User is responsible for physical access control to workstation
- File system encryption (BitLocker, FileVault, LUKS) is user-configured separately

## 3. Data Access Controls

### 3.1 File System Permissions

**Principle**: Application respects OS-level file permissions and never attempts privilege escalation.

**Input Folder Access**:
- **Read-only access**: Application only reads AIS data files (CSV, JSON)
- **Permission check**: Verify folder readability before scanning
- **Error handling**: Display clear error if read permission denied

```csharp
public static IReadOnlyList<long> GetAvailableMmsi(string rootPath)
{
    // Validate folder exists and is readable
    if (!Directory.Exists(rootPath))
    {
        throw new DirectoryNotFoundException($"Input folder not found: {rootPath}");
    }

    try
    {
        // Test read access
        _ = Directory.GetDirectories(rootPath);
    }
    catch (UnauthorizedAccessException ex)
    {
        throw new UnauthorizedAccessException(
            $"Cannot access input folder. Check permissions: {rootPath}", ex);
    }

    // Proceed with scan...
}
```

**Output Folder Access**:
- **Write permission verification**: Check before attempting export
- **Graceful failure**: Display actionable error message if write denied
- **No privilege escalation**: Never attempt to modify folder permissions

```csharp
public static string ExportRoute(/* parameters */)
{
    // Verify output folder is writable
    if (!IsDirectoryWritable(outputFolder))
    {
        throw new UnauthorizedAccessException(
            "Selected folder is not writable. Choose a different folder.");
    }

    // Proceed with export...
}

private static bool IsDirectoryWritable(string folderPath)
{
    try
    {
        // Attempt to create and delete a temporary file
        string testFile = Path.Combine(folderPath, $".write_test_{Guid.NewGuid()}");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);
        return true;
    }
    catch
    {
        return false;
    }
}
```

### 3.2 Path Traversal Prevention

**Risk**: User-provided folder paths could include ".." or absolute paths attempting to access restricted areas.

**Mitigation**:
- Validate and canonicalize all user-provided paths
- Reject paths containing directory traversal patterns
- Use `Path.GetFullPath()` to resolve relative paths
- Verify resolved path is within expected boundaries

```csharp
private static string ValidateAndCanonicalizePath(string userPath)
{
    if (string.IsNullOrWhiteSpace(userPath))
        throw new ArgumentException("Path cannot be empty");

    // Resolve to absolute path
    string fullPath = Path.GetFullPath(userPath);

    // Check for suspicious patterns (additional defense layer)
    if (fullPath.Contains("..") || fullPath.Contains("~"))
    {
        throw new SecurityException("Path contains invalid traversal patterns");
    }

    return fullPath;
}
```

### 3.3 Secure File Operations

**Principles**:
- Use secure file deletion (overwrite before delete) for sensitive temporary files
- Avoid creating world-readable temporary files
- Clean up all temporary files on error or application exit

**Implementation**:
```csharp
private static void SecureDeleteFile(string filePath)
{
    if (!File.Exists(filePath)) return;

    try
    {
        // Overwrite with random data before deletion
        var random = new Random();
        byte[] buffer = new byte[4096];
        using (var fs = File.OpenWrite(filePath))
        {
            long length = fs.Length;
            while (length > 0)
            {
                random.NextBytes(buffer);
                int toWrite = (int)Math.Min(buffer.Length, length);
                fs.Write(buffer, 0, toWrite);
                length -= toWrite;
            }
        }

        File.Delete(filePath);
    }
    catch (Exception ex)
    {
        // Log error but don't crash
        Console.WriteLine($"Failed to securely delete file: {ex.Message}");
    }
}
```

## 4. Input Validation and Sanitization

### 4.1 File Format Validation

**CSV File Validation**:
- Verify file extension is `.csv`
- Check file size before loading (reject files > 100 MB per file as potential DoS)
- Validate CSV header row matches expected schema
- Skip malformed rows with logging (don't crash on bad data)

```csharp
public static IReadOnlyList<ShipState> ParsePositions(string csvFilePath)
{
    // Validate file exists and is reasonable size
    var fileInfo = new FileInfo(csvFilePath);
    if (!fileInfo.Exists)
        throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

    if (fileInfo.Length > 100 * 1024 * 1024) // 100 MB limit
    {
        throw new InvalidOperationException(
            $"CSV file too large (>{100}MB): {csvFilePath}");
    }

    // Validate extension
    if (!csvFilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
    {
        throw new ArgumentException($"Invalid file extension (expected .csv): {csvFilePath}");
    }

    // Proceed with parsing...
}
```

**JSON File Validation**:
- Verify file extension is `.json`
- Limit file size (reject files > 10 MB)
- Handle deserialization errors gracefully
- Validate MMSI is 9-digit positive integer

```csharp
public static ShipStaticData? ParseShipStatic(string jsonFilePath)
{
    var fileInfo = new FileInfo(jsonFilePath);
    if (!fileInfo.Exists) return null;

    if (fileInfo.Length > 10 * 1024 * 1024) // 10 MB limit
    {
        throw new InvalidOperationException(
            $"JSON file too large (>{10}MB): {jsonFilePath}");
    }

    try
    {
        string jsonText = File.ReadAllText(jsonFilePath);
        var data = JsonSerializer.Deserialize<ShipStaticData>(jsonText);

        // Validate MMSI format
        if (data != null && (data.Mmsi < 100000000 || data.Mmsi > 999999999))
        {
            throw new InvalidOperationException(
                $"Invalid MMSI format (must be 9 digits): {data.Mmsi}");
        }

        return data;
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Malformed JSON in {jsonFilePath}: {ex.Message}");
        return null; // Graceful failure
    }
}
```

### 4.2 Data Type Validation

**Geographic Coordinates**:
- Latitude: Must be in range [-90.0, 90.0]
- Longitude: Must be in range [-180.0, 180.0]
- Reject records with out-of-range coordinates

```csharp
private static bool IsValidLatitude(double lat) => lat >= -90.0 && lat <= 90.0;
private static bool IsValidLongitude(double lon) => lon >= -180.0 && lon <= 180.0;

public static ShipState ParseShipStateRow(CsvRow row)
{
    double lat = row.GetDouble("Latitude");
    double lon = row.GetDouble("Longitude");

    if (!IsValidLatitude(lat) || !IsValidLongitude(lon))
    {
        throw new InvalidDataException(
            $"Invalid coordinates: lat={lat}, lon={lon}");
    }

    // Continue parsing...
}
```

**Timestamp Validation**:
- Must be valid UTC datetime
- Reject timestamps far in the future (> 1 year from now)
- Reject timestamps before year 2000 (unlikely for modern AIS)

```csharp
private static bool IsValidTimestamp(DateTime timestamp)
{
    DateTime now = DateTime.UtcNow;
    DateTime minDate = new DateTime(2000, 1, 1, DateTimeKind.Utc);
    DateTime maxDate = now.AddYears(1);

    return timestamp >= minDate && timestamp <= maxDate;
}
```

### 4.3 Resource Limits

**Memory Protection**:
- Limit maximum number of positions loaded (e.g., 1 million records)
- Stream CSV parsing to avoid loading entire file into memory
- Clear large collections when no longer needed

```csharp
private const int MaxPositionRecords = 1_000_000;

public static IReadOnlyList<ShipState> LoadShipStates(
    string vesselFolder, 
    TimeInterval interval)
{
    var positions = new List<ShipState>();

    foreach (var csvFile in GetCsvFilesInRange(vesselFolder, interval))
    {
        foreach (var state in ParsePositionsStreaming(csvFile))
        {
            if (positions.Count >= MaxPositionRecords)
            {
                throw new InvalidOperationException(
                    $"Exceeded maximum position limit ({MaxPositionRecords})");
            }

            if (state.TimestampUtc >= interval.StartUtc && 
                state.TimestampUtc <= interval.EndUtc)
            {
                positions.Add(state);
            }
        }
    }

    return positions;
}
```

## 5. Output Security

### 5.1 XML Generation Security

**XML Injection Prevention**:
- Use `System.Xml.Linq` for structured generation (automatically escapes content)
- Never concatenate raw strings into XML
- Validate all data before insertion

```csharp
public static string ExportToXml(
    IReadOnlyList<RouteWaypoint> waypoints,
    long mmsi,
    TimeInterval interval,
    string outputFolder)
{
    // Use XElement for safe construction (auto-escapes)
    var root = new XElement("RouteTemplate",
        new XAttribute("Name", mmsi)); // Automatically escaped

    foreach (var waypoint in waypoints)
    {
        root.Add(new XElement("WayPoint",
            new XAttribute("Seq", waypoint.Sequence),
            new XAttribute("Lat", waypoint.Latitude),
            new XAttribute("Lon", waypoint.Longitude),
            // ... additional attributes
        ));
    }

    var doc = new XDocument(
        new XDeclaration("1.0", "utf-8", null),
        root);

    // Save with proper encoding
    doc.Save(outputPath, SaveOptions.None);
    return outputPath;
}
```

**File Naming Security**:
- Sanitize MMSI and timestamp components
- Use only alphanumeric characters and underscores in filenames
- Prevent directory traversal in filename

```csharp
private static string GenerateSafeFilename(long mmsi, TimeInterval interval)
{
    // MMSI already validated as 9-digit number
    string mmsiStr = mmsi.ToString();

    // Format timestamps safely (no special characters)
    string startStr = interval.StartUtc.ToString("yyyyMMddTHHmmss");
    string endStr = interval.EndUtc.ToString("yyyyMMddTHHmmss");

    // Construct filename with safe characters only
    string filename = $"{mmsiStr}_{startStr}_{endStr}.xml";

    // Double-check for any path separators (defense in depth)
    if (filename.Contains(Path.DirectorySeparatorChar) || 
        filename.Contains(Path.AltDirectorySeparatorChar))
    {
        throw new SecurityException("Generated filename contains invalid path characters");
    }

    return filename;
}
```

### 5.2 Output File Permissions

**Windows**:
- Respect inherited folder permissions
- Don't explicitly set world-readable permissions
- Use default security descriptors

**macOS/Linux**:
- Create files with `0600` (owner read/write only) or `0644` (owner read/write, others read) permissions
- Respect umask settings

```csharp
// On Unix-like systems, set restrictive permissions after creation
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    // Use .NET 7+ UnixFileMode API
    File.SetUnixFileMode(outputPath, 
        UnixFileMode.UserRead | UnixFileMode.UserWrite);
}
```

## 6. Error Handling and Information Disclosure

### 6.1 Secure Error Messages

**Principle**: Error messages should be actionable for users without disclosing sensitive system information.

**Good Error Messages**:
- "Cannot access input folder. Check permissions."
- "Selected folder is not writable. Choose a different folder."
- "Invalid CSV format in file 2025-03-15.csv"

**Bad Error Messages (Avoid)**:
- "Access denied to C:\Users\alice\Documents\SecretVesselData\205196000\" (discloses full path)
- "SQL error: connection string invalid" (not applicable but illustrates info leak)
- "Unhandled exception: System.IO.FileNotFoundException at line 142 in Parser.cs" (stack traces to users)

**Implementation**:
```csharp
public static IReadOnlyList<ShipState> LoadShipStates(
    string vesselFolder, 
    TimeInterval interval)
{
    try
    {
        // Attempt load
        return LoadShipStatesInternal(vesselFolder, interval);
    }
    catch (UnauthorizedAccessException)
    {
        // User-friendly message, no sensitive details
        throw new ApplicationException(
            "Cannot access vessel data folder. Check file permissions.");
    }
    catch (FileNotFoundException ex)
    {
        // Show filename only, not full path
        string filename = Path.GetFileName(ex.FileName);
        throw new ApplicationException(
            $"Required file not found: {filename}");
    }
    catch (Exception ex)
    {
        // Log full details for diagnostics
        Console.WriteLine($"Unexpected error loading ship states: {ex}");
        
        // Show generic message to user
        throw new ApplicationException(
            "An error occurred loading AIS data. Check the application log for details.");
    }
}
```

### 6.2 Logging Security

**Principle**: Logs may contain sensitive information and should be protected.

**Logging Best Practices**:
- **Don't log**: Full file paths, MMSI (if considered sensitive), vessel names
- **Do log**: File basenames, error types, record counts, timestamps
- **Sanitize**: Replace sensitive fields with placeholders

**Implementation**:
```csharp
// Bad: Logs full path and MMSI
_logger.LogInformation($"Loading data for MMSI {mmsi} from {vesselFolder}");

// Good: Logs minimal information
_logger.LogInformation($"Loading data for vessel from folder");

// Good: Logs counts for diagnostics without sensitive data
_logger.LogInformation($"Loaded {positions.Count} position records in time range");
```

### 6.3 Exception Handling Strategy

**Three-Layer Approach**:

1. **Internal Layer**: Throw specific exceptions with full details for debugging
2. **Service Layer**: Catch specific exceptions, log details, rethrow user-friendly versions
3. **UI Layer**: Display user-friendly messages, offer recovery actions

```csharp
// UI Layer (ViewModel)
[RelayCommand]
private void LoadPositions()
{
    try
    {
        Positions = Helper.LoadShipStates(VesselFolder, TimeInterval).ToList();
        StatusMessage = $"Loaded {Positions.Count} positions";
    }
    catch (ApplicationException ex)
    {
        // User-friendly error already prepared
        StatusMessage = $"Error: {ex.Message}";
    }
    catch (Exception ex)
    {
        // Unexpected error
        _logger.LogError(ex, "Unexpected error loading positions");
        StatusMessage = "An unexpected error occurred. Please try again.";
    }
}
```

## 7. Compliance Considerations

### 7.1 Data Privacy (GDPR/CCPA)

**Applicability**: AIS data may contain personally identifiable information (PII) if vessel ownership is traceable to individuals.

**Compliance Measures**:
- **Data minimization**: Application processes only necessary fields (position, speed, heading)
- **No cloud storage**: All data remains on user's local machine
- **No telemetry**: Application does not transmit usage data or analytics
- **User control**: User explicitly selects input/output folders and data processing parameters

**Right to erasure**: User can delete input/output files directly via OS file manager (application does not prevent deletion).

### 7.2 Maritime Data Security

**Sensitive Information**:
- Vessel positions reveal trade routes, port visits, operational patterns
- Speed and course data may indicate cargo type or urgency
- Aggregated data could reveal fleet operations

**Protection Measures**:
- **Local-only processing**: No transmission to external systems
- **Output file protection**: User responsible for securing exported XML files
- **User guidance**: Documentation advises users to protect AIS data per company policy

### 7.3 Export Control

**Consideration**: Navigation systems and route optimization could be subject to export control regulations in some jurisdictions.

**Mitigation**:
- Application documentation includes notice about potential export restrictions
- No built-in encryption or obfuscation (avoids cryptography export concerns)
- Source code openly available (not proprietary encryption)

## 8. Security Testing Approach

### 8.1 Unit Tests for Security

**Path Validation Tests**:
```csharp
[Theory]
[InlineData("../../etc/passwd")]
[InlineData("C:\\Windows\\System32")]
[InlineData("~/../../root/.ssh")]
public void ValidatePath_TraversalAttempt_ThrowsException(string maliciousPath)
{
    // Act & Assert
    Assert.Throws<SecurityException>(() => 
        ValidateAndCanonicalizePath(maliciousPath));
}
```

**Input Validation Tests**:
```csharp
[Theory]
[InlineData(-91.0, 0.0)] // Latitude too low
[InlineData(91.0, 0.0)]  // Latitude too high
[InlineData(0.0, -181.0)] // Longitude too low
[InlineData(0.0, 181.0)]  // Longitude too high
public void ParsePosition_InvalidCoordinates_ThrowsException(double lat, double lon)
{
    // Arrange
    var invalidRow = CreateCsvRow(lat, lon);

    // Act & Assert
    Assert.Throws<InvalidDataException>(() => 
        CsvParser.ParseShipStateRow(invalidRow));
}
```

**File Size Limit Tests**:
```csharp
[Fact]
public void ParsePositions_FileTooLarge_ThrowsException()
{
    // Arrange: Create file exceeding size limit
    string largeCsvPath = CreateLargeTestFile(101 * 1024 * 1024); // 101 MB

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => 
        CsvParser.ParsePositions(largeCsvPath));
}
```

### 8.2 Integration Tests for Security

**Unauthorized Access Tests**:
```csharp
[Fact]
public void ScanFolder_NoReadPermission_ThrowsException()
{
    // Arrange: Create folder with no read permissions (Unix)
    string restrictedFolder = CreateRestrictedFolder(UnixFileMode.None);

    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() => 
        Helper.GetAvailableMmsi(restrictedFolder));
}

[Fact]
public void ExportRoute_NoWritePermission_ThrowsException()
{
    // Arrange: Create read-only output folder
    string readOnlyFolder = CreateReadOnlyFolder();

    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() => 
        Helper.ExportRoute(waypoints, mmsi, interval, readOnlyFolder));
}
```

**Malicious Input Tests**:
```csharp
[Fact]
public void ParsePositions_MalformedCsv_SkipsBadRowsContinuesProcessing()
{
    // Arrange: CSV with mixed valid and invalid rows
    string csvPath = CreateMixedValidityCsv();

    // Act
    var positions = CsvParser.ParsePositions(csvPath);

    // Assert: Valid rows parsed, invalid rows skipped
    Assert.True(positions.Count > 0);
    Assert.True(positions.Count < GetTotalRowCount(csvPath));
}
```

### 8.3 Manual Security Testing

**Checklist for Release**:

1. **Path Traversal**:
   - [ ] Try selecting folder with ".." in path
   - [ ] Try entering UNC paths (Windows)
   - [ ] Try symbolic links to restricted folders

2. **File Size Attacks**:
   - [ ] Load CSV file > 100 MB
   - [ ] Load JSON file > 10 MB
   - [ ] Select folder with 10,000+ subfolders

3. **Malformed Data**:
   - [ ] Load CSV with missing required columns
   - [ ] Load CSV with invalid UTF-8 encoding
   - [ ] Load JSON with syntax errors
   - [ ] Load JSON with wrong MMSI format

4. **Permission Issues**:
   - [ ] Select input folder with no read permission
   - [ ] Select output folder with no write permission
   - [ ] Export to folder that becomes read-only during processing

5. **Error Message Review**:
   - [ ] Verify no full paths shown in error dialogs
   - [ ] Verify no stack traces shown to users
   - [ ] Verify error messages are actionable

## 9. Secure Deployment

### 9.1 Code Signing

**Windows**:
- Sign executable with authenticode certificate
- Users can verify publisher identity
- SmartScreen reputation builds over time

**macOS**:
- Sign application bundle with Apple Developer ID
- Notarize with Apple for Gatekeeper approval
- Users can verify signature: `codesign -dv --verbose=4 AisToXmlRouteConvertor.app`

### 9.2 Dependency Security

**NuGet Package Verification**:
- Use only well-maintained, reputable packages
- Review package dependencies for vulnerabilities
- Enable NuGet audit in project file:

```xml
<PropertyGroup>
  <NuGetAudit>true</NuGetAudit>
  <NuGetAuditMode>all</NuGetAuditMode>
  <NuGetAuditLevel>low</NuGetAuditLevel>
</PropertyGroup>
```

**Regular Updates**:
- Monitor for security advisories in dependencies
- Update packages regularly (monthly security review)
- Test updates before deployment

### 9.3 Installation Security

**Installer Best Practices**:
- Don't require admin privileges for per-user install
- Use OS-provided installer frameworks (MSI, .app bundle, .deb)
- Don't bundle unrelated software or adware
- Provide hash (SHA-256) for download verification

## 10. Incident Response

### 10.1 Vulnerability Disclosure

**Process**:
1. Users report security issues to dedicated email: security@example.com
2. Acknowledge receipt within 24 hours
3. Investigate and develop fix within 7 days for critical, 30 days for low severity
4. Release patched version
5. Publish security advisory with CVE if applicable

### 10.2 User Notification

**Critical Vulnerabilities**:
- Email notification to registered users (if contact info available)
- Prominent notice on download page
- In-app update notification (if future versions add update mechanism)

### 10.3 Security Audit Trail

**Logging for Forensics**:
- Application version and startup time
- Folders accessed (without full paths)
- File operations (read/write counts)
- Errors and exceptions (sanitized)

**Log Protection**:
- Store logs in user-specific folder (avoid global logs)
- Rotate logs (keep last 10 files, max 10 MB each)
- Don't log sensitive data (MMSIs, vessel names, full paths)

## 11. Future Security Enhancements

### 11.1 Phase 2 Considerations

**File Encryption**:
- Optional AES-256 encryption for exported XML files
- User-provided passphrase (not stored by application)
- Use .NET `ProtectedData` API for key derivation

**Digital Signatures**:
- Sign exported XML files with user's certificate
- Verify signature on import (if import feature added)

**Audit Logging**:
- Detailed audit log of all file accesses
- Tamper-evident log file (hash chain)
- Export audit log in standard format (CEF, JSON)

### 11.2 Not Planned (Out of Scope)

- Multi-user authentication/authorization (single-user app)
- Network communication security (no network features)
- Database encryption (no database)
- Role-based access control (no roles)

## 12. Summary

The AisToXmlRouteConvertor security architecture focuses on protecting local file system operations, validating input data, and preventing information disclosure. As a local desktop application with no network communication or authentication requirements, security measures emphasize input validation, resource limits, secure file handling, and user-friendly error messages that don't leak sensitive information. The application respects OS-level security controls and provides clear guidance for users to protect sensitive maritime navigation data. Comprehensive security testing ensures robustness against malicious input and improper file access attempts.
