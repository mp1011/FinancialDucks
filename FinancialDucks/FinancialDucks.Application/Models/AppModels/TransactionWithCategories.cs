namespace FinancialDucks.Application.Models.AppModels
{
    public record TransactionWithCategories(ITransactionDetail Transaction, IEnumerable<ICategory> Categories) : ITransactionDetail
    {
        public ITransactionSource Source => Transaction.Source;

        public decimal Amount => Transaction.Amount;

        public DateTime Date => Transaction.Date;

        public string Description => Transaction.Description;

        public int SourceId => Transaction.SourceId;

        public int Id => Transaction.Id;
    }
}
