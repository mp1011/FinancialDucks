using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Client.Models
{
    public record CategorySelectedEventArgs(ICategoryDetail Category, DescriptionWithCount[] Descriptions) { }
}
