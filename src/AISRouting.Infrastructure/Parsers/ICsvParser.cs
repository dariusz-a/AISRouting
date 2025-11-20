namespace AISRouting.Infrastructure.Parsers
{
    /// <summary>
    /// Interface for parsing CSV files.
    /// </summary>
    public interface ICsvParser<T> where T : class
    {
        /// <summary>
        /// Parses a CSV file and returns records of type T.
        /// </summary>
        Task<IEnumerable<T>> ParseFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
