using System.Net;
using System.Net.Http.Headers;
using Ru.AenSidhe.RuCenterApi.Auth;
using YAXLib;
using YAXLib.Attributes;

namespace Ru.AenSidhe.RuCenterApi.Dns;

public sealed class DnsClient : IDnsClient, IDisposable
{
    private readonly HttpClient _client;

    public DnsClient(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri("https://api.nic.ru/dns-master/");
    }

    public async Task<DnsResult<DnsRecord>> CreateDnsRecord(DnsRecordCreationRequest request, AccessToken accessToken, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"services/{request.Zone.Service}/zones/{request.Zone.Name}/records");

        var content = new YAXSerializer<XmlRecordCreationRequest>().Serialize(new XmlRecordCreationRequest(request.Data));

        httpRequest.Content = new StringContent(
            $"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>{Environment.NewLine}{content}",
            new MediaTypeHeaderValue("text/xml", "UTF-8"));
        var response = await ExecuteRequest(httpRequest, accessToken, ct);
        return Map(response, xml => xml.Data!
            .SelectMany(x => x.Records!)
            .Select(ParseDnsRecord)
            .Single(x => x != null)
        );
    }

    public async Task<DnsResult<Unit>> DeleteDnsRecord(DnsRecordDeletionRequest request, AccessToken accessToken, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"services/{request.Zone.Service}/zones/{request.Zone.Name}/records/{request.RecordId}");
        var response = await ExecuteRequest(httpRequest, accessToken, ct);
        return Map(response, _ => Unit.Instance);
    }

    public async Task<DnsResult<Unit>> Commit(DnsZone zone, AccessToken accessToken, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"services/{zone.Service}/zones/{zone.Name}/commit");
        var response = await ExecuteRequest(httpRequest, accessToken, ct);
        return Map(response, _ => Unit.Instance);
    }

    public async Task<DnsResult<Unit>> Rollback(DnsZone zone, AccessToken accessToken, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"services/{zone.Service}/zones/{zone.Name}/rollback");
        var response = await ExecuteRequest(httpRequest, accessToken, ct);
        return Map(response, _ => Unit.Instance);
    }

    public async Task<DnsResult<DnsZone[]>> GetAllDnsZones(AccessToken accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "zones");
        var response = await ExecuteRequest(request, accessToken, ct);

        return Map(response, xml => xml.Data!
            .Select(x => new DnsZone(new DnsZoneId(x.Id), x.Name!, new DnsServiceName(x.Service), x.HasChanges == true))
            .ToArray());
    }

    public async Task<DnsResult<DnsRecord[]>> GetDnsRecords(DnsZone zone, AccessToken accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"services/{zone.Service}/zones/{zone.Name}/records");
        var response = await ExecuteRequest(request, accessToken, ct);
        return Map(response, xml => xml.Data!
            .SelectMany(x => x.Records!)
            .Select(ParseDnsRecord)
            .Where(x => x != null)
            .ToArray()
        );
    }

    public void Dispose() => _client.Dispose();

    private static DnsRecord ParseDnsRecord(Record x) => x switch
    {
        { Name: null } => null!,
        { Type: RecordType.A, A.IP: not null } => new DnsRecord(
            new DnsRecordId(x.Id),
            new DnsData.A(x.Name, x.A.IP, x.Ttl != null ? new Ttl(x.Ttl.Value) : null)),
        { Type: RecordType.CNAME, CName.Name: not null } => new DnsRecord(
            new DnsRecordId(x.Id),
            new DnsData.CName(x.Name, x.CName.Name, x.Ttl != null ? new Ttl(x.Ttl.Value) : null)),
        { Type: RecordType.TXT, Txt: not null } => new DnsRecord(
            new DnsRecordId(x.Id),
            new DnsData.Txt(x.Name, string.Join(" ", x.Txt.Strings ?? Array.Empty<string>()), x.Ttl != null ? new Ttl(x.Ttl.Value) : null)),
        _ => null!
    };

    private async Task<DnsResult<XmlResponse>> ExecuteRequest(HttpRequestMessage request, AccessToken accessToken, CancellationToken ct)
    {
        try
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AsPrimitive());
            using var httpResponse = await _client.SendAsync(request, ct);
            var content = httpResponse.Content;

            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new DnsResult<XmlResponse>.Error(new DnsError.Unauthorized());
            }

            var response = new YAXSerializer<XmlResponse>()
                .Deserialize(await content.ReadAsStringAsync(ct))!;

            if (httpResponse.IsSuccessStatusCode)
            {
                return new DnsResult<XmlResponse>.Ok(response);
            }

            var firstError = response.Errors![0];

            return new DnsResult<XmlResponse>.Error(new DnsError.ServerError(firstError.Code, firstError.Message));
        }
        catch (Exception ex)
        {
            return new DnsResult<XmlResponse>.Error(new DnsError.Fail(ex));
        }
    }

    private static DnsResult<T> Map<T>(
        DnsResult<XmlResponse> xmlResponse,
        Func<XmlResponse, T> f
    )
    {
        return xmlResponse switch
        {
            DnsResult<XmlResponse>.Ok x => new DnsResult<T>.Ok(f(x.Value)),
            DnsResult<XmlResponse>.Error x => new DnsResult<T>.Error(x.Value),
            _ => new DnsResult<T>.Error(new DnsError.Fail(new ArgumentOutOfRangeException(nameof(xmlResponse))))
        };
    }

    [YAXSerializeAs("request")]
    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class XmlRecordCreationRequest
    {
        public XmlRecordCreationRequest(DnsData data)
        {
            Records = new [] { new RecordRequest(data) };
        }

        [YAXCollection(YAXLib.Enums.YAXCollectionSerializationTypes.Recursive, EachElementName = "rr")]
        [YAXSerializeAs("rr-list")]
        public RecordRequest[] Records { get; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class RecordRequest
    {
        public RecordRequest(DnsData data)
        {
            switch (data)
            {
                case DnsData.A a:
                    A = new A { IP = a.Ip };
                    Name = a.Name;
                    Type = RecordType.A;
                    if (a.Ttl is not null)
                        Ttl = a.Ttl.Value.AsPrimitive();
                    break;
                case DnsData.CName cname:
                    CName = new CName { Name = cname.Alias };
                    Name = cname.Name;
                    Type = RecordType.CNAME;
                    if (cname.Ttl is not null)
                        Ttl = cname.Ttl.Value.AsPrimitive();
                    break;
                case DnsData.Txt txt:
                    Txt = new TXT {
                        Strings = txt.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    };
                    Name = txt.Name;
                    Type = RecordType.TXT;
                    if (txt.Ttl is not null)
                        Ttl = txt.Ttl.Value.AsPrimitive();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data));
            }
        }

        [YAXSerializeAs("name")]
        public string Name { get; }

        [YAXSerializeAs("ttl")]
        public int? Ttl { get; }

        [YAXSerializeAs("type")]
        public RecordType Type { get; }

        [YAXSerializeAs("a")]
        public A? A { get; }

        [YAXSerializeAs("cname")]
        public CName? CName { get; }

        [YAXSerializeAs("txt")]
        public TXT? Txt { get; }
    }

    [YAXSerializeAs("response")]
    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class XmlResponse
    {
        [YAXSerializeAs("status")]
        public string? Status { get; set; }

        [YAXSerializeAs("errors")]
        public Error[]? Errors { get; set; }

        [YAXSerializeAs("data")]
        [YAXDontSerializeIfNull]
        public Zone[]? Data { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class Error
    {
        [YAXAttributeForClass]
        [YAXSerializeAs("code")]
        public int Code { get; set; }

        [YAXValueForClass]
        public string? Message { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class Zone
    {
        [YAXAttributeForClass]
        [YAXSerializeAs("admin")]
        public string? Admin { get; set; }

        [YAXAttributeForClass]
        [YAXSerializeAs("has-changes")]
        public bool HasChanges { get; set; }

        [YAXAttributeForClass]
        [YAXSerializeAs("id")]
        public int Id { get; set; }

        [YAXAttributeForClass]
        [YAXSerializeAs("idn-name")]
        public string? IdnName { get; set; }

        [YAXAttributeForClass]
        [YAXSerializeAs("name")]
        public string? Name { get; set; }

        [YAXAttributeForClass]
        [YAXSerializeAs("service")]
        public string? Service { get; set; }

        [YAXCollection(YAXLib.Enums.YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "rr")]
        public Record[]? Records { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class Record
    {
        [YAXAttributeForClass]
        [YAXSerializeAs("id")]
        public int Id { get; set; }

        [YAXSerializeAs("name")]
        public string? Name { get; set; }

        [YAXSerializeAs("idn-name")]
        public string? IdnName { get; set; }

        [YAXSerializeAs("type")]
        public RecordType Type { get; set; }

        [YAXSerializeAs("ttl")]
        public int? Ttl { get; set; }

        [YAXSerializeAs("cname")]
        public CName? CName { get; set; }

        [YAXSerializeAs("a")]
        public A? A { get; set;}

        [YAXSerializeAs("txt")]
        public TXT? Txt { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class TXT
    {
        [YAXCollection(YAXLib.Enums.YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "string")]
        public string[]? Strings { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class A
    {
        [YAXValueForClass]
        public string? IP { get; set; }
    }

    [YAXSerializableType(Options = YAXLib.Enums.YAXSerializationOptions.DontSerializeNullObjects | YAXLib.Enums.YAXSerializationOptions.DisplayLineInfoInExceptions)]
    private sealed class CName
    {
        [YAXSerializeAs("name")]
        public string? Name { get; set; }
    }

    private enum RecordType
    {
        Unknown,
        A,
        CNAME,
        TXT,
        SOA,
        NS,
        MX
    }
}