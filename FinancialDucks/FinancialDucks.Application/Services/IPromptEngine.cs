namespace FinancialDucks.Application.Services
{
    public interface IPromptEngine
    {
        Task<string> Chat(string prompt);
    }
}
