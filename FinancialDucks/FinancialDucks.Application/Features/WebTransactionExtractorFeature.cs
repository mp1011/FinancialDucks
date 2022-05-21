using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class WebTransactionExtractorFeature
    {
        public record Query(ITransactionSource[] SourceFilter) : IRequest<FileInfo[]>
        {

        }

        public record QueryPreview(IScraperCommandDetail[] Commands) : IRequest<FileInfo[]>
        {

        }

        public record Notification(FetchStatus FetchStatus, IScraperCommandDetail Command, string Message, DateTime Timestamp, ScrapedElement[] Elements) : INotification 
        {
            public class Handler : INotificationHandler<Notification>
            {
                private readonly NotificationDispatcher<Notification> _dispatcher;

                public Handler(NotificationDispatcher<Notification> dispatcher)
                {
                    _dispatcher = dispatcher;
                }

                public async Task Handle(Notification notification, CancellationToken cancellationToken)
                {
                    await _dispatcher.DispatchEvent(notification, cancellationToken);
                }
            }
        
        }

        public class Handler : IRequestHandler<Query, FileInfo[]>, 
            IRequestHandler<QueryPreview, FileInfo[]>
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
                var commands = await GetScraperCommands(request.SourceFilter);

                var sourceIds = commands
                    .Select(p => p.Source.Id)
                    .Distinct()
                    .ToArray();

                List<FileInfo> files = new List<FileInfo>();
                foreach(var sourceId in sourceIds)
                {
                    var source = commands.First(p => p.SourceId == sourceId).Source;
                    files.AddRange(await _scraperService.Execute(source, commands, showBrowser:true, cancellationToken: cancellationToken));
                }

                return files.ToArray();
            }

            public async Task<FileInfo[]> Handle(QueryPreview request, CancellationToken cancellationToken)
            {
                var commands = request.Commands;

                var sourceIds = commands
                    .Select(p => p.SourceId)
                    .Distinct()
                    .ToArray();

                List<FileInfo> files = new List<FileInfo>();
                foreach (var sourceId in sourceIds)
                {
                    var source = commands.First(p => p.SourceId == sourceId).Source;
                    files.AddRange(await _scraperService.Execute(source, commands, showBrowser: true, cancellationToken: cancellationToken));
                }

                return files.ToArray();
            }

            private async Task<IScraperCommandDetail[]> GetScraperCommands(ITransactionSource[] sources)
            {
                var sourceIds = sources
                    .Select(s => s.Id)
                    .ToArray();

                using var ctx = _dataContextProvider.CreateDataContext();
                return await ctx.ScraperCommandsDetail
                    .Where(p=> sourceIds.Contains(p.SourceId))
                    .ToArrayAsync(ctx);
            }


            private async Task<IScraperCommandDetail[]> GetScraperCommands()
            {
                using var ctx = _dataContextProvider.CreateDataContext();
                return await ctx.ScraperCommandsDetail.ToArrayAsync(ctx);
            }         
        }
    }
}
