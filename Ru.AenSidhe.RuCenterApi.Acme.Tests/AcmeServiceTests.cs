using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;

namespace Ru.AenSidhe.RuCenterApi.Acme.Tests;

public class AcmeServiceTests
{
    [Fact]
    public async Task CreateRecord_HappyPath()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), false);
        var data = new DnsData.Txt("abc", "test record", new Ttl(60));
        var id = new DnsRecordId(234);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone)
            .AddCreateDnsRecordOnce(token, zone, data, id)
            .AddCommitOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Ok);
        message.Should().Be("Changes were committed");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_HappyPath()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), false);
        var data = new DnsData.Txt("abc", "test record", new Ttl(60));
        var id = new DnsRecordId(234);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone)
            .AddGetDnsRecordOnce(token, zone, data, id)
            .AddDeleteDnsRecordOnce(token, zone, id)
            .AddCommitOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Ok);
        message.Should().Be("Changes were committed");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }
}
