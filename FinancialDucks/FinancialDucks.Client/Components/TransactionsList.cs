using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class TransactionsList
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public TransactionsFeature.TransactionsFilter Filter { get; set; }

        public bool Loading { get; private set; }

        public ITransaction[] Transactions { get; private set; } = new ITransaction[0];

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

        protected override async Task OnParametersSetAsync()
        {
            Page = 1;
            await LoadTransactions(recalcPages: true);
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
            TotalPages = await Mediator.Send(new TransactionsFeature.QueryTotalPages(
                Filter,
                ResultsPerPage: PageSize));

            Transactions = await Mediator.Send(new TransactionsFeature.QueryTransactions(
                Filter,
                Page: Page - 1,
                ResultsPerPage: PageSize));

            Loading = false;
            StateHasChanged();
        }
    }
}
