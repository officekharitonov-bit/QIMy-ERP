using AutoMapper;
using QIMy.Application.Units.Commands.CreateUnit;
using QIMy.Application.Units.Commands.UpdateUnit;
using QIMy.Application.Units.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitDto>();

        CreateMap<CreateUnitCommand, Unit>();

        CreateMap<UpdateUnitCommand, Unit>();
    }
}
