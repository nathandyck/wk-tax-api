using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
DotEnv.LoadFromAncestors(builder.Environment.ContentRootPath);
builder.Services.AddHttpClient<CchApiClient>(client => client.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddSingleton<CchApiClient>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", (CchApiClient client) => Results.Ok(client.PublicConfiguration));
app.MapPost("/api/authenticate", async (CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await client.AuthenticateAsync(cancellationToken);
        return Results.Ok(result);
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.MapGet("/api/client", async (string id, string? subid, CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await client.GetClientAsync(id, subid ?? string.Empty, cancellationToken));
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.MapGet("/api/tax/return-status", async (CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await client.GetReturnStatusesAsync(cancellationToken));
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.MapGet("/api/tax/returns", async (string taxYear, string returnType, CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await client.GetReturnsAsync(taxYear, returnType, cancellationToken));
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.MapPost("/api/tax/return-status", async (ReturnStatusUpdate request, CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await client.UpdateReturnStatusAsync(request, cancellationToken));
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.MapPost("/api/workflow", async (WorkflowRequest request, CchApiClient client, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await client.RunWorkflowAsync(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (CchApiException exception)
    {
        return Results.Problem(exception.Message, statusCode: exception.StatusCode);
    }
});

app.Run();

public sealed record WorkflowRequest(string ClientId, string? ClientSubId);
public sealed record ReturnStatusUpdate(IReadOnlyList<string> ReturnId, string Status);

public sealed class CchApiClient
{
    private readonly HttpClient httpClient;
    private readonly CchApiOptions options;
    private string? securityToken;

    public CchApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        options = CchApiOptions.FromConfiguration(configuration);
    }

    public object PublicConfiguration => new
    {
        options.AuthBaseUrl,
        options.ClientBaseUrl,
        options.TaxBaseUrl,
        HasIntegratorKey = !string.IsNullOrWhiteSpace(options.IntegratorKey),
        HasCredentials = !string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password)
    };

    public async Task<object> AuthenticateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.IntegratorKey))
            throw new CchApiException("INTEGRATOR_KEY is missing from .env.", 500);

        var payload = new
        {
            UserName = options.Username,
            UserSid = options.UserSid,
            Password = options.Password,
            Realm = options.Realm,
            AdfsPilotLoginCode = options.AdfsPilotLoginCode,
            IsInternal = options.IsInternal
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{options.AuthBaseUrl}/v1.0/{options.AuthOperation}");
        request.Headers.TryAddWithoutValidation("Integratorkey", options.IntegratorKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null }),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "Authentication");

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "unknown content type";
            throw new CchApiException($"Authentication returned HTTP {(int)response.StatusCode} ({contentType}) with a non-JSON response:\n{Preview(body)}", 502);
        }
        securityToken = ExtractToken(document.RootElement);
        if (string.IsNullOrWhiteSpace(securityToken))
            throw new CchApiException($"Authentication returned HTTP {(int)response.StatusCode}, but no usable token was found. Service response:\n{Preview(body)}", 502);

        return new
        {
            Success = true,
            Message = "Authenticated successfully. The token is redacted below and remains on the server.",
            UpstreamStatus = (int)response.StatusCode,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? "unknown",
            ServiceResponse = RedactAuthenticationResponse(body)
        };
    }

    public async Task<JsonElement> GetClientAsync(string id, string subid, CancellationToken cancellationToken)
    {
        EnsureToken();
        var url = $"{options.ClientBaseUrl}/v1.0/Client?id={Uri.EscapeDataString(id)}&subid={Uri.EscapeDataString(subid)}";
        using var request = CreateAuthorizedRequest(HttpMethod.Get, url, options.ClientBaseUrl);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "Client lookup");
        return ParseJson(body);
    }

    public async Task<JsonElement> GetReturnStatusesAsync(CancellationToken cancellationToken)
    {
        EnsureToken();
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"{options.TaxBaseUrl}/api/v1/ReturnStatus", options.TaxBaseUrl);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "Tax return status lookup");
        return ParseJson(body);
    }

    public async Task<JsonElement> GetReturnsAsync(string taxYear, string returnType, CancellationToken cancellationToken)
    {
        EnsureToken();
        if (string.IsNullOrWhiteSpace(taxYear) || string.IsNullOrWhiteSpace(returnType))
            throw new CchApiException("Tax year and return type are required.", 400);

        var filter = $"TaxYear eq '{EscapeOData(taxYear)}' and ReturnType eq '{EscapeOData(returnType)}'";
        var url = $"{options.TaxBaseUrl}/api/v1/Returns?$filter={Uri.EscapeDataString(filter)}";
        using var request = CreateAuthorizedRequest(HttpMethod.Get, url, options.TaxBaseUrl);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "Return lookup");
        return ParseJson(body);
    }

    public async Task<JsonElement> UpdateReturnStatusAsync(ReturnStatusUpdate update, CancellationToken cancellationToken)
    {
        EnsureToken();
        if (update.ReturnId is null || update.ReturnId.Count == 0 || update.ReturnId.Count > 10)
            throw new CchApiException("Provide between 1 and 10 return IDs.", 400);
        if (string.IsNullOrWhiteSpace(update.Status))
            throw new CchApiException("A return status is required.", 400);

        var payload = JsonSerializer.Serialize(
            new { ReturnId = update.ReturnId, Status = update.Status },
            new JsonSerializerOptions { PropertyNamingPolicy = null });
        using var request = CreateAuthorizedRequest(HttpMethod.Post, $"{options.TaxBaseUrl}/api/v1/ReturnStatus", options.TaxBaseUrl);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "Return status update");
        return ParseJson(body);
    }

    public async Task<object> RunWorkflowAsync(WorkflowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw new CchApiException("ClientId is required.", 400);

        await AuthenticateAsync(cancellationToken);
        var client = await GetClientAsync(request.ClientId, request.ClientSubId ?? string.Empty, cancellationToken);
        var statuses = await GetReturnStatusesAsync(cancellationToken);
        return new { Authenticated = true, Client = client, ReturnStatuses = statuses };
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string baseUrl)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.TryAddWithoutValidation("Integratorkey", options.IntegratorKey);
        request.Headers.TryAddWithoutValidation(options.SecurityHeader, securityToken);
        return request;
    }

    private void EnsureToken()
    {
        if (string.IsNullOrWhiteSpace(securityToken))
            throw new CchApiException("Authenticate first so the API token is available.", 401);
    }

    private static string? ExtractToken(JsonElement root)
    {
        if (!root.TryGetProperty("Token", out var token))
            token = root;
        if (token.ValueKind == JsonValueKind.String)
            return token.GetString();
        foreach (var name in new[] { "access_token", "AccessToken", "token", "Token" })
            if (token.ValueKind == JsonValueKind.Object && token.TryGetProperty(name, out var nested) && nested.ValueKind == JsonValueKind.String)
                return nested.GetString();
        return token.GetRawText();
    }

    private static JsonElement ParseJson(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new CchApiException("The API returned a non-JSON response.", 502);
        }
    }

    private static string RedactAuthenticationResponse(string body)
    {
        try
        {
            var node = JsonNode.Parse(body);
            RedactSecrets(node);
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "null";
        }
        catch (JsonException)
        {
            return Preview(body);
        }
    }

    private static void RedactSecrets(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Key.Equals("Token", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Equals("AccessToken", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Equals("access_token", StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject[property.Key] = "[redacted]";
                }
                else
                {
                    RedactSecrets(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
                RedactSecrets(item);
        }
    }

    private static string Preview(string body) => body.Length <= 4000 ? body : body[..4000] + "\n[response truncated]";

    private static string EscapeOData(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static void EnsureSuccess(HttpResponseMessage response, string body, string operation)
    {
        if (response.IsSuccessStatusCode)
            return;
        var detail = string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body;
        throw new CchApiException($"{operation} failed with {(int)response.StatusCode}: {detail}", (int)response.StatusCode);
    }
}

public sealed class CchApiOptions
{
    public string IntegratorKey { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string UserSid { get; init; } = string.Empty;
    public string Realm { get; init; } = string.Empty;
    public string AdfsPilotLoginCode { get; init; } = string.Empty;
    public bool IsInternal { get; init; } = true;
    public string AuthOperation { get; init; } = "Authenticate";
    public string SecurityHeader { get; init; } = "Security";
    public string AuthBaseUrl { get; init; } = "https://api.cchaxcess.com/api/AuthService";
    public string ClientBaseUrl { get; init; } = "https://api.cchaxcess.com/api/ClientService";
    public string TaxBaseUrl { get; init; } = "https://api.cchaxcess.com/taxservices/oiptax";

    public static CchApiOptions FromConfiguration(IConfiguration configuration) => new()
    {
        IntegratorKey = Env("INTEGRATOR_KEY"),
        Username = Env("CCH_USERNAME"),
        Password = Env("CCH_PASSWORD"),
        UserSid = Env("CCH_USER_SID"),
        Realm = Env("CCH_REALM"),
        AdfsPilotLoginCode = Env("ADFS_PILOT_LOGIN_CODE"),
        IsInternal = !bool.TryParse(Env("IS_INTERNAL"), out var value) || value,
        AuthOperation = Env("AUTH_OPERATION", "Authenticate"),
        SecurityHeader = Env("SECURITY_HEADER", "Security"),
        AuthBaseUrl = Setting(configuration, "AuthBaseUrl", "AUTH_BASE_URL", "https://api.cchaxcess.com/api/AuthService"),
        ClientBaseUrl = Setting(configuration, "ClientBaseUrl", "CLIENT_BASE_URL", "https://api.cchaxcess.com/api/ClientService"),
        TaxBaseUrl = Setting(configuration, "TaxBaseUrl", "TAX_BASE_URL", "https://api.cchaxcess.com/taxservices/oiptax")
    };

    private static string Env(string name, string fallback = "") => Environment.GetEnvironmentVariable(name) ?? fallback;

    private static string Setting(IConfiguration configuration, string key, string environmentName, string fallback)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentName);
        if (!string.IsNullOrWhiteSpace(environmentValue))
            return environmentValue.Trim().TrimEnd('/');

        var configuredValue = configuration[$"CchApi:{key}"];
        return string.IsNullOrWhiteSpace(configuredValue) ? fallback : configuredValue.Trim().TrimEnd('/');
    }
}

public sealed class CchApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public static class DotEnv
{
    public static void LoadFromAncestors(string contentRoot)
    {
        var directory = new DirectoryInfo(contentRoot);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (File.Exists(path))
            {
                Load(path);
                return;
            }
            directory = directory.Parent;
        }
    }

    public static void Load(string path)
    {
        if (!File.Exists(path))
            return;
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;
            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"', '\'');
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
