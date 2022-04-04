using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Client.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class TransactionsList
    {
        [Inject]
        public IMediator Mediator { get; set; }

        private ChangeTracked<TransactionsFeature.TransactionsFilter> _filter = new ChangeTracked<TransactionsFeature.TransactionsFilter>();

        [Parameter]
        public TransactionsFeature.TransactionsFilter Filter
        {
            get => _filter;
            set => _filter.Value = value;
        }

        [Parameter]
        public EventCallback<TransactionMouseUpEventArgs> OnTransactionMouseUp { get; set; }

        public SortDirection? DateSortDirection
        {
            get
            {
                if (Filter != null && Filter.SortColumn == TransactionSortColumn.Date)
                    return Filter.SortDirection;
                else
                    return null;
            }
        }

        public SortDirection? AmountSortDirection
        {
            get
            {
                if (Filter != null && Filter.SortColumn == TransactionSortColumn.Amount)
                    return Filter.SortDirection;
                else
                    return null;
            }
        }

        public bool Loading { get; private set; }

        public ITransactionDetail[] Transactions { get; private set; } = new ITransactionDetail[0];

        public int PageSize { get; private set; } = 10;

        public int Page { get; private set; } = 1;

        public int TotalPages { get; private set; }

        public int VisibleNavigationPageRange { get; } = 3;

        public int[] VisibleNavigationPages
        {
            get
            {
                if (TotalPages == 0)
                    return new int[0];

                var start = Math.Max(1, Page - VisibleNavigationPageRange);
                var end = Math.Min(TotalPages, start + (VisibleNavigationPageRange * 2));

                return Enumerable.Range(start, (end - start) + 1).ToArray();
            }
        }

        public async Task ToggleSortDate()
        {
            Filter = Filter.ToggleSortDate();
            await LoadTransactions(recalcPages:false);
        }

        public async Task ToggleSortAmount()
        {
            Filter = Filter.ToggleSortAmount();
            await LoadTransactions(recalcPages: false);
        }

        protected override async Task OnParametersSetAsync()
        {
            Page = 1;
            if (_filter.HasChanges)
            {
                await LoadTransactions(recalcPages: true);
                _filter.AcceptChanges();
            }
        }

        public async Task SetPage(int page)
        {
            Page = page;
            await LoadTransactions(recalcPages: false);
        }

        public async Task LoadTransactions(bool recalcPages = true)
        {
            if (Filter == null)
                return;
            
            Loading = true;
            if (recalcPages)
            {
                TotalPages = await Mediator.Send(new TransactionsFeature.QueryTotalPages(
                    Filter,
                    ResultsPerPage: PageSize));
            }

            Transactions = await Mediator.Send(new TransactionsFeature.QueryTransactions(
                Filter,
                Page: Page - 1,
                ResultsPerPage: PageSize));

            Loading = false;
            StateHasChanged();
        }

    }
}
