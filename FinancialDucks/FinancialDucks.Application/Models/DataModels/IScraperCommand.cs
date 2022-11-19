using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Models
{
    public interface IScraperCommandDetail : IWithId
    {
        int SourceId => Source.Id;
        ScraperCommandType TypeId { get; set; }
        int Sequence { get; set; }
        bool PauseBeforeStep { get; set; }
        string Selector { get; set; }
        string Text { get; set; }
        ITransactionSource Source { get; }
    }
}
