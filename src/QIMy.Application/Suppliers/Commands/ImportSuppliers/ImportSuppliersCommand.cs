using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Suppliers.Commands.ImportSuppliers;

public record ImportSuppliersCommand : IRequest<Result<ImportSuppliersResult>>
{
    public int BusinessId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
}

public class ImportSuppliersResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int DuplicateCount { get; set; }
    public List<ImportError> Errors { get; set; } = new();
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
