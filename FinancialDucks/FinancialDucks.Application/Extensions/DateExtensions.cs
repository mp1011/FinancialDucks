using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Extensions
{
    public static class DateExtensions
    {
        public static IEnumerable<TimeSlice> SliceTime(this DateTime start, TimeInterval interval, DateTime end)
        {
            DateTime date = start;
            while (date < end)
            {
                var endDate = date.Add(interval);
                yield return new TimeSlice(date, endDate);
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
