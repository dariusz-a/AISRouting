namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for validating file system paths and permissions.
    /// </summary>
    public interface IPathValidator
    {
        /// <summary>
        /// Validates that an output file path is writable.
        /// </summary>
        /// <param name="path">The directory path to validate.</param>
        /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when directory is not writable.</exception>
        void ValidateOutputFilePath(string path);
    }
}
