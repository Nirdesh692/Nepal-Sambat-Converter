using System;
using NepalSambat.Models;

namespace NepalSambat.Interfaces
{
    /// <summary>
    /// Provides methods for converting between Gregorian and Nepal Sambat calendars.
    /// </summary>
    public interface INepalSambatConverter
    {
        /// <summary>
        /// Converts a Gregorian date to a Nepal Sambat date.
        /// </summary>
        NepalSambatDate FromGregorian(DateTime gregorianDate);

        /// <summary>
        /// Converts a Nepal Sambat date to one or more corresponding Gregorian dates.
        /// </summary>
        DateTime[] ToGregorian(NepalSambatDate nsDate);

        /// <summary>
        /// Checks if the specified Nepal Sambat year is a leap year (13 months).
        /// </summary>
        bool IsLeapYear(int nsYear);
    }
}
