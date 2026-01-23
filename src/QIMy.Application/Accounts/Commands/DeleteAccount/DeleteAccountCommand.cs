using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Accounts.Commands.DeleteAccount;

public record DeleteAccountCommand(int AccountId) : IRequest<Result>;
