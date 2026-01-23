using AutoMapper;
using QIMy.Application.Businesses.Commands.CreateBusiness;
using QIMy.Application.Businesses.Commands.UpdateBusiness;
using QIMy.Application.Businesses.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class BusinessProfile : Profile
{
    public BusinessProfile()
    {
        CreateMap<Business, BusinessDto>();

        CreateMap<CreateBusinessCommand, Business>();

        CreateMap<UpdateBusinessCommand, Business>();
    }
}
