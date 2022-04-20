using FinancialDucks.Application.Extensions;

namespace FinancialDucks.Application.Models.AppModels
{
    public record SourceSnapshot(DateTime Date, string Label, decimal Amount) 
    {
        public SourceSnapshot(ISourceSnapshot? snapshot, TimeInterval timeInterval) :this(DateTime.MinValue,"",0)
        {
            if (snapshot == null)
                return;

            Date = snapshot.Date;
            Label = snapshot.Date.GetLabel(timeInterval);
            Amount = snapshot.Amount;
        }
    }
}
