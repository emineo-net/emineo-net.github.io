using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PaddleRcl.Services;

public class PaddlePortalService
{
    private readonly HttpClient _http;
    private readonly ILogger<PaddlePortalService> _logger;

    // Passe <project-ref> an dein Supabase-Projekt an, z.B. https://abcdefgh.supabase.co/functions/v1
    private const string SupabaseFunctionsBaseUrl = "https://otyvyonmcdigfrhngsqe.supabase.co/functions/v1";

    public PaddlePortalService(HttpClient http, ILogger<PaddlePortalService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Ruft das Paddle Customer Portal für den eingeloggten User ab.
    /// accessToken = das Supabase-Access-Token des aktuell eingeloggten Users
    /// (client.Auth.CurrentSession?.AccessToken).
    /// </summary>
    public async Task<string?> GetCustomerPortalUrlAsync(string accessToken, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{SupabaseFunctionsBaseUrl}/paddle-customer-portal");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "paddle-customer-portal Aufruf fehlgeschlagen ({StatusCode}): {Body}",
                response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<PortalResponse>(cancellationToken: ct);
        return result?.Url;
    }

    private sealed class PortalResponse
    {
        public string? Url { get; set; }
    }
}