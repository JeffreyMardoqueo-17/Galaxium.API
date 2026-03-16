using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = GalaxiumPolicyNames.ReportsAccess)]
public class StockAlertController : ControllerBase
{
    private readonly IStockAlertService _stockAlertService;

    public StockAlertController(IStockAlertService stockAlertService)
    {
        _stockAlertService = stockAlertService;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var alerts = await _stockAlertService.RefreshAlertsAsync();
        return Ok(alerts);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var alerts = await _stockAlertService.GetActiveAlertsAsync();
        return Ok(alerts);
    }

    [HttpPatch("{alertId:int}/resolve")]
    public async Task<IActionResult> Resolve(int alertId)
    {
        var resolved = await _stockAlertService.ResolveAlertAsync(alertId);
        if (resolved == null)
            return NotFound();

        return Ok(resolved);
    }
}
