using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Infrastructure.Models;

namespace FinancialDucks.Infrastructure.Services
{
    public class DataContextProvider : IDataContextProvider
    {
        private readonly ISettingsService _settingsService;
        private readonly IEqualityComparer<ITransaction> _transactionEqualityComparer;
        private readonly IObjectMapper _objectMapper;

        public DataContextProvider(ISettingsService settingsService, IEqualityComparer<ITransaction> transactionEqualityComparer, IObjectMapper objectMapper)
        {
            _settingsService = settingsService;
            _transactionEqualityComparer = transactionEqualityComparer;
            _objectMapper = objectMapper;
        }

        public FinancialDucksContext CreateDataContext()
        {
            return new FinancialDucksContext(_settingsService, _transactionEqualityComparer, _objectMapper);
        }

        IDataContext IDataContextProvider.CreateDataContext() => CreateDataContext();
    }
}
