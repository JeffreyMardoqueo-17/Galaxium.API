namespace Galaxium.Api.Services.AI.Interfaces;

public interface IResponseFormatter
{
    string FormatNaturalLanguage<T>(
        T data,
        string metric,
        string? comparisonMetric = null,
        double? percentageChange = null);

    string FormatError(string errorMessage);
    string FormatClarification(string question);
}
