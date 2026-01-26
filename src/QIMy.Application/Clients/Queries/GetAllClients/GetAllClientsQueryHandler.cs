using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Clients.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Clients.Queries.GetAllClients;

/// <summary>
/// Обработчик запроса для получения всех клиентов
/// </summary>
public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, IEnumerable<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllClientsQueryHandler> _logger;

    public GetAllClientsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetAllClientsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDto>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all clients");

        var clients = await _unitOfWork.Clients.GetAllAsync(cancellationToken);

        // Фильтрация по бизнесу, если указан
        if (request.BusinessId.HasValue)
        {
            clients = clients.Where(c => c.BusinessId == request.BusinessId.Value).ToList();
            _logger.LogInformation("Filtered clients by BusinessId={BusinessId}", request.BusinessId.Value);
        }

        var clientDtos = _mapper.Map<IEnumerable<ClientDto>>(clients);

        _logger.LogInformation("Retrieved {Count} clients", clientDtos.Count());

        return clientDtos;
    }
}
