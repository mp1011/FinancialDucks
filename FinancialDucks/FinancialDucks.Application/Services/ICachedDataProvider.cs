using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using MediatR;

namespace FinancialDucks.Application.Services
{
    public interface ICachedDataProvider<T>
    {
        Task<CachedData<T>?> GetOrCompute();
    }

    public abstract class CachedDataProviderWithChangeNotificationListener<TData,TNotification> :
        ICachedDataProvider<TData>, INotificationHandler<TNotification>
        where TNotification:INotification
    {
        private CachedData<TData>? _cachedData;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount:1, maxCount:1);

        public async Task<CachedData<TData>?> GetOrCompute()
        {
            return _cachedData ?? (_cachedData = await ComputeThreadsafe());
        }

        public DateTime GetLastUpdatedDate()
        {
            if (_cachedData == null)
                return DateTime.MinValue;
            else
                return _cachedData.ComputedDate;
        }

        public async Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            if (_cachedData != null && ShouldInvalidateCache(_cachedData, notification))
                _cachedData = null;

            if (_cachedData == null)
                _cachedData = await ComputeThreadsafe();
        }

        private async Task<CachedData<TData>> ComputeThreadsafe()
        {
            await _semaphore.WaitAsync();

            try
            {
                return await Compute();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected abstract Task<CachedData<TData>> Compute(); 
        protected abstract bool ShouldInvalidateCache(CachedData<TData> current, TNotification changeNotification);
    }
}
