using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Queries.GetSuppliers;

public record GetSuppliersQuery : IRequest<Result<List<SupplierDto>>>
{
    public int? BusinessId { get; init; }
    public string? SearchTerm { get; init; }
}
