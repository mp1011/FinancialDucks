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

        public TransactionsFeature.TransactionsFilter CurrentFilter { get; private set; }
       
        public DateTime RangeStart { get; set; } = DateTime.Now.AddMonths(-6);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public string TextFilter { get; set; }

        public ICategoryDetail? CategoryFilter { get; set; }

       
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

            UpdateCurrentFilter();
            StateHasChanged();
        }

        public void OnCategorySelected(ICategoryDetail category)
        {
            CategoryFilter = category;
            UpdateCurrentFilter();
        }

        private void UpdateCurrentFilter()
        {
            if (CurrentFilter == null
                || CurrentFilter.RangeStart != RangeStart
                || CurrentFilter.RangeEnd != RangeEnd
                || CurrentFilter.TextFilter != TextFilter
                || CurrentFilter.Category != CategoryFilter)
            {
                CurrentFilter = new TransactionsFeature.TransactionsFilter(RangeStart, RangeEnd, CategoryFilter, TextFilter);
                StateHasChanged();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            UpdateCurrentFilter();
        }
    }
}
