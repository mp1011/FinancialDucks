namespace FinancialDucks.Application.Models
{
    public interface ISourceSnapshot : IWithId
    {
        DateTime Date { get;  }
        decimal Amount { get;  }
        int SourceId { get;  }
    }
}
