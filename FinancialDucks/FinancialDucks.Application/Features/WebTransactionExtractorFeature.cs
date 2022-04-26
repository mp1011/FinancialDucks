using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class WebTransactionExtractorFeature
    {
        public record Query : IRequest<FileInfo[]>
        {

        }

        public class Handler : IRequestHandler<Query, FileInfo[]>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly IScraperService _scraperService;

            public Handler(IDataContextProvider dataContextProvider, IScraperService scraperService)
            {
                _dataContextProvider = dataContextProvider;
                _scraperService = scraperService;
            }

            public async Task<FileInfo[]> Handle(Query request, CancellationToken cancellationToken)
            {
                var commands = await GetScraperCommands();

                var sources = commands
                    .Select(p => p.Source)
                    .Distinct()
                    .ToArray();

                List<FileInfo> files = new List<FileInfo>();
                foreach(var source in sources)
                {
                    var file = await _scraperService.Execute(source, commands);
                    if (file != null)
                        files.Add(file);
                }

                return files.ToArray();
            }

            private async Task<IScraperCommandDetail[]> GetScraperCommands()
            {
                using var ctx = _dataContextProvider.CreateDataContext();
                return await ctx.ScraperCommandsDetail.ToArrayAsync(ctx);
            }         
        }
    }
}
