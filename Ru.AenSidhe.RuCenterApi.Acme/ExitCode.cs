namespace Ru.AenSidhe.RuCenterApi.Acme;

public enum ExitCode
{
    Ok = 0,
    UnknownError = 1,
    DnsError = 2,
    AuthError = 3,
    AccessTokenError = 4,
    ZoneNotFound = 5,
    ZoneIsDirty = 6,
}
