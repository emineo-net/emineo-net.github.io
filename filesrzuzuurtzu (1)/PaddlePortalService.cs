using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace YourApp.Services;

public class PaddlePortalService
{
    private readonly HttpClient _http;

    // Basis-URL deines Supabase-Projekts, z.B. https://xxxxx.supabase.co
    private const string SupabaseFunctionsBaseUrl = "https://<project-ref>.supabase.co/functions/v1";

    public PaddlePortalService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Ruft das Paddle Customer Portal für den eingeloggten User ab.
    /// accessToken = das Supabase-Access-Token des aktuell eingeloggten Users.
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
            // Hier ggf. Logging / Fehlerbehandlung ergänzen
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
