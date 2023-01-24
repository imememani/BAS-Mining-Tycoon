namespace MiningTycoon
{
    /// <summary>
    /// Tycoon utility class to aid with various stuff.
    /// </summary>
    public static class TycoonUtilities
    {
        /// <summary>
        /// Format a doubloon string.
        /// </summary>
        public static string FormatDoubloons(this float value)
        {
            // TODO: Add, a, ab, ac, ad, etc so we can get past the 2bn limit.
            return $"{value:0.00}";
        }
    }
}