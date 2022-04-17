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

        private readonly ChangeTracked<TransactionsFilter> _filter = new ChangeTracked<TransactionsFilter>();

        [Parameter]
        public TransactionsFilter Filter
        {
            get => _filter;
            set => _filter.Value = value;
        }

        [Parameter]
        public EventCallback<TransactionMouseUpEventArgs> OnTransactionMouseUp { get; set; }

        public TransactionSortColumn SortColumn { get; set; } = TransactionSortColumn.Date;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        public SortDirection? DateSortDirection
        {
            get
            {
                if (Filter != null && SortColumn == TransactionSortColumn.Date)
                    return SortDirection;
                else
                    return null;
            }
        }

        public SortDirection? AmountSortDirection
        {
            get
            {
                if (Filter != null && SortColumn == TransactionSortColumn.Amount)
                    return SortDirection;
                else
                    return null;
            }
        }

        public bool Loading { get; private set; }

        public ITransactionDetail[] Transactions { get; private set; } = Array.Empty<ITransactionDetail>();

        public int PageSize { get; private set; } = 10;

        public int Page { get; private set; } = 1;

        public int TotalPages { get; private set; }

        public int VisibleNavigationPageRange { get; } = 3;

        public int[] VisibleNavigationPages
        {
            get
            {
                if (TotalPages == 0)
                    return Array.Empty<int>();

                var start = Math.Max(1, Page - VisibleNavigationPageRange);
                var end = Math.Min(TotalPages, start + (VisibleNavigationPageRange * 2));

                return Enumerable.Range(start, (end - start) + 1).ToArray();
            }
        }

        public async Task ToggleSortDate()
        {
            SortColumn = TransactionSortColumn.Date;
            SortDirection = SortDirection.Toggle();
            await LoadTransactions(recalcPages:false);
        }

        public async Task ToggleSortAmount()
        {
            SortColumn = TransactionSortColumn.Amount;
            SortDirection = SortDirection.Toggle();
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
                SortColumn,
                SortDirection,
                Page: Page - 1,
                ResultsPerPage: PageSize));

            Loading = false;
            StateHasChanged();
        }

    }
}
