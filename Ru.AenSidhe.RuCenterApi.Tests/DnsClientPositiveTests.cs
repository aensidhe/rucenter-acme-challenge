using System.Net;
using System.Net.Http.Headers;
using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;
using Ru.AenSidhe.RuCenterApi.Tests.Mocks;

namespace Ru.AenSidhe.RuCenterApi.Tests;

public class DnsClientPositiveTests
{
    private const string SuccessResponse = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<response>
  <status>success</status>
</response>";

    [Fact]
    public async Task Commit()
    {
        var zone = new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false);
        var httpClientFactory = new HttpClientFactory { String(SuccessResponse) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.Commit(
            zone,
            accessToken,
            CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be(new Uri($"https://api.nic.ru/dns-master/services/{zone.Service}/zones/{zone.Name}/commit"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        requestContent.Should().BeNull();

        response.Should().BeOfType<DnsResult<Unit>.Ok>();
    }

    [Fact]
    public async Task Rollback()
    {
        var zone = new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false);
        var httpClientFactory = new HttpClientFactory { String(SuccessResponse) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.Rollback(
            zone,
            accessToken,
            CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be(new Uri($"https://api.nic.ru/dns-master/services/{zone.Service}/zones/{zone.Name}/rollback"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        requestContent.Should().BeNull();

        response.Should().BeOfType<DnsResult<Unit>.Ok>();
    }

    [Fact]
    public async Task DeleteRecord()
    {
        var zone = new DnsZone(new DnsZoneId(1), "asd", new DnsServiceName("asd"), false);
        var id = new DnsRecordId(123);
        var httpClientFactory = new HttpClientFactory { String(SuccessResponse) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.DeleteDnsRecord(
            new DnsRecordDeletionRequest(zone, id),
            accessToken,
            CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Delete);
        request.RequestUri.Should().Be(new Uri($"https://api.nic.ru/dns-master/services/{zone.Service}/zones/{zone.Name}/records/{id}"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        requestContent.Should().BeNull();

        response.Should().BeOfType<DnsResult<Unit>.Ok>();
    }

    [Fact]
    public async Task GetAllZones()
    {
        var expectedZone = new DnsZone(new DnsZoneId(618094), "aensidhe.ru", new DnsServiceName("DP2309245470"), true);
        var responseContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<response>
  <status>success</status>
  <data>
    <zone admin=""4717191/NIC-D"" enable=""true"" has-changes=""true"" has-primary=""true"" id=""618094"" idn-name=""aensidhe.ru"" name=""aensidhe.ru"" payer=""4717191/NIC-D"" service=""DP2309245470"" />
  </data>
</response>";
        var httpClientFactory = new HttpClientFactory { String(responseContent) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.GetAllDnsZones(accessToken, CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri.Should().Be(new Uri("https://api.nic.ru/dns-master/zones"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        requestContent.Should().BeNull();

        response.Should().BeOfType<DnsResult<DnsZone[]>.Ok>().Which.Value
            .Should().NotBeEmpty().And.HaveCount(1)
                .And.HaveElementAt(0, expectedZone);
    }

    [Fact]
    public async Task GetDnsRecords()
    {
        var expectedRecords = new [] {
            new DnsRecord(new DnsRecordId(58393240), new DnsData.A("@", "104.198.14.52")),
            new DnsRecord(new DnsRecordId(58393251), new DnsData.A("kaput", "81.177.33.6", new Ttl(60))),
            new DnsRecord(new DnsRecordId(58393242), new DnsData.CName("ansible-nas.asgard.storage", "asgard.storage", new Ttl(60))),
            new DnsRecord(new DnsRecordId(58393236), new DnsData.CName("asgard.storage", "cfa7ad9ce8.keenetic.link.", new Ttl(60))),
            new DnsRecord(new DnsRecordId(58393238), new DnsData.CName("www", "copy-writer-polecat-77638.netlify.com.")),
            new DnsRecord(new DnsRecordId(58393225), new DnsData.Txt("@", "keybase-site-verification=GyxDDME-Ptm1acaE8hu7Wgxog0hkaXb4lIK34SCJcJk")),
            new DnsRecord(new DnsRecordId(58393241), new DnsData.Txt("aensidhe.ru", "mailru-domain: PhFnt17YaQ6s1Xxg")),
            new DnsRecord(new DnsRecordId(58393237), new DnsData.Txt("mailru._domainkey", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC+Tl8RmqQfVefjyBXULXYrmUkEydArnC/1PbuiTK760flKVyNuDj4n2IgDHl8SkRCNLL92TMGuuyojGIOyzYGRKc/Drxdn4xudKY1ggsbuFrekNHYT61HT7gg1H47FYiQv4V3JQI4+qLy+ijFM305XBy57+idCevd63ByZDIvpswIDAQAB")),
        };

        var responseContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<response>
  <status>success</status>
  <data>
    <zone admin=""4717191/NIC-D"" has-changes=""false"" id=""618094"" idn-name=""aensidhe.ru"" name=""aensidhe.ru"" service=""DP2309245470"">
      <rr id=""58496884"">
        <name>@</name>
        <idn-name>@</idn-name>
        <type>SOA</type>
        <soa>
          <mname>
            <name>ns3-l2.nic.ru.</name>
            <idn-name>ns3-l2.nic.ru.</idn-name>
          </mname>
          <rname>
            <name>dns.nic.ru.</name>
            <idn-name>dns.nic.ru.</idn-name>
          </rname>
          <serial>2023092506</serial>
          <refresh>1440</refresh>
          <retry>3600</retry>
          <expire>2592000</expire>
          <minimum>60</minimum>
        </soa>
      </rr>
      <rr id=""58293533"">
        <name>@</name>
        <idn-name>@</idn-name>
        <type>NS</type>
        <ns>
          <name>ns3-l2.nic.ru.</name>
          <idn-name>ns3-l2.nic.ru.</idn-name>
        </ns>
      </rr>
      <rr id=""58393240"">
        <name>@</name>
        <idn-name>@</idn-name>
        <type>A</type>
        <a>104.198.14.52</a>
      </rr>
      <rr id=""58393251"">
        <name>kaput</name>
        <idn-name>kaput</idn-name>
        <ttl>60</ttl>
        <type>A</type>
        <a>81.177.33.6</a>
      </rr>
      <rr id=""58393228"">
        <name>@</name>
        <idn-name>@</idn-name>
        <type>MX</type>
        <mx>
          <preference>10</preference>
          <exchange>
            <name>emx.mail.ru.</name>
            <idn-name>emx.mail.ru.</idn-name>
          </exchange>
        </mx>
      </rr>
      <rr id=""58393242"">
        <name>ansible-nas.asgard.storage</name>
        <idn-name>ansible-nas.asgard.storage</idn-name>
        <ttl>60</ttl>
        <type>CNAME</type>
        <cname>
          <name>asgard.storage</name>
          <idn-name>asgard.storage</idn-name>
        </cname>
      </rr>
      <rr id=""58393236"">
        <name>asgard.storage</name>
        <idn-name>asgard.storage</idn-name>
        <ttl>60</ttl>
        <type>CNAME</type>
        <cname>
          <name>cfa7ad9ce8.keenetic.link.</name>
          <idn-name>cfa7ad9ce8.keenetic.link.</idn-name>
        </cname>
      </rr>
      <rr id=""58393238"">
        <name>www</name>
        <idn-name>www</idn-name>
        <type>CNAME</type>
        <cname>
          <name>copy-writer-polecat-77638.netlify.com.</name>
          <idn-name>copy-writer-polecat-77638.netlify.com.</idn-name>
        </cname>
      </rr>
      <rr id=""58393225"">
        <name>@</name>
        <idn-name>@</idn-name>
        <type>TXT</type>
        <txt>
          <string>keybase-site-verification=GyxDDME-Ptm1acaE8hu7Wgxog0hkaXb4lIK34SCJcJk</string>
        </txt>
      </rr>
      <rr id=""58393241"">
        <name>aensidhe.ru</name>
        <idn-name>aensidhe.ru</idn-name>
        <type>TXT</type>
        <txt>
          <string>mailru-domain:</string>
          <string>PhFnt17YaQ6s1Xxg</string>
        </txt>
      </rr>
      <rr id=""58393237"">
        <name>mailru._domainkey</name>
        <idn-name>mailru._domainkey</idn-name>
        <type>TXT</type>
        <txt>
          <string>v=DKIM1;</string>
          <string>k=rsa;</string>
          <string>p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC+Tl8RmqQfVefjyBXULXYrmUkEydArnC/1PbuiTK760flKVyNuDj4n2IgDHl8SkRCNLL92TMGuuyojGIOyzYGRKc/Drxdn4xudKY1ggsbuFrekNHYT61HT7gg1H47FYiQv4V3JQI4+qLy+ijFM305XBy57+idCevd63ByZDIvpswIDAQAB</string>
        </txt>
      </rr>
    </zone>
  </data>
</response>";

        var zone = new DnsZone(new DnsZoneId(618094), "aensidhe.ru", new DnsServiceName("DP2309245470"));
        var httpClientFactory = new HttpClientFactory { String(responseContent) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.GetDnsRecords(zone, accessToken, CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri.Should().Be(new Uri($"https://api.nic.ru/dns-master/services/{zone.Service}/zones/{zone.Name}/records"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        requestContent.Should().BeNull();

        response.Should().BeOfType<DnsResult<DnsRecord[]>.Ok>().Which.Value
            .Should().Equal(expectedRecords);
    }

    [Fact]
    public async Task CreateDnsRecord_TtlIsNotNull()
    {
        var responseContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<response>
  <status>success</status>
  <data>
    <zone admin=""4717191/NIC-D"" has-changes=""true"" id=""618094"" idn-name=""aensidhe.ru"" name=""aensidhe.ru"" service=""DP2309245470"">
      <rr id=""58527977"">
        <name>test-ttl</name>
        <idn-name>test-ttl</idn-name>
        <ttl>60</ttl>
        <type>TXT</type>
        <txt>
          <string>a</string>
          <string>b</string>
          <string>c</string>
        </txt>
      </rr>
    </zone>
  </data>
</response>";
        var expectedRequestBody = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<request>
  <rr-list>
    <rr>
      <name>test-ttl</name>
      <ttl>60</ttl>
      <type>TXT</type>
      <txt>
        <string>a</string>
        <string>b</string>
        <string>c</string>
      </txt>
    </rr>
  </rr-list>
</request>";
        var expectedContentType = "text/xml; charset=UTF-8";

        var zone = new DnsZone(new DnsZoneId(618094), "aensidhe.ru", new DnsServiceName("DP2309245470"));
        var data = new DnsData.Txt("test-ttl", "a b c", new Ttl(60));
        var createRequest = new DnsRecordCreationRequest(zone, data);
        var expectedRecord = new DnsRecord(new DnsRecordId(58527977), data);
        var httpClientFactory = new HttpClientFactory { String(responseContent) };
        var dns = new DnsClient(httpClientFactory);
        var accessToken = new AccessToken(Guid.NewGuid().ToString());

        var response = await dns.CreateDnsRecord(createRequest, accessToken, CancellationToken.None);

        var (request, requestContent) = httpClientFactory.PopSeenRequest();

        request.Method.Should().Be(HttpMethod.Put);
        request.RequestUri.Should().Be(new Uri($"https://api.nic.ru/dns-master/services/{zone.Service}/zones/{zone.Name}/records"));
        request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive()));
        request.Content.Should().NotBeNull();
        request.Content!.Headers.ContentType.Should().BeEquivalentTo(MediaTypeHeaderValue.Parse(expectedContentType));
        requestContent.Should().NotBeNull().And.Be(expectedRequestBody);

        response.Should().BeOfType<DnsResult<DnsRecord>.Ok>().Which.Value
            .Should().BeEquivalentTo(expectedRecord);
    }

    private static HttpResponseMessage String(string content) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, MediaTypeHeaderValue.Parse("text/xml; charset=UTF-8"))
    };
}