using AutoMapper;
using QIMy.Application.Clients.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

/// <summary>
/// AutoMapper профиль для маппинга Client
/// </summary>
public class ClientProfile : Profile
{
    public ClientProfile()
    {
        // Client → ClientDto
        CreateMap<Client, ClientDto>()
            .ForMember(d => d.ClientTypeName,
                opt => opt.MapFrom(s => s.ClientType != null ? s.ClientType.Name : null))
            .ForMember(d => d.ClientAreaName,
                opt => opt.MapFrom(s => s.ClientArea != null ? s.ClientArea.Name : null))
            .ForMember(d => d.DefaultPaymentMethodName,
                opt => opt.MapFrom(s => s.DefaultPaymentMethod != null ? s.DefaultPaymentMethod.Name : null))
            .ForMember(d => d.CurrencyCode,
                opt => opt.MapFrom(s => s.Currency != null ? s.Currency.Code : null));

        // CreateClientDto → Client
        CreateMap<CreateClientDto, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ClientCode, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.ClientType, opt => opt.Ignore())
            .ForMember(d => d.ClientArea, opt => opt.Ignore())
            .ForMember(d => d.DefaultPaymentMethod, opt => opt.Ignore())
            .ForMember(d => d.Currency, opt => opt.Ignore())
            .ForMember(d => d.Invoices, opt => opt.Ignore());

        // UpdateClientDto → Client
        CreateMap<UpdateClientDto, Client>()
            .ForMember(d => d.ClientCode, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.ClientType, opt => opt.Ignore())
            .ForMember(d => d.ClientArea, opt => opt.Ignore())
            .ForMember(d => d.DefaultPaymentMethod, opt => opt.Ignore())
            .ForMember(d => d.Currency, opt => opt.Ignore())
            .ForMember(d => d.Invoices, opt => opt.Ignore());
    }
}
