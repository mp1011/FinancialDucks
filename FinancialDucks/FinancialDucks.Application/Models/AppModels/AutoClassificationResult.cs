namespace FinancialDucks.Application.Models.AppModels
{
    public record AutoClassificationResult(ITransaction Transaction, ICategoryDetail[] MatchedCategories);
}
