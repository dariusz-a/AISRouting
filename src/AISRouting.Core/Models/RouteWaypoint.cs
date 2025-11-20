namespace AISRouting.Core.Models
{
    /// <summary>
    /// Represents an optimized waypoint for route generation and XML export.
    /// </summary>
    public class RouteWaypoint
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public double Speed { get; set; }
        public long ETA { get; set; }
        public int Delay { get; set; }
        public string Mode { get; set; } = string.Empty;
        public string TrackMode { get; set; } = "Track";
        public int Heading { get; set; }
        public double PortXTE { get; set; } = 20;
        public double StbdXTE { get; set; } = 20;
        public double MinSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public DateTime Time { get; set; }
    }
}
