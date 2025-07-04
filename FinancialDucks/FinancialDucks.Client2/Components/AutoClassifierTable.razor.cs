using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Client2.Helpers;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading;
using static FinancialDucks.Application.Features.AutoClassifierFeature;
using static FinancialDucks.Application.Features.CategoriesFeature;
using static FinancialDucks.Application.Features.TransactionsFeature;

namespace FinancialDucks.Client2.Components
{
    public partial class AutoClassifierTable : ComponentBase, INotificationHandler<AutoClassifierFeature.AutoClassifyNotification>, IDisposable
    {
        [Inject]
        public NotificationDispatcher<AutoClassifierFeature.AutoClassifyNotification> Dispatcher { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        
        [Inject]
        public IMediator Mediator { get; set; }


        private bool IncludeAllTransactions
        {
            get => _includeAllTransactions;
            set
            {
                if (_includeAllTransactions != value)
                {
                    _includeAllTransactions = value;
                    OnIncludeAllTransactionsChanged.InvokeAsync(value);
                }
            }
        }
        private bool _includeAllTransactions;
        private EventCallback<bool> OnIncludeAllTransactionsChanged;
    

        public List<AutoClassificationResult> Results = new();

        public string SelectedTransactionText { get; set; }
        public ITransaction SelectedTransaction { get; set; }
        public ICategoryDetail? SelectedCategory { get; set; }

        public int PageSize { get; set; } = 10; 
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int ResultsPerPage { get; set; } = 10;


        protected override async Task OnInitializedAsync()
        {
            Dispatcher.Register(this, NotificationPriority.Low);
            OnIncludeAllTransactionsChanged = EventCallback.Factory.Create<bool>(this, async (value) => { _filter = null; await SetPage(Page); });
            await SetPage(Page);            
        }

        private TransactionsFilter? _filter;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

        private async Task<TransactionsFilter> Filter()
        {
            if(_filter != null)
                return _filter;

            var categoryTree = await Mediator.Send(new CategoryTreeRequest());
            var category = IncludeAllTransactions ? categoryTree : categoryTree.GetDescendant(SpecialCategory.Unclassified.ToString())!;
            _filter = new TransactionsFilter(new DateTime(2000, 1, 1), DateTime.Now, category, null);
            return _filter;
        }

        public async Task SetPage(int page)
        {
            Results.Clear();   
            _cancelTokenSource.Cancel();
            _cancelTokenSource = new CancellationTokenSource();
            Page = page;
            await UpdateTotalPages();          
            await Mediator.Send(new AutoClassifyTransactionsQuery(Page, PageSize, await Filter()), _cancelTokenSource.Token);
        }

        private async Task UpdateTotalPages()
        {            
            var summary = await Mediator.Send(new QuerySummary(await Filter(), ResultsPerPage));
            TotalPages = summary.TotalPages;
        }

        public async Task OnCategoryClicked(AutoClassificationResult result, ICategoryDetail? category)
        {
            SelectedTransactionText = result.Transaction.Description;
            SelectedTransaction = result.Transaction;
            SelectedCategory = category;
            await JSRuntime.ShowModal(nameof(CategoryQuickAdd));
        }

        public Task Handle(AutoClassifierFeature.AutoClassifyNotification notification, CancellationToken cancellationToken)
        {
            Results.Add(notification.Result);
            StateHasChanged();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispatcher.Unregister(this);
        }
    }
}