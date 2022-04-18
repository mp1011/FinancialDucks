using FinancialDucks.Application.Extensions;

namespace FinancialDucks.Application.Models.AppModels
{
    public record TransactionsFilter(
           DateTime RangeStart,
           DateTime RangeEnd,
           ICategoryDetail Category,
           ITransactionSource[] Sources,
           string? TextFilter=null)
    {
        public TransactionsFilter(
           DateTime RangeStart,
           DateTime RangeEnd,
           ICategoryDetail Category,
           string? TextFilter = null) : this(RangeStart,RangeEnd,Category, new ITransactionSource[] { }, TextFilter)
        {

        }

        public bool IsValid(bool requireCategory)
        {
            return !RangeStart.IsInvalid()
                && !RangeEnd.IsInvalid()
                && (!requireCategory || Category != null);
        }

        public TransactionsFilter ChangeCategory(ICategoryDetail newCategory)
        {
            return new TransactionsFilter(RangeStart, RangeEnd, newCategory, Sources, TextFilter);
        }
    }

}
