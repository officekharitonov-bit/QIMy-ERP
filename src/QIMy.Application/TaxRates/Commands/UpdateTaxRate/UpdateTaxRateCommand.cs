using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Commands.UpdateTaxRate;

public record UpdateTaxRateCommand : IRequest<Result<TaxRateDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsActive { get; set; }
}
