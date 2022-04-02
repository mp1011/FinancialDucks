namespace FinancialDucks.Application.Models.AppModels
{
    public record CachedData<T>(T Data, DateTime ComputedDate) { }
}
