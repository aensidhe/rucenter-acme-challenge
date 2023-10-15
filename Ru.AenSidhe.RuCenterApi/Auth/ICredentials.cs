namespace Ru.AenSidhe.RuCenterApi.Auth;

public interface IUserCredentials
{
    string Username { get; }

    string Password { get; }
}

public interface IApplicationCredentials
{
    string ClientId { get; }

    string ClientSecret { get; }
}
