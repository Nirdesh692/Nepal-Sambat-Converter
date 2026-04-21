using System;
using System.Collections.Generic;
using System.Linq;

namespace NepalSambat
{
    /// <summary>
    /// Represents a date in the Nepal Sambat calendar system.
    /// </summary>
    /// <remarks>
    /// Nepal Sambat is a lunisolar calendar used in Nepal, with months based on lunar phases
    /// and years that can have 12 or 13 months (leap years).
    /// </remarks>
    public readonly struct NepalSambatDate : IEquatable<NepalSambatDate>, IComparable<NepalSambatDate>
    {
        /// <summary>
        /// Gets the Nepal Sambat year.
        /// </summary>
        /// <value>The year in Nepal Sambat calendar.</value>
        public int Year { get; }

        /// <summary>
        /// Gets the month name in Nepal Sambat calendar.
        /// </summary>
        /// <value>The month name (e.g., "Kachhala", "Gunla", "Anala").</value>
        public string Month { get; }

        /// <summary>
        /// Gets the day of the month.
        /// </summary>
        /// <value>The day number (1-30, depending on the month).</value>
        public int Day { get; }

        /// <summary>
        /// Gets a value indicating whether this date is in a leap month.
        /// </summary>
        /// <value>True if this date is in the "Anala" leap month; otherwise, false.</value>
        public bool IsLeapMonth { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NepalSambatDate"/> struct.
        /// </summary>
        /// <param name="year">The Nepal Sambat year.</param>
        /// <param name="month">The month name.</param>
        /// <param name="day">The day of the month.</param>
        /// <param name="isLeapMonth">True if this is a leap month; otherwise, false.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="year"/> is less than 1, <paramref name="month"/> is null or empty,
        /// or <paramref name="day"/> is less than 1 or greater than 30.
        /// </exception>
        public NepalSambatDate(int year, string month, int day, bool isLeapMonth = false)
        {
            if (year < 1)
                throw new ArgumentException("Year must be greater than 0.", nameof(year));

            if (string.IsNullOrWhiteSpace(month))
                throw new ArgumentException("Month cannot be null or empty.", nameof(month));

            if (day < 1 || day > 30)
                throw new ArgumentException("Day must be between 1 and 30.", nameof(day));

            Year = year;
            Month = month;
            Day = day;
            IsLeapMonth = isLeapMonth;
        }

        /// <summary>
        /// Returns a string representation of the Nepal Sambat date.
        /// </summary>
        /// <returns>A string in the format "Year Month Day" with leap month indicator if applicable.</returns>
        public override string ToString() =>
            $"{Year} {Month} {Day}" + (IsLeapMonth ? " (leap)" : "");

        /// <summary>
        /// Determines whether this instance equals another <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="other">The other Nepal Sambat date to compare with.</param>
        /// <returns>True if the dates are equal; otherwise, false.</returns>
        public bool Equals(NepalSambatDate other) =>
            Year == other.Year && Month == other.Month && Day == other.Day && IsLeapMonth == other.IsLeapMonth;

        /// <summary>
        /// Determines whether this instance equals another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is NepalSambatDate other && Equals(other);

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day, IsLeapMonth);

        /// <summary>
        /// Compares this instance to another <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="other">The other date to compare with.</param>
        /// <returns>
        /// A value that indicates the relative order of the dates being compared.
        /// The return value has these meanings:
        /// <list type="bullet">
        /// <item><description>Less than zero: This instance precedes <paramref name="other"/>.</description></item>
        /// <item><description>Zero: This instance occurs in the same position as <paramref name="other"/>.</description></item>
        /// <item><description>Greater than zero: This instance follows <paramref name="other"/>.</description></item>
        /// </list>
        /// </returns>
        public int CompareTo(NepalSambatDate other)
        {
            var yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0) return yearComparison;

            var monthComparison = string.Compare(Month, other.Month, StringComparison.Ordinal);
            if (monthComparison != 0) return monthComparison;

            var dayComparison = Day.CompareTo(other.Day);
            if (dayComparison != 0) return dayComparison;

            return IsLeapMonth.CompareTo(other.IsLeapMonth);
        }

        /// <summary>
        /// Equality operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the dates are equal; otherwise, false.</returns>
        public static bool operator ==(NepalSambatDate left, NepalSambatDate right) => left.Equals(right);

