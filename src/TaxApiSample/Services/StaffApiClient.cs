using System.Net.Http.Headers;
using System.Text.Json;
using TaxApiSample.Configuration;

namespace TaxApiSample.Services;

public sealed class StaffApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CchApiOptions _options;

    public StaffApiClient(HttpClient httpClient, CchApiOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<JsonElement?> GetStaffAsync(
        string staffId,
        string integratorKey,
        string? securityToken,
        string? authorizationToken,
        CancellationToken cancellationToken)
    {
        var uri = $"{_options.StaffServiceBaseUrl.TrimEnd('/')}/v1.0/Staff?id={Uri.EscapeDataString(staffId)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("IntegratorKey", integratorKey);

        if (!string.IsNullOrWhiteSpace(authorizationToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                authorizationToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorizationToken["Bearer ".Length..].Trim()
                    : authorizationToken);
        }
        else if (!string.IsNullOrWhiteSpace(securityToken))
        {
            request.Headers.Add("Security", securityToken);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var correlationId = response.Headers.TryGetValues("X-Correlation-ID", out var values)
                ? string.Join(", ", values)
                : null;
            responseBody = RedactSecrets(responseBody, integratorKey, securityToken, authorizationToken);
            if (responseBody.Length > 4000)
            {
                responseBody = responseBody[..4000] + " [truncated]";
            }

            var diagnostics = new List<string>();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                diagnostics.Add($"Correlation ID: {correlationId}");
            }

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                diagnostics.Add($"Response body: {responseBody}");
            }

            throw new InvalidOperationException(
                $"Staff lookup failed for StaffId '{staffId}' ({(int)response.StatusCode} {response.StatusCode})." +
                (diagnostics.Count == 0 ? string.Empty : $" {string.Join(" ", diagnostics)}"));
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"Staff lookup for StaffId '{staffId}' returned an unexpected response shape.");
        }

        return document.RootElement.Clone();
    }

    private static string RedactSecrets(
        string body,
        string integratorKey,
        string? securityToken,
        string? authorizationToken)
    {
        foreach (var secret in new[]
        {
            integratorKey,
            securityToken,
            authorizationToken,
            authorizationToken?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? authorizationToken["Bearer ".Length..].Trim()
                : null
        })
        {
            if (!string.IsNullOrWhiteSpace(secret))
            {
                body = body.Replace(secret, "[REDACTED]", StringComparison.OrdinalIgnoreCase);
            }
        }

        return body;
    }
}
