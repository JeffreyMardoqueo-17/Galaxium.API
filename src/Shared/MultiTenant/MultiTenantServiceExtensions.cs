namespace Galaxium.Api.Shared.MultiTenant;

public static class MultiTenantServiceExtensions
{
    public static IServiceCollection AddMultiTenant(this IServiceCollection services)
    {
        services.AddScoped<TenantMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantMiddleware>();
        return app;
    }
}
