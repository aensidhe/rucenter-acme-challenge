using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

    result = await Parser.Default.ParseArguments<PresentOptions, CleanupOptions>(args)
        .MapResult(
            (PresentOptions x) => acme.CreateRecord(x.Fqdn, x.Record),
            (CleanupOptions x) => acme.DeleteRecord(x.Fqdn, x.Record),
            errors => Task.FromResult(new AcmeResult(ExitCode.ArgParseError, $"Can't parse this command line: '{string.Join(' ', args)}'")));
}
catch (Exception ex)
{
    result = new AcmeResult(ExitCode.UnknownError, ex.ToString());
}

Console.WriteLine(result);

return (int)result.ExitCode;
