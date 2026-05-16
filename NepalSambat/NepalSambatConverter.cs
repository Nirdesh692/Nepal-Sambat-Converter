using System;
using System.Collections.Generic;
using System.Linq;
using NepalSambat.Interfaces;
using NepalSambat.Internal;
using NepalSambat.Models;

namespace NepalSambat
{
    /// <summary>
    /// Provides methods for converting between Gregorian and Nepal Sambat calendars.
    /// This implementation uses high-precision astronomical calculations and matches traditional month boundaries.
    /// </summary>
    public class NepalSambatConverter : INepalSambatConverter
    {
        /// <summary>Gets the singleton instance of the converter.</summary>
        public static NepalSambatConverter Instance { get; } = new NepalSambatConverter();

        private static readonly Dictionary<int, DateTime> _newYearCache = new();
        private static readonly Dictionary<int, int> _leapMonthCache = new();

        /// <summary>
        /// Converts a Gregorian date to a Nepal Sambat date.
        /// </summary>
        public static NepalSambatDate FromGregorianDate(DateTime date) => Instance.FromGregorian(date);

        /// <summary>
        /// Converts a Nepal Sambat date to one or more corresponding Gregorian dates.
        /// </summary>
        public static DateTime[] ToGregorianDate(NepalSambatDate date) => Instance.ToGregorian(date);

        /// <inheritdoc />
        public NepalSambatDate FromGregorian(DateTime gregorianDate)
        {
            ValidateGregorianDate(gregorianDate);
            int nsYear = FindNepalSambatYear(gregorianDate);
            
            // Search in the estimated year and its neighbors
            foreach (int year in new[] { nsYear, nsYear - 1, nsYear + 1 })
            {
                if (year < 1) continue;
                var months = GetMonthsInYear(year);
                foreach (var month in months)
                {
                    if (gregorianDate.Date >= month.start.Date && gregorianDate.Date <= month.end.Date)
                    {
                        int tithi = AstronomicalCalculator.GetTithiAtSunrise(gregorianDate.Date);
                        return new NepalSambatDate(year, GetFullMonthName(month.name, tithi), tithi, month.isLeap);
                    }
                }
            }

            throw new InvalidOperationException($"Date {gregorianDate:yyyy-MM-dd} could not be converted to Nepal Sambat.");
        }

        /// <inheritdoc />
        public DateTime[] ToGregorian(NepalSambatDate nsDate)
        {
            string baseName = GetBaseMonthName(nsDate.Month);
            var months = GetMonthsInYear(nsDate.Year);

            foreach (var month in months)
            {
                if (month.name != baseName || month.isLeap != nsDate.IsLeapMonth)
                    continue;

                var results = new List<DateTime>();
                for (DateTime d = month.start.Date; d <= month.end.Date; d = d.AddDays(1))
                {
                    if (AstronomicalCalculator.GetTithiAtSunrise(d) == nsDate.Day)
                        results.Add(d);
                }

                if (results.Count > 0) return results.ToArray();

                // Kshaya handling: return the day before the first day that has a higher tithi
                for (DateTime d = month.start.Date; d <= month.end.Date; d = d.AddDays(1))
                {
                    if (AstronomicalCalculator.GetTithiAtSunrise(d) > nsDate.Day)
                        return new[] { d.AddDays(-1) };
                }
                return new[] { month.end.Date };
            }

            throw new InvalidOperationException($"Month '{nsDate.Month}' not found in NS year {nsDate.Year}.");
        }

        /// <inheritdoc />
        public bool IsLeapYear(int nsYear) => FindLeapMonthIndex(nsYear) >= 0;

        // ─────────────────────────────────────────────────────────────────────
        // Private Helpers
        // ─────────────────────────────────────────────────────────────────────

        private int FindNepalSambatYear(DateTime date)
        {
            if (date.Date == CalendarConstants.Epoch.Date) return 1;

            int approxYear = (int)((date - CalendarConstants.Epoch).TotalDays / 365.25) + 1;
            approxYear = Math.Max(1, approxYear);

            for (int offset = -2; offset <= 2; offset++)
            {
                int testYear = approxYear + offset;
                if (testYear < 1) continue;

                DateTime testNewYear = CalculateNewYear(testYear);
                DateTime nextNewYear = CalculateNewYear(testYear + 1);

                if (date >= testNewYear && date < nextNewYear)
                    return testYear;
            }

            return approxYear;
        }

