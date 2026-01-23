using AutoMapper;
using QIMy.Application.Currencies.Commands.CreateCurrency;
using QIMy.Application.Currencies.Commands.UpdateCurrency;
using QIMy.Application.Currencies.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class CurrencyProfile : Profile
{
    public CurrencyProfile()
    {
        CreateMap<Currency, CurrencyDto>();

        CreateMap<CreateCurrencyCommand, Currency>();

        CreateMap<UpdateCurrencyCommand, Currency>();
    }
}
