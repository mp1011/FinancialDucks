namespace FinancialDucks.Application.Models.AppModels
{
    public record TimeSlice(DateTime SliceStart, DateTime SliceEnd);
    public record CategoryTimeSlice(decimal Amount, TimeInterval TimeInterval, DateTime SliceStart, DateTime SliceEnd) 
        : TimeSlice(SliceStart,SliceEnd) 
    { 
        
       
        public decimal AmountAbs => Math.Abs(Amount);

        public string Label
        {
            get
            {
                switch (TimeInterval)
                {
                    case TimeInterval.Daily:
                        return SliceStart.ToShortDateString();
                    case TimeInterval.Monthly:
                        return SliceStart.ToString("MMMyy");
                    case TimeInterval.Quarterly:
                        var year = SliceStart.ToString("yy");
                        if (SliceStart.Month <= 3)
                            return $"{year}Q1";
                        else if (SliceStart.Month <= 3)
                            return $"{year}Q2";
                        else if(SliceStart.Month <= 3)
                            return $"{year}Q3";
                        else
                            return $"{year}Q4";
                    case TimeInterval.Weekly:
                        year = SliceStart.ToString("yy");
                        return $"{year}w{SliceStart.DayOfYear}";
                    case TimeInterval.Annual:
                        return SliceStart.ToString("yy");
                    default:
                        return SliceStart.ToShortDateString();
                }
            }
        }

        public CategoryTimeSlice ChangeAmount(decimal newAmount) => 
            new CategoryTimeSlice(newAmount,TimeInterval, SliceStart,SliceEnd);


    }

    public record CategorySnapshot(decimal Amount, DateTime Date);
}
