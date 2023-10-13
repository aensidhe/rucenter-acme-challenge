using Ru.AenSidhe.RuCenterApi.Auth;
using UnitGenerator;

namespace Ru.AenSidhe.RuCenterApi.Dns;

public interface IDnsClient
{
    Task<DnsResult<DnsZone[]>> GetAllDnsZones(AccessToken accessToken, CancellationToken ct);

    Task<DnsResult<DnsRecord[]>> GetDnsRecords(DnsZone zone, AccessToken accessToken, CancellationToken ct);

    Task<DnsResult<DnsRecord>> CreateDnsRecord(DnsRecordCreationRequest request, AccessToken accessToken, CancellationToken ct);

    Task<DnsResult<Unit>> DeleteDnsRecord(DnsRecordDeletionRequest request, AccessToken accessToken, CancellationToken ct);

    Task<DnsResult<Unit>> Commit(DnsZone zone, AccessToken accessToken, CancellationToken ct);

    Task<DnsResult<Unit>> Rollback(DnsZone zone, AccessToken accessToken, CancellationToken ct);
}

public abstract record DnsResult<T>
{
    public sealed record Ok(T Value) : DnsResult<T>;

    public sealed record Error(DnsError Value) : DnsResult<T>;
}

public abstract record DnsError
{
    public sealed record ServerError(int Code, string? Message) : DnsError;

    public sealed record Unauthorized() : DnsError;

    public sealed record Fail(Exception Exception) : DnsError;
}

public sealed record DnsRecordDeletionRequest(DnsZone Zone, DnsRecordId RecordId);

public abstract record DnsData
{
    public sealed record A(string Name, string Ip, Ttl? Ttl = null) : DnsData;

    public sealed record Txt(string Name, string Value, Ttl? Ttl = null) : DnsData;

    public sealed record CName(string Name, string Alias, Ttl? Ttl = null) : DnsData;
}

public sealed record DnsRecordCreationRequest(DnsZone Zone, DnsData Data);

public sealed record DnsRecord(DnsRecordId Id, DnsData Data);

[UnitOf(typeof(int))]
public readonly partial struct DnsRecordId {}

[UnitOf(typeof(int))]
public readonly partial struct Ttl {}

public sealed record DnsZone(DnsZoneId Id, string Name, DnsServiceName Service, bool HasChanges = false);

[UnitOf(typeof(int))]
public readonly partial struct DnsZoneId {}

[UnitOf(typeof(string))]
public readonly partial struct DnsServiceName {}