using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Models
{
    public interface IScraperCommandDetail : IWithId
    {
        int SourceId { get; set; }
        ScraperCommandType TypeId { get; set; }
        int Sequence { get; set; }
        bool SearchInnerText { get; set; }
        bool WaitForNavigate { get; set; }
        string Selector { get; set; }
        ITransactionSource Source {get;}
    }
}
