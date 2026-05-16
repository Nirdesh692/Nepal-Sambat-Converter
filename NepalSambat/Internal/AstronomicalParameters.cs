namespace NepalSambat.Internal
{
    /// <summary>
    /// Global parameters for Nepal Sambat astronomical calculations.
    /// </summary>
    public static class AstronomicalParameters
    {
        /// <summary>
        /// Empirical correction (days) applied to the new moon calculation.
        /// </summary>
        public static double NewMoonCorrectionDays = -0.36181;

        /// <summary>
        /// Empirical correction (minutes) applied to the sunrise calculation.
        /// </summary>
        public static double SunriseCorrectionMinutes = 5.0;

        /// <summary>
        /// Kathmandu latitude in degrees.
        /// </summary>
        public const double KathmanduLatDeg = 27.7172;

        /// <summary>
        /// Kathmandu longitude in degrees.
        /// </summary>
        public const double KathmanduLonDeg = 85.3240;
    }
}
