namespace FinancialDucks.Application.Models.AppModels
{
    public record CategoryRule(int Id,
                                short Priority,
                                ICategory Category,
                                string? SubstringMatch=null,
                                decimal? AmountMin = null,
                                decimal? AmountMax = null,
                                DateTime? DateMin = null,
                                DateTime? DateMax = null
                                ) : ICategoryRuleDetail
    {

        int ICategoryRule.CategoryId => Category.Id;
    }
}