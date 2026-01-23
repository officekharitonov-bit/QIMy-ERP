using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Commands.CreateTaxRate;

public record CreateTaxRateCommand : IRequest<Result<TaxRateDto>>
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsActive { get; set; } = true;
}
