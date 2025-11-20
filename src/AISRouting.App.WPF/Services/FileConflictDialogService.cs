using System.IO;
using System.Windows;
using AISRouting.Core.Models;
using AISRouting.Core.Services.Interfaces;

namespace AISRouting.App.WPF.Services
{
    /// <summary>
    /// Service for displaying file conflict resolution dialogs.
    /// </summary>
    public class FileConflictDialogService : IFileConflictDialogService
    {
        public FileConflictResolution ShowFileConflictDialog(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                $"File '{filename}' already exists.\n\nClick OK to overwrite, Cancel to create a new file with a suffix, or close this dialog to cancel the export.",
                "File Conflict",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                return FileConflictResolution.Overwrite;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                // Ask if they want to append suffix or cancel completely
                var suffixResult = MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Create a new file with a numeric suffix (e.g., '{Path.GetFileNameWithoutExtension(filename)} (1){Path.GetExtension(filename)}')?",
                    "Create New File",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return suffixResult == MessageBoxResult.Yes 
                    ? FileConflictResolution.AppendSuffix 
                    : FileConflictResolution.Cancel;
            }

            return FileConflictResolution.Cancel;
        }
    }
}
