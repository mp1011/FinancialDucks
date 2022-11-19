namespace FinancialDucks.Application.Models
{
    public interface IBudgetLine : IWithId
    {
        public decimal Budget { get; set; }

        public ICategory Category { get; }
    }
}

