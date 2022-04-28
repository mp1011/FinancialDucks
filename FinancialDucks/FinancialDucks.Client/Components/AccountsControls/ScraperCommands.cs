using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.AccountsControls
{
    public partial class ScraperCommands
    {
        [Parameter]
        public ITransactionSourceDetail Source { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        private IScraperCommandDetail[] _commands;

        public IEnumerable<IScraperCommandDetail> Commands => _commands
            .NullToEmpty()
            .OrderBy(p => p.Sequence);

        protected override async Task OnParametersSetAsync()
        {
            if (Source == null)
                return;

            _commands = await Mediator.Send(new ScraperCommandsFeature.Query(Source));
        }
    }
}
