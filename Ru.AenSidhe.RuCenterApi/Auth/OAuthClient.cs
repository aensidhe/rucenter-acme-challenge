using System.Net.Http.Headers;
using System.Text;

namespace Ru.AenSidhe.RuCenterApi.Auth;

public sealed class OAuthClient : IOAuthClient, IDisposable
{
    private readonly string _clientId = "eadc0acc4244694857d95e18ba1841a5";
    private readonly string _clientSecret = "yA-pcYObuPWbusb477o-oHQUvLVHwlPg7dECrdFfnSE";
    private readonly HttpClient _httpClient;

    public OAuthClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.nic.ru/oauth");
    }

    public async Task<TokenResult> GetToken(TokenRequest request, CancellationToken ct)
    {
        return await MakeTokenRequest(Map(request), ct);
    }

    public async Task<TokenResult> RefreshToken(RefreshToken refreshToken, CancellationToken ct)
    {
        return await MakeTokenRequest(Map(refreshToken), ct);
    }

    public void Dispose() => _httpClient.Dispose();

    private async Task<TokenResult> MakeTokenRequest(IEnumerable<KeyValuePair<string, string>> formMap, CancellationToken ct)
    {
        // ReSharper disable once UsingStatementResourceInitialization
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "token")
        {
            Headers = { Authorization = GetAuthorizationHeader() },
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

    private AuthenticationHeaderValue GetAuthorizationHeader()
    {
        var value = $"{_clientId}:{_clientSecret}";
        return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(value)));
    }

    private static IEnumerable<KeyValuePair<string,string>> Map(TokenRequest request)
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", request.Login },
            { "password", request.Password },
            { "scope", request.Scope }
        };
    }

    private IEnumerable<KeyValuePair<string,string>> Map(RefreshToken refreshToken)
    {
        return new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken.AsPrimitive() }
        };
    }
}