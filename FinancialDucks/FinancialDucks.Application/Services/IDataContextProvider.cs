namespace FinancialDucks.Application.Services
{
    public interface IDataContextProvider    
    {
        IDataContext CreateDataContext();
    }
}
