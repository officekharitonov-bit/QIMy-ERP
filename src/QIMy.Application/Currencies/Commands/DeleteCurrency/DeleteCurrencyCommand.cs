using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Currencies.Commands.DeleteCurrency;

public record DeleteCurrencyCommand(int CurrencyId) : IRequest<Result>;
