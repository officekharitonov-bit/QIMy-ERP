using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Clients.Commands.DeleteClient;

/// <summary>
/// Команда для удаления клиента (soft delete)
/// </summary>
public record DeleteClientCommand(int ClientId) : IRequest<Result>;
