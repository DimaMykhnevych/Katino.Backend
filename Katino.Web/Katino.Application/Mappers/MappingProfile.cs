using AutoMapper;
using Katino.Application.Commands.ProductN.AddProduct;
using Katino.Application.Commands.User.CreateUser;
using Katino.Application.DTOs;
using Katino.Domain.Entities;

namespace Katino.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateUserCommand, AppUser>()
        .ForMember(u => u.Role, m => m.MapFrom(u => u.Role))
        .ForMember(u => u.UserName, m => m.MapFrom(u => u.UserName));

        CreateMap<AppUser, UserAuthInfoDto>()
            .ForMember(u => u.UserId, m => m.MapFrom(u => u.Id));

        CreateMap<AddProductCommand, Product>();
    }
}

