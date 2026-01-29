using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand : IRequest<Result<SupplierDto>>
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? BankAccount { get; set; }

    public bool IgnoreDuplicateWarning { get; set; }
    public bool DoubleConfirmed { get; set; }
}
