using MediatR;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetAllTaxRates;

public record GetAllTaxRatesQuery : IRequest<IEnumerable<TaxRateDto>>;
