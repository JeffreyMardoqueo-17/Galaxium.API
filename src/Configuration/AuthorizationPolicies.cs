using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Galaxium.Api.Utils;

public static class GalaxiumRoleNames
{
    public const string Administrator = "Administrador";
    public const string InventoryManager = "Encargado de inventario";
    public const string Cashier = "Cajero";
    public const string Supervisor = "Supervisor";
}

public static class GalaxiumPolicyNames
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrSupervisor = "AdminOrSupervisor";
    public const string InventoryManagement = "InventoryManagement";
    public const string SalesAccess = "SalesAccess";
    public const string ReportsAccess = "ReportsAccess";
}

public static class AuthorizationPolicies
{
    private static readonly Dictionary<string, string[]> RoleAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        [GalaxiumRoleNames.Administrator] = new[] { "administrador", "admin", "administrator" },
        [GalaxiumRoleNames.InventoryManager] = new[] { "encargado de inventario", "inventorymanager", "inventory manager", "inventario" },
        [GalaxiumRoleNames.Cashier] = new[] { "cajero", "cashier", "vendedor", "seller" },
        [GalaxiumRoleNames.Supervisor] = new[] { "supervisor" },
    };

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(GalaxiumPolicyNames.AdminOnly, policy =>
            policy.RequireAssertion(context => HasAnyRole(context.User, GalaxiumRoleNames.Administrator)));

        options.AddPolicy(GalaxiumPolicyNames.AdminOrSupervisor, policy =>
            policy.RequireAssertion(context => HasAnyRole(
                context.User,
                GalaxiumRoleNames.Administrator,
                GalaxiumRoleNames.Supervisor)));

        options.AddPolicy(GalaxiumPolicyNames.InventoryManagement, policy =>
            policy.RequireAssertion(context => HasAnyRole(
                context.User,
                GalaxiumRoleNames.Administrator,
                GalaxiumRoleNames.Supervisor,
                GalaxiumRoleNames.InventoryManager)));

        options.AddPolicy(GalaxiumPolicyNames.SalesAccess, policy =>
            policy.RequireAssertion(context => HasAnyRole(
                context.User,
                GalaxiumRoleNames.Administrator,
                GalaxiumRoleNames.Supervisor,
                GalaxiumRoleNames.Cashier)));

        options.AddPolicy(GalaxiumPolicyNames.ReportsAccess, policy =>
            policy.RequireAssertion(context => HasAnyRole(
                context.User,
                GalaxiumRoleNames.Administrator,
                GalaxiumRoleNames.Supervisor)));
    }

    private static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        var roleClaims = user
            .FindAll(ClaimTypes.Role)
            .Select(c => Normalize(c.Value))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (!RoleAliases.TryGetValue(role, out var aliases))
            {
                aliases = new[] { role };
            }

            if (aliases.Any(alias => roleClaims.Contains(Normalize(alias))))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string value)
    {
        return string.Join(' ', value
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}