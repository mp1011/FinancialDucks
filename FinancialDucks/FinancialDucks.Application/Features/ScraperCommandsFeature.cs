using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class ScraperCommandsFeature
    {
        public record Query(ITransactionSource Source) : IRequest<IScraperCommandDetail[]>
        {
        }

        public class Handler : IRequestHandler<Query, IScraperCommandDetail[]>
        {
            private readonly IDataContextProvider _contextProvider;

            public Handler(IDataContextProvider contextProvider)
            {
                _contextProvider = contextProvider;
            }

            public async Task<IScraperCommandDetail[]> Handle(Query request, CancellationToken cancellationToken)
            {
                using var context = _contextProvider.CreateDataContext();
                return await context.ScraperCommandsDetail
                    .Where(p => p.SourceId == request.Source.Id)
                    .ToArrayAsync(context);
            }
        }
    }
}
