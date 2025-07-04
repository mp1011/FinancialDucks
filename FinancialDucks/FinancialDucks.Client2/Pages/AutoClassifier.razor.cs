using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Infrastructure.Models;
using MediatR;
using Microsoft.AspNetCore.Components;
using static FinancialDucks.Application.Features.AutoClassifierFeature;
using static FinancialDucks.Application.Features.CategoriesFeature;

namespace FinancialDucks.Client2.Pages
{
    partial class AutoClassifier
    {
        [Inject]
        public IMediator Mediator { get; set; }

        private ChangeTracked<string> _quickTest = new ChangeTracked<string>(string.Empty);
        private EventCallback<string> QuickTestTextChanged;
      
        public string QuickTest
        {
            get => _quickTest;
            set
            {
                _quickTest.Value = value;
                if(_quickTest.HasChanges)
                {
                    _quickTest.AcceptChanges();
                    QuickTestTextChanged.InvokeAsync(_quickTest.Value);
                }                
            }
        }

        public string QuickTestResult { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            QuickTestTextChanged = EventCallback.Factory.Create<string>(this, async (newValue) => await OnQuickTestChangedAsync(newValue));
        }

        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private async Task OnQuickTestChangedAsync(string text)
        {
            QuickTestResult= string.Empty;
            _cancelTokenSource.Cancel();
            _cancelTokenSource = new CancellationTokenSource();

            var categoryTree = await Mediator.Send(new CategoryTreeRequest());
            var result = await Mediator.Send(new AutoClassifyTransactionQuery(new Transaction { Description = text }, categoryTree), _cancelTokenSource.Token);
            QuickTestResult = result.MatchedCategories.Select(p => p.Name).StringJoin(", ");

//            QuickTestResult = await Mediator.Send(new ChatFeature.ChatRequest(text));

            StateHasChanged();
        }
    }
}
