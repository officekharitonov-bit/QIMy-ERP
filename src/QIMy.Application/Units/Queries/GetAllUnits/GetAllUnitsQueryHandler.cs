using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Units.DTOs;
using QIMy.Core.Entities;
using Unit = QIMy.Core.Entities.Unit;

namespace QIMy.Application.Units.Queries.GetAllUnits;

public class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, IEnumerable<UnitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUnitsQueryHandler> _logger;

    public GetAllUnitsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllUnitsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all Units");
        var Units = await _unitOfWork.Units.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UnitDto>>(Units);
    }
}
