using System.Diagnostics;
using Galaxium.Api.DTOs.Reports;
using Galaxium.Api.Services.AI.Interfaces;
using Galaxium.Api.Services.AI.Models;
using Galaxium.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Galaxium.Api.Services.AI.Core;

public class ToolExecutor : IToolExecutor
{
    private readonly ISaleService _saleService;
    private readonly IReportService _reportService;
    private readonly IDashboardService _dashboardService;
    private readonly IProductService _productService;
    private readonly IStockEntryService _stockEntryService;
    private readonly ICustomerService _customerService;
    private readonly IToolRegistry _toolRegistry;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(
        ISaleService saleService,
        IReportService reportService,
        IDashboardService dashboardService,
        IProductService productService,
        IStockEntryService stockEntryService,
        ICustomerService customerService,
        IToolRegistry toolRegistry,
        ILogger<ToolExecutor> logger)
    {
        _saleService = saleService;
        _reportService = reportService;
        _dashboardService = dashboardService;
        _productService = productService;
        _stockEntryService = stockEntryService;
        _customerService = customerService;
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    public bool CanExecute(string toolName)
    {
        return _toolRegistry.HasTool(toolName);
    }

    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        Dictionary<string, object?> arguments,
        int tenantId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing tool: {ToolName} with args: {@Arguments}", toolName, arguments);

            var result = toolName switch
            {
                IntentConstants.GetBusinessMetric => await ExecuteGetBusinessMetricAsync(arguments, cancellationToken),
                IntentConstants.ExplainTrend => await ExecuteExplainTrendAsync(arguments),
                _ => new ToolExecutionResult
                {
                    Success = false,
                    Error = $"Herramienta '{toolName}' no reconocida."
                }
            };

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Tool {ToolName} executed in {ElapsedMs}ms, Success: {Success}",
                toolName, result.ExecutionTimeMs, result.Success);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);

            return new ToolExecutionResult
            {
                Success = false,
                Error = $"Error al ejecutar la herramienta: {ex.Message}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task<ToolExecutionResult> ExecuteGetBusinessMetricAsync(
        Dictionary<string, object?> args,
        CancellationToken cancellationToken)
    {
        var metric = GetStringArg(args, "metric") ?? IntentConstants.MetricSales;
        var rangeType = GetStringArg(args, "rangeType") ?? IntentConstants.RangeCurrentMonth;
        var groupBy = GetStringArg(args, "groupBy") ?? IntentConstants.GroupByDay;
        var comparison = GetStringArg(args, "comparison");

        var (startDate, endDate) = GetDateRange(rangeType, args);

        var metricData = new MetricData
        {
            Metric = metric,
            StartDate = startDate,
            EndDate = endDate,
            GroupBy = groupBy
        };

        object? data = null;
        object? comparisonData = null;

        switch (metric.ToLower())
        {
            case IntentConstants.MetricSales:
                var salesResult = await _reportService.GetSalesByDayAsync(startDate, endDate);
                metricData.Total = (double)(salesResult?.Sum(s => s.TotalAmount) ?? 0);
                metricData.Items = salesResult?.Select(s => new MetricItem
                {
                    Label = s.Date.ToString("yyyy-MM-dd"),
                    Value = (double)s.TotalAmount,
                    Count = s.Transactions
                }).ToList() ?? new List<MetricItem>();
                data = salesResult;
                break;

            case IntentConstants.MetricProfit:
                var profitResult = await _reportService.GetProfitSummaryAsync(startDate, endDate);
                metricData.Total = (double)(profitResult?.Profit ?? 0);
                data = profitResult;
                break;

            case IntentConstants.MetricCustomers:
                var customersResult = await _customerService.GetAllCustomersAsync();
                metricData.Total = customersResult?.Count() ?? 0;
                data = customersResult;
                break;

            case IntentConstants.MetricProducts:
                var productsResult = await _productService.GetProductsAsync();
                metricData.Total = productsResult?.Count() ?? 0;
                data = productsResult;
                break;

            case IntentConstants.MetricInventory:
                var stockResult = await _reportService.GetInventorySnapshotAsync();
                var totalStock = stockResult?.Sum(s => s.Stock) ?? 0;
                metricData.Total = totalStock;
                metricData.Items = stockResult?.Select(s => new MetricItem
                {
                    Label = s.ProductName,
                    Value = s.Stock,
                    Count = s.Stock
                }).ToList() ?? new List<MetricItem>();
                data = stockResult;
                break;

            default:
                return new ToolExecutionResult
                {
                    Success = false,
                    Error = $"Métrica '{metric}' no reconocida."
                };
        }

        if (!string.IsNullOrEmpty(comparison))
        {
            var (compStart, compEnd) = GetComparisonDateRange(rangeType, comparison);
            comparisonData = await GetComparisonDataAsync(metric, compStart, compEnd, cancellationToken);
            metricData.ComparisonTotal = GetComparisonTotal(comparisonData, metric);
            if (metricData.Total > 0 && metricData.ComparisonTotal > 0)
            {
                metricData.PercentageChange = ((metricData.Total - metricData.ComparisonTotal) / metricData.ComparisonTotal) * 100;
                metricData.ChangeDirection = metricData.PercentageChange >= 0 ? "up" : "down";
            }
        }

        metricData.NumericValue = metricData.Total;
        metricData.FormattedValue = FormatMetricValue(metric, metricData.Total);

        return new ToolExecutionResult
        {
            Success = true,
            Data = new
            {
                metric = metricData.Metric,
                total = metricData.Total,
                formattedValue = metricData.FormattedValue,
                startDate = metricData.StartDate.ToString("yyyy-MM-dd"),
                endDate = metricData.EndDate.ToString("yyyy-MM-dd"),
                groupBy = metricData.GroupBy,
                comparison = comparisonData != null ? new
                {
                    total = metricData.ComparisonTotal,
                    percentageChange = metricData.PercentageChange,
                    direction = metricData.ChangeDirection
                } : null,
                items = metricData.Items.Take(20)
            },
            NumericValue = metricData.Total,
            FormattedValue = metricData.FormattedValue
        };
    }

    private async Task<ToolExecutionResult> ExecuteExplainTrendAsync(Dictionary<string, object?> args)
    {
        var metric = GetStringArg(args, "metric") ?? "unknown";
        var changePercentage = GetDoubleArg(args, "changePercentage") ?? 0;
        var direction = GetStringArg(args, "direction") ?? "unknown";

        var explanation = direction switch
        {
            "up" => GeneratePositiveTrendExplanation(metric, changePercentage),
            "down" => GenerateNegativeTrendExplanation(metric, changePercentage),
            _ => "No se puede determinar la tendencia."
        };

        return new ToolExecutionResult
        {
            Success = true,
            Data = new
            {
                metric,
                changePercentage,
                direction,
                explanation
            }
        };
    }

    private string GeneratePositiveTrendExplanation(string metric, double change)
    {
        return metric switch
        {
            IntentConstants.MetricSales => $"Las ventas crecieron un {Math.Abs(change):F1}% compared al periodo anterior. Esto podría deberse a nuevas estrategias de marketing, productos populares o temporada favorable.",
            IntentConstants.MetricProfit => $"La ganancia neta aumentó un {Math.Abs(change):F1}%. Los costos posiblemente se redujeron o los márgenes mejoraron.",
            IntentConstants.MetricCustomers => $"Se incorporó un {Math.Abs(change):F1}% más de clientes. Las estrategias de adquisición están funcionando.",
            _ => $"La métrica '{metric}' creció un {Math.Abs(change):F1}%."
        };
    }

    private string GenerateNegativeTrendExplanation(string metric, double change)
    {
        return metric switch
        {
            IntentConstants.MetricSales => $"Las ventas disminuyeron un {Math.Abs(change):F1}%. Podría ser necesario revisar estrategias de venta, precios o calidad del producto.",
            IntentConstants.MetricProfit => $"La ganancia neta bajó un {Math.Abs(change):F1}%. Revisa si los costos aumentaron o los precios necesitan ajuste.",
            IntentConstants.MetricCustomers => $"Se perdieron un {Math.Abs(change):F1}% de clientes. Considera encuestas de satisfacción y programas de fidelización.",
            _ => $"La métrica '{metric}' cayó un {Math.Abs(change):F1}%."
        };
    }

    private (DateTime start, DateTime end) GetDateRange(string rangeType, Dictionary<string, object?> args)
    {
        var today = DateTime.UtcNow.Date;

        return rangeType switch
        {
            IntentConstants.RangeToday => (today, today),
            IntentConstants.RangeYesterday => (today.AddDays(-1), today.AddDays(-1)),
            IntentConstants.RangeCurrentWeek => (today.AddDays(-(int)today.DayOfWeek), today),
            IntentConstants.RangeLastWeek => (today.AddDays(-(int)today.DayOfWeek - 7), today.AddDays(-(int)today.DayOfWeek - 1)),
            IntentConstants.RangeCurrentMonth => (new DateTime(today.Year, today.Month, 1), today),
            IntentConstants.RangeLastMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1).AddDays(-1)),
            IntentConstants.RangeLast7Days => (today.AddDays(-6), today),
            IntentConstants.RangeLast30Days => (today.AddDays(-29), today),
            IntentConstants.RangeLast90Days => (today.AddDays(-89), today),
            IntentConstants.RangeCustom => (
                GetDateFromArg(args["startDate"]) ?? today.AddDays(-30),
                GetDateFromArg(args["endDate"]) ?? today
            ),
            _ => (today.AddDays(-30), today)
        };
    }

    private (DateTime start, DateTime end) GetComparisonDateRange(string rangeType, string comparison)
    {
        var (currentStart, currentEnd) = GetDateRange(rangeType, new Dictionary<string, object?>());
        var daysDiff = (currentEnd - currentStart).Days + 1;

        return comparison switch
        {
            IntentConstants.ComparisonPreviousWeek => (currentStart.AddDays(-7), currentEnd.AddDays(-7)),
            IntentConstants.ComparisonPreviousMonth => (currentStart.AddMonths(-1), currentEnd.AddMonths(-1)),
            IntentConstants.ComparisonPreviousYear => (currentStart.AddYears(-1), currentEnd.AddYears(-1)),
            _ => (currentStart.AddDays(-daysDiff * 2), currentStart.AddDays(-1))
        };
    }

    private async Task<object?> GetComparisonDataAsync(string metric, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        switch (metric.ToLower())
        {
            case IntentConstants.MetricSales:
                return await _reportService.GetSalesByDayAsync(start, end);
            case IntentConstants.MetricProfit:
                return await _reportService.GetProfitSummaryAsync(start, end);
            default:
                return null;
        }
    }

    private double GetComparisonTotal(object? data, string metric)
    {
        if (data == null) return 0;

        if (metric == IntentConstants.MetricSales && data is IEnumerable<SalesByDayItemDto> sales)
        {
            return (double)sales.Sum(s => s.TotalAmount);
        }

        if (metric == IntentConstants.MetricProfit && data is ProfitSummaryDto profit)
        {
            return (double)profit.Profit;
        }

        return 0;
    }

    private string FormatMetricValue(string metric, double value)
    {
        return metric switch
        {
            IntentConstants.MetricSales or IntentConstants.MetricProfit =>
                $"${value:N2} MXN",
            IntentConstants.MetricInventory =>
                $"${value:N2} MXN (valor total)",
            _ => value.ToString("N0")
        };
    }

    private static string? GetStringArg(Dictionary<string, object?> args, string key)
    {
        if (args.TryGetValue(key, out var value) && value != null)
        {
            return value.ToString();
        }
        return null;
    }

    private static double? GetDoubleArg(Dictionary<string, object?> args, string key)
    {
        if (args.TryGetValue(key, out var value) && value != null)
        {
            if (value is double d) return d;
            if (value is int i) return i;
            if (double.TryParse(value.ToString(), out var parsed)) return parsed;
        }
        return null;
    }

    private static DateTime? GetDateFromArg(object? value)
    {
        if (value is DateTime dt) return dt;
        if (DateTime.TryParse(value?.ToString(), out var parsed)) return parsed;
        return null;
    }
}
