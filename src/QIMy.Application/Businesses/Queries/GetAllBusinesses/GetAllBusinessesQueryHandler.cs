using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Businesses.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Businesses.Queries.GetAllBusinesses;

public class GetAllBusinessesQueryHandler : IRequestHandler<GetAllBusinessesQuery, IEnumerable<BusinessDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllBusinessesQueryHandler> _logger;

    public GetAllBusinessesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllBusinessesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<BusinessDto>> Handle(GetAllBusinessesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all businesses");

        var businesses = await _unitOfWork.Businesses.GetAllAsync(cancellationToken);

        var businessDtos = _mapper.Map<IEnumerable<BusinessDto>>(businesses.OrderBy(b => b.Name));

        _logger.LogInformation("Retrieved {Count} businesses", businessDtos.Count());

        return businessDtos;
    }
}
