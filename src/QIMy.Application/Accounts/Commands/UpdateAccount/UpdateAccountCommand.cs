using MediatR;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand : IRequest<Result<AccountDto>>
{
    public int Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public int? DefaultTaxRateId { get; init; }
}