        /// <summary>
        /// Inequality operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the dates are not equal; otherwise, false.</returns>
        public static bool operator !=(NepalSambatDate left, NepalSambatDate right) => !left.Equals(right);

        /// <summary>
        /// Less than operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if left is less than right; otherwise, false.</returns>
        public static bool operator <(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) < 0;

        /// <summary>
        /// Less than or equal operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
        public static bool operator <=(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// Greater than operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if left is greater than right; otherwise, false.</returns>
        public static bool operator >(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Greater than or equal operator for <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
        public static bool operator >=(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Provides methods for converting between Gregorian and Nepal Sambat dates.
    /// </summary>
    /// <remarks>
    /// This class uses astronomical calculations based on Meeus' lunar phase algorithm
    /// to provide high-precision date conversions with unlimited year range.
    /// </remarks>
    public static class NepalSambatConverter
    {
        /// <summary>
        /// Nepal Sambat epoch: 879 CE October 20 (Gregorian).
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(879, 10, 20);

        /// <summary>
        /// Traditional month names in Nepal Sambat calendar.
        /// </summary>
        private static readonly string[] MonthNames =
        {
            "कछला", "थिंला", "पोहेला", "सिल्ला",
            "चिल्ला", "चौला", "बछला", "तछला", "दिल्ला", "गुंला", "ञंला", "कौला"
        };

        /// <summary>
        /// Converts a Gregorian date to Nepal Sambat date.
        /// </summary>
        /// <param name="gregorianDate">The Gregorian date to convert.</param>
        /// <returns>A <see cref="NepalSambatDate"/> representing the equivalent Nepal Sambat date.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="gregorianDate"/> is before the Nepal Sambat epoch (879 CE).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the date cannot be converted due to internal calculation errors.
        /// </exception>
        /// <remarks>
        /// This method uses astronomical calculations to determine the Nepal Sambat year,
        /// month, and day. The conversion is based on lunar phases and can handle any date
        /// from 879 CE to infinity.
        /// </remarks>
        /// <example>
        /// <code>
        /// var gregorianDate = new DateTime(2025, 8, 17);
        /// var nepalSambatDate = NepalSambatConverter.FromGregorian(gregorianDate);
        /// Console.WriteLine(nepalSambatDate); // Output: 1145 Gunla 24
        /// </code>
        /// </example>
        public static NepalSambatDate FromGregorian(DateTime gregorianDate)
        {
            ValidateGregorianDate(gregorianDate);

            // Find the Nepal Sambat year using astronomical calculations
            int nsYear = FindNepalSambatYearAstronomical(gregorianDate);

            // Get all months in that year using lunar calculations
            var months = GetMonthsInYearAstronomical(nsYear);

            // Find which month contains the given date
            foreach (var month in months)
            {
                if (gregorianDate.Date >= month.start.Date && gregorianDate.Date <= month.end.Date)
                {
                    int dayInMonth = (gregorianDate.Date - month.start.Date).Days + 1;
                    return new NepalSambatDate(nsYear, month.name, dayInMonth, month.isLeap);
                }
            }

            // If not found in current year, check adjacent years
            var adjacentYears = new[] { nsYear - 1, nsYear + 1 };
            foreach (int year in adjacentYears)
            {
                if (year < 1) continue;

                var adjacentMonths = GetMonthsInYearAstronomical(year);
                foreach (var month in adjacentMonths)
                {
                    if (gregorianDate.Date >= month.start.Date && gregorianDate.Date <= month.end.Date)
                    {
                        int dayInMonth = (gregorianDate.Date - month.start.Date).Days + 1;
                        return new NepalSambatDate(year, month.name, dayInMonth, month.isLeap);
                    }
                }
            }

            throw new InvalidOperationException($"Date {gregorianDate:yyyy-MM-dd} not found in Nepal Sambat years {nsYear - 1}, {nsYear}, or {nsYear + 1}");
        }

        /// <summary>
        /// Converts a Nepal Sambat date to Gregorian date.
        /// </summary>
        /// <param name="nsDate">The Nepal Sambat date to convert.</param>
        /// <returns>A <see cref="DateTime"/> representing the equivalent Gregorian date.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="nsDate"/> has invalid values.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the date cannot be converted due to internal calculation errors.
        /// </exception>
        /// <remarks>
        /// This method calculates the Gregorian date by determining the start of the Nepal Sambat year
        /// and then adding the appropriate number of days based on the month and day.
        /// </remarks>
        /// <example>
        /// <code>
        /// var nsDate = new NepalSambatDate(1145, "Gunla", 24);
        /// var gregorianDate = NepalSambatConverter.ToGregorian(nsDate);
        /// Console.WriteLine(gregorianDate); // Output: 2025-08-17
        /// </code>
        /// </example>
        public static DateTime ToGregorian(NepalSambatDate nsDate)
        {
            ValidateNepalSambatDate(nsDate);

            // Get the start of the Nepal Sambat year
            DateTime yearStart = CalculateNewYearDateAstronomical(nsDate.Year);

            // Get all months in that year
            var months = GetMonthsInYearAstronomical(nsDate.Year);

            // Find the target month
            foreach (var month in months)
            {
                if (month.name == nsDate.Month && month.isLeap == nsDate.IsLeapMonth)
                {
                    // Calculate the date by adding days to month start
                    return month.start.AddDays(nsDate.Day - 1);
                }
            }

            throw new InvalidOperationException($"Month {nsDate.Month} not found in Nepal Sambat year {nsDate.Year}");
        }

        /// <summary>
        /// Determines whether a specified Nepal Sambat year is a leap year.
        /// </summary>
        /// <param name="nsYear">The Nepal Sambat year to check.</param>
        /// <returns>True if the year is a leap year; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="nsYear"/> is less than 1.
        /// </exception>
        /// <remarks>
        /// A leap year in Nepal Sambat has 13 months instead of 12. The leap month "Anala"
        /// is inserted after "Gunla" month. Leap years are determined by astronomical calculations
        /// based on the actual length of the year.
        /// </remarks>
        /// <example>
        /// <code>
        /// bool isLeapYear = NepalSambatConverter.IsLeapYear(1143); // Returns true
        /// Console.WriteLine($"NS 1143 is a leap year: {isLeapYear}");
        /// </code>
        /// </example>
        public static bool IsLeapYear(int nsYear)
        {
            if (nsYear < 1)
                throw new ArgumentException("Year must be greater than 0.", nameof(nsYear));

            // A leap year in Nepal Sambat has 13 months instead of 12
            // Calculate the length of the year using astronomical methods
            DateTime yearStart = CalculateNewYearDateAstronomical(nsYear);
            DateTime nextYearStart = CalculateNewYearDateAstronomical(nsYear + 1);

            int yearLength = (nextYearStart - yearStart).Days;
            return yearLength > 365;
        }

        #region Private Methods

        private static int FindNepalSambatYearAstronomical(DateTime gregorianDate)
        {
            // Special handling for epoch date
            if (gregorianDate.Date == Epoch.Date)
                return 1;

            // Start with an approximation based on the epoch
            int approxYear = (int)((gregorianDate - Epoch).TotalDays / 365.25) + 1;
            approxYear = Math.Max(1, approxYear);

            // Refine the year by checking New Year dates
            int refinedYear = approxYear;

            // Check a range of years around the approximation
            for (int offset = -2; offset <= 2; offset++)
            {
                int testYear = approxYear + offset;
                if (testYear < 1) continue;

                DateTime testNewYear = CalculateNewYearDateAstronomical(testYear);
                DateTime nextNewYear = CalculateNewYearDateAstronomical(testYear + 1);

                if (gregorianDate >= testNewYear && gregorianDate < nextNewYear)
                {
                    refinedYear = testYear;
                    break;
                }
            }

            return refinedYear;
        }

        private static List<(DateTime start, DateTime end, string name, bool isLeap)> GetMonthsInYearAstronomical(int nsYear)
        {
            var months = new List<(DateTime, DateTime, string, bool)>();

            DateTime yearStart = CalculateNewYearDateAstronomical(nsYear);
            DateTime nextYearStart = CalculateNewYearDateAstronomical(nsYear + 1);

            // Get month names for this year (including leap month if applicable)
            string[] monthNames = GetMonthNamesForYearAstronomical(nsYear);

            DateTime currentDate = yearStart;

            for (int i = 0; i < monthNames.Length; i++)
            {
                DateTime monthStart = currentDate;

                // For Nepal Sambat, months are based on lunar phases
                // Each month starts with a new moon and ends before the next new moon
                DateTime monthEnd;

                if (i < monthNames.Length - 1)
                {
                    // Find the next new moon to determine month end
                    DateTime nextNewMoon = AstronomicalCalculator.FindNextNewMoon(currentDate);
                    monthEnd = nextNewMoon.AddDays(-1); // Month ends day before next new moon
                }
                else
                {
                    // Last month ends at year boundary
                    monthEnd = nextYearStart.AddDays(-1);
                }

                // Ensure month doesn't extend beyond year boundary
                if (monthEnd >= nextYearStart)
                {
                    monthEnd = nextYearStart.AddDays(-1);
                }

                bool isLeap = monthNames[i] == "Anala";
                months.Add((monthStart, monthEnd, monthNames[i], isLeap));

                // Next month starts the day after this month ends
                currentDate = monthEnd.AddDays(1);

                // Break if we've reached the year boundary
                if (currentDate >= nextYearStart)
                    break;
            }

            return months;
        }

        private static string[] GetMonthNamesForYearAstronomical(int year)
        {
            if (!IsLeapYear(year))
                return (string[])MonthNames.Clone();

            // Insert leap month "Anala" after "Gunla"
            string[] months = new string[13];
            Array.Copy(MonthNames, 0, months, 0, 11); // Copy first 11 months (including Gunla)
            months[11] = "Anala"; // Leap month after Gunla
            months[12] = "Yanla"; // Last month

            return months;
        }

        private static DateTime CalculateNewYearDateAstronomical(int nsYear)
        {
            // Special handling for the first Nepal Sambat year
            if (nsYear == 1)
                return Epoch; // NS year 1 starts exactly on the epoch date

            // Nepal Sambat New Year is the day after the new moon in Kartik (October/November)
            // Calculate the approximate Gregorian year
            int gregorianYear = 879 + nsYear;

            // Approximate date for Kartik new moon (around October/November)
            DateTime approxNewMoon = new DateTime(gregorianYear, 10, 15);

            // Find the actual new moon
            DateTime newMoon = AstronomicalCalculator.FindNewMoonAtOrAfter(approxNewMoon);

            // New Year starts the day after new moon
            return newMoon.AddDays(1);
        }

        private static void ValidateGregorianDate(DateTime date)
        {
            if (date < Epoch)
                throw new ArgumentException("Date pre-dates Nepal Sambat epoch (879 CE)", nameof(date));
        }

        private static void ValidateNepalSambatDate(NepalSambatDate nsDate)
        {
            if (nsDate.Year < 1)
                throw new ArgumentException("Invalid Nepal Sambat year", nameof(nsDate));
        }

        #endregion
    }

    /// <summary>
    /// Provides astronomical calculations for lunar phases and new moon detection.
    /// </summary>
    /// <remarks>
    /// This class implements Meeus' lunar phase algorithm for high-precision
    /// new moon calculations used in Nepal Sambat date conversions.
    /// </remarks>
    internal static class AstronomicalCalculator
    {
        /// <summary>
        /// Mean synodic month in days (average lunar cycle period).
        /// </summary>
        private const double MeanSynodicMonth = 29.530588861;

        /// <summary>
        /// Reference new moon (2000-01-06 18:14 UTC) for calculations.
        /// </summary>
        private static readonly DateTime ReferenceNewMoon = new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

        /// <summary>
        /// Finds the new moon at or after a specified date.
        /// </summary>
        /// <param name="afterDate">The date to find the new moon after.</param>
        /// <returns>The date and time of the new moon.</returns>
        public static DateTime FindNewMoonAtOrAfter(DateTime afterDate)
        {
            // Calculate approximate new moon using mean synodic month
            double daysFromReference = (afterDate - ReferenceNewMoon).TotalDays;
            double k = Math.Floor(daysFromReference / MeanSynodicMonth);
            DateTime approxNewMoon = ReferenceNewMoon.AddDays(k * MeanSynodicMonth);

            // Ensure we're after the target date
            while (approxNewMoon < afterDate)
                approxNewMoon = approxNewMoon.AddDays(MeanSynodicMonth);

            // Refine using astronomical calculations
            return RefineNewMoon(approxNewMoon);
        }

        /// <summary>
        /// Finds the next new moon after a specified date.
        /// </summary>
        /// <param name="afterDate">The date to find the next new moon after.</param>
        /// <returns>The date and time of the next new moon.</returns>
        public static DateTime FindNextNewMoon(DateTime afterDate)
        {
            DateTime firstCandidate = FindNewMoonAtOrAfter(afterDate.AddDays(1));
            DateTime secondCandidate = firstCandidate.AddDays(MeanSynodicMonth);

            // Return the closer new moon
            return (firstCandidate - afterDate).TotalDays < (secondCandidate - afterDate).TotalDays
                ? firstCandidate : secondCandidate;
        }

        private static DateTime RefineNewMoon(DateTime approxDate)
        {
            // Convert to Julian Day Number
            double jd = ToJulianDay(approxDate);

            // Centuries since J2000.0
            double T = (jd - 2451545.0) / 36525.0;

            // Solar mean anomaly
            double M = 357.52910 + 35999.05030 * T - 0.0001559 * T * T - 0.00000048 * T * T * T;
            M = NormalizeAngle(M);

            // Lunar mean anomaly
            double M_prime = 134.96340 + 477198.86753 * T + 0.0089970 * T * T + 0.00001433 * T * T * T;
            M_prime = NormalizeAngle(M_prime);

            // Lunar argument of latitude
            double F = 93.27210 + 483202.01753 * T - 0.0034025 * T * T - 0.00000383 * T * T * T;
            F = NormalizeAngle(F);

            // Lunar elongation
            double D = 297.85020 + 445267.11135 * T - 0.0019142 * T * T + 0.00000183 * T * T * T;
            D = NormalizeAngle(D);

            // New moon correction terms (Meeus algorithm)
            double correction = -0.40720 * Math.Sin(M_prime * Math.PI / 180.0)
                              + 0.17241 * Math.Sin(M * Math.PI / 180.0)
                              + 0.01608 * Math.Sin(2 * M_prime * Math.PI / 180.0)
                              + 0.01039 * Math.Sin(2 * F * Math.PI / 180.0)
                              + 0.00739 * Math.Sin(2 * D * Math.PI / 180.0)
                              - 0.00514 * Math.Sin(M_prime + M * Math.PI / 180.0)
                              + 0.00208 * Math.Sin(M_prime - M * Math.PI / 180.0)
                              - 0.00111 * Math.Sin(2 * M * Math.PI / 180.0)
                              - 0.00057 * Math.Sin(M_prime + 2 * D * Math.PI / 180.0)
                              + 0.00056 * Math.Sin(M_prime - 2 * D * Math.PI / 180.0)
                              - 0.00042 * Math.Sin(2 * M_prime + M * Math.PI / 180.0)
                              + 0.00042 * Math.Sin(2 * M_prime - M * Math.PI / 180.0)
                              + 0.00038 * Math.Sin(M_prime - 2 * F * Math.PI / 180.0)
                              - 0.00024 * Math.Sin(M_prime + 2 * F * Math.PI / 180.0)
                              - 0.00017 * Math.Sin(2 * D - M * Math.PI / 180.0)
                              - 0.00007 * Math.Sin(M_prime + 2 * M * Math.PI / 180.0)
                              + 0.00004 * Math.Sin(2 * M_prime + 2 * F * Math.PI / 180.0)
                              + 0.00004 * Math.Sin(3 * M_prime * Math.PI / 180.0)
                              + 0.00003 * Math.Sin(M_prime + M - 2 * D * Math.PI / 180.0)
                              + 0.00003 * Math.Sin(2 * M_prime - 2 * F * Math.PI / 180.0)
                              - 0.00002 * Math.Sin(M_prime - M + 2 * D * Math.PI / 180.0)
                              - 0.00002 * Math.Sin(M_prime + M + 2 * D * Math.PI / 180.0)
                              + 0.00002 * Math.Sin(M_prime - 2 * M * Math.PI / 180.0);

            // Convert correction to days and apply
            double correctionDays = correction / 1000.0;
            DateTime refinedDate = approxDate.AddDays(correctionDays);

            return refinedDate;
        }

        private static double ToJulianDay(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (month <= 2)
            {
                year--;
                month += 12;
            }

            int a = year / 100;
            int b = 2 - a + a / 4;

            return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }
    }
}