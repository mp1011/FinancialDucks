namespace FinancialDucks.Application.Models.AppModels
{
    public class ScraperCommandEdit : IScraperCommandDetail
    {
        public int SourceId => Source.Id;
        public ScraperCommandType TypeId { get; set; }
        public int Sequence { get; set; }
        public bool SearchInnerText { get; set; }
        public bool PauseBeforeStep { get; set; }
        public string? Selector { get; set; }

        public string? Text { get; set; }

        public ITransactionSource? Source { get; set; }

        public int Id { get; set; }

        public bool IsDefault => TypeId == 0 && string.IsNullOrEmpty(Selector);
    }
}
