using Ru.AenSidhe.RuCenterApi.Dns;

namespace Ru.AenSidhe.RuCenterApi.Acme;

public sealed record AcmeResult(ExitCode ExitCode, string Message)
{
    public AcmeResult(ExitCode code)
        : this(code, code.ToString())
    {
    }

    public AcmeResult(DnsError error)
        : this(
            error switch {
                DnsError.Fail => ExitCode.UnknownError,
                DnsError.ServerError => ExitCode.DnsError,
                DnsError.Unauthorized => ExitCode.AccessTokenError,
                _ => throw new ArgumentOutOfRangeException(nameof(error)),
            },
            error.ToString()
        )
    {
    }
}
