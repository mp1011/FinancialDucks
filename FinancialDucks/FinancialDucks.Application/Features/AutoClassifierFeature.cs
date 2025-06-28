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
        public record AutoClassifyTransactionsQuery() : IRequest<AutoClassificationResult[]>;
        public record AutoClassifyTransactionQuery(ITransaction Transaction, ICategoryDetail[] PossibleCategories) : IRequest<AutoClassificationResult>;
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

            private ICategoryDetail[] PossibleCategories(ICategoryDetail root)
            {
                string[] Ignore = { SpecialCategory.Credits.ToString(), SpecialCategory.Debits.ToString(), SpecialCategory.Unclassified.ToString() };
                return root.GetDescendants()
                           .Where(p => !Ignore.Contains(p.Name))
                           .ToArray();
            }

            public async Task<AutoClassificationResult[]> Handle(AutoClassifyTransactionsQuery request, CancellationToken cancellationToken)
            {
                var categoryTree = await _mediator.Send(new CategoryTreeRequest());
                var unclassifiedCategory = categoryTree.GetDescendant(SpecialCategory.Unclassified.ToString())!;

                var possibleCategories = PossibleCategories(categoryTree);
                var transactions = await _mediator.Send(new QueryTransactions(
                    new TransactionsFilter(new DateTime(2000, 1, 1), DateTime.Now, unclassifiedCategory, null),
                    TransactionSortColumn.Date,
                    SortDirection.Ascending,
                    Page: 0,
                    ResultsPerPage: 1000));

                List<AutoClassificationResult> results = new();

                return await Task.WhenAll(transactions.Select(transaction =>
                  _mediator.Send(new AutoClassifyTransactionQuery(transaction, possibleCategories), cancellationToken)));
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

            private string Prompt(AutoClassifyTransactionQuery request) =>
                $"Classify the following transaction into one of the following categories: {request.PossibleCategories.Select(p=>p.Name).StringJoin(", ")} \n" +
                $"Description: {request.Transaction.Description}\n" +
                $"Amount: {request.Transaction.Amount}\n" +
                $"Date: {request.Transaction.Date:yyyy-MM-dd}\n" +
                $"Return only the three most likely categories, separated by a comma. Do not return any other text.";

            public async Task<AutoClassificationResult> Handle(AutoClassifyTransactionQuery request, CancellationToken cancellationToken)
            {
                var chatResult = await _promptEngine.Chat(Prompt(request));

                var matchedCategories = chatResult.Split(',')
                                                  .Select(p=> request.PossibleCategories.FirstOrDefault(c => c.Name.Equals(p.Trim())))
                                                  .Where(c => c != null)
                                                  .ToArray();

                var result = new AutoClassificationResult(request.Transaction, matchedCategories!);
                await _mediator.Publish(new AutoClassifyNotification(result), cancellationToken);
                return result;
            }
        }
    }
}
