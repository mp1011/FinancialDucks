using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;

namespace FinancialDucks.Client2.Services
{
    internal class UserPreferencesService : IUserPreferencesService
    {
        public UserPreferences Load()
        {
            return new UserPreferences(
                RangeStart: Preferences.Default.Get("RangeStart", DateTime.Now.AddMonths(-1)),
                RangeEnd: Preferences.Default.Get("RangeEnd", DateTime.Now));            
        }

        public void Save(UserPreferences preferences)
        {
            Preferences.Default.Set("RangeStart", preferences.RangeStart);
            Preferences.Default.Set("RangeEnd", preferences.RangeEnd);
        }
    }
}
