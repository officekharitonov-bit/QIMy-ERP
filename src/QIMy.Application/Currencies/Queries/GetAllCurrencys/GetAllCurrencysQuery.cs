using MediatR;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Queries.GetAllCurrencies;

public record GetAllCurrenciesQuery : IRequest<IEnumerable<CurrencyDto>>;
