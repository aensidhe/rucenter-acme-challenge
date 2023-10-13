using Microsoft.Extensions.DependencyInjection;
using Ru.AenSidhe.RuCenterApi;
using Ru.AenSidhe.RuCenterApi.Acme;

using var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddHttpClient()
    .AddRuCenterApi()
    .AddSingleton<AcmeService>()
    .BuildServiceProvider();

AcmeResult result;

try
{
    var acme = serviceProvider.GetRequiredService<AcmeService>();

    result = await acme.CreateRecord("asd", "asd");
}
catch (Exception ex)
{
    result = new AcmeResult(ExitCode.UnknownError, ex.ToString());
}

Console.WriteLine(result);

return (int)result.ExitCode;
