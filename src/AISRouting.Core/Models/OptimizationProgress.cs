namespace AISRouting.Core.Models
{
    /// <summary>
    /// Represents progress during track optimization.
    /// </summary>
    public class OptimizationProgress
    {
        public int ProcessedPoints { get; set; }
        public int TotalPoints { get; set; }
        public int DefaultedHeadingCount { get; set; }
        public int DefaultedSOGCount { get; set; }
    }
}
