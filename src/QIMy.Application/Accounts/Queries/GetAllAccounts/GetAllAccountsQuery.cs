using MediatR;
using QIMy.Application.Accounts.DTOs;

namespace QIMy.Application.Accounts.Queries.GetAllAccounts;

public record GetAllAccountsQuery : IRequest<IEnumerable<AccountDto>>;
