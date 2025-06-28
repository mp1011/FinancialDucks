using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Services
{
    public interface IUserPreferencesService
    {
        UserPreferences Load();
        void Save(UserPreferences preferences);
    }
}
