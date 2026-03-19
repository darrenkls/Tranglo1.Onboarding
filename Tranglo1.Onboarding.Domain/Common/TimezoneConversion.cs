using CSharpFunctionalExtensions;
using System;
using System.Linq;
using TimeZoneNames;

namespace Tranglo1.Onboarding.Domain.Common
{
    public static class TimezoneConversion
    {
        public static DateTime ConvertFromUTC(string regionCountry, string languageCode, DateTime dateTime)
        {
            var timeZoneValues = TZNames.GetNamesForTimeZone(regionCountry, languageCode);
            var timezoneName = TimeZoneInfo.FindSystemTimeZoneById(timeZoneValues.Standard);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timezoneName);
        }

        public static Result<DateTime> ConvertFromUTCWithTimezoneName(string timezoneName, string languageCode, DateTime dateTime, bool omitException = false)
        {
            if (string.IsNullOrEmpty(timezoneName))
                return dateTime;

            var timeZoneValues = TZNames.GetDisplayNames(languageCode);
            var timezoneId = timeZoneValues.FirstOrDefault(x => x.Value == timezoneName).Key;

            if (timezoneId == null)
            {
                if (omitException)
                    return dateTime;

                return Result.Failure<DateTime>("Invalid timezone name.");
            }

            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, tzi);
        }
    }
}
