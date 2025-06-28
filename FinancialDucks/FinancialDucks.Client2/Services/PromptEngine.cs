using FinancialDucks.Application.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace FinancialDucks.Client2.Services
{
    public class OllamaPromptEngine : IPromptEngine
    {
        private readonly ISettingsService _settingsService;

        public OllamaPromptEngine(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<string> Chat(string prompt)
        {
            var requestBody = new
            {
                model = _settingsService.LLModel,
                prompt,
                stream = false
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{_settingsService.LLMHost}/api/generate", requestBody);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement.GetProperty("response").GetString();
            return content ?? "";
        }
    }
}
