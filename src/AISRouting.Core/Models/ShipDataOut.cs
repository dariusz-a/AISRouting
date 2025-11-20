namespace AISRouting.Core.Models
{
    /// <summary>
    /// Represents a single AIS position report from CSV files.
    /// </summary>
    public class ShipDataOut
    {
        public long Time { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public int? NavigationalStatusIndex { get; set; }
        public double? ROT { get; set; }
        public double? SOG { get; set; }
        public double? COG { get; set; }
        public int? Heading { get; set; }
        public double? Draught { get; set; }
        public int? DestinationIndex { get; set; }
        public long? EtaSecondsUntil { get; set; }

        /// <summary>
        /// The base date used for time calculation (T0 at 00:00:00 UTC).
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// Gets the absolute timestamp by adding Time seconds to BaseDate.
        /// </summary>
        public DateTime BaseDateTime => BaseDate.AddSeconds(Time);

        /// <summary>
        /// Gets whether this position has valid coordinates.
        /// </summary>
        public bool IsValid => Lat.HasValue && Lon.HasValue;
    }
}
