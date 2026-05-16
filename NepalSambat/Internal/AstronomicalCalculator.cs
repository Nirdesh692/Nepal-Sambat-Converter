using System;

namespace NepalSambat.Internal
{
    /// <summary>
    /// Provides astronomical calculations for lunar phases, solar longitude, and tithis.
    /// Based on Jean Meeus' "Astronomical Algorithms" (ELP2000 and VSOP87).
    /// </summary>
    internal static class AstronomicalCalculator
    {
        private const double MeanSynodicMonth = 29.530588861;

        private static readonly DateTime ReferenceNewMoon =
            new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

        // Kathmandu geographic coordinates
        private const double KathmanduLatRad = AstronomicalParameters.KathmanduLatDeg * Math.PI / 180.0;
        private const double KathmanduLonDeg = AstronomicalParameters.KathmanduLonDeg;

        public static DateTime FindNewMoonAtOrAfter(DateTime afterDate, double? correctionDays = null)
        {
            double corr = correctionDays ?? AstronomicalParameters.NewMoonCorrectionDays;
            double daysFromReference = (afterDate - ReferenceNewMoon).TotalDays;
            double k = Math.Floor(daysFromReference / MeanSynodicMonth);
            DateTime approxNewMoon = ReferenceNewMoon.AddDays(k * MeanSynodicMonth);

            while (approxNewMoon < afterDate)
                approxNewMoon = approxNewMoon.AddDays(MeanSynodicMonth);

            DateTime refinedUtc = RefineNewMoon(approxNewMoon);
            DateTime nst = refinedUtc.Add(new TimeSpan(5, 45, 0)); 
            nst = nst.AddDays(corr);
            return nst;
        }

        public static DateTime FindNextNewMoon(DateTime currentNewMoon, double? correctionDays = null)
        {
            return FindNewMoonAtOrAfter(currentNewMoon.AddDays(1), correctionDays);
        }

        public static int GetTithiAtSunrise(DateTime date)
        {
            DateTime sunriseUtc = CalculateSunriseUtc(date);
            return CalculateTithi(sunriseUtc);
        }

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

        private static DateTime CalculateSunriseUtc(DateTime date)
        {
            double jd = ToJulianDay(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc));
            double T = (jd - 2451545.0) / 36525.0;

            double L0 = NormalizeAngle(280.46646 + 36000.76983 * T);
            double M = NormalizeAngle(357.52911 + 35999.05029 * T - 0.0001537 * T * T);

            double C = (1.914602 - 0.004817 * T) * Math.Sin(ToRad(M))
                     + 0.019993 * Math.Sin(ToRad(2 * M))
                     + 0.000290 * Math.Sin(ToRad(3 * M));
            double sunLon = NormalizeAngle(L0 + C);

            double obliquity = 23.439291 - 0.013004 * T;
            double decRad = Math.Asin(Math.Sin(ToRad(obliquity)) * Math.Sin(ToRad(sunLon)));

            double cosH = (Math.Sin(ToRad(-0.833)) - Math.Sin(KathmanduLatRad) * Math.Sin(decRad))
                        / (Math.Cos(KathmanduLatRad) * Math.Cos(decRad));
            cosH = Math.Clamp(cosH, -1.0, 1.0);
            double H = Math.Acos(cosH) * 180.0 / Math.PI;

            double y = Math.Tan(ToRad(obliquity / 2));
            y = y * y;
            double eot = 4.0 * (180.0 / Math.PI) * (y * Math.Sin(ToRad(2 * L0))
                       - 2.0 * 0.016708 * Math.Sin(ToRad(M))
                       + 4.0 * 0.016708 * y * Math.Sin(ToRad(M)) * Math.Cos(ToRad(2 * L0))
                       - 0.5 * y * y * Math.Sin(ToRad(4 * L0))
                       - 1.25 * 0.016708 * 0.016708 * Math.Sin(ToRad(2 * M)));

