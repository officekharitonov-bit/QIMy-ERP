using AutoMapper;
using QIMy.Application.TaxRates.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class TaxRateProfile : Profile
{
    public TaxRateProfile()
    {
        CreateMap<TaxRate, TaxRateDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.EffectiveUntil == null))
            .ForMember(d => d.RateType, opt => opt.MapFrom(s => s.RateType.ToString()));
        
        CreateMap<TaxRate, VatRateDto>()
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.EffectiveUntil == null))
            .ForMember(d => d.RateType, opt => opt.MapFrom(s => s.RateType.ToString()));
        
        CreateMap<CreateTaxRateDto, TaxRate>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.IsDefault, opt => opt.MapFrom(s => s.IsActive));
        CreateMap<UpdateTaxRateDto, TaxRate>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.IsDefault, opt => opt.MapFrom(s => s.IsActive));
    }
}
