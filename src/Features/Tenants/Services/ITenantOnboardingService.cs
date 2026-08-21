using Galaxium.Api.Features.Tenants.Contracts.Requests;
using Galaxium.Api.Features.Tenants.Contracts.Responses;

namespace Galaxium.Api.Features.Tenants.Services;

public interface ITenantOnboardingService
{
    Task<TenantOnboardingResponse> OnboardAsync(TenantOnboardingRequest request);
}
