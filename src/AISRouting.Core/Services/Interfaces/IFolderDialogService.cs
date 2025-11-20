namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Abstracts folder browsing dialog functionality for testability.
    /// </summary>
    public interface IFolderDialogService
    {
        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="initialDirectory">Optional initial directory to display.</param>
        /// <returns>Selected folder path or null if cancelled.</returns>
        string? ShowFolderBrowser(string? initialDirectory = null);
    }
}
