using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class Transactions 
    {
        [Inject]
        public IMediator Mediator { get; set; }

        public string ImportMessage { get; private set; }

        public ITransaction[] TransactionsList { get; private set; } = new ITransaction[0];

        public int PageSize { get; private set; } = 10;

        public int Page { get; private set; } = 1;

        public int TotalPages { get; private set; }

        public int VisibleNavigationPageRange { get; } = 3;

        public DateTime RangeStart { get; set; } = DateTime.Now.AddMonths(-6);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public int[] VisibleNavigationPages
        {
            get
            {
                if(TotalPages == 0)
                    return new int[0];

                var start = Math.Max(1, Page - VisibleNavigationPageRange);
                var end = Math.Min(TotalPages, start + (VisibleNavigationPageRange*2));

                return Enumerable.Range(start, (end - start)+1).ToArray();
            }
        }

        public async Task RefreshTransactionsFromDisk()
        {
            ImportMessage = $"Reading transactions from disk...";
            StateHasChanged();

            var localTransactions = await Mediator.Send(new ReadLocalTransactions.Request());

            ImportMessage = $"Read {localTransactions.Count()} transactions from disk. Syncing with database...";
            StateHasChanged();

            var newRecords = await Mediator.Send(new UploadTransactions.Command(localTransactions));
            if (newRecords.Any())
                ImportMessage = $"Inserted {newRecords.Count()} transactions into database.";
            else
                ImportMessage = $"No new transactions to insert.";

            TransactionsList = await Mediator.Send(new TransactionsFeature.QueryTransactions(
                RangeStart: DateTime.Now.AddMonths(-1),
                RangeEnd: DateTime.Now,
                Page: 0,
                ResultsPerPage: 50));

            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await LoadTransactions();
        }

        public async Task SetPage(int page)
        {
            Page = page;
            await LoadTransactions(recalcPages: false);
        }

        public async Task LoadTransactions(bool recalcPages=true)
        {
            TotalPages = await Mediator.Send(new TransactionsFeature.QueryTotalPages(
                RangeStart: RangeStart,
                RangeEnd: RangeEnd,
                ResultsPerPage: PageSize));

            TransactionsList = await Mediator.Send(new TransactionsFeature.QueryTransactions(
                RangeStart: RangeStart,
                RangeEnd: RangeEnd,
                Page: Page-1,
                ResultsPerPage: PageSize));

            StateHasChanged();
        }
    }
}
