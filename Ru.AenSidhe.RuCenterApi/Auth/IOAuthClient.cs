using UnitGenerator;

namespace Ru.AenSidhe.RuCenterApi.Auth;

public interface IOAuthClient
{
    Task<TokenResult> GetToken(TokenRequest request, CancellationToken ct);

    Task<TokenResult> RefreshToken(RefreshTokenRequest request, CancellationToken token);
}

[UnitOf(typeof(string))]
public readonly partial struct AccessToken {}

[UnitOf(typeof(string))]
public readonly partial struct RefreshToken {}

public sealed record Token(AccessToken AccessToken, RefreshToken? RefreshToken, DateTimeOffset ExpiresIn);

public abstract record TokenResult
{
    public sealed record Ok(Token Token) : TokenResult;

    public sealed record Error(string Message) : TokenResult;
}

public sealed record TokenRequest(IUserCredentials UserCredentials, IApplicationCredentials ApplicationCredentials, string Scope = ".*");

public sealed record RefreshTokenRequest(RefreshToken RefreshToken, IApplicationCredentials ApplicationCredentials);