using System.Net.Http.Json;
using System.Text.Json;

var question = "What is the capital of France?";
var model = "llama3.2"; // Change to your preferred model

var requestBody = new
{
    model = model,
    prompt = question,
    stream = false
};

using var httpClient = new HttpClient();
var response = await httpClient.PostAsJsonAsync(
    "http://localhost:11434/api/generate", requestBody);

response.EnsureSuccessStatusCode();

var responseString = await response.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(responseString);
var content = doc.RootElement.GetProperty("response").GetString();

Console.WriteLine("Ollama says: " + content);