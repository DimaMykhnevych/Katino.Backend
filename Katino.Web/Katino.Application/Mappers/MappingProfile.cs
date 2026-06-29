using AutoMapper;
using Katino.Application.Commands.CollectionN.AddCollection;
using Katino.Application.Commands.CategoryN.AddCategory;
using Katino.Application.Commands.ColorN.AddColor;
using Katino.Application.Commands.MeasurementTypeN.AddMeasurementType;
using Katino.Application.Commands.OrderN.AddOrder;
using Katino.Application.Commands.OrderN.UpdateOrder;
using Katino.Application.Commands.ProductN.AddProduct;
using Katino.Application.Commands.User.CreateUser;
using Katino.Application.DTOs;
using Katino.Application.DTOs.Category;
using Katino.Application.DTOs.Discount;
using Katino.Application.DTOs.Collection;
using Katino.Application.DTOs.Color;
using Katino.Application.DTOs.CrmUserSettings;
using Katino.Application.DTOs.FinanceCategory;
using Katino.Application.DTOs.FinanceEntry;
using Katino.Application.DTOs.MeasurementType;
using Katino.Application.DTOs.NovaPost;
using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpContactPerson;
using Katino.Application.DTOs.NpOptionsSeat;
using Katino.Application.DTOs.NpWarehouse;
using Katino.Application.DTOs.Order;
using Katino.Application.DTOs.Order.NovaPost;
using Katino.Application.DTOs.OrderAddressInfo;
using Katino.Application.DTOs.OrderItem;
using Katino.Application.DTOs.OrderNpOptionsSeat;
using Katino.Application.DTOs.OrderRecipient;
using Katino.Application.DTOs.OrderTag;
using Katino.Application.DTOs.Pnl;
using Katino.Application.DTOs.Product;
using Katino.Application.DTOs.ProductPhoto;
using Katino.Application.DTOs.ProductVariant;
using Katino.Application.DTOs.ProductVariantMeasurement;
using Katino.Application.DTOs.ProductVariantRedistributionHistory;
using Katino.Application.DTOs.Size;
using Katino.Application.DTOs.Statistics;
using Katino.Application.DTOs.Telegram;
using Katino.Application.DTOs.User;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Models.Pnl;
using Katino.Domain.Models.Pricing;
using Katino.Domain.Models.Statistics;

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
        CreateMap<Product, ProductForOrderDto>();

        CreateMap<SizeType, SizeTypeDto>();
        CreateMap<Size, SizeDto>();

        CreateMap<Color, ColorDto>().ReverseMap();
        CreateMap<MeasurementType, MeasurementTypeDto>().ReverseMap();

        CreateMap<ProductStatus, ProductStatusDto>();
        CreateMap<SewingQueueVisibility, SewingQueueVisibilityDto>().ReverseMap();
        CreateMap<ProductPhoto, ProductPhotoDto>();

        CreateMap<ProductVariantMeasurement, GetProductVariantMeasurementDto>();
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(pv => pv.Sewers, m => m.MapFrom(s => s.Sewers.Select(sw => new SewerDto
            {
                Id = sw.SewerId,
                UserName = sw.Sewer.UserName,
                Email = sw.Sewer.Email,
            })));
        CreateMap<ProductVariant, ProductVariantForOrderDto>();
        CreateMap<AddProductVariantDto, ProductVariant>()
            .ForMember(pv => pv.Photos, m => m.Ignore())
            .ForMember(pv => pv.Sewers, m => m.Ignore());
        CreateMap<AddProductVariantMeasurementDto, ProductVariantMeasurement>();
        CreateMap<UpdateProductVariantDto, ProductVariant>()
            .ForMember(pv => pv.Sewers, m => m.Ignore());

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Tags, m => m.MapFrom(s => s.OrderTags.Select(ot => ot.OrderTag).OrderBy(t => t.Type).ThenBy(t => t.Value).ToList()));
        CreateMap<OrderTag, OrderTagDto>();
        CreateMap<OrderTagType, OrderTagTypeDto>().ReverseMap();
        CreateMap<ProductVariantQuantityChangeReason, ProductVariantQuantityChangeReasonDto>();
        CreateMap<ProductVariantRedistributionHistory, ProductVariantRedistributionHistoryDto>()
            .ForMember(d => d.ProductVariantArticle, m => m.MapFrom(s => s.ProductVariant.Article))
            .ForMember(d => d.SourceOrderTtn, m => m.MapFrom(s => s.SourceOrderTtnSnapshot))
            .ForMember(d => d.TargetOrderTtn, m => m.MapFrom(s => s.TargetOrder != null ? s.TargetOrder.InternetDocumentIntDocNumber : null));
        CreateMap<AddOrderCommand, Order>();
        CreateMap<UpdateOrderCommand, Order>();

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<OrderInternetDocStatus, OrderInternetDocStatusDto>();
        CreateMap<OrderItemStatus, OrderItemStatusDto>();

        CreateMap<OrderCreationResult, OrderCreationResultDto>();
        CreateMap<OrderUpdateResult, OrderUpdateResultDto>();
        CreateMap<OrderDeleteResult, OrderDeleteResultDto>();

        CreateMap<AddOrderItemDto, OrderItem>();
        CreateMap<UpdateOrderItemDto, OrderItem>();
        CreateMap<AddNpContactPersonDto, NpContactPerson>();
        CreateMap<UpdateNpContactPersonDto, NpContactPerson>();
        CreateMap<AddOrderRecipientDto, OrderRecipient>();
        CreateMap<UpdateOrderRecipientDto, OrderRecipient>();
        CreateMap<AddNpOptionsSeatDto, NpOptionsSeat>();
        CreateMap<UpdateNpOptionsSeatDto, NpOptionsSeat>();
        CreateMap<AddOrderNpOptionsSeatDto, OrderNpOptionsSeat>();
        CreateMap<UpdateOrderNpOptionsSeatDto, OrderNpOptionsSeat>();

        CreateMap<NpContactPerson, NpContactPersonDto>();
        CreateMap<OrderRecipient, OrderRecipientDto>();

        CreateMap<OrderNpOptionsSeat, OrderNpOptionsSeatDto>()
            .ForMember(s => s.VolumetricWidth, m => m.MapFrom(s => s.NpOptionsSeat.VolumetricWidth))
            .ForMember(s => s.VolumetricLength, m => m.MapFrom(s => s.NpOptionsSeat.VolumetricLength))
            .ForMember(s => s.VolumetricHeight, m => m.MapFrom(s => s.NpOptionsSeat.VolumetricHeight))
            .ForMember(s => s.Weight, m => m.MapFrom(s => s.NpOptionsSeat.Weight));

        CreateMap<AddOrderAddressInfoDto, OrderAddressInfo>();
        CreateMap<UpdateOrderAddressInfoDto, OrderAddressInfo>();
        CreateMap<OrderAddressInfo, OrderAddressInfoDto>();

        CreateMap<PayerTypeDto, PayerType>().ReverseMap();
        CreateMap<PaymentMethodDto, PaymentMethod>().ReverseMap();
        CreateMap<SaleTypeDto, SaleType>().ReverseMap();
        CreateMap<DeliveryTypeDto, DeliveryType>().ReverseMap();
        CreateMap<OptionsSeatDto, OptionsSeat>().ReverseMap();

        CreateMap<SyncStatusDto, SyncStatus>().ReverseMap();
        CreateMap<SyncTypeDto, SyncType>().ReverseMap();

        CreateMap<NovaPoshtaSyncStatus, SyncRecordDto>();
        CreateMap<OrderStatusDto, OrderStatus>().ReverseMap();
        CreateMap<OrderSortDto, OrderSort>().ReverseMap();

        CreateMap<CityResponse, NpCityResponseDto>();
        CreateMap<GetCitiesResponse, GetNpCitiesResponseDto>();
        CreateMap<NpContactPersonResponse, NpContactPersonResponseDto>();
        CreateMap<NpWarehouse, NpWarehouseDto>();

        CreateMap<AddNpCityDto, NpCity>();
        CreateMap<UpdateNpCityDto, NpCity>();
        CreateMap<NpCity, GetNpCityDto>();
        CreateMap<AddCrmUserSettingsDto, CrmUserSettings>();
        CreateMap<UpdateCrmUserSettingsDto, CrmUserSettings>();

        CreateMap<SewingQueueItem, SewingQueueItemDto>();
        CreateMap<SubmitSewedReportItemDto, SewedReport>();

        CreateMap<FinanceCategoryType, FinanceCategoryTypeDto>().ReverseMap();
        CreateMap<FinanceCategory, FinanceCategoryDto>();

        CreateMap<FinanceEntry, FinanceExpenseDto>()
            .ForMember(s => s.CategoryName, m => m.MapFrom(s => s.Category.Name));

        CreateMap<PnlRowKind, PnlRowKindDto>();
        CreateMap<PnlRow, PnlRowDto>();
        CreateMap<PnlReport, PnlReportDto>();

        CreateMap<TopSellingProductItem, TopSellingProductDto>();
        CreateMap<TopSellingProductsResult, GetTopSellingProductsDto>()
            .ForMember(d => d.ResultsAmount, m => m.MapFrom(s => s.TotalCount));

        CreateMap<SewingStatisticsItem, SewingStatisticsItemDto>();
        CreateMap<SewingStatisticsResult, GetSewingStatisticsDto>()
            .ForMember(d => d.ResultsAmount, m => m.MapFrom(s => s.TotalCount));

        CreateMap<TelegramNotificationType, TelegramNotificationTypeDto>().ReverseMap();
        CreateMap<TelegramChatConfig, TelegramChatConfigDto>().ReverseMap();

        CreateMap<AddCollectionCommand, Collection>();
        CreateMap<Product, ProductInCollectionDto>();
        CreateMap<Collection, CollectionDto>()
            .ForMember(d => d.Products, m => m.MapFrom(s =>
                s.ProductCollections.Select(pc => pc.Product).OrderBy(p => p.Name).ToList()));

        CreateMap<DiscountType, DiscountTypeDto>().ReverseMap();
        CreateMap<DiscountValueType, DiscountValueTypeDto>().ReverseMap();
        CreateMap<Collection, DiscountCollectionDto>();
        CreateMap<Discount, DiscountDto>()
            .ForMember(d => d.Products, m => m.MapFrom(s => s.DiscountProducts.Select(dp => dp.Product).ToList()))
            .ForMember(d => d.Collections, m => m.MapFrom(s => s.DiscountCollections.Select(dc => dc.Collection).ToList()))
            .ForMember(d => d.BundleProducts, m => m.MapFrom(s => s.BundleProducts.Select(bp => bp.Product).ToList()));
        CreateMap<OrderPricingRequestDto, OrderPricingRequest>();
        CreateMap<ItemPricingResult, ItemPricingResultDto>();
        CreateMap<OrderPricingResult, OrderPricingResultDto>();
    }
}

