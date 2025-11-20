namespace AISRouting.Core.Models
{
    /// <summary>
    /// Represents progress during CSV position data loading.
    /// </summary>
    public class LoadProgress
    {
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public int RecordsLoaded { get; set; }
        public int SkippedRecords { get; set; }
    }
}
