namespace FinancialDucks.Application.Models.AppModels
{
    public record AutoClassificationResult(ITransaction Transaction, ICategory[] MatchedCategories)
    {
    }
}
