using AutoMapper;
using QIMy.Application.Accounts.Commands.CreateAccount;
using QIMy.Application.Accounts.Commands.UpdateAccount;
using QIMy.Application.Accounts.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<CreateAccountCommand, Account>();

        CreateMap<UpdateAccountCommand, Account>();
    }
}
