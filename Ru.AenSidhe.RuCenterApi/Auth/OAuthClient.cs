using System.Net.Http.Headers;
using System.Text;

namespace Ru.AenSidhe.RuCenterApi.Auth;

public sealed class OAuthClient : IOAuthClient, IDisposable
{
    private readonly HttpClient _httpClient;

    public OAuthClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.nic.ru/oauth");
    }

    public async Task<TokenResult> GetToken(TokenRequest request, CancellationToken ct)
    {
        return await MakeTokenRequest(Map(request), request.ApplicationCredentials, ct);
    }

    public async Task<TokenResult> RefreshToken(RefreshTokenRequest request, CancellationToken ct)
    {
        return await MakeTokenRequest(Map(request), request.ApplicationCredentials, ct);
    }

    public void Dispose() => _httpClient.Dispose();

    private async Task<TokenResult> MakeTokenRequest(IEnumerable<KeyValuePair<string, string>> formMap, IApplicationCredentials applicationCredentials, CancellationToken ct)
    {
        // ReSharper disable once UsingStatementResourceInitialization
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "token")
        {
            Headers = { Authorization = GetAuthorizationHeader(applicationCredentials) },
            Content = new FormUrlEncodedContent(formMap)
        };

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return response.IsSuccessStatusCode
                ? new TokenResult.Ok(ParseTokenResult(content))
                : new TokenResult.Error($"{response.StatusCode} {response.ReasonPhrase}\n\t{content}");
        }
        catch (Exception e)
        {
            return new TokenResult.Error(e.Message);
        }
    }

    private static Token ParseTokenResult(string content)
    {
        var token = content.DeserializeAnonymousType(new
        {
            access_token = string.Empty,
            expires_in = 0,
            refresh_token = default(string?)
        });

        return new Token(
            new AccessToken(token!.access_token),
            token.refresh_token is null ? null : new RefreshToken(token.refresh_token),
            DateTimeOffset.UtcNow.AddSeconds(token.expires_in));
    }

    private static AuthenticationHeaderValue GetAuthorizationHeader(IApplicationCredentials credentials)
    {
        var value = $"{credentials.ClientId}:{credentials.ClientSecret}";
        return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(value)));
    }

    private static IEnumerable<KeyValuePair<string,string>> Map(TokenRequest request)
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", request.UserCredentials.Username },
            { "password", request.UserCredentials.Password },
            { "scope", request.Scope },
            { "client_id", request.ApplicationCredentials.ClientId },
            { "client_secret", request.ApplicationCredentials.ClientSecret },
        };
    }

    private static IEnumerable<KeyValuePair<string,string>> Map(RefreshTokenRequest request)
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", request.RefreshToken.AsPrimitive() },
            { "client_id", request.ApplicationCredentials.ClientId },
            { "client_secret", request.ApplicationCredentials.ClientSecret },
        };
    }
}