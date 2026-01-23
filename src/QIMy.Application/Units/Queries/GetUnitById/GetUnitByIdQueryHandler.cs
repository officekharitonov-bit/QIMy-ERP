using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Units.DTOs;
using QIMy.Core.Entities;
using Unit = QIMy.Core.Entities.Unit;

namespace QIMy.Application.Units.Queries.GetUnitById;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUnitByIdQueryHandler> _logger;

    public GetUnitByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetUnitByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UnitDto?> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Unit by Id: {Id}", request.UnitId);
        var Unit = await _unitOfWork.Units.GetByIdAsync(request.UnitId, cancellationToken);
        return Unit == null ? null : _mapper.Map<UnitDto>(Unit);
    }
}
