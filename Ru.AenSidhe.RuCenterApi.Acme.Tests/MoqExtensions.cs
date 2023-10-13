using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;

namespace Ru.AenSidhe.RuCenterApi.Acme.Tests;

internal static class MoqExtensions
{
    public static Mock<IOAuthClient> AddGetTokenOnce(this Mock<IOAuthClient> auth, AccessToken token)
    {
        auth
            .Setup(x => x.GetToken(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResult.Ok(new Token(token, default, DateTimeOffset.UtcNow.AddSeconds(14400))))
            .Verifiable(Times.Once);
        return auth;
    }

    public static Mock<IDnsClient> AddGetZonesOnce(this Mock<IDnsClient> dns, AccessToken token, DnsZone dnsZone)
    {
        dns
            .Setup(x => x.GetAllDnsZones(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DnsResult<DnsZone[]>.Ok(new [] { dnsZone }))
            .Verifiable(Times.Once);
        return dns;
    }

    public static Mock<IDnsClient> AddGetDnsRecordOnce(this Mock<IDnsClient> dns, AccessToken token, DnsZone dnsZone, DnsData data, DnsRecordId id)
    {
        dns
            .Setup(x => x.GetDnsRecords(dnsZone, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DnsResult<DnsRecord[]>.Ok(new [] { new DnsRecord(id, data) }))
            .Verifiable(Times.Once);
        return dns;
    }

    public static Mock<IDnsClient> AddCreateDnsRecordOnce(this Mock<IDnsClient> dns, AccessToken token, DnsZone dnsZone, DnsData data, DnsRecordId newId)
    {
        dns
            .Setup(x => x.CreateDnsRecord(new DnsRecordCreationRequest(dnsZone, data), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DnsResult<DnsRecord>.Ok(new DnsRecord(newId, data)))
            .Verifiable(Times.Once);
        return dns;
    }

    public static Mock<IDnsClient> AddDeleteDnsRecordOnce(this Mock<IDnsClient> dns, AccessToken token, DnsZone dnsZone, DnsRecordId id)
    {
        dns
            .Setup(x => x.DeleteDnsRecord(new DnsRecordDeletionRequest(dnsZone, id), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DnsResult<Unit>.Ok(Unit.Instance))
            .Verifiable(Times.Once);
        return dns;
    }

    public static Mock<IDnsClient> AddCommitOnce(this Mock<IDnsClient> dns, AccessToken token, DnsZone dnsZone)
    {
        dns
            .Setup(x => x.Commit(dnsZone, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DnsResult<Unit>.Ok(Unit.Instance))
            .Verifiable(Times.Once);
        return dns;
    }
}
