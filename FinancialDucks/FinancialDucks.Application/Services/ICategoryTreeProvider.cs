using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ICategoryTreeProvider
    {
        Task<ICategoryDetail> GetCategoryTree();
    }
}
