namespace AISRouting.Infrastructure.Validation
{
    /// <summary>
    /// Interface for validating file system paths.
    /// </summary>
    public interface IPathValidator
    {
        /// <summary>
        /// Validates that an input folder path exists and is accessible.
        /// </summary>
        void ValidateInputFolderPath(string path);

        /// <summary>
        /// Validates that an output file path is writable.
        /// </summary>
        void ValidateOutputFilePath(string path);
    }
}
