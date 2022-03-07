namespace FinancialDucks.Application.Models
{
    public interface ITransaction : IWithId
    {
        decimal Amount { get; }
    }
}
