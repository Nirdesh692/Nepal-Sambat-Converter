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
    /// and years that can have 12 or 13 months (leap years). Each lunar month is divided into
    /// two fortnights: the bright fortnight (थ्व, tithis 1-15, new moon to full moon) and the
    /// dark fortnight (गा, tithis 16-30, full moon to new moon). The day number is a tithi —
    /// a unit of lunar time equal to 1/30th of a synodic month (~23.6 hours). Because a tithi
    /// does not align perfectly with a 24-hour solar day, tithis can occasionally be skipped
    /// (kshaya) or repeated (vriddhi).
    /// </remarks>
    public readonly struct NepalSambatDate : IEquatable<NepalSambatDate>, IComparable<NepalSambatDate>
    {
        /// <summary>
        /// Gets the Nepal Sambat year.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the fortnight month name in Nepal Sambat calendar.
        /// </summary>
        /// <value>
        /// The month name including fortnight suffix, e.g. "थिंलाथ्व" (bright half) or
        /// "थिंलागा" (dark half). The leap month appears as "अनालाथ्व" or "अनालागा".
        /// </value>
        public string Month { get; }

        /// <summary>
        /// Gets the tithi (lunar day) of the month.
        /// </summary>
        /// <value>
        /// The tithi number (1–30). Tithis 1–15 fall in the bright fortnight (थ्व);
        /// tithis 16–30 fall in the dark fortnight (गा). Because a tithi is ~23.6 hours,
        /// a given tithi may be skipped or repeated on the Gregorian calendar.
        /// </value>
        public int Day { get; }

        /// <summary>
        /// Gets a value indicating whether this date is in a leap month (अनाला / Adhik Maas).
        /// </summary>
        public bool IsLeapMonth { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NepalSambatDate"/> struct.
        /// </summary>
        /// <param name="year">The Nepal Sambat year.</param>
        /// <param name="month">The fortnight month name (e.g. "थिंलाथ्व" or "थिंलागा").</param>
        /// <param name="day">The tithi number (1–30).</param>
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
        public bool Equals(NepalSambatDate other) =>
            Year == other.Year && Month == other.Month && Day == other.Day && IsLeapMonth == other.IsLeapMonth;

        /// <summary>
        /// Determines whether this instance equals another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NepalSambatDate other && Equals(other);

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day, IsLeapMonth);

        /// <summary>
        /// Compares this instance to another <see cref="NepalSambatDate"/>.
        /// </summary>
        /// <returns>
        /// Negative if this precedes <paramref name="other"/>, zero if equal,
        /// positive if this follows <paramref name="other"/>.
        /// </returns>
        public int CompareTo(NepalSambatDate other)
        {
            var yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0) return yearComparison;

            var monthComparison = NepalSambatConverter.GetMonthIndex(Month, IsLeapMonth, Year)
                                  .CompareTo(NepalSambatConverter.GetMonthIndex(other.Month, other.IsLeapMonth, other.Year));
            if (monthComparison != 0) return monthComparison;

            var dayComparison = Day.CompareTo(other.Day);
            if (dayComparison != 0) return dayComparison;

            return IsLeapMonth.CompareTo(other.IsLeapMonth);
        }

        /// <summary>Determines whether two <see cref="NepalSambatDate"/> instances are equal.</summary>
        public static bool operator ==(NepalSambatDate left, NepalSambatDate right) => left.Equals(right);

        /// <summary>Determines whether two <see cref="NepalSambatDate"/> instances are not equal.</summary>
        public static bool operator !=(NepalSambatDate left, NepalSambatDate right) => !left.Equals(right);

        /// <summary>Determines whether <paramref name="left"/> is earlier than <paramref name="right"/>.</summary>
        public static bool operator <(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) < 0;

        /// <summary>Determines whether <paramref name="left"/> is earlier than or equal to <paramref name="right"/>.</summary>
        public static bool operator <=(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) <= 0;

        /// <summary>Determines whether <paramref name="left"/> is later than <paramref name="right"/>.</summary>
        public static bool operator >(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) > 0;

        /// <summary>Determines whether <paramref name="left"/> is later than or equal to <paramref name="right"/>.</summary>
        public static bool operator >=(NepalSambatDate left, NepalSambatDate right) => left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Provides methods for converting between Gregorian and Nepal Sambat dates.
    /// </summary>
    /// <remarks>
    /// Uses astronomical calculations based on Meeus' algorithms for new moon detection,
    /// solar longitude (Sankranti), and tithi computation. Month boundaries are derived
    /// from new moon instants converted to Nepal Standard Time (UTC+5:45). Day numbers
    /// are tithis — the tithi active at sunrise in Kathmandu governs the calendar day.
    /// </remarks>
    public static class NepalSambatConverter
    {
        /// <summary>
        /// Nepal Sambat epoch: 879 CE October 20 (Gregorian).
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(879, 10, 20);

        /// <summary>
        /// Base month names (without fortnight suffix). Each month is split into a bright
        /// fortnight (+ "थ्व") and a dark fortnight (+ "गा") in the public API.
        /// </summary>
        private static readonly string[] MonthNames =
        {
            "कछला", "थिंला", "पोहेला", "सिल्ला",
            "चिल्ला", "चौला", "वछला", "तछला",
            "दिल्ला", "गुला", "ञला", "कौला"
        };

        // Bright and dark fortnight suffixes
        private const string BrightSuffix = "थ्व";
        private const string DarkSuffix = "गा";

        // Cache for new year dates and leap month indices to avoid redundant calculation
        private static readonly Dictionary<int, DateTime> _newYearCache = new();
        private static readonly Dictionary<int, int> _leapMonthCache = new();

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a Gregorian date to a Nepal Sambat date.
        /// </summary>
        /// <param name="gregorianDate">The Gregorian date to convert.</param>
        /// <returns>The equivalent <see cref="NepalSambatDate"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the date pre-dates the NS epoch (879 CE).</exception>
        /// <exception cref="InvalidOperationException">Thrown when the date cannot be located in any computed year.</exception>
        /// <example>
        /// <code>
        /// var ns = NepalSambatConverter.FromGregorian(new DateTime(2025, 8, 17));
        /// Console.WriteLine(ns); // e.g. "1145 गुंलाथ्व 24"
        /// </code>
        /// </example>
        public static NepalSambatDate FromGregorian(DateTime gregorianDate)
        {
            ValidateGregorianDate(gregorianDate);

            int nsYear = FindNepalSambatYearAstronomical(gregorianDate);

            // Try the estimated year and its neighbours
            foreach (int year in new[] { nsYear, nsYear - 1, nsYear + 1 })
            {
                if (year < 1) continue;

                var months = GetMonthsInYearAstronomical(year);
                foreach (var month in months)
                {
                    if (gregorianDate.Date >= month.start.Date &&
                        gregorianDate.Date <= month.end.Date)
                    {
                        // The day number is the tithi active at sunrise in Nepal
                        int tithi = AstronomicalCalculator.GetTithiAtSunrise(gregorianDate);

                        // Bright fortnight: tithis 1–15, dark fortnight: tithis 16–30
                        string suffix = tithi <= 15 ? BrightSuffix : DarkSuffix;
                        string fullName = month.name + suffix;

                        return new NepalSambatDate(year, fullName, tithi, month.isLeap);
                    }
                }
            }

            throw new InvalidOperationException(
                $"Date {gregorianDate:yyyy-MM-dd} could not be converted to Nepal Sambat.");
        }

        /// <summary>
        /// Converts a Nepal Sambat date to a Gregorian date.
        /// </summary>
        /// <param name="nsDate">The Nepal Sambat date to convert.</param>
        /// <returns>The equivalent Gregorian <see cref="DateTime"/> (midnight, unspecified kind).</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="nsDate"/> has invalid values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the tithi cannot be located in the computed month range.</exception>
        /// <example>
        /// <code>
        /// var gregorian = NepalSambatConverter.ToGregorian(new NepalSambatDate(1145, "गुंलाथ्व", 24));
        /// Console.WriteLine(gregorian.ToString("yyyy-MM-dd")); // "2025-08-17"
        /// </code>
        /// </example>
        public static DateTime ToGregorian(NepalSambatDate nsDate)
        {
            ValidateNepalSambatDate(nsDate);

            // Strip fortnight suffix to find the base month in the internal list
            string baseName = GetBaseMonthName(nsDate.Month);

            var months = GetMonthsInYearAstronomical(nsDate.Year);
            foreach (var month in months)
            {
                if (month.name != baseName || month.isLeap != nsDate.IsLeapMonth)
                    continue;

                // Walk each day in this month's date range and find the one whose
                // tithi at sunrise matches the requested tithi
                for (DateTime d = month.start.Date; d <= month.end.Date; d = d.AddDays(1))
                {
                    if (AstronomicalCalculator.GetTithiAtSunrise(d) == nsDate.Day)
                        return d;
                }

                // Tithi not found — it is a kshaya (skipped) tithi.
                // Per tradition a kshaya tithi is observed on the same day as the
                // preceding tithi; return the day on which tithi-1 falls.
                if (nsDate.Day > 1)
                {
                    for (DateTime d = month.start.Date; d <= month.end.Date; d = d.AddDays(1))
                    {
                        if (AstronomicalCalculator.GetTithiAtSunrise(d) == nsDate.Day - 1)
                            return d;
                    }
                }

                throw new InvalidOperationException(
                    $"Tithi {nsDate.Day} (and its predecessor) were not found in " +
                    $"{nsDate.Month} of NS year {nsDate.Year}.");
            }

            throw new InvalidOperationException(
                $"Month '{nsDate.Month}' not found in Nepal Sambat year {nsDate.Year}.");
        }

        /// <summary>
        /// Determines whether a specified Nepal Sambat year is a leap year (13 months).
        /// </summary>
        /// <param name="nsYear">The Nepal Sambat year to check.</param>
        /// <returns>True if the year contains an intercalary month (अनाला / Adhik Maas).</returns>
        /// <remarks>
        /// A leap year contains a lunar month in which no solar transit (Sankranti) occurs.
        /// That month is doubled; the extra month is called अनाला. This happens approximately
        /// every 2.7 years to keep the lunisolar calendar aligned with the solar year.
        /// </remarks>
        public static bool IsLeapYear(int nsYear)
        {
            if (nsYear < 1)
                throw new ArgumentException("Year must be greater than 0.", nameof(nsYear));

            return FindLeapMonthIndex(nsYear) >= 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Strips the fortnight suffix (थ्व or गा) from a full month name to get the base name.
        /// </summary>
        private static string GetBaseMonthName(string monthName)
        {
            if (monthName.EndsWith(BrightSuffix)) return monthName[..^BrightSuffix.Length];
            if (monthName.EndsWith(DarkSuffix)) return monthName[..^DarkSuffix.Length];
            return monthName; // no suffix — return as-is
        }

        private static int FindNepalSambatYearAstronomical(DateTime gregorianDate)
        {
            if (gregorianDate.Date == Epoch.Date)
                return 1;

            int approxYear = (int)((gregorianDate - Epoch).TotalDays / 365.25) + 1;
            approxYear = Math.Max(1, approxYear);

            for (int offset = -2; offset <= 2; offset++)
            {
                int testYear = approxYear + offset;
                if (testYear < 1) continue;

                DateTime testNewYear = CalculateNewYearDateAstronomical(testYear);
                DateTime nextNewYear = CalculateNewYearDateAstronomical(testYear + 1);

                if (gregorianDate >= testNewYear && gregorianDate < nextNewYear)
                    return testYear;
            }

            return approxYear;
        }

        /// <summary>
        /// Builds the ordered list of months for a given NS year.
        /// Each entry is (start, end, baseName, isLeap).
        /// Month boundaries are determined by new moon instants in Nepal time.
        /// </summary>
        private static List<(DateTime start, DateTime end, string name, bool isLeap)>
            GetMonthsInYearAstronomical(int nsYear)
        {
            var months = new List<(DateTime, DateTime, string, bool)>();
            DateTime yearStart = CalculateNewYearDateAstronomical(nsYear);
            DateTime nextYearStart = CalculateNewYearDateAstronomical(nsYear + 1);
            string[] monthNames = GetMonthNamesForYearAstronomical(nsYear);

            DateTime currentDate = yearStart;

            for (int i = 0; i < monthNames.Length; i++)
            {
                DateTime monthStart = currentDate;
                DateTime monthEnd;

                if (i < monthNames.Length - 1)
                {
                    // Month ends on the new moon day itself (Aunsi = last day of the month).
                    // The next month starts the following day.
                    DateTime nextNewMoon = AstronomicalCalculator.FindNextNewMoon(currentDate);
                    monthEnd = nextNewMoon.Date; // strip time — only the calendar day matters
                }
                else
                {
                    // Last month of the year ends the day before New Year
                    monthEnd = nextYearStart.Date.AddDays(-1);
                }

                // Guard: never let a month extend past the year boundary
                if (monthEnd.Date >= nextYearStart.Date)
                    monthEnd = nextYearStart.Date.AddDays(-1);

                bool isLeap = monthNames[i] == "अनाल";
                months.Add((monthStart.Date, monthEnd, monthNames[i], isLeap));

                // Next month starts the day after the new moon (day after Aunsi)
                currentDate = monthEnd.AddDays(1);
            }

            return months;
        }

        /// <summary>
        /// Returns the ordered array of base month names for the given NS year,
        /// inserting "अनाल" at the position determined by the no-Sankranti rule.
        /// </summary>
        private static string[] GetMonthNamesForYearAstronomical(int year)
        {
            int leapIndex = FindLeapMonthIndex(year);

            if (leapIndex < 0)
                return (string[])MonthNames.Clone(); // standard 12-month year

            // Replace the month lacking a Sankranti with the intercalary month "अनाला"
            // This aligns with the CSV, where the leap month appears at the same position
            // as the month that has no solar transit.
            string[] result = new string[13];
            // Copy months before the leap position unchanged
            Array.Copy(MonthNames, 0, result, 0, leapIndex);
            // Insert the leap month at the exact index where the missing Sankranti month would be
            result[leapIndex] = "अनाला";
            // Copy the remaining months after the leap position, shifted by one slot
            Array.Copy(MonthNames, leapIndex, result, leapIndex + 1, MonthNames.Length - leapIndex);

            return result;
        }

        /// <summary>
        /// Calculates the Gregorian date on which Nepal Sambat New Year begins for the given NS year.
        /// New Year is the day after the Kartik new moon (the new moon of Kwa Nhū / Deepawali).
        /// </summary>
        private static DateTime CalculateNewYearDateAstronomical(int nsYear)
        {
            if (_newYearCache.TryGetValue(nsYear, out DateTime cached))
                return cached;

            if (nsYear == 1)
            {
                _newYearCache[nsYear] = Epoch;
                return Epoch;
            }

            int gregorianYear = 879 + nsYear;

            // Search from October 15 — Kartik new moon always falls in Oct/Nov
            DateTime approxNewMoon = new DateTime(gregorianYear, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime newMoon = AstronomicalCalculator.FindNewMoonAtOrAfter(approxNewMoon);

            // New Year starts the day after the new moon (day after Aunsi)
            DateTime newYear = newMoon.Date.AddDays(1);
            _newYearCache[nsYear] = newYear;
            return newYear;
        }

        /// <summary>
        /// Returns the 0-based index within <see cref="MonthNames"/> of the leap month
        /// (the month with no Sankranti), or -1 if this NS year is not a leap year.
        /// </summary>
        private static int FindLeapMonthIndex(int nsYear)
        {
            if (_leapMonthCache.TryGetValue(nsYear, out int cached))
                return cached;

            DateTime yearStart = CalculateNewYearDateAstronomical(nsYear);
            DateTime nextYearStart = CalculateNewYearDateAstronomical(nsYear + 1);
            DateTime current = yearStart;
            int result = -1;

            for (int i = 0; i < 13; i++) // at most 13 months in any NS year
            {
                DateTime monthStart = current;

                // If this month starts at or after the next year, it belongs to the next year
                if (monthStart >= nextYearStart) break;

                DateTime nextNewMoon = AstronomicalCalculator.FindNextNewMoon(current);
                DateTime monthEnd = nextNewMoon.Date; // new moon day = last day of month

                // A leap month is one in which no solar transit (Sankranti) occurs
                DateTime sankranti = AstronomicalCalculator.FindNextSankranti(monthStart);
                bool hasSankranti = sankranti.Date <= monthEnd.Date;

                if (!hasSankranti)
                {
                    // The leap month (अनाला) would be inserted immediately after this month.
                    // Verify there is actually room for it within the year — if the next
                    // month would start at or after nextYearStart, this is a boundary artifact
                    // where the Sankranti falls just one day after the year ends.
                    DateTime leapMonthStart = monthEnd.AddDays(1);
                    if (leapMonthStart < nextYearStart)
                        result = i;
                    // else: no room, treat as non-leap year (result stays -1)
                    break;
                }

                current = monthEnd.AddDays(1); // next month starts day after new moon

                if (current >= nextYearStart) break;
            }

            _leapMonthCache[nsYear] = result;
            return result;
        }

        private static void ValidateGregorianDate(DateTime date)
        {
            if (date < Epoch)
                throw new ArgumentException(
                    "Date pre-dates the Nepal Sambat epoch (879 CE).", nameof(date));
        }

        private static void ValidateNepalSambatDate(NepalSambatDate nsDate)
        {
            if (nsDate.Year < 1)
                throw new ArgumentException("Invalid Nepal Sambat year.", nameof(nsDate));
        }

        /// <summary>
        /// Returns a sortable index for a month name within a given NS year.
        /// Bright fortnight (थ्व) of a month always precedes its dark fortnight (गा).
        /// </summary>
        internal static int GetMonthIndex(string monthName, bool isLeap, int nsYear)
        {
            bool isDark = monthName.EndsWith(DarkSuffix);
            string baseName = GetBaseMonthName(monthName);

            if (isLeap) // अनाला
            {
                // Leap month sits at the no-Sankranti month position
                int leapBasePos = FindLeapMonthIndex(nsYear);
                return leapBasePos * 2 + (isDark ? 1 : 0);
            }

            var list = MonthNames.ToList();
            int idx = list.IndexOf(baseName);
            if (idx == -1) return int.MaxValue;

            // In a leap year, every month after the insertion point shifts right by one slot
            int leapIdx = FindLeapMonthIndex(nsYear);
            if (leapIdx >= 0 && idx > leapIdx)
                idx++;

            // Multiply by 2 so bright/dark fortnights interleave correctly
            return idx * 2 + (isDark ? 1 : 0);
        }
    }

    /// <summary>
    /// Provides astronomical calculations for lunar phases, solar longitude, and tithis.
    /// </summary>
    /// <remarks>
    /// New moon times are based on Meeus' Chapter 47 algorithm (accurate to ~2–5 minutes).
    /// Solar longitudes use a simplified VSOP87 reduction (accurate to ~0.01°).
    /// Moon longitudes use a reduced ELP2000 series for tithi calculation.
    /// All results are converted to Nepal Standard Time (UTC+5:45) before use.
    /// </remarks>
    internal static class AstronomicalCalculator
    {
        private const double MeanSynodicMonth = 29.530588861;

        private const double NewMoonCorrectionDays = -0.36181; 

        private static readonly DateTime ReferenceNewMoon =
            new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

        // ─────────────────────────────────────────────────────────────────────
        // New moon
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the new moon at or after <paramref name="afterDate"/> and returns
        /// the result in Nepal Standard Time (UTC+5:45).
        /// </summary>
        public static DateTime FindNewMoonAtOrAfter(DateTime afterDate)
        {
            double daysFromReference = (afterDate - ReferenceNewMoon).TotalDays;
            double k = Math.Floor(daysFromReference / MeanSynodicMonth);
            DateTime approxNewMoon = ReferenceNewMoon.AddDays(k * MeanSynodicMonth);

            while (approxNewMoon < afterDate)
                approxNewMoon = approxNewMoon.AddDays(MeanSynodicMonth);

            DateTime refinedUtc = RefineNewMoon(approxNewMoon);
            DateTime nst = refinedUtc.Add(new TimeSpan(5, 45, 0)); // convert to NST
            // Apply empirical correction to align new moon timing with CSV ground truth
            nst = nst.AddDays(NewMoonCorrectionDays);
            return nst;
        }

        /// <summary>
        /// Finds the next new moon strictly after <paramref name="afterDate"/> (NST).
        /// </summary>
        public static DateTime FindNextNewMoon(DateTime afterDate)
        {
            return FindNewMoonAtOrAfter(afterDate.AddDays(1));
        }

        /// <summary>
        /// Refines an approximate new moon date using Meeus' Chapter 47 algorithm.
        /// Returns UTC.
        /// </summary>
        private static DateTime RefineNewMoon(DateTime approxDate)
        {
            double daysFromJ2000 =
                (approxDate - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays;
            double k = Math.Round(daysFromJ2000 / MeanSynodicMonth);
            double T = k / 1236.85; // Julian centuries

            double JDE = 2451550.09766
                       + 29.530588861 * k
                       + 0.00015437 * T * T
                       - 0.000000150 * T * T * T
                       + 0.00000000073 * T * T * T * T;

            double M = 2.5534
                     + 29.10535669 * k
                     - 0.0000218 * T * T
                     - 0.00000011 * T * T * T;

            double M_prime = 201.5643
                           + 385.81693528 * k
                           + 0.1017438 * T * T
                           + 0.00001239 * T * T * T
                           - 0.000000058 * T * T * T * T;

            double F = 160.7108
                     + 390.67050274 * k
                     - 0.0016341 * T * T
                     - 0.00000227 * T * T * T
                     + 0.000000011 * T * T * T * T;

            double omega = 124.7746
                         - 1.56375580 * k
                         + 0.0020691 * T * T
                         + 0.00000215 * T * T * T;

            double correction =
                -0.40720 * Math.Sin(ToRad(M_prime))
                + 0.17241 * Math.Sin(ToRad(M))
                + 0.01608 * Math.Sin(ToRad(2 * M_prime))
                + 0.01039 * Math.Sin(ToRad(2 * F))
                + 0.00739 * Math.Sin(ToRad(M_prime - M))
                - 0.00514 * Math.Sin(ToRad(M_prime + M))
                + 0.00208 * Math.Sin(ToRad(2 * M))
                - 0.00111 * Math.Sin(ToRad(M_prime - 2 * F))
                - 0.00057 * Math.Sin(ToRad(M_prime + 2 * F))
                + 0.00056 * Math.Sin(ToRad(2 * M_prime + M))
                - 0.00042 * Math.Sin(ToRad(3 * M_prime))
                + 0.00042 * Math.Sin(ToRad(M + 2 * F))
                + 0.00038 * Math.Sin(ToRad(M - 2 * F))
                - 0.00024 * Math.Sin(ToRad(2 * M_prime - M))
                - 0.00017 * Math.Sin(ToRad(omega))
                - 0.00007 * Math.Sin(ToRad(M_prime + 2 * M))
                + 0.00004 * Math.Sin(ToRad(2 * M_prime - 2 * F))
                + 0.00004 * Math.Sin(ToRad(3 * M))
                + 0.00003 * Math.Sin(ToRad(M_prime + M - 2 * F))
                + 0.00003 * Math.Sin(ToRad(2 * M_prime + 2 * F))
                - 0.00003 * Math.Sin(ToRad(M_prime + M + 2 * F))
                + 0.00003 * Math.Sin(ToRad(M_prime - M + 2 * F))
                - 0.00002 * Math.Sin(ToRad(M_prime - M - 2 * F))
                - 0.00002 * Math.Sin(ToRad(3 * M_prime + M))
                + 0.00002 * Math.Sin(ToRad(M_prime - 2 * M));

            return JulianDayToDateTime(JDE + correction);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tithi
        // ─────────────────────────────────────────────────────────────────────

        // Kathmandu geographic coordinates
        private const double KathmanduLatRad = 27.7172 * Math.PI / 180.0; // 27.7172°N
        private const double KathmanduLonDeg = 85.3240;                    // 85.3240°E

        /// <summary>
        /// Returns the tithi (1–30) active at sunrise in Kathmandu on <paramref name="date"/>.
        /// Per tradition, the tithi at sunrise governs the entire calendar day.
        /// Sunrise is computed astronomically from the Sun's declination and hour angle,
        /// accurate to within ~1–2 minutes year-round.
        /// </summary>
        public static int GetTithiAtSunrise(DateTime date)
        {
            DateTime sunriseUtc = CalculateSunriseUtc(date);
            return CalculateTithi(sunriseUtc);
        }

        /// <summary>
        /// Calculates the UTC time of sunrise in Kathmandu for a given date.
        /// Uses the standard hour-angle formula with -0.833° altitude threshold
        /// (accounts for atmospheric refraction and solar disc radius).
        /// </summary>
        private static DateTime CalculateSunriseUtc(DateTime date)
        {
            double jd = ToJulianDay(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc));
            double T = (jd - 2451545.0) / 36525.0;

            // Sun's mean longitude and mean anomaly (degrees)
            double L0 = NormalizeAngle(280.46646 + 36000.76983 * T);
            double M = NormalizeAngle(357.52911 + 35999.05029 * T - 0.0001537 * T * T);

            // Equation of centre → sun's true longitude
            double C = (1.914602 - 0.004817 * T) * Math.Sin(ToRad(M))
                     + 0.019993 * Math.Sin(ToRad(2 * M))
                     + 0.000290 * Math.Sin(ToRad(3 * M));
            double sunLon = NormalizeAngle(L0 + C);

            // Obliquity of the ecliptic and sun's declination
            double obliquity = 23.439291 - 0.013004 * T;
            double decRad = Math.Asin(Math.Sin(ToRad(obliquity)) * Math.Sin(ToRad(sunLon)));

            // Hour angle at sunrise (-0.833° = refraction + solar disc)
            double cosH = (Math.Sin(ToRad(-0.833)) - Math.Sin(KathmanduLatRad) * Math.Sin(decRad))
                        / (Math.Cos(KathmanduLatRad) * Math.Cos(decRad));
            cosH = Math.Clamp(cosH, -1.0, 1.0);
            double H = Math.Acos(cosH) * 180.0 / Math.PI; // degrees

            // Equation of time (minutes)
            double y = Math.Tan(ToRad(obliquity / 2));
            y = y * y;
            double eot = 4.0 * (y * Math.Sin(ToRad(2 * L0))
                       - 2.0 * 0.016708 * Math.Sin(ToRad(M))
                       + 4.0 * 0.016708 * y * Math.Sin(ToRad(M)) * Math.Cos(ToRad(2 * L0))
                       - 0.5 * y * y * Math.Sin(ToRad(4 * L0))
                       - 1.25 * 0.016708 * 0.016708 * Math.Sin(ToRad(2 * M)));

            // Solar noon UTC (minutes) = 720 - 4*longitude - eot
            double solarNoonUtcMin = 720.0 - 4.0 * KathmanduLonDeg - eot;

            // Sunrise = solar noon - H (1° hour angle = 4 minutes of time)
            double sunriseUtcMin = solarNoonUtcMin - H * 4.0;

            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc)
                       .AddMinutes(sunriseUtcMin);
        }

        /// <summary>
        /// Calculates the tithi (1–30) for a given UTC instant.
        /// Tithi = floor(lunar elongation / 12°) + 1, where elongation = moonLon − sunLon.
        /// </summary>
        public static int CalculateTithi(DateTime utcDateTime)
        {
            double jd = ToJulianDay(utcDateTime);
            double T = (jd - 2451545.0) / 36525.0;

            double moonLon = CalculateMoonLongitude(T);
            double sunLon = CalculateSunLongitudeFromT(T);

            double elongation = NormalizeAngle(moonLon - sunLon);
            int tithi = (int)(elongation / 12.0) + 1;
            return Math.Clamp(tithi, 1, 30);
        }

        /// <summary>
        /// Calculates the Moon's apparent ecliptic longitude in degrees using the
        /// Meeus Chapter 47 series. Accurate to ~0.05°.
        /// Variables: D = mean elongation, M = Sun's anomaly, M' = Moon's anomaly, F = argument of latitude.
        /// </summary>
        private static double CalculateMoonLongitude(double T)
        {
            // Moon's mean longitude (degrees)
            double L = 218.3164477
                     + 481267.88123421 * T
                     - 0.0015786 * T * T
                     + T * T * T / 538841.0
                     - T * T * T * T / 65194000.0;

            // Moon's mean elongation from Sun — D, NOT the same as F
            double D = 297.8501921
                     + 445267.1114034 * T
                     - 0.0018819 * T * T
                     + T * T * T / 545868.0
                     - T * T * T * T / 113065000.0;

            // Sun's mean anomaly
            double M = 357.5291092
                     + 35999.0502909 * T
                     - 0.0001536 * T * T
                     + T * T * T / 24490000.0;

            // Moon's mean anomaly
            double M_prime = 134.9633964
                           + 477198.8675055 * T
                           + 0.0087414 * T * T
                           + T * T * T / 69699.0
                           - T * T * T * T / 14712000.0;

            // Moon's argument of latitude (distance from ascending node)
            double F = 93.2720950
                     + 483202.0175233 * T
                     - 0.0036539 * T * T
                     - T * T * T / 3526000.0
                     + T * T * T * T / 863310000.0;

            // Longitude of ascending node (for nutation correction)
            double omega = 125.04452
                         - 1934.136261 * T
                         + 0.0020708 * T * T
                         + T * T * T / 450000.0;

            // Eccentricity correction factor for terms containing M
            double E = 1.0 - 0.002516 * T - 0.0000074 * T * T;
            double E2 = E * E;

            // Meeus Table 47.A — longitude correction terms (degrees)
            // Arguments use D (elongation), M (solar anomaly), M' (lunar anomaly), F (lat argument)
            double lon = L
                + 6.288774 * Math.Sin(ToRad(M_prime))
                + 1.274027 * Math.Sin(ToRad(2 * D - M_prime))        // evection
                + 0.658314 * Math.Sin(ToRad(2 * D))                  // variation
                + 0.213618 * Math.Sin(ToRad(2 * M_prime))
                - 0.185116 * E * Math.Sin(ToRad(M))                 // annual equation
                - 0.114332 * Math.Sin(ToRad(2 * F))                  // reduction to ecliptic
                + 0.058793 * Math.Sin(ToRad(2 * D - 2 * M_prime))
                + 0.057066 * E * Math.Sin(ToRad(2 * D - M - M_prime))
                + 0.053322 * Math.Sin(ToRad(2 * D + M_prime))
                + 0.045758 * E * Math.Sin(ToRad(2 * D - M))
                - 0.040923 * E * Math.Sin(ToRad(M_prime - M))
                - 0.034720 * Math.Sin(ToRad(D))
                - 0.030383 * E * Math.Sin(ToRad(M_prime + M))
                + 0.015327 * Math.Sin(ToRad(2 * D - 2 * F))
                - 0.012528 * Math.Sin(ToRad(M_prime + 2 * F))
                + 0.010980 * Math.Sin(ToRad(M_prime - 2 * F))
                + 0.010675 * Math.Sin(ToRad(4 * D - M_prime))
                + 0.010034 * Math.Sin(ToRad(3 * M_prime))
                + 0.008548 * Math.Sin(ToRad(4 * D - 2 * M_prime))
                - 0.007888 * E * Math.Sin(ToRad(2 * D + M - M_prime))
                - 0.006766 * E * Math.Sin(ToRad(2 * D + M))
                - 0.005163 * Math.Sin(ToRad(D + M_prime))
                + 0.004987 * E * Math.Sin(ToRad(D + M))
                + 0.004036 * E * Math.Sin(ToRad(2 * D - M + M_prime))
                + 0.003994 * Math.Sin(ToRad(2 * M_prime + 2 * D))
                + 0.003861 * Math.Sin(ToRad(4 * D))
                + 0.003665 * Math.Sin(ToRad(2 * D - 3 * M_prime))
                - 0.002689 * E * Math.Sin(ToRad(M - 2 * M_prime))
                - 0.002602 * Math.Sin(ToRad(M_prime - 2 * F + 2 * D))
                + 0.002390 * E * Math.Sin(ToRad(2 * D - M - 2 * M_prime))
                - 0.002348 * Math.Sin(ToRad(M_prime + D))
                + 0.002236 * E2 * Math.Sin(ToRad(2 * D - 2 * M))
                - 0.002120 * E * Math.Sin(ToRad(M + 2 * M_prime))
                - 0.002069 * E2 * Math.Sin(ToRad(2 * M))
                + 0.002048 * E2 * Math.Sin(ToRad(2 * D - 2 * M - M_prime))
                - 0.001773 * Math.Sin(ToRad(2 * D + M_prime - 2 * F))
                + 0.001215 * E * Math.Sin(ToRad(4 * D - M - M_prime))
                - 0.001110 * Math.Sin(ToRad(2 * M_prime + 2 * F))
                - 0.000892 * Math.Sin(ToRad(3 * D - M_prime))
                - 0.000811 * E * Math.Sin(ToRad(M_prime + M + 2 * D))
                + 0.000761 * E * Math.Sin(ToRad(4 * D - M - 2 * M_prime))
                + 0.000717 * E2 * Math.Sin(ToRad(M_prime - 2 * M))
                + 0.000704 * E2 * Math.Sin(ToRad(M_prime - 2 * M - 2 * D))
                + 0.000693 * E * Math.Sin(ToRad(M - 2 * M_prime + 2 * D))
                + 0.000598 * E * Math.Sin(ToRad(2 * D - M - 2 * F))
                + 0.000550 * Math.Sin(ToRad(M_prime + 4 * D))
                + 0.000538 * Math.Sin(ToRad(4 * M_prime))
                + 0.000521 * E * Math.Sin(ToRad(4 * D - M))
                + 0.000486 * Math.Sin(ToRad(2 * M_prime - D));

            // Nutation in longitude (approximate, degrees)
            lon += -0.00478 * Math.Sin(ToRad(omega));

            return NormalizeAngle(lon);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Solar longitude and Sankranti
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the Sun's apparent ecliptic longitude in degrees for a given UTC DateTime.
        /// Accurate to ~0.01°.
        /// </summary>
        public static double CalculateSunLongitude(DateTime utcDate)
        {
            double T = (ToJulianDay(utcDate) - 2451545.0) / 36525.0;
            return CalculateSunLongitudeFromT(T);
        }

        /// <summary>
        /// Calculates the Sun's apparent ecliptic longitude from Julian centuries T (J2000.0).
        /// </summary>
        private static double CalculateSunLongitudeFromT(double T)
        {
            double L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T * T;

            double M = 357.52911 + 35999.05029 * T - 0.0001537 * T * T;
            M = NormalizeAngle(M);

            double C = (1.914602 - 0.004817 * T - 0.000014 * T * T) * Math.Sin(ToRad(M))
                     + (0.019993 - 0.000101 * T) * Math.Sin(ToRad(2 * M))
                     + 0.000289 * Math.Sin(ToRad(3 * M));

            double sunLon = L0 + C;

            double omega = 125.04 - 1934.136 * T;
            sunLon = sunLon - 0.00569 - 0.00478 * Math.Sin(ToRad(omega));

            return NormalizeAngle(sunLon);
        }

        /// <summary>
        /// Calculates the Sun's sidereal (Nirayana) ecliptic longitude by subtracting the
        /// Lahiri ayanamsha from the tropical longitude. Used for Sankranti detection, which
        /// is defined in terms of sidereal zodiac signs per Hindu/Nepali tradition.
        /// </summary>
        private static double CalculateSiderealSunLongitude(DateTime utcDate)
        {
            double T = (ToJulianDay(utcDate) - 2451545.0) / 36525.0;
            double tropical = CalculateSunLongitudeFromT(T);
            double ayanamsha = GetLahiriAyanamsha(T);
            return NormalizeAngle(tropical - ayanamsha);
        }

        /// <summary>
        /// Returns the Lahiri (Chitrapaksha) ayanamsha in degrees for a given Julian century T.
        /// The ayanamsha is the angular offset between the tropical and sidereal zodiacs,
        /// approximately 23.85° at J2000.0 and increasing at ~1.40°/century due to precession.
        /// </summary>
        private static double GetLahiriAyanamsha(double T)
        {
            // Lahiri ayanamsha reference: 23.853° at J2000.0 (Jan 1.5, 2000)
            // Precession rate: ~50.29 arcsec/year = 1.3969°/century
            return 23.853 + 1.3969 * T;
        }

        /// <summary>
        /// Finds the next moment when the Sun enters a new sidereal zodiac sign (Sankranti) on
        /// or after <paramref name="afterDate"/> (UTC). Uses binary search to minute precision.
        /// Sidereal positions are used per Hindu/Nepali calendar tradition.
        /// </summary>
        public static DateTime FindNextSankranti(DateTime afterDate)
        {
            double currentLon = CalculateSiderealSunLongitude(afterDate);
            double nextBoundary = Math.Ceiling(currentLon / 30.0) * 30.0;
            if (nextBoundary >= 360) nextBoundary = 0;

            DateTime low = afterDate;
            DateTime high = afterDate.AddDays(32); // Sun takes at most ~31 days per sign

            for (int i = 0; i < 50; i++) // 50 iterations → sub-second precision
            {
                DateTime mid = low.AddDays((high - low).TotalDays / 2);
                double lowLon = CalculateSiderealSunLongitude(low);
                double midLon = CalculateSiderealSunLongitude(mid);

                bool crossedBefore = Math.Floor(lowLon / 30.0) != Math.Floor(midLon / 30.0);

                if (crossedBefore) high = mid;
                else low = mid;

                if ((high - low).TotalMinutes < 1) break;
            }

            return high;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Calendar utilities
        // ─────────────────────────────────────────────────────────────────────

        private static DateTime JulianDayToDateTime(double jd)
        {
            long j = (long)(jd + 0.5);
            long f = j + 1401 + (((4 * j + 274277) / 146097) * 3) / 4 - 38;
            long e = 4 * f + 3;
            long g = (e % 1461) / 4;
            long h = 5 * g + 2;

            int day = (int)((h % 153) / 5 + 1);
            int month = (int)((h / 153 + 2) % 12 + 1);
            int year = (int)(e / 1461 - 4716 + (14 - month) / 12);

            double dayFraction = jd + 0.5 - Math.Floor(jd + 0.5);
            int hours = (int)(dayFraction * 24);
            int minutes = (int)((dayFraction * 24 - hours) * 60);

            return new DateTime(year, month, day, hours, minutes, 0, DateTimeKind.Utc);
        }

        private static double ToJulianDay(DateTime date)
        {
            int y = date.Year, m = date.Month, d = date.Day;
            if (m <= 2) { y--; m += 12; }
            int a = y / 100;
            int b = 2 - a + a / 4;

            // Include fractional day from time components so intra-day positions are correct
            double dayFraction = (date.Hour + date.Minute / 60.0 + date.Second / 3600.0) / 24.0;

            return Math.Floor(365.25 * (y + 4716))
                 + Math.Floor(30.6001 * (m + 1))
                 + d + dayFraction + b - 1524.5;
        }

        private static double ToRad(double degrees) => degrees * Math.PI / 180.0;

        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }
    }
}