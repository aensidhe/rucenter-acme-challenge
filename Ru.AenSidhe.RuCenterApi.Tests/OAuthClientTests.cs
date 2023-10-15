using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Tests.Mocks;

namespace Ru.AenSidhe.RuCenterApi.Tests;

public class OAuthClientTests
{

    [Fact]
    public async Task AccessToken_GreenPath()
    {
        var returnedToken = new { access_token = Guid.NewGuid().ToString(), expires_in = 14400 };
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(returnedToken), MediaTypeHeaderValue.Parse("application/json"))
        };
        var httpClientFactory = new HttpClientFactory(mockResponse);
        var oauthClient = new OAuthClient(httpClientFactory);

        var response = await oauthClient.GetToken(new TokenRequest(MockCreds.Instance, MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=password&username=c&password=d&scope=.%2A&client_id=a&client_secret=b");

        var token = response.Should().BeOfType<TokenResult.Ok>().Which.Token;
        token.AccessToken.Should().Be(returnedToken.access_token);
        token.ExpiresIn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(returnedToken.expires_in), TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task AccessToken_Error(HttpStatusCode code)
    {
        var httpClientFactory = new HttpClientFactory(new HttpResponseMessage(code));
        var oauthClient = new OAuthClient(httpClientFactory);

        var response = await oauthClient.GetToken(new TokenRequest(MockCreds.Instance, MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=password&username=c&password=d&scope=.%2A&client_id=a&client_secret=b");

        response.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Contain(code.ToString());
    }

    [Fact]
    public async Task AccessToken_Exception()
    {
        var httpClientFactory = new HttpClientFactory(new ExceptionHandler());
        var oauthClient = new OAuthClient(httpClientFactory);

        var tokenResult = await oauthClient.GetToken(new TokenRequest(MockCreds.Instance, MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=password&username=c&password=d&scope=.%2A&client_id=a&client_secret=b");

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task RefreshToken_GreenPath()
    {
        var returnedToken = new { access_token = Guid.NewGuid().ToString(), expires_in = 14400 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(returnedToken), MediaTypeHeaderValue.Parse("application/json"))
        };
        var httpClientFactory = new HttpClientFactory(response);
        var oauthClient = new OAuthClient(httpClientFactory);

        var tokenResult = await oauthClient.RefreshToken(new RefreshTokenRequest(new RefreshToken("f"), MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=refresh_token&refresh_token=f&client_id=a&client_secret=b");

        var token = tokenResult.Should().BeOfType<TokenResult.Ok>().Which.Token;
        token.AccessToken.Should().Be(returnedToken.access_token);
        token.ExpiresIn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(returnedToken.expires_in), TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task RefreshToken_Error(HttpStatusCode code)
    {
        var httpClientFactory = new HttpClientFactory(new HttpResponseMessage(code));
        var oauthClient = new OAuthClient(httpClientFactory);

        var tokenResult = await oauthClient.RefreshToken(new RefreshTokenRequest(new RefreshToken("f"), MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=refresh_token&refresh_token=f&client_id=a&client_secret=b");

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Contain(code.ToString());
    }

    [Fact]
    public async Task RefreshToken_Exception()
    {
        var httpClientFactory = new HttpClientFactory(new ExceptionHandler());
        var oauthClient = new OAuthClient(httpClientFactory);

        var tokenResult = await oauthClient.RefreshToken(new RefreshTokenRequest(new RefreshToken("f"), MockCreds.Instance), CancellationToken.None);
        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://api.nic.ru/oauth/token");
        request.Content!.Headers.ContentType!.ToString().Should().Be("application/x-www-form-urlencoded");
        requestContent.Should().Be("grant_type=refresh_token&refresh_token=f&client_id=a&client_secret=b");

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Be("BOOM");
    }
}