using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(int Id) : IRequest<Result<bool>>;
