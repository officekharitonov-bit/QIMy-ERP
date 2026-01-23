using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Businesses.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Businesses.Commands.UpdateBusiness;

public class UpdateBusinessCommandHandler : IRequestHandler<UpdateBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBusinessCommandHandler> _logger;

    public UpdateBusinessCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BusinessDto>> Handle(UpdateBusinessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating business: Id={Id}, Name={Name}", request.Id, request.Name);

        var business = await _unitOfWork.Businesses.GetByIdAsync(request.Id, cancellationToken);

        if (business == null)
        {
            throw new NotFoundException(nameof(QIMy.Core.Entities.Business), request.Id);
        }

        // Check for duplicate TaxNumber (excluding current business)
        if (!string.IsNullOrEmpty(request.TaxNumber) && request.TaxNumber != business.TaxNumber)
        {
            var existingByTaxNumber = await _unitOfWork.Businesses
                .FindAsync(b => b.TaxNumber == request.TaxNumber && !b.IsDeleted && b.Id != request.Id, cancellationToken);

            if (existingByTaxNumber.Any())
            {
                return Result<BusinessDto>.Failure("Business with this Tax Number already exists.");
            }
        }

        // Check for duplicate VatNumber (excluding current business)
        if (!string.IsNullOrEmpty(request.VatNumber) && request.VatNumber != business.VatNumber)
        {
            var existingByVatNumber = await _unitOfWork.Businesses
                .FindAsync(b => b.VatNumber == request.VatNumber && !b.IsDeleted && b.Id != request.Id, cancellationToken);

            if (existingByVatNumber.Any())
            {
                return Result<BusinessDto>.Failure("Business with this VAT Number already exists.");
            }
        }

        _mapper.Map(request, business);

        await _unitOfWork.Businesses.UpdateAsync(business, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var businessDto = _mapper.Map<BusinessDto>(business);

        _logger.LogInformation("Business updated successfully: Id={Id}, Name={Name}", business.Id, business.Name);

        return Result<BusinessDto>.Success(businessDto);
    }
}
