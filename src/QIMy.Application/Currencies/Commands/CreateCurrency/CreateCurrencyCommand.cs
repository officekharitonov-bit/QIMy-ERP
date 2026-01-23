using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Commands.CreateCurrency;

public record CreateCurrencyCommand : IRequest<Result<CurrencyDto>>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal ExchangeRate { get; init; }
    public bool IsDefault { get; init; } = false;
}
