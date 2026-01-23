using MediatR;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetTaxRateById;

public record GetTaxRateByIdQuery(int TaxRateId) : IRequest<TaxRateDto?>;
