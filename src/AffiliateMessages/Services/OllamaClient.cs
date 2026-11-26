using System.Net.Http.Json;
using System.Text.Json;
using AffiliateMessages.Configuration;
using Microsoft.Extensions.Logging;

namespace AffiliateMessages.Services;

public sealed class OllamaClient
{
    private readonly HttpClient httpClient;
    private readonly AppOptions options;
    private readonly ILogger<OllamaClient>? logger;

    public OllamaClient(HttpClient httpClient, AppOptions options, ILogger<OllamaClient>? logger = null)
    {
        this.httpClient = httpClient;
        this.options = options;
        this.logger = logger;
        this.httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<string?> GeneratePhraseAsync(string productName, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Crie uma frase persuasiva em portugues do Brasil com no maximo 7 palavras e no maximo 1 emoji para este produto: {productName}";
            var response = await httpClient.PostAsJsonAsync(
                $"{options.OllamaUrl.TrimEnd('/')}/api/generate",
                new { model = options.OllamaModel, prompt, stream = false },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger?.LogWarning("Ollama retornou status HTTP {StatusCode}; a mensagem seguira sem frase.",
                    (int)response.StatusCode);
                return null;
            }

            try
            {
                var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken);
                if (payload is not null && PhraseValidator.TryValidate(payload.Response, out var phrase))
                {
                    return phrase;
                }

                logger?.LogWarning("Ollama retornou frase vazia ou fora do formato; a mensagem seguira sem frase.");
                return null;
            }
            catch (JsonException exception)
            {
                logger?.LogWarning(exception, "Resposta do Ollama nao e JSON valido; a mensagem seguira sem frase.");
                return null;
            }
        }
        catch (HttpRequestException exception)
        {
            logger?.LogWarning(exception, "Nao foi possivel acessar o Ollama local; a mensagem seguira sem frase.");
            return null;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning(exception, "Tempo limite do Ollama excedido; a mensagem seguira sem frase.");
            return null;
        }
    }

    private sealed class OllamaResponse
    {
        public string? Response { get; set; }
    }
}
