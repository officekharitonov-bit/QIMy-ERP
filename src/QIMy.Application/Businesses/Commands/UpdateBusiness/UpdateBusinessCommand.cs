using MediatR;
using QIMy.Application.Businesses.DTOs;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Businesses.Commands.UpdateBusiness;

public record UpdateBusinessCommand : IRequest<Result<BusinessDto>>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TaxNumber { get; init; }
    public string? VatNumber { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Logo { get; init; }
}
