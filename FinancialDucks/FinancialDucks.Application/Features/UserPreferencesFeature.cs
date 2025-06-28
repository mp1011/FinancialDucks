using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class UserPreferencesFeature
    {
        public record SaveUserPreferencesCommand(UserPreferences Settings) : IRequest<UserPreferences>;
        public record LoadUserPreferencesQuery() : IRequest<UserPreferences>;

        public class SaveUserPreferencesHandler : IRequestHandler<SaveUserPreferencesCommand, UserPreferences>
        {
            private readonly IUserPreferencesService _userPreferencesService;
            public SaveUserPreferencesHandler(IUserPreferencesService userPreferencesService)
            {
                _userPreferencesService = userPreferencesService;
            }

            public Task<UserPreferences> Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken)
            {
                _userPreferencesService.Save(request.Settings);
                return Task.FromResult(request.Settings);
            }

        }

        public class LoadUserPreferencesHandler : IRequestHandler<LoadUserPreferencesQuery, UserPreferences>
        {
            private readonly IUserPreferencesService _userPreferencesService;
            public LoadUserPreferencesHandler(IUserPreferencesService userPreferencesService)
            {
                _userPreferencesService = userPreferencesService;
            }

            public Task<UserPreferences> Handle(LoadUserPreferencesQuery request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_userPreferencesService.Load());  
            }
        }
    }
}