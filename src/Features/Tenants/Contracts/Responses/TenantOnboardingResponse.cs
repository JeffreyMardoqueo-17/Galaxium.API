namespace Galaxium.Api.Features.Tenants.Contracts.Responses;

public record TenantOnboardingResponse(
    int TenantId,
    string TenantName,
    int AdministratorUserId,
    string AdministratorUsername,
    string Message);
