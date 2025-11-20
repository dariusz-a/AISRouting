using AISRouting.Core.Services.Interfaces;

namespace AISRouting.Core.Services.Implementations
{
    /// <summary>
    /// Default permission service that allows all operations.
    /// Can be replaced with actual authentication/authorization in the future.
    /// </summary>
    public class AlwaysAllowPermissionService : IPermissionService
    {
        public bool CanCreateTrack()
        {
            return true;
        }

        public string GetPermissionDeniedReason(string operation)
        {
            return string.Empty;
        }
    }
}
