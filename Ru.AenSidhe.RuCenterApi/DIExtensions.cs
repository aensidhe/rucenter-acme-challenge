using Microsoft.Extensions.DependencyInjection;
using Ru.AenSidhe.RuCenterApi.Auth;
using Ru.AenSidhe.RuCenterApi.Dns;

namespace Ru.AenSidhe.RuCenterApi;

public static class DIExtensions
{
    public static IServiceCollection AddRuCenterApi(this IServiceCollection services) => services
        .AddSingleton<IOAuthClient, OAuthClient>()
        .AddSingleton<IDnsClient, DnsClient>();
}
