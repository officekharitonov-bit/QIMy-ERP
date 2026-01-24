using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand : IRequest<Result<SupplierDto>>
{
    public int BusinessId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TaxNumber { get; init; }
    public string? VatNumber { get; init; }
    public string? BankAccount { get; init; }
    
    public bool IgnoreDuplicateWarning { get; init; }
    public bool DoubleConfirmed { get; init; }
}
