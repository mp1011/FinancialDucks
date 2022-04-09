namespace FinancialDucks.Application.Models.AppModels
{
    public record TimeSlice(DateTime SliceStart, DateTime SliceEnd);
    public record CategoryTimeSlice(decimal Amount, DateTime SliceStart, DateTime SliceEnd) 
        : TimeSlice(SliceStart,SliceEnd) 
    { 

        public decimal AmountAbs => Math.Abs(Amount);

        public string Label => SliceStart.ToShortDateString();

        public CategoryTimeSlice ChangeAmount(decimal newAmount) => 
            new CategoryTimeSlice(newAmount,SliceStart,SliceEnd);


    }

    public record CategorySnapshot(decimal Amount, DateTime Date);
}
