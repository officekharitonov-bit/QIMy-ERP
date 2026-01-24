using AutoMapper;
using QIMy.Application.Suppliers.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

/// <summary>
/// AutoMapper профиль для маппинга Supplier
/// </summary>
public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        // Supplier → SupplierDto
        CreateMap<Supplier, SupplierDto>();

        // SupplierDto → Supplier (reverse map)
        CreateMap<SupplierDto, Supplier>()
            .ForMember(d => d.Business, opt => opt.Ignore())
            .ForMember(d => d.ExpenseInvoices, opt => opt.Ignore());
    }
}
