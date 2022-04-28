using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.AccountsControls
{
    public partial class AccountSnapshots
    {
        [Parameter]
        public ITransactionSourceDetail Source { get; set; }

        public IEnumerable<ISourceSnapshot> Snapshots => Source?
                                                            .SourceSnapshots?
                                                            .NullToEmpty()
                                                            .OrderBy(x => x.Date);
    }
}
