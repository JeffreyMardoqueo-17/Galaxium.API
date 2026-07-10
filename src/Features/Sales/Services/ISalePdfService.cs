using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces;

public interface ISalePdfService
{
    byte[] GenerateInvoicePdf(Sale sale);
    byte[] GenerateSalesReportPdf(
        IReadOnlyCollection<Sale> sales,
        DateTime startDate,
        DateTime endDate,
        string title);
}
