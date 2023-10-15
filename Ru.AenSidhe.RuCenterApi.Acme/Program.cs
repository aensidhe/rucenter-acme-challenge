using Microsoft.Extensions.DependencyInjection;
using Ru.AenSidhe.RuCenterApi;
using Ru.AenSidhe.RuCenterApi.Acme;
using Ru.AenSidhe.RuCenterApi.Auth;
using Serilog;

using var serviceProvider = new ServiceCollection()
    .AddLogging(loggingBuilder =>
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console();
        loggingBuilder.AddSerilog(config.CreateLogger(), true);
    })
    .AddHttpClient()
    .AddRuCenterApi()
    .AddSingleton<IUserCredentials, EnvironmentUserCredentials>()
    .AddSingleton<IApplicationCredentials, EnvironmentApplicationCredentials>()
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
