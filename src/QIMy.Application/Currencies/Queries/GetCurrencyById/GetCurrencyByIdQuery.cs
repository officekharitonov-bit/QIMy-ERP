using MediatR;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Queries.GetCurrencyById;

public record GetCurrencyByIdQuery(int CurrencyId) : IRequest<CurrencyDto?>;
