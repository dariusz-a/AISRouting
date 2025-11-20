namespace AISRouting.Core.Models
{
    /// <summary>
    /// Configuration options for route export operations.
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Default export options with standard settings.
        /// </summary>
        public static ExportOptions Default { get; } = new ExportOptions();

        /// <summary>
        /// File conflict resolution strategy to use when target file already exists.
        /// </summary>
        public FileConflictResolution ConflictResolution { get; set; } = FileConflictResolution.Cancel;

        /// <summary>
        /// Whether to include XML declaration in the output.
        /// </summary>
        public bool IncludeXmlDeclaration { get; set; } = true;

        /// <summary>
        /// Whether to format the XML output with indentation.
        /// </summary>
        public bool FormatOutput { get; set; } = true;
    }
}
