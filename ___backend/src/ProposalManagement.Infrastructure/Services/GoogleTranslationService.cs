using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

public class GoogleTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleTranslationService> _logger;
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private const string BaseUrl = "https://translate.googleapis.com/translate_a/single";

    public GoogleTranslationService(HttpClient httpClient, ILogger<GoogleTranslationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> TranslateAsync(string text, string sourceLang, string targetLang, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        if (sourceLang == targetLang) return text;

        var cacheKey = $"{sourceLang}:{targetLang}:{text}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            var url = $"{BaseUrl}?client=gtx&sl={Uri.EscapeDataString(sourceLang)}&tl={Uri.EscapeDataString(targetLang)}&dt=t&q={Uri.EscapeDataString(text)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = ParseTranslation(json);

            if (!string.IsNullOrEmpty(result))
            {
                _cache.TryAdd(cacheKey, result);
                return result;
            }

            _logger.LogWarning("Translation returned empty for text length {TextLength}, {Source}->{Target}", text.Length, sourceLang, targetLang);
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation failed for {Source}->{Target}", sourceLang, targetLang);
            return text; // Fallback: return original
        }
    }

    public async Task<IReadOnlyList<string>> TranslateBatchAsync(IReadOnlyList<string> texts, string sourceLang, string targetLang, CancellationToken cancellationToken = default)
    {
        var results = new string[texts.Count];
        for (int i = 0; i < texts.Count; i++)
        {
            results[i] = await TranslateAsync(texts[i], sourceLang, targetLang, cancellationToken);
        }
        return results;
    }

    private static string ParseTranslation(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // Response format: [[["translated","original",null,null,10]],null,"en",...]
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var sentences = root[0];
                if (sentences.ValueKind == JsonValueKind.Array)
                {
                    var result = string.Empty;
                    foreach (var sentence in sentences.EnumerateArray())
                    {
                        if (sentence.ValueKind == JsonValueKind.Array && sentence.GetArrayLength() > 0)
                        {
                            result += sentence[0].GetString();
                        }
                    }
                    return result;
                }
            }
        }
        catch { /* Parse failure — return empty */ }
        return string.Empty;
    }
}
