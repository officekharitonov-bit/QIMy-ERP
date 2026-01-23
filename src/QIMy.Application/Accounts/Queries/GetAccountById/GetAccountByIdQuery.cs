using MediatR;
using QIMy.Application.Accounts.DTOs;

namespace QIMy.Application.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(int AccountId) : IRequest<AccountDto?>;
