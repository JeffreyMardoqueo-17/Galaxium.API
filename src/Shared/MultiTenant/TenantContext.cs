namespace Galaxium.Api.Shared.MultiTenant;

public static class TenantContext
{
    private static readonly AsyncLocal<int?> _tenantId = new();

    public static int TenantId
    {
        get => _tenantId.Value
            ?? throw new InvalidOperationException(
                "Tenant context has not been initialized. " +
                "Ensure TenantMiddleware is registered and the request contains a valid tenant_id claim.");
    }

    public static int? TryGetTenantId() => _tenantId.Value;

    public static void SetTenantId(int tenantId) => _tenantId.Value = tenantId;

    public static void Clear() => _tenantId.Value = null;
}