            double solarNoonUtcMin = 720.0 - 4.0 * KathmanduLonDeg - eot;
            double sunriseUtcMin = solarNoonUtcMin - H * 4.0 + AstronomicalParameters.SunriseCorrectionMinutes;

            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc)
                       .AddMinutes(sunriseUtcMin);
        }

        private static DateTime RefineNewMoon(DateTime approxDate)
        {
            double daysFromJ2000 = (approxDate - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays;
            double k = Math.Round(daysFromJ2000 / MeanSynodicMonth);
            double T = k / 1236.85;

            double JDE = 2451550.09766 + 29.530588861 * k + 0.00015437 * T * T - 0.000000150 * T * T * T + 0.00000000073 * T * T * T * T;
            double M = 2.5534 + 29.10535669 * k - 0.0000218 * T * T - 0.00000011 * T * T * T;
            double M_prime = 201.5643 + 385.81693528 * k + 0.1017438 * T * T + 0.00001239 * T * T * T - 0.000000058 * T * T * T * T;
            double F = 160.7108 + 390.67050274 * k - 0.0016341 * T * T - 0.00000227 * T * T * T + 0.000000011 * T * T * T * T;
            double omega = 124.7746 - 1.56375580 * k + 0.0020691 * T * T + 0.00000215 * T * T * T;

            double correction = -0.40720 * Math.Sin(ToRad(M_prime)) + 0.17241 * Math.Sin(ToRad(M)) + 0.01608 * Math.Sin(ToRad(2 * M_prime)) + 0.01039 * Math.Sin(ToRad(2 * F)) + 0.00739 * Math.Sin(ToRad(M_prime - M)) - 0.00514 * Math.Sin(ToRad(M_prime + M)) + 0.00208 * Math.Sin(ToRad(2 * M)) - 0.00111 * Math.Sin(ToRad(M_prime - 2 * F)) - 0.00057 * Math.Sin(ToRad(M_prime + 2 * F)) + 0.00056 * Math.Sin(ToRad(2 * M_prime + M)) - 0.00042 * Math.Sin(ToRad(3 * M_prime)) + 0.00042 * Math.Sin(ToRad(M + 2 * F)) + 0.00038 * Math.Sin(ToRad(M - 2 * F)) - 0.00024 * Math.Sin(ToRad(2 * M_prime - M)) - 0.00017 * Math.Sin(ToRad(omega));

            return JulianDayToDateTime(JDE + correction);
        }

        private static double CalculateMoonLongitude(double T)
        {
            double L = 218.3164477 + 481267.88123421 * T - 0.0015786 * T * T + T * T * T / 538841.0;
            double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T * T + T * T * T / 545868.0;
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T * T + T * T * T / 24490000.0;
            double M_prime = 134.9633964 + 477198.8675055 * T + 0.0087414 * T * T + T * T * T / 69699.0;
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T * T - T * T * T / 3526000.0;
            double omega = 125.04452 - 1934.136261 * T + 0.0020708 * T * T;

            double E = 1.0 - 0.002516 * T;
            double lon = L 
                + 6.288774 * Math.Sin(ToRad(M_prime)) 
                + 1.274027 * Math.Sin(ToRad(2 * D - M_prime)) 
                + 0.658314 * Math.Sin(ToRad(2 * D)) 
                + 0.213618 * Math.Sin(ToRad(2 * M_prime)) 
                - 0.185116 * E * Math.Sin(ToRad(M)) 
                - 0.114332 * Math.Sin(ToRad(2 * F))
                + 0.058793 * Math.Sin(ToRad(2 * D - 2 * M_prime))
                + 0.057066 * E * Math.Sin(ToRad(2 * D - M - M_prime))
                + 0.053322 * Math.Sin(ToRad(2 * D + M_prime))
                + 0.045758 * E * Math.Sin(ToRad(2 * D - M))
                - 0.040923 * E * Math.Sin(ToRad(M_prime - M))
                - 0.034720 * Math.Sin(ToRad(D))
                - 0.030383 * E * Math.Sin(ToRad(M_prime + M))
                + 0.015327 * Math.Sin(ToRad(2 * D - 2 * F));

            lon += -0.00478 * Math.Sin(ToRad(omega));
            return NormalizeAngle(lon);
        }

        public static double CalculateSunLongitude(DateTime utcDate)
        {
            double T = (ToJulianDay(utcDate) - 2451545.0) / 36525.0;
            return CalculateSunLongitudeFromT(T);
        }

        private static double CalculateSunLongitudeFromT(double T)
        {
            double L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T * T;
            double M = NormalizeAngle(357.52911 + 35999.05029 * T - 0.0001537 * T * T);
            double C = (1.914602 - 0.004817 * T) * Math.Sin(ToRad(M)) + 0.019993 * Math.Sin(ToRad(2 * M));
            double sunLon = L0 + C;
            double omega = 125.04 - 1934.136 * T;
            sunLon = sunLon - 0.00569 - 0.00478 * Math.Sin(ToRad(omega));
            return NormalizeAngle(sunLon);
        }

        public static DateTime FindNextSankranti(DateTime afterDate)
        {
            DateTime low = afterDate;
            DateTime high = afterDate.AddDays(32);
            double currentSidereal = CalculateSiderealSunLongitude(afterDate);
            double target = Math.Ceiling(currentSidereal / 30.0) * 30.0;
            if (target >= 360) target = 0;

            for (int i = 0; i < 30; i++)
            {
                DateTime mid = low.AddDays((high - low).TotalDays / 2);
                double midSidereal = CalculateSiderealSunLongitude(mid);
                if (IsAngleCrossed(currentSidereal, midSidereal, target)) high = mid;
                else low = mid;
            }
            return high;
        }

        private static bool IsAngleCrossed(double start, double end, double target)
        {
            if (target == 0) return end < start;
            return start < target && end >= target;
        }

        private static double CalculateSiderealSunLongitude(DateTime utcDate)
        {
            double T = (ToJulianDay(utcDate) - 2451545.0) / 36525.0;
            return NormalizeAngle(CalculateSunLongitudeFromT(T) - (23.853 + 1.3969 * T));
        }

        private static double ToJulianDay(DateTime date)
        {
            int y = date.Year, m = date.Month, d = date.Day;
            if (m <= 2) { y--; m += 12; }
            int a = y / 100, b = 2 - a + a / 4;
            double dayFraction = (date.Hour + date.Minute / 60.0 + date.Second / 3600.0) / 24.0;
            return Math.Floor(365.25 * (y + 4716)) + Math.Floor(30.6001 * (m + 1)) + d + dayFraction + b - 1524.5;
        }

        private static DateTime JulianDayToDateTime(double jd)
        {
            long j = (long)(jd + 0.5);
            long f = j + 1401 + (((4 * j + 274277) / 146097) * 3) / 4 - 38;
            long e = 4 * f + 3, g = (e % 1461) / 4, h = 5 * g + 2;
            int day = (int)((h % 153) / 5 + 1), month = (int)((h / 153 + 2) % 12 + 1), year = (int)(e / 1461 - 4716 + (14 - month) / 12);
            double dayFraction = jd + 0.5 - Math.Floor(jd + 0.5);
            return new DateTime(year, month, day, (int)(dayFraction * 24), (int)((dayFraction * 24 - (int)(dayFraction * 24)) * 60), 0, DateTimeKind.Utc);
        }

        private static double ToRad(double degrees) => degrees * Math.PI / 180.0;
        private static double NormalizeAngle(double angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
}
