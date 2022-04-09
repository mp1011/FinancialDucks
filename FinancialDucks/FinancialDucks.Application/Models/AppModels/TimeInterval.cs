namespace FinancialDucks.Application.Models.AppModels
{
    public enum TimeInterval
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Annual
    }

    public static class TimeIntervalExtensions
    {
        public static DateTime Add(this DateTime date, TimeInterval t)
        {
            switch(t)
            {
                case TimeInterval.Daily:
                    return date.AddDays(1);
                case TimeInterval.Weekly:
                    return date.AddDays(7);
                case TimeInterval.Monthly:
                    return date.AddMonths(1);
                case TimeInterval.Quarterly:
                    return date.AddMonths(3);
                case TimeInterval.Annual:
                    return date.AddYears(1);
                default:
                    return date;
            }
        }
    }
}
