using FinancialDucks.Application.Models;
using Microsoft.AspNetCore.Components.Web;

namespace FinancialDucks.Client.Models
{
    public record TransactionMouseUpEventArgs(MouseEventArgs MouseArgs, ITransactionDetail Transaction);
}
