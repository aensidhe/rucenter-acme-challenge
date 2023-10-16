using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;

namespace Ru.AenSidhe.RuCenterApi.Acme;

public sealed record AcmeService(IOAuthClient Auth, IDnsClient Dns, IUserCredentials UserCredentials, IApplicationCredentials ApplicationCredentials) : IAcmeService
{
    private readonly Ttl _ttl = new(60);

    public async Task<AcmeResult> CreateRecord(string fqdn, string value, CancellationToken ct = default)
    {
        return await Auth.GetToken(new TokenRequest(UserCredentials, ApplicationCredentials), ct) switch
        {
            TokenResult.Ok ok => await CreateRecord(ok.Token.AccessToken, fqdn, value, ct),
            TokenResult.Error e => new(ExitCode.AuthError, e.Message),
            _ => throw new ArgumentOutOfRangeException("TokenResult")
        };
    }

    private async Task<AcmeResult> CreateRecord(AccessToken accessToken, string fqdn, string value, CancellationToken ct)
    {
        return await Dns.GetAllDnsZones(accessToken, ct) switch
        {
            DnsResult<DnsZone[]>.Ok ok => await CreateRecord(ok.Value, fqdn, value, accessToken, ct),
            DnsResult<DnsZone[]>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    private async Task<AcmeResult> CreateRecord(DnsZone[] zones, string fqdn, string value, AccessToken accessToken, CancellationToken ct)
    {
        var (domain, zoneName) = ParseFqdn(fqdn);
        var zone = zones.FirstOrDefault(x => x.Name == zoneName);
        if (zone == null) return new(ExitCode.ZoneNotFound, $"'{zoneName}' is not found on provided account. Check your data");
        if (zone.HasChanges) return new(ExitCode.ZoneIsDirty, $"'{zoneName}' has uncommitted changes to it. Contact the administrator");

        var request = new DnsRecordCreationRequest(zone, new DnsData.Txt(domain, value, _ttl));
        return await Dns.CreateDnsRecord(request, accessToken, ct) switch
        {
            DnsResult<DnsRecord>.Ok ok => await Commit(zone, accessToken, ct),
            DnsResult<DnsRecord>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    private async Task<AcmeResult> Commit(DnsZone dnsZone, AccessToken accessToken, CancellationToken ct)
    {
        return await Dns.Commit(dnsZone, accessToken, ct) switch
        {
            DnsResult<Unit>.Ok => new(ExitCode.Ok, "Changes were committed"),
            DnsResult<Unit>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    public async Task<AcmeResult> DeleteRecord(string fqdn, string value, CancellationToken ct = default)
    {
        return await Auth.GetToken(new TokenRequest(UserCredentials, ApplicationCredentials), ct) switch
        {
            TokenResult.Ok ok => await DeleteRecord(ok.Token.AccessToken, fqdn, ct),
            TokenResult.Error e => new(ExitCode.AuthError, e.Message),
            _ => throw new ArgumentOutOfRangeException("TokenResult")
        };
    }

    private async Task<AcmeResult> DeleteRecord(AccessToken accessToken, string fqdn, CancellationToken ct)
    {
        return await Dns.GetAllDnsZones(accessToken, ct) switch
        {
            DnsResult<DnsZone[]>.Ok ok => await DeleteRecord(ok.Value, fqdn, accessToken, ct),
            DnsResult<DnsZone[]>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    private async Task<AcmeResult> DeleteRecord(DnsZone[] zones, string fqdn, AccessToken accessToken, CancellationToken ct)
    {
        var (domain, zoneName) = ParseFqdn(fqdn);
        var zone = zones.FirstOrDefault(x => x.Name == zoneName);
        if (zone == null) return new(ExitCode.ZoneNotFound, $"'{zoneName}' is not found on provided account. Check your data");
        if (zone.HasChanges) return new(ExitCode.ZoneIsDirty, $"'{zoneName}' has uncommitted changes to it. Contact the administrator");

        return await Dns.GetDnsRecords(zone, accessToken, ct) switch
        {
            DnsResult<DnsRecord[]>.Ok ok => await DeleteRecord(ok.Value, domain, zone, accessToken, ct),
            DnsResult<DnsRecord[]>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    private async Task<AcmeResult> DeleteRecord(DnsRecord[] records, string domain, DnsZone zone, AccessToken accessToken, CancellationToken ct)
    {
        var recordId = records
            .Where(x => x.Data is DnsData.Txt txt && txt.Name == domain) // possibly to check value here and fail if it's not ours
            .Select(x => x.Id)
            .FirstOrDefault();

        if (recordId == default)
            return new(ExitCode.Ok, "Nothing to do here, there's no such record");

        return await Dns.DeleteDnsRecord(new DnsRecordDeletionRequest(zone, recordId), accessToken, ct) switch
        {
            DnsResult<Unit>.Ok ok => await Commit(zone, accessToken, ct),
            DnsResult<Unit>.Error e => new(e.Value),
            _ => throw new ArgumentOutOfRangeException("DnsResult"),
        };
    }

    private static (string domain, string zone) ParseFqdn(string fqdn)
    {
        if (string.IsNullOrWhiteSpace(fqdn))
            throw new ArgumentNullException(nameof(fqdn));

        var domains = fqdn.Split(".", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (domains.Length < 3)
            throw new ArgumentOutOfRangeException(nameof(fqdn), $"{fqdn} is not even L3 domain, but L{domains.Length} domain. Can't work on it");

        return (string.Join('.', domains[..^2]), string.Join('.', domains[^2..]));
    }
}
