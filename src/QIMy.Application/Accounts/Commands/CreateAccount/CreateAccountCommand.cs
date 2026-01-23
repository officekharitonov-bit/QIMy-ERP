using MediatR;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Accounts.Commands.CreateAccount;

public record CreateAccountCommand : IRequest<Result<AccountDto>>
{
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public int? DefaultTaxRateId { get; init; }
}
