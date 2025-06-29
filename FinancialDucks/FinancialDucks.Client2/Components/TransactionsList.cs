using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Client2.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
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

        public TransactionWithCategories[] Transactions { get; private set; } = Array.Empty<TransactionWithCategories>();
        public int Page { get; set; } = 1;
        public int PageSize { get; private set; } = 10;
        public int TotalPages => Summary.TotalPages;
        public TransactionsSummary Summary { get; private set; } = new TransactionsSummary(0, 0, 0);
     
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
                Summary = await Mediator.Send(new TransactionsFeature.QuerySummary(
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
