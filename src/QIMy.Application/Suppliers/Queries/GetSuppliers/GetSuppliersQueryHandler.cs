using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Queries.GetSuppliers;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, Result<List<SupplierDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSuppliersQueryHandler> _logger;

    public GetSuppliersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetSuppliersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync(cancellationToken);

            // Filter by BusinessId if provided
            if (request.BusinessId.HasValue)
            {
                suppliers = suppliers.Where(s => s.BusinessId == request.BusinessId.Value).ToList();
            }

            // Filter by search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                suppliers = suppliers.Where(s =>
                    s.CompanyName.ToLower().Contains(searchTerm) ||
                    (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(searchTerm)) ||
                    (s.Email != null && s.Email.ToLower().Contains(searchTerm)) ||
                    (s.VatNumber != null && s.VatNumber.ToLower().Contains(searchTerm))
                ).ToList();
            }

            var supplierDtos = _mapper.Map<List<SupplierDto>>(suppliers);

            _logger.LogInformation("Retrieved {Count} suppliers", supplierDtos.Count);
            return Result<List<SupplierDto>>.Success(supplierDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return Result<List<SupplierDto>>.Failure($"Error retrieving suppliers: {ex.Message}");
        }
    }
}
