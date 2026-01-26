using MediatR;
using QIMy.Application.Clients.DTOs;

namespace QIMy.Application.Clients.Queries.GetAllClients;

/// <summary>
/// Запрос для получения всех клиентов
/// </summary>
public record GetAllClientsQuery : IRequest<IEnumerable<ClientDto>>
{
    /// <summary>
    /// Фильтр по бизнесу (опционально). Если null - возвращает всех.
    /// </summary>
    public int? BusinessId { get; init; }
}
