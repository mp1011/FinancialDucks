using Microsoft.Extensions.Configuration;

namespace FinancialDucks.Application.Services
{
    public interface ISettingsService
    {
        DirectoryInfo SourcePath { get; }
        string ConnectionString { get; }
    }

    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration _configuration;

        public SettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DirectoryInfo SourcePath => new DirectoryInfo(_configuration[nameof(SourcePath)]);

        public string ConnectionString => _configuration.GetConnectionString("DB");
    }
}
