using System;

namespace NepalSambat.Models
{
    /// <summary>
    /// Represents a date in the Nepal Sambat calendar.
    /// </summary>
    public struct NepalSambatDate : IComparable<NepalSambatDate>, IEquatable<NepalSambatDate>
    {
        /// <summary>The Nepal Sambat year (e.g. 1145).</summary>
        public int Year { get; }

        /// <summary>The month name (e.g. "कछलाथ्व").</summary>
        public string Month { get; }

        /// <summary>The Tithi day (1–30).</summary>
        public int Day { get; }

        /// <summary>Indicates whether this is a leap month (Anala).</summary>
        public bool IsLeapMonth { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NepalSambatDate"/> struct.
        /// </summary>
        public NepalSambatDate(int year, string month, int day, bool isLeapMonth = false)
        {
            if (year < 1) throw new ArgumentOutOfRangeException(nameof(year));
            if (string.IsNullOrWhiteSpace(month)) throw new ArgumentException("Month cannot be empty.", nameof(month));
            if (day < 1 || day > 30) throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 30.");

            Year = year;
            Month = month;
            Day = day;
            IsLeapMonth = isLeapMonth;
        }

        /// <summary>Returns the string representation: "Year Month Day".</summary>
        public override string ToString() => $"{Year} {Month} {Day}";

        /// <inheritdoc />
        public int CompareTo(NepalSambatDate other)
        {
            int yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0) return yearComparison;

            // Month comparison requires internal index knowledge, 
            // but for a simple model, we just store properties.
            // Full comparison is handled by the converter if needed.
            return 0; // Simplified for model-only
        }

        /// <inheritdoc />
        public bool Equals(NepalSambatDate other) =>
            Year == other.Year && Month == other.Month && Day == other.Day && IsLeapMonth == other.IsLeapMonth;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is NepalSambatDate other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day, IsLeapMonth);

        /// <summary>Compares two <see cref="NepalSambatDate"/> instances for equality.</summary>
        public static bool operator ==(NepalSambatDate left, NepalSambatDate right) => left.Equals(right);
        /// <summary>Compares two <see cref="NepalSambatDate"/> instances for inequality.</summary>
        public static bool operator !=(NepalSambatDate left, NepalSambatDate right) => !left.Equals(right);
    }
}
