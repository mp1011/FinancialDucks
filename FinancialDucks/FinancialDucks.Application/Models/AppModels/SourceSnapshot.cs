namespace FinancialDucks.Application.Models.AppModels
{
    public record SourceSnapshot(DateTime Date, decimal Amount) 
    {
        public SourceSnapshot(ISourceSnapshot? snapshot) :this(DateTime.MinValue,0)
        {
            if (snapshot == null)
                return;

            Date = snapshot.Date;
            Amount = snapshot.Amount;
        }
    }
}
