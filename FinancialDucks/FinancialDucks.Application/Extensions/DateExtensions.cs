using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Extensions
{
    public static class DateExtensions
    {

        public static DateTime StartOfDay(this DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day);

        public static DateTime EndOfDay(this DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, 23, 59, 59,999);

        public static string GetLabel(this DateTime date, TimeInterval interval)
        {
            switch (interval)
            {
                case TimeInterval.Daily:
                case TimeInterval.Biweekly:
                    return date.ToShortDateString();
                case TimeInterval.Monthly:
                    return date.ToString("MMMyy");
                case TimeInterval.Quarterly:
                    var year = date.ToString("yy");
                    if (date.Month <= 3)
                        return $"{year}Q1";
                    else if (date.Month <= 6)
                        return $"{year}Q2";
                    else if (date.Month <= 9)
                        return $"{year}Q3";
                    else
                        return $"{year}Q4";
                case TimeInterval.Weekly:
                    year = date.ToString("yy");
                    return $"{year}w{(date.DayOfYear / 7) + 1}";
                case TimeInterval.Annual:
                    return date.ToString("yy");
                default:
                    return date.ToShortDateString();
            }
        }
        public static IEnumerable<TimeSlice> SliceTime(this DateTime start, TimeInterval interval, DateTime end)
        {
            DateTime date = start;
            while (date < end)
            {
                var endDate = date.Add(interval);
                yield return new TimeSlice(date.StartOfDay(), endDate.AddDays(-1).EndOfDay());
                date = endDate;
            }
        }

        public static bool IsInvalid(this DateTime date)
        {
            return date.Year <= 1900;
        }

        public static DateTime GetClosestInterval(this DateTime date, TimeInterval interval)
        {
            switch(interval)
            {
                case TimeInterval.Weekly:
                    var dayOfWeek = date.DayOfWeek;
                    return date.AddDays(-(int)dayOfWeek);
                case TimeInterval.Biweekly:
                    return date;
                case TimeInterval.Monthly:
                    return date.AddDays(-(date.Day - 1));
                case TimeInterval.Quarterly:
                    var year = date.Year;
                    if (date.Month <= 3)
                        return new DateTime(year, 1, 1);
                    else if (date.Month <= 6)
                        return new DateTime(year, 4, 1);
                    else if (date.Month <= 9)
                        return new DateTime(year, 7, 1);
                    else
                        return new DateTime(year, 10, 1);
                default:
                    return date;
            }
        }
    }
}
