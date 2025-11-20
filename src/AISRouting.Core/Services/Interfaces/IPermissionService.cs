namespace AISRouting.Core.Services.Interfaces
{
    /// <summary>
    /// Service for checking user permissions.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Checks if the current user can create tracks.
        /// </summary>
        bool CanCreateTrack();

        /// <summary>
        /// Gets the reason why a permission was denied.
        /// </summary>
        string GetPermissionDeniedReason(string operation);
    }
}
