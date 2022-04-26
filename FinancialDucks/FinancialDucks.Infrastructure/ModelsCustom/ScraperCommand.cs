using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class ScraperCommand : IScraperCommandDetail
    {
        ScraperCommandType IScraperCommandDetail.TypeId
        {
            get => (ScraperCommandType)TypeId;
            set => TypeId = (int)value;
        }

        ITransactionSource IScraperCommandDetail.Source => Source;
    }
}
