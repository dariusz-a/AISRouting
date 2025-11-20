namespace AISRouting.Infrastructure.Validation
{
    /// <summary>
    /// Validator for MMSI values.
    /// </summary>
    public static class MmsiValidator
    {
        /// <summary>
        /// Validates that an MMSI is a positive 9-digit number.
        /// </summary>
        public static void Validate(int mmsi)
        {
            if (mmsi < 100000000 || mmsi > 999999999)
                throw new ArgumentException($"MMSI must be a 9-digit number. Got: {mmsi}", nameof(mmsi));
        }
    }
}
