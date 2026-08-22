using Katino.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Katino.Domain.Context;

public interface IKatinoDbContext
{
    DatabaseFacade Database { get; }

    DbSet<AppUser> AppUsers { get; set; }
    DbSet<Customer> Customers { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<ProductVariant> ProductVariants { get; set; }
    DbSet<Size> Sizes { get; set; }
    DbSet<Color> Colors { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<ProductPhoto> ProductPhotos { get; set; }
    DbSet<ProductVariantMeasurement> ProductVariantMeasurements { get; set; }
    DbSet<MeasurementType> MeasurementTypes { get; set; }

    DbSet<NpWarehouse> NpWarehouses { get; set; }
    DbSet<NovaPoshtaSyncStatus> NovaPoshtaSyncStatuses { get; set; }
    DbSet<NpCity> NpCities { get; set; }
    DbSet<NpContactPerson> NpContactPersons { get; set; }
    DbSet<CrmUserSettings> CrmUserSettings { get; set; }
    DbSet<NpOptionsSeat> NpOptionsSeats { get; set; }
    DbSet<OrderNpOptionsSeat> OrderNpOptionsSeats { get; set; }
    DbSet<OrderRecipient> OrderRecipients { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<OrderAddressInfo> OrderAddressInfo { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<SewingHistory> SewingHistory { get; set; }
    DbSet<FinanceCategory> FinanceCategories { get; set; }
    DbSet<FinanceEntry> FinanceEntries { get; set; }
    DbSet<OrderTag> OrderTags { get; set; }
    DbSet<OrderOrderTag> OrderOrderTags { get; set; }
    DbSet<ProductVariantSewer> ProductVariantSewers { get; set; }
    DbSet<TelegramSettings> TelegramSettings { get; set; }
    DbSet<TelegramChatConfig> TelegramChatConfigs { get; set; }
    DbSet<Collection> Collections { get; set; }
    DbSet<ProductCollection> ProductCollections { get; set; }
    DbSet<Discount> Discounts { get; set; }
    DbSet<DiscountProduct> DiscountProducts { get; set; }
    DbSet<DiscountCollection> DiscountCollections { get; set; }
    DbSet<DiscountBundleProduct> DiscountBundleProducts { get; set; }
}
