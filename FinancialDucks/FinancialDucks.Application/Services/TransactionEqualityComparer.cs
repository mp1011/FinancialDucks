using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using System.Diagnostics.CodeAnalysis;

namespace FinancialDucks.Application.Services
{
    public class TransactionEqualityComparer : IEqualityComparer<ITransaction>
    {
        public bool Equals(ITransaction? x, ITransaction? y)
        {
            if(x == null || y ==null)
                return x == null && y == null;

            var xAdjusted = x.Description.RemoveNonAlphanumeric();
            var yAdjusted = y.Description.RemoveNonAlphanumeric();

            return x.Amount == y.Amount
                && x.Date == y.Date
                && xAdjusted.Equals(yAdjusted, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] ITransaction obj)
        {
            return obj.Id;
        }
    }
}
