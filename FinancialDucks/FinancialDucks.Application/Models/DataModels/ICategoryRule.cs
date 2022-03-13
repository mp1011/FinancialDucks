namespace FinancialDucks.Application.Models
{
    public interface ICategoryRule: IWithId
    {
        int CategoryId { get; }
        string? SubstringMatch { get; }
        decimal? AmountMin { get; }
        decimal? AmountMax { get; }
        DateTime? DateMin { get; }
        DateTime? DateMax { get;  }
        short Priority { get;  }
    }

    public interface ICategoryRuleDetail : ICategoryRule
    {
        ICategory Category { get; }
    }
}
