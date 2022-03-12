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

            return x.Amount == y.Amount
                && x.Date == y.Date
                && x.Description == y.Description;
        }

        public int GetHashCode([DisallowNull] ITransaction obj)
        {
            return obj.Id;
        }
    }
}
