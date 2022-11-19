namespace FinancialDucks.Application.Models.AppModels
{
    public class BudgetLineEdit : IBudgetLine
    {
        public int Id { get; set; }

        public decimal Budget { get; set; }

        public ICategory Category { get; set; }
    }
}
