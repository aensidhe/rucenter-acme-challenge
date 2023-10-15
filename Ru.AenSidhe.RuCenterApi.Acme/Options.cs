using CommandLine;

namespace Ru.AenSidhe.RuCenterApi.Acme;

[Verb("present", HelpText = "Add TXT record to DNS zone.")]
public sealed class PresentOptions
{
    public PresentOptions() => Fqdn = Record = string.Empty;

    [Value(0, Required = true, MetaName = "FQDN")]
    public string Fqdn { get; set; }


    [Value(1, Required = true, MetaName = "record")]
    public string Record { get; set; }
}


[Verb("cleanup", HelpText = "Add TXT record to DNS zone.")]
public sealed class CleanupOptions
{
    public CleanupOptions() => Fqdn = Record = string.Empty;

    [Value(0, Required = true, MetaName = "FQDN")]
    public string Fqdn { get; set; }


    [Value(1, Required = true, MetaName = "record")]
    public string Record { get; set; }
}
