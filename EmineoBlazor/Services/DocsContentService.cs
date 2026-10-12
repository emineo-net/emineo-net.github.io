using System.Net.Http;

namespace EmineoBlazor.Services;

/// <summary>
/// Loads README markdown files from wwwroot/docs/ at runtime.
/// Caches each file after the first load so navigation is instant.
/// </summary>
public sealed class DocsContentService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, string> _cache = new();

    public DocsContentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetMarkdownAsync(string fileName)
    {
        if (_cache.TryGetValue(fileName, out var cached))
            return cached;

        try
        {
            var content = await _http.GetStringAsync($"docs/{fileName}");
            _cache[fileName] = content;
            return content;
        }
        catch
        {
            return $"# Not found\n\nThe file `docs/{fileName}` could not be loaded.";
        }
    }
}