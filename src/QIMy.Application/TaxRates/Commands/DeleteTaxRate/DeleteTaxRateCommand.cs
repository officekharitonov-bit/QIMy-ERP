using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.TaxRates.Commands.DeleteTaxRate;

public record DeleteTaxRateCommand(int TaxRateId) : IRequest<Result>;
