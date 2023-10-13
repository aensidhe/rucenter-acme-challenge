namespace Ru.AenSidhe.RuCenterApi.Acme;

public interface IAcmeService
{
    Task<AcmeResult> CreateRecord(string fqdn, string value, CancellationToken ct = default);

    Task<AcmeResult> DeleteRecord(string fqdn, string value, CancellationToken ct = default);
}
