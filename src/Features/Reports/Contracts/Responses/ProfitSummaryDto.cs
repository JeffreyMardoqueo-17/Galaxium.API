namespace Galaxium.Api.DTOs.Reports;

public record ProfitSummaryDto(
    decimal Revenue,
    decimal Investment,
    decimal Profit
);
