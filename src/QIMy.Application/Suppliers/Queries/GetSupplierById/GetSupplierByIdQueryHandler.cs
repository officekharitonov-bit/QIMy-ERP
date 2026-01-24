using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSupplierByIdQueryHandler> _logger;

    public GetSupplierByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetSupplierByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            _logger.LogWarning("Supplier not found with Id: {SupplierId}", request.Id);
            return Result<SupplierDto>.Failure($"Supplier with Id {request.Id} not found.");
        }

        var supplierDto = _mapper.Map<SupplierDto>(supplier);
        return Result<SupplierDto>.Success(supplierDto);
    }
}