        private DateTime CalculateNewYear(int nsYear)
        {
            if (_newYearCache.TryGetValue(nsYear, out DateTime cached)) return cached;

            if (nsYear == 1) return CalendarConstants.Epoch;

            int gregYear = 879 + nsYear;
            DateTime approx = new DateTime(gregYear, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime nm = AstronomicalCalculator.FindNewMoonAtOrAfter(approx);

            // New Year starts the day after the Aunsi day
            DateTime newYear = FindAunsiDay(nm).AddDays(1);
            _newYearCache[nsYear] = newYear;
            return newYear;
        }

        private List<(DateTime start, DateTime end, string name, bool isLeap)> GetMonthsInYear(int nsYear)
        {
            DateTime yearStart = CalculateNewYear(nsYear);
            DateTime nextYearStart = CalculateNewYear(nsYear + 1);
            int leapIdx = FindLeapMonthIndex(nsYear);
            var names = GetMonthNamesForYear(nsYear, leapIdx);

            var result = new List<(DateTime, DateTime, string, bool)>();
            DateTime current = yearStart;

            for (int i = 0; i < names.Length; i++)
            {
                DateTime monthStart = current;
                DateTime monthEnd;

                if (i < names.Length - 1)
                {
                    DateTime nextNM = AstronomicalCalculator.FindNextNewMoon(current);
                    monthEnd = FindAunsiDay(nextNM);
                }
                else
                {
                    monthEnd = nextYearStart.Date.AddDays(-1);
                }

                if (monthEnd.Date >= nextYearStart.Date)
                    monthEnd = nextYearStart.Date.AddDays(-1);

                bool isLeap = names[i] == CalendarConstants.LeapMonthName;
                result.Add((monthStart.Date, monthEnd, names[i], isLeap));
                current = monthEnd.AddDays(1);
            }
            return result;
        }

        private int FindLeapMonthIndex(int nsYear)
        {
            if (_leapMonthCache.TryGetValue(nsYear, out int cached)) return cached;

            DateTime yearStart = CalculateNewYear(nsYear);
            DateTime nextYearStart = CalculateNewYear(nsYear + 1);
            DateTime current = yearStart;
            int result = -1;

            for (int i = 0; i < 13; i++)
            {
                if (current >= nextYearStart) break;

                DateTime nextNM = AstronomicalCalculator.FindNextNewMoon(current);
                DateTime monthEnd = nextNM.Date;

                DateTime sankranti = AstronomicalCalculator.FindNextSankranti(current);
                bool hasSankranti = sankranti.Date <= monthEnd.Date;

                if (!hasSankranti)
                {
                    DateTime leapMonthStart = monthEnd.AddDays(1);
                    if (leapMonthStart < nextYearStart)
                        result = i;
                    break;
                }

                current = monthEnd.AddDays(1);
                if (current >= nextYearStart) break;
            }

            _leapMonthCache[nsYear] = result;
            return result;
        }

        private DateTime FindAunsiDay(DateTime newMoon)
        {
            foreach (int offset in new[] { 1, 0, 2, -1 })
            {
                DateTime candidate = newMoon.Date.AddDays(offset);
                if (AstronomicalCalculator.GetTithiAtSunrise(candidate) == 30)
                    return candidate;
            }
            return newMoon.Date;
        }

        private string[] GetMonthNamesForYear(int year, int leapIdx)
        {
            if (leapIdx == -1) return (string[])CalendarConstants.MonthNames.Clone();
            
            string[] result = new string[13];
            Array.Copy(CalendarConstants.MonthNames, 0, result, 0, leapIdx);
            result[leapIdx] = CalendarConstants.LeapMonthName;
            Array.Copy(CalendarConstants.MonthNames, leapIdx, result, leapIdx + 1, CalendarConstants.MonthNames.Length - leapIdx);
            return result;
        }

        private string GetFullMonthName(string baseName, int tithi) => 
            baseName + (tithi <= 15 ? CalendarConstants.BrightSuffix : CalendarConstants.DarkSuffix);

        private string GetBaseMonthName(string month) => 
            month.Replace(CalendarConstants.BrightSuffix, "").Replace(CalendarConstants.DarkSuffix, "");

        private void ValidateGregorianDate(DateTime date)
        {
            if (date < CalendarConstants.Epoch)
                throw new ArgumentException($"Date {date:yyyy-MM-dd} precedes NS epoch.");
        }

        private void ValidateNepalSambatDate(NepalSambatDate nsDate)
        {
            if (nsDate.Year < 1)
                throw new ArgumentException("Invalid Nepal Sambat year.", nameof(nsDate));
        }

        // Used by NepalSambatDate for sorting
        internal static int GetMonthIndex(string monthName, bool isLeap, int nsYear)
        {
            bool isDark = monthName.EndsWith(CalendarConstants.DarkSuffix);
            string baseName = Instance.GetBaseMonthName(monthName);

            if (isLeap)
            {
                int leapBasePos = Instance.FindLeapMonthIndex(nsYear);
                return leapBasePos * 2 + (isDark ? 1 : 0);
            }

            var list = CalendarConstants.MonthNames.ToList();
            int idx = list.IndexOf(baseName);
            if (idx == -1) return int.MaxValue;

            int leapIdx = Instance.FindLeapMonthIndex(nsYear);
            if (leapIdx >= 0 && idx > leapIdx)
                idx++;

            return idx * 2 + (isDark ? 1 : 0);
        }
    }
}
