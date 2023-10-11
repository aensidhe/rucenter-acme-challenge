using System.Net;
using System.Net.Http.Headers;
using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;
using Ru.AenSidhe.RuCenterApi.Tests.Mocks;

namespace Ru.AenSidhe.RuCenterApi.Tests;

public class DnsClientNegativeTests
{
    [Fact]
    public async Task GetAllZones_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.GetAllDnsZones(new AccessToken(Guid.NewGuid().ToString()), CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsZone[]>.Unauthorized>();
    }

    [Fact]
    public async Task GetDnsRecords_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.GetDnsRecords(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord[]>.Unauthorized>();
    }

    [Fact]
    public async Task Commit_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.Commit(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Unauthorized>();
    }

    [Fact]
    public async Task Rollback_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.Rollback(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Unauthorized>();
    }

    [Fact]
    public async Task CreateDnsRecord_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.CreateDnsRecord(
            new DnsRecordCreationRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsData.A("name", "0.0.0.0", new Ttl(60))
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord>.Unauthorized>();
    }

    [Fact]
    public async Task DeleteDnsRecord_Unauthorized()
    {
        var dns = new DnsClient(HttpClientFactory.Unauthorized);

        var response = await dns.DeleteDnsRecord(
            new DnsRecordDeletionRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsRecordId(123)
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Unauthorized>();
    }

    [Fact]
    public async Task GetAllZones_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.GetAllDnsZones(new AccessToken(Guid.NewGuid().ToString()), CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsZone[]>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task GetDnsRecords_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.GetDnsRecords(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord[]>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task Commit_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.Commit(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task Rollback_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.Rollback(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task CreateDnsRecord_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.CreateDnsRecord(
            new DnsRecordCreationRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsData.A("name", "0.0.0.0", new Ttl(60))
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    [Fact]
    public async Task DeleteDnsRecord_Exception()
    {
        var dns = new DnsClient(HttpClientFactory.BOOMFactory);

        var response = await dns.DeleteDnsRecord(
            new DnsRecordDeletionRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsRecordId(123)
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.Fail>()
            .Which.Exception.Message.Should().Be("BOOM");
    }

    private const string ServerErrorResponse = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<response>
  <status>fail</status>
  <errors>
    <error code=""4097"">Some server error</error>
  </errors>
</response>";

    [Fact]
    public async Task GetAllZones_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.GetAllDnsZones(new AccessToken(Guid.NewGuid().ToString()), CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsZone[]>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }

    [Fact]
    public async Task GetDnsRecords_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.GetDnsRecords(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord[]>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }

    [Fact]
    public async Task Commit_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.Commit(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }

    [Fact]
    public async Task Rollback_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.Rollback(
            new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }

    [Fact]
    public async Task CreateDnsRecord_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.CreateDnsRecord(
            new DnsRecordCreationRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsData.A("name", "0.0.0.0", new Ttl(60))
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<DnsRecord>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }

    [Fact]
    public async Task DeleteDnsRecord_ServerError()
    {
        var dns = new DnsClient(new HttpClientFactory(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(ServerErrorResponse, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
        }));

        var response = await dns.DeleteDnsRecord(
            new DnsRecordDeletionRequest(
                new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false),
                new DnsRecordId(123)
            ),
            new AccessToken(Guid.NewGuid().ToString()),
            CancellationToken.None);

        response.Should().BeOfType<DnsResult<Unit>.ServerError>();
        response.Should().BeEquivalentTo(new { Code = 4097, Message = "Some server error" });
    }
}
