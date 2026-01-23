using MediatR;
using QIMy.Core.DTOs;

namespace QIMy.Application.Clients.Commands.ImportClients;

public record ImportClientsCommand : IRequest<ImportResult>
{
    public Stream FileStream { get; init; } = null!;
    public bool SkipErrors { get; init; } = true;
}
