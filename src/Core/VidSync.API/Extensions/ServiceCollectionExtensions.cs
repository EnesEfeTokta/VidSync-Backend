using System.Text.Json;
using VidSync.Signaling;

namespace VidSync.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalingServices(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
        services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        return services;
    }
}