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
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(returnedToken), MediaTypeHeaderValue.Parse("application/json"))
        };
        var oauthClient = new OAuthClient(new HttpClientFactory(response));

        var request = new TokenRequest("a", "b");
        var tokenResult = await oauthClient.GetToken(request, CancellationToken.None);

        var token = tokenResult.Should().BeOfType<TokenResult.Ok>().Which.Token;
        token.AccessToken.Should().Be(returnedToken.access_token);
        token.ExpiresIn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(returnedToken.expires_in), TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task AccessToken_Error(HttpStatusCode code)
    {
        var oauthClient = new OAuthClient(new HttpClientFactory(new HttpResponseMessage(code)));

        var request = new TokenRequest("a", "b");
        var tokenResult = await oauthClient.GetToken(request, CancellationToken.None);

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Contain(code.ToString());
    }

    [Fact]
    public async Task AccessToken_Exception()
    {
        var oauthClient = new OAuthClient(HttpClientFactory.BOOMFactory);

        var request = new TokenRequest("a", "b");
        var tokenResult = await oauthClient.GetToken(request, CancellationToken.None);

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
        var oauthClient = new OAuthClient(new HttpClientFactory(response));

        var tokenResult = await oauthClient.RefreshToken(new RefreshToken(Guid.NewGuid().ToString()), CancellationToken.None);

        var token = tokenResult.Should().BeOfType<TokenResult.Ok>().Which.Token;
        token.AccessToken.Should().Be(returnedToken.access_token);
        token.ExpiresIn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(returnedToken.expires_in), TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task RefreshToken_Error(HttpStatusCode code)
    {
        var oauthClient = new OAuthClient(new HttpClientFactory(new HttpResponseMessage(code)));

        var tokenResult = await oauthClient.RefreshToken(new RefreshToken(Guid.NewGuid().ToString()), CancellationToken.None);

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Contain(code.ToString());
    }

    [Fact]
    public async Task RefreshToken_Exception()
    {
        var oauthClient = new OAuthClient(HttpClientFactory.BOOMFactory);

        var tokenResult = await oauthClient.RefreshToken(new RefreshToken(Guid.NewGuid().ToString()), CancellationToken.None);

        tokenResult.Should().BeOfType<TokenResult.Error>().Which.Message.Should().Be("BOOM");
    }
}