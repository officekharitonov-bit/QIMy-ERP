using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Clients.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Clients.Queries.GetClientById;

/// <summary>
/// Обработчик запроса для получения клиента по ID
/// </summary>
public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetClientByIdQueryHandler> _logger;

    public GetClientByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetClientByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting client by Id: {ClientId}", request.ClientId);

        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);

        if (client == null)
        {
            _logger.LogWarning("Client with Id {ClientId} not found", request.ClientId);
            return null;
        }

        return _mapper.Map<ClientDto>(client);
    }
}
