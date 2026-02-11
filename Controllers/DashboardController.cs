using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.DTOs.Dashboard;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ============================================================
        // SUMMARY
        // GET: api/dashboard/summary
        // ============================================================

        /// <summary>
        /// Obtiene el resumen general del dashboard
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        ///
        /// {
        ///   "totalCustomers": 2,
        ///   "totalSales": 8,
        ///   "totalRevenue": 258.5,
        ///   "totalInvestment": 120.0,
        ///   "totalStock": 24,
        ///   "netProfit": 138.5
        /// }
        ///
        /// </remarks>
        /// <response code="200">Resumen obtenido correctamente</response>
        /// <response code="500">Error interno</response>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDTO), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardSummaryDTO>> GetSummary()
        {
            try
            {
                var summary = await _dashboardService
                    .GetDashboardSummaryAsync();

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving dashboard summary.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // TOP PRODUCTS
        // GET: api/dashboard/top-products?top=5
        // ============================================================

        /// <summary>
        /// Obtiene los productos más vendidos
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        ///
        /// {
        ///   "requestedTop": 5,
        ///   "products": [
        ///     {
        ///       "productId": 1,
        ///       "productName": "Audífonos",
        ///       "totalSold": 15,
        ///       "totalRevenue": 105.00
        ///     },
        ///     {
        ///       "productId": 2,
        ///       "productName": "Mouse Gamer",
        ///       "totalSold": 10,
        ///       "totalRevenue": 80.00
        ///     }
        ///   ]
        /// }
        ///
        /// </remarks>
        /// <param name="top">Cantidad de productos</param>
        /// <response code="200">Listado obtenido correctamente</response>
        /// <response code="400">Parámetro inválido</response>
        /// <response code="500">Error interno</response>
        [HttpGet("top-products")]
        [ProducesResponseType(typeof(TopSellingProductsResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TopSellingProductsResponseDTO>>
            GetTopSellingProducts([FromQuery] int top = 5)
        {
            try
            {
                var result = await _dashboardService
                    .GetTopSellingProductsAsync(top);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving top products.",
                    error = ex.Message
                });
            }
        }
    }
}
