using Ru.AenSidhe.RuCenterApi.Auth;

namespace Ru.AenSidhe.RuCenterApi.Acme;

public sealed class EnvironmentUserCredentials : IUserCredentials
{
    private readonly Lazy<string> _userName = new(() => Environment.GetEnvironmentVariable("_REG_RU_USERNAME") ?? throw new InvalidOperationException("Please set '_REG_RU_USERNAME'"));

    private readonly Lazy<string> _password = new(() => Environment.GetEnvironmentVariable("_REG_RU_TECHNICALPASSWORD") ?? throw new InvalidOperationException("Please set '_REG_RU_TECHNICALPASSWORD'"));

    public string Username => _userName.Value;

    public string Password => _password.Value;
}

public sealed class EnvironmentApplicationCredentials : IApplicationCredentials
{
    private readonly Lazy<string> _clientId = new(() => Environment.GetEnvironmentVariable("_REG_RU_CLIENT_ID") ?? throw new InvalidOperationException("Please set '_REG_RU_CLIENT_ID'"));

    private readonly Lazy<string> _clientSecret = new(() => Environment.GetEnvironmentVariable("_REG_RU_CLIENT_SECRET") ?? throw new InvalidOperationException("Please set '_REG_RU_CLIENT_SECRET'"));

    public string ClientId => _clientId.Value;

    public string ClientSecret => _clientSecret.Value;
}
