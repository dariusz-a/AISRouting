using Microsoft.Extensions.Logging;

namespace AISRouting.Infrastructure.Validation
{
    /// <summary>
    /// Path validator implementation for file system operations.
    /// </summary>
    public class PathValidator : IPathValidator, Core.Services.Interfaces.IPathValidator
    {
        private readonly ILogger<PathValidator> _logger;

        public PathValidator(ILogger<PathValidator> logger)
        {
            _logger = logger;
        }

        public void ValidateInputFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

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
}
