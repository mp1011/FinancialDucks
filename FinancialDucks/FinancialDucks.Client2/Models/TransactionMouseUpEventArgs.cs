using FinancialDucks.Application.Models;
using Microsoft.AspNetCore.Components.Web;

namespace FinancialDucks.Client2.Models
{
    public record TransactionMouseUpEventArgs(MouseEventArgs MouseArgs, ITransactionDetail Transaction);
}
