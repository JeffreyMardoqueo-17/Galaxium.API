using System.Threading.Tasks;
using Galaxium.Api.DTOs.Dashboard;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDTO>
            GetDashboardSummaryAsync();

        Task<TopSellingProductsResponseDTO>
            GetTopSellingProductsAsync(int top);
    }
}
