using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.Converters
{
    public static class EATConverter
    {
        private static readonly TimeZoneInfo EastAfricaTimeZone = GetEATTimeZone();
        private const string Format = "yyyy-MM-dd HH:mm:ss.fff";

        public static string ConvertFromUtcToEAT(string utcDateTime)
        {
            try
            {
                // Parse input with our format 
                DateTime parsedDateTime = 
                    DateTime.ParseExact(utcDateTime, Format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

                // Convert to east african time
                DateTime eatDateTime = TimeZoneInfo.ConvertTimeFromUtc(parsedDateTime, EastAfricaTimeZone);
                return eatDateTime.ToString(Format);
            }
            catch
            {
                return null;
            }
        }

        private static TimeZoneInfo GetEATTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            }
            catch
            {
                // Fallback to UTC if EAT is not available
                return TimeZoneInfo.CreateCustomTimeZone(
                    "EAT",
                    TimeSpan.FromHours(3),
                    "East Africa Time",
                    "East Africa Time");

            }
        }
    }
}
