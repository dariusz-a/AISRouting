namespace AISRouting.Core.Models
{
    /// <summary>
    /// Options for track optimization algorithms.
    /// </summary>
    public class OptimizationOptions
    {
        public double ToleranceMeters { get; set; } = 50.0;

        public static OptimizationOptions Default => new OptimizationOptions();
    }
}
