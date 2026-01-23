using MediatR;
using QIMy.Application.Clients.DTOs;

namespace QIMy.Application.Clients.Queries.GetClientById;

/// <summary>
/// Запрос для получения клиента по ID
/// </summary>
public record GetClientByIdQuery(int ClientId) : IRequest<ClientDto?>;
