using Microsoft.Extensions.DependencyInjection;

namespace PaddleRcl.Services;

// PaddleRcl/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaddleRcl(this IServiceCollection services)
    {
        services.AddHttpClient<PaddlePortalService>();
        return services;
    }
}