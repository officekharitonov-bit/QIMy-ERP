using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(int Id) : IRequest<Result<SupplierDto>>;
