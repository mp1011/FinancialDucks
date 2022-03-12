using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public class TransactionsPage : ComponentBase
    {
        [Inject]
        public IMediator Mediator { get; set; }

        public string ImportMessage { get; private set; }

        public ITransaction[] Transactions { get; private set; }

        protected override void OnInitialized()
        {
            ImportMessage = $"Reading transactions from disk...";
            Transactions = new ITransaction[0];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var localTransactions = await Mediator.Send(new ReadLocalTransactions.Request());

            ImportMessage = $"Read {localTransactions.Count()} transactions from disk. Syncing with database...";
            StateHasChanged();

            var newRecords = await Mediator.Send(new UploadTransactions.Command(localTransactions));
            if (newRecords.Any())
                ImportMessage = $"Inserted {newRecords.Count()} transactions into database.";
            else
                ImportMessage = $"No new transactions to insert.";

            Transactions = await Mediator.Send(new QueryTransactions(
                RangeStart: DateTime.Now.AddMonths(-1),
                RangeEnd: DateTime.Now,
                Page: 0,
                ResultsPerPage: 50));

            StateHasChanged();

        }
    }
}
