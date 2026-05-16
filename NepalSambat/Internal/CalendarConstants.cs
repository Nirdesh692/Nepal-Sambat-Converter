using System;

namespace NepalSambat.Internal
{
    internal static class CalendarConstants
    {
        public static readonly DateTime Epoch = new DateTime(879, 10, 20);

        public static readonly string[] MonthNames =
        {
            "कछला", "थिंला", "पोहेला", "सिल्ला",
            "चिल्ला", "चौला", "वछला", "तछला",
            "दिल्ला", "गुला", "ञला", "कौला"
        };

        public const string BrightSuffix = "थ्व";
        public const string DarkSuffix = "गा";
        public const string LeapMonthName = "अनाला";
    }
}
