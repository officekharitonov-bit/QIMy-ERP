using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Businesses.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Businesses.Commands.CreateBusiness;

public class CreateBusinessCommandHandler : IRequestHandler<CreateBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBusinessCommandHandler> _logger;

    public CreateBusinessCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BusinessDto>> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating business: {Name}", request.Name);

        // Check for duplicate TaxNumber
        if (!string.IsNullOrEmpty(request.TaxNumber))
        {
            var existingByTaxNumber = await _unitOfWork.Businesses
                .FindAsync(b => b.TaxNumber == request.TaxNumber && !b.IsDeleted, cancellationToken);

            if (existingByTaxNumber.Any())
            {
                return Result<BusinessDto>.Failure("Business with this Tax Number already exists.");
            }
        }

        // Check for duplicate VatNumber
        if (!string.IsNullOrEmpty(request.VatNumber))
        {
            var existingByVatNumber = await _unitOfWork.Businesses
                .FindAsync(b => b.VatNumber == request.VatNumber && !b.IsDeleted, cancellationToken);

            if (existingByVatNumber.Any())
            {
                return Result<BusinessDto>.Failure("Business with this VAT Number already exists.");
            }
        }

        var business = _mapper.Map<Business>(request);

        await _unitOfWork.Businesses.AddAsync(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var businessDto = _mapper.Map<BusinessDto>(business);

        _logger.LogInformation("Business created successfully: Id={Id}, Name={Name}", business.Id, business.Name);

        return Result<BusinessDto>.Success(businessDto);
    }
}
