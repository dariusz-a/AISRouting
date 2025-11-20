namespace AISRouting.Core.Models
{
    /// <summary>
    /// Defines strategies for resolving file naming conflicts during export operations.
    /// </summary>
    public enum FileConflictResolution
    {
        /// <summary>
        /// Cancel the export operation.
        /// </summary>
        Cancel,

        /// <summary>
        /// Overwrite the existing file.
        /// </summary>
        Overwrite,

        /// <summary>
        /// Append a numeric suffix to create a unique filename.
        /// </summary>
        AppendSuffix
    }
}
