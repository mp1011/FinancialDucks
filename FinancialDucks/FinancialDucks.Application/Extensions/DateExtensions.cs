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
    }
}
