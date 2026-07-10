using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = GalaxiumPolicyNames.ReportsAccess)]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales-by-day")]
    public async Task<IActionResult> GetSalesByDay([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetSalesByDayAsync(startDate, endDate));
    }

    [HttpGet("sales-by-product")]
    public async Task<IActionResult> GetSalesByProduct([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetSalesByProductAsync(startDate, endDate));
    }

    [HttpGet("sales-by-category")]
    public async Task<IActionResult> GetSalesByCategory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetSalesByCategoryAsync(startDate, endDate));
    }

    [HttpGet("profits")]
    public async Task<IActionResult> GetProfits([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetProfitSummaryAsync(startDate, endDate));
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory()
    {
        return Ok(await _reportService.GetInventorySnapshotAsync());
    }

    [HttpGet("purchase-history")]
    public async Task<IActionResult> GetPurchaseHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetPurchaseHistoryAsync(startDate, endDate));
    }
}
