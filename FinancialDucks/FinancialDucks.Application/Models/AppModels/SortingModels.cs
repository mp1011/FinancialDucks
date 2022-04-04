namespace FinancialDucks.Application.Models.AppModels
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum TransactionSortColumn
    {
        Date,
        Amount
    } 

    public static class SortExtensions
    {
        public static SortDirection Toggle(this SortDirection s)
        {
            if (s == SortDirection.Ascending)
                return SortDirection.Descending;
            else 
                return SortDirection.Ascending;
        }
    }
}
