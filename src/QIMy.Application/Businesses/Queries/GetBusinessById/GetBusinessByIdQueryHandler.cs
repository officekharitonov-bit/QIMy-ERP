using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Businesses.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Businesses.Queries.GetBusinessById;

public class GetBusinessByIdQueryHandler : IRequestHandler<GetBusinessByIdQuery, BusinessDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBusinessByIdQueryHandler> _logger;

    public GetBusinessByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetBusinessByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BusinessDto?> Handle(GetBusinessByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting business by ID: {BusinessId}", request.BusinessId);

        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);

        if (business == null)
        {
            _logger.LogWarning("Business not found: {BusinessId}", request.BusinessId);
            return null;
        }

        var businessDto = _mapper.Map<BusinessDto>(business);

        _logger.LogInformation("Business retrieved: {BusinessId} - {Name}", business.Id, business.Name);

        return businessDto;
    }
}
