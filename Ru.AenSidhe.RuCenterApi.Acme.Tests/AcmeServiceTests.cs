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
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

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
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Ok);
        message.Should().Be("Changes were committed");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRecord_AuthFail()
    {
        var auth = new Mock<IOAuthClient>().AddFailedAuth();
        var dns = new Mock<IDnsClient>();
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.AuthError);
        message.Should().Be("Mock error");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_AuthFail()
    {
        var auth = new Mock<IOAuthClient>().AddFailedAuth();
        var dns = new Mock<IDnsClient>();
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.AuthError);
        message.Should().Be("Mock error");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRecord_ServerError()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesServerError(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.DnsError);
        message.Should().Be("ServerError { Code = 42, Message = Test error }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_ServerError()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesServerError(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.DnsError);
        message.Should().Be("ServerError { Code = 42, Message = Test error }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRecord_Fail()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesFail(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.UnknownError);
        message.Should().Be("Fail { Exception = System.Threading.AbandonedMutexException: The wait completed due to an abandoned mutex. }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_Fail()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesFail(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.UnknownError);
        message.Should().Be("Fail { Exception = System.Threading.AbandonedMutexException: The wait completed due to an abandoned mutex. }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRecord_Unauthorized()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesUnauthorized(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Unauthorized);
        message.Should().Be("Unauthorized { }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_Unauthorized()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>().AddGetTokenOnce(token);
        var dns = new Mock<IDnsClient>().AddGetZonesUnauthorized(token);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Unauthorized);
        message.Should().Be("Unauthorized { }");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }


    [Fact]
    public async Task CreateRecord_NoZone()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), false);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme2.com", "test record");

        exitCode.Should().Be(ExitCode.ZoneNotFound);
        message.Should().Be("'acme2.com' is not found on provided account. Check your data");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateRecord_ZoneIsDirty()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), true);
        var dns = new Mock<IDnsClient>().AddGetZonesOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.CreateRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.ZoneIsDirty);
        message.Should().Be("'acme.com' has uncommitted changes to it. Contact the administrator");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_NoZone()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), false);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme2.com", "test record");

        exitCode.Should().Be(ExitCode.ZoneNotFound);
        message.Should().Be("'acme2.com' is not found on provided account. Check your data");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteRecord_ZoneIsDirty()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), true);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.ZoneIsDirty);
        message.Should().Be("'acme.com' has uncommitted changes to it. Contact the administrator");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }


    [Fact]
    public async Task DeleteRecord_NoOp()
    {
        var token = new AccessToken(Guid.NewGuid().ToString());
        var auth = new Mock<IOAuthClient>()
            .AddGetTokenOnce(token);

        var zone = new DnsZone(new DnsZoneId(123), "acme.com", new DnsServiceName(Guid.NewGuid().ToString()), false);
        var dns = new Mock<IDnsClient>()
            .AddGetZonesOnce(token, zone)
            .AddGetZeroDnsRecordOnce(token, zone);
        var acme = new AcmeService(auth.Object, dns.Object, MockCreds.Instance);

        var (exitCode, message) = await acme.DeleteRecord("abc.acme.com", "test record");

        exitCode.Should().Be(ExitCode.Ok);
        message.Should().Be("Nothing to do here, there's no such record");
        auth.VerifyAll();
        auth.VerifyNoOtherCalls();
        dns.VerifyAll();
        dns.VerifyNoOtherCalls();
    }

    private sealed class MockCreds : ICredentials
    {
        private MockCreds() {}

        public string Username => "throw new NotImplementedException();";

        public string Password => "throw new NotImplementedException();";

        public static readonly MockCreds Instance = new();
    }
}
