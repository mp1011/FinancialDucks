using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using static FinancialDucks.Application.Features.CategoriesFeature;
using static FinancialDucks.Application.Features.TransactionsFeature;
using static FinancialDucks.Application.Features.WebTransactionExtractorFeature;

namespace FinancialDucks.Application.Features
{  
    public class AutoClassifierFeature
    {
        public record AutoClassifyTransactionsQuery(int Page, int ResultsPerPage, TransactionsFilter Filter) : IRequest<AutoClassificationResult[]>;
        public record AutoClassifyTransactionQuery(ITransaction Transaction, ICategoryDetail CategoryTree) : IRequest<AutoClassificationResult>;
        public record AutoClassifyNotification(AutoClassificationResult Result) : INotification
        {
            public class Handler : INotificationHandler<AutoClassifyNotification>
            {
                private readonly NotificationDispatcher<AutoClassifyNotification> _dispatcher;

                public Handler(NotificationDispatcher<AutoClassifyNotification> dispatcher)
                {
                    _dispatcher = dispatcher;
                }

                public async Task Handle(AutoClassifyNotification notification, CancellationToken cancellationToken)
                {
                    await _dispatcher.DispatchEvent(notification, cancellationToken);
                }
            }
        }
        public class AutoClassifyTransactionsHandler : IRequestHandler<AutoClassifyTransactionsQuery, AutoClassificationResult[]>
        {
            private readonly IMediator _mediator;

            public AutoClassifyTransactionsHandler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<AutoClassificationResult[]> Handle(AutoClassifyTransactionsQuery request, CancellationToken cancellationToken)
            {
                var categoryTree = await _mediator.Send(new CategoryTreeRequest());
                              
                var summary = await _mediator.Send(new QuerySummary(request.Filter, request.ResultsPerPage), cancellationToken);
                var transactions = await _mediator.Send(new QueryTransactions(
                    request.Filter,
                    TransactionSortColumn.Date,
                    SortDirection.Ascending,
                    Page: request.Page-1,
                    ResultsPerPage: request.ResultsPerPage));
                
                return await Task.WhenAll(transactions.Select(transaction =>
                  _mediator.Send(new AutoClassifyTransactionQuery(transaction, categoryTree), cancellationToken)));
            }
        }
        public class AutoClassifyTransactionHandler : IRequestHandler<AutoClassifyTransactionQuery, AutoClassificationResult>
        {
            private readonly IPromptEngine _promptEngine;
            private readonly ICategoryTreeProvider _categoryTreeProvider;
            private readonly IMediator _mediator;

            public AutoClassifyTransactionHandler(IMediator mediator, IPromptEngine promptEngine, ICategoryTreeProvider categoryTreeProvider)
            {
                _mediator = mediator;
                _promptEngine = promptEngine;
                _categoryTreeProvider = categoryTreeProvider;
            }

            private string Prompt(AutoClassifyTransactionQuery request, ICategory[] categories) =>
                $"Which categories best match \"{request.Transaction.Description}\"? Choices are {categories.Select(p => p.Name).StringJoin(", ")}. Return only the category names separated by comma.";

            private async Task<ICategoryDetail[]> BestMatches(AutoClassifyTransactionQuery request, IEnumerable<ICategoryDetail> possibleCategories)
            {
                var chatResult = await _promptEngine.Chat(Prompt(request, possibleCategories.ToArray())) ?? "";

                return chatResult.Split(',')
                                 .Select(p => possibleCategories.FirstOrDefault(c => c.Name.Equals(p.Trim())))
                                 .Where(c => c != null)
                                 .ToArray()!;
            }

            private ICategoryDetail[] InitialCategories(ICategoryDetail categoryTree)
            {
                var debits = categoryTree.GetDescendant(SpecialCategory.Debits.ToString())!;
                var credits = categoryTree.GetDescendant(SpecialCategory.Credits.ToString())!;

                return debits.Children.Union(credits.Children).ToArray();
            }
            public async Task<AutoClassificationResult> Handle(AutoClassifyTransactionQuery request, CancellationToken cancellationToken)
            {
                var initialClassification = await BestMatches(request, InitialCategories(request.CategoryTree));
              
                List<ICategoryDetail> bestMatches = new List<ICategoryDetail>();    
                foreach (var initialCategory in initialClassification)
                {
                    var subMatches = await BestMatchesInCategory(request, initialCategory);
                    bestMatches.AddRange(subMatches);
                }

                var result = new AutoClassificationResult(request.Transaction, bestMatches.Distinct().ToArray());
                await _mediator.Publish(new AutoClassifyNotification(result), cancellationToken);
                return result;
            }

            private async Task<IEnumerable<ICategoryDetail>> BestMatchesInCategory(AutoClassifyTransactionQuery query, ICategoryDetail category)
            {
                List<ICategoryDetail> bestMatches = new List<ICategoryDetail>();
                bestMatches.Add(category);

                var matches = await BestMatches(query, category.Children);
                bestMatches.AddRange(matches);

                foreach (var match in matches)
                {
                    var subMatches = await BestMatchesInCategory(query, match);
                    bestMatches.AddRange(subMatches);
                }

                return bestMatches;
            }
        }
    }
}
