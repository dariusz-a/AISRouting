namespace AISRouting.Infrastructure.Validation
{
    /// <summary>
    /// Path validator implementation for file system operations.
    /// </summary>
    public class PathValidator : IPathValidator
    {
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
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Output directory not found: {directory}");
        }
    }
}
