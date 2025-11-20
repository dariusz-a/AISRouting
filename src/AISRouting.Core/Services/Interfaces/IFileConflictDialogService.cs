using AISRouting.Core.Models;

namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for displaying file conflict resolution dialogs to the user.
    /// </summary>
    public interface IFileConflictDialogService
    {
        /// <summary>
        /// Shows a dialog prompting the user to resolve a file naming conflict.
        /// </summary>
        /// <param name="filePath">The path to the conflicting file.</param>
        /// <returns>The user's chosen resolution strategy.</returns>
        FileConflictResolution ShowFileConflictDialog(string filePath);
    }
}
