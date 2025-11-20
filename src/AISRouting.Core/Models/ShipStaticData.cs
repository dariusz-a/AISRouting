namespace AISRouting.Core.Models
{
    /// <summary>
    /// Represents static data for a vessel including metadata and date range of available AIS data.
    /// </summary>
    public class ShipStaticData
    {
        public long MMSI { get; set; }
        public string? Name { get; set; }
        public double? Length { get; set; }
        public double? Beam { get; set; }
        public double? Draught { get; set; }
        public int? TypeCode { get; set; }
        public string? CallSign { get; set; }
        public long? IMO { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets a user-friendly display name for the vessel.
        /// </summary>
        public string DisplayName => Name ?? $"Vessel {MMSI}";

        public ShipStaticData()
        {
        }

        public ShipStaticData(long mmsi, string folderPath)
        {
            MMSI = mmsi;
            FolderPath = folderPath;
        }
    }
}
