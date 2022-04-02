using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;

namespace FinancialDucks.Application.Services
{
    public interface ICategoryTreeProvider
    {
        Task<ICategoryDetail> GetCategoryTree();
    }

    public class CachedCategoryTreeProvider : 
        CachedDataProviderWithChangeNotificationListener<ICategoryDetail, CategoryChangeNotification>,
        ICategoryTreeProvider
    {
        private readonly ICategoryTreeProvider _realProvider;

        public CachedCategoryTreeProvider(ICategoryTreeProvider realProvider, NotificationDispatcher<CategoryChangeNotification> dispatcher)
        {
            _realProvider = realProvider;
            dispatcher.Register(this);
        }

        public async Task<ICategoryDetail> GetCategoryTree()
        {
            var result = await GetOrCompute();
            return result!.Data;
        }

        protected override async Task<CachedData<ICategoryDetail>> Compute()
        {
            var data = await _realProvider.GetCategoryTree();
            return new CachedData<ICategoryDetail>(data, DateTime.Now);
        }

        protected override bool ShouldInvalidateCache(CachedData<ICategoryDetail> current, CategoryChangeNotification changeNotification)
        {
            return true;
        }
    }
}
