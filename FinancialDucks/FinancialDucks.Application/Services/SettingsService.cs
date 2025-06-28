// Stryker disable all
using Microsoft.Extensions.Configuration;

namespace FinancialDucks.Application.Services
{
    public interface ISettingsService
    {
        DirectoryInfo SourcePath { get; }
        DirectoryInfo DownloadsFolder { get; }
        string ConnectionString { get; }

        string LLMHost { get; }
        string LLModel { get; } 
    }

    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration _configuration;

        public SettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DirectoryInfo SourcePath => new DirectoryInfo(_configuration[nameof(SourcePath)]);
        public DirectoryInfo DownloadsFolder => new DirectoryInfo(_configuration[nameof(DownloadsFolder)]);
        public string ConnectionString => _configuration.GetConnectionString("DB");
        public string LLMHost => _configuration[nameof(LLMHost)];
        public string LLModel => _configuration[nameof(LLModel)];
    }

    // used for command line tools
    public class EfToolsSettingsService : ISettingsService
    {
        public DirectoryInfo SourcePath => null;
        public DirectoryInfo DownloadsFolder => null;

        public string ConnectionString => "Server=localhost;Database=FinancialDucks;Trusted_Connection=True;";

        public string LLMHost => null;
        public string LLModel => null;
    }
}
