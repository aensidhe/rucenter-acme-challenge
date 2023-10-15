using Ru.AenSidhe.RuCenterApi.Auth;

namespace Ru.AenSidhe.RuCenterApi.Tests.Mocks;
public class MockCreds : IUserCredentials, IApplicationCredentials
{
    private MockCreds() {}

    public string ClientId => "a";

    public string ClientSecret => "b";

    public string Username => "c";

    public string Password => "d";

    public static readonly MockCreds Instance = new();
}
