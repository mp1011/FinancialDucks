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

        public record SaveCommand(IScraperCommandDetail Command) : IRequest<IScraperCommandDetail> { }

        public class Handler 
            : IRequestHandler<Query, IScraperCommandDetail[]>,
              IRequestHandler<SaveCommand, IScraperCommandDetail>
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
                    .OrderBy(p=>p.Sequence)
                    .ToArrayAsync(context);
            }

            public async Task<IScraperCommandDetail> Handle(SaveCommand request, CancellationToken cancellationToken)
            {
                using var context = _contextProvider.CreateDataContext();

                var conflict = context.ScraperCommandsDetail
                    .SingleOrDefault(p => p.SourceId == request.Command.SourceId
                                        && p.Sequence == request.Command.Sequence
                                        && p.Id != request.Command.Id);

                if(conflict != null)
                {
                    var laterInSequence = context.ScraperCommandsDetail
                                          .Where(p => p.SourceId == request.Command.SourceId
                                                 && p.Sequence >= request.Command.Sequence
                                                 && p.Id != request.Command.Id)
                                          .OrderByDescending(p => p.Sequence)
                                          .ToArray();

                    foreach (var s in laterInSequence)
                    {
                        s.Sequence++;
                        await context.Update(s);
                    }
                }

             


                return await context.Update(request.Command);
            }
        }
    }
}
