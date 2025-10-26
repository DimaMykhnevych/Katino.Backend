using AutoMapper;
using Katino.Application.Commands.CategoryN.AddCategory;
using Katino.Application.Commands.ColorN.AddColor;
using Katino.Application.Commands.MeasurementTypeN.AddMeasurementType;
using Katino.Application.Commands.ProductN.AddProduct;
using Katino.Application.Commands.ProductVariantN.AddProductVariant;
using Katino.Application.Commands.User.CreateUser;
using Katino.Application.DTOs;
using Katino.Application.DTOs.Category;
using Katino.Application.DTOs.Color;
using Katino.Application.DTOs.MeasurementType;
using Katino.Application.DTOs.Product;
using Katino.Application.DTOs.ProductVariant;
using Katino.Application.DTOs.ProductVariantMeasurement;
using Katino.Application.DTOs.Size;
using Katino.Domain.Entities;
using Katino.Domain.Enums;

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
        CreateMap<AddCategoryCommand, Category>();
        CreateMap<AddColorCommand, Color>();
        CreateMap<AddMeasurementTypeCommand, MeasurementType>();

        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Product, ProductDto>();

        CreateMap<SizeType, SizeTypeDto>();
        CreateMap<Size, SizeDto>();

        CreateMap<Color, ColorDto>().ReverseMap();
        CreateMap<MeasurementType, MeasurementTypeDto>().ReverseMap();

        CreateMap<ProductStatus, ProductStatusDto>();

        CreateMap<ProductVariantMeasurement, GetProductVariantMeasurementDto>();
        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<AddProductVariantDto, ProductVariant>()
            .ForMember(pv => pv.Photos, m => m.Ignore());
        CreateMap<AddProductVariantMeasurementDto, ProductVariantMeasurement>();
        CreateMap<UpdateProductVariantDto, ProductVariant>();
    }
}

