namespace Ru.AenSidhe.RuCenterApi.Acme;

public sealed class EnvironmentCredentials : ICredentials
{
    private readonly Lazy<string> _userName = new(() => Environment.GetEnvironmentVariable("_REG_RU_USERNAME") ?? throw new InvalidOperationException("Please set '_REG_RU_USERNAME'"));
    private readonly Lazy<string> _password = new(() => Environment.GetEnvironmentVariable("_REG_RU_TECHNICALPASSWORD") ?? throw new InvalidOperationException("Please set '_REG_RU_TECHNICALPASSWORD'"));
    public string Username { get => _userName.Value; }
    public string Password { get => _password.Value; }
}
