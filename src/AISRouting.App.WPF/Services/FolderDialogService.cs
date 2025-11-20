using AISRouting.Core.Services.Interfaces;
using Ookii.Dialogs.Wpf;

namespace AISRouting.App.WPF.Services
{
    /// <summary>
    /// Implements folder browsing dialog using Ookii.Dialogs.Wpf.
    /// </summary>
    public class FolderDialogService : IFolderDialogService
    {
        public string? ShowFolderBrowser(string? initialDirectory = null)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select AIS data input folder",
                UseDescriptionForTitle = true
            };

            if (!string.IsNullOrEmpty(initialDirectory))
            {
                dialog.SelectedPath = initialDirectory;
            }

            return dialog.ShowDialog() == true ? dialog.SelectedPath : null;
        }
    }
}
