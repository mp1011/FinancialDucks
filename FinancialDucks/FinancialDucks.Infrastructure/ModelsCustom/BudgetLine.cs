using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class BudgetLine : IBudgetLine
    {
        ICategory IBudgetLine.Category => Category;
    }
}
