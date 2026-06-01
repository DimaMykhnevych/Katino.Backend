using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Katino.Domain.Entities;
using Katino.Domain.Context;

namespace Katino.Infrastructure.Persistance.Context;

public class KatinoDbContext : IdentityDbContext<AppUser, UserRole, Guid>, IKatinoDbContext
{
    public KatinoDbContext(DbContextOptions<KatinoDbContext> options) : base(options)
    {

    }

    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductPhoto> ProductPhotos { get; set; }
    public DbSet<ProductVariantMeasurement> ProductVariantMeasurements { get; set; }
    public DbSet<MeasurementType> MeasurementTypes { get; set; }
    public DbSet<NpWarehouse> NpWarehouses { get; set; }
    public DbSet<NovaPoshtaSyncStatus> NovaPoshtaSyncStatuses { get; set; }
    public DbSet<NpCity> NpCities { get; set; }
    public DbSet<NpContactPerson> NpContactPersons { get; set; }
    public DbSet<CrmUserSettings> CrmUserSettings { get; set; }
    public DbSet<NpOptionsSeat> NpOptionsSeats { get; set; }
    public DbSet<OrderNpOptionsSeat> OrderNpOptionsSeats { get; set; }
    public DbSet<OrderRecipient> OrderRecipients { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderAddressInfo> OrderAddressInfo { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<SewingHistory> SewingHistory { get; set; }
    public DbSet<FinanceCategory> FinanceCategories { get; set; }
    public DbSet<FinanceEntry> FinanceEntries { get; set; }
    public DbSet<OrderTag> OrderTags { get; set; }
    public DbSet<OrderOrderTag> OrderOrderTags { get; set; }
    public DbSet<ProductVariantSewer> ProductVariantSewers { get; set; }
    public DbSet<TelegramSettings> TelegramSettings { get; set; }
    public DbSet<TelegramChatConfig> TelegramChatConfigs { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<ProductCollection> ProductCollections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Product configuration
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CostPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DropPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.WholesalePrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

            // Relationships

            entity.HasOne(d => d.Category)
                  .WithMany(p => p.Products)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.Article).IsRequired().HasMaxLength(50);

            // Relationships
            entity.HasOne(d => d.Color)
                   .WithMany(p => p.ProductVariants)
                   .HasForeignKey(d => d.ColorId)
                   .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Product)
                  .WithMany(p => p.Variants)
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Size)
                  .WithMany(p => p.ProductVariants)
                  .HasForeignKey(d => d.SizeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductVariant>().HasQueryFilter(pv => pv.DeletedAt == null);

        // Size configuration
        builder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Color configuration
        builder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.HexCode).HasMaxLength(7);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // ProductPhoto configuration
        builder.Entity<ProductPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhotoUrl).IsRequired();
            entity.Property(e => e.AltText).HasMaxLength(255);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // Relationships
            entity.HasOne(d => d.ProductVariant)
                  .WithMany(p => p.Photos)
                  .HasForeignKey(d => d.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductVariantMeasurement configuration
        builder.Entity<ProductVariantMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasMaxLength(512);

            // Unique constraint
            entity.HasIndex(e => new { e.ProductVariantId, e.MeasurementTypeId }).IsUnique();

            // Relationships
            entity.HasOne(d => d.ProductVariant)
                  .WithMany(p => p.Measurements)
                  .HasForeignKey(d => d.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.MeasurementType)
                  .WithMany(p => p.Measurements)
                  .HasForeignKey(d => d.MeasurementTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // MeasurementType configuration
        builder.Entity<MeasurementType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<NpWarehouse>(entity =>
        {
            entity.HasIndex(e => e.Ref).IsUnique();
            entity.HasIndex(e => e.CityRef);
            entity.HasIndex(e => e.Description);
        });

        builder.Entity<CrmUserSettings>()
            .HasOne(x => x.NpCity)
            .WithMany()
            .HasForeignKey(x => x.NpCityId);

        builder.Entity<CrmUserSettings>()
            .HasOne(x => x.NpWarehouse)
            .WithMany()
            .HasForeignKey(x => x.NpWarehouseId);

        builder.Entity<OrderRecipient>()
            .HasOne(x => x.NpContactPerson)
            .WithOne()
            .HasForeignKey<OrderRecipient>(x => x.NpContactPersonId);

        builder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.SenderNpWarehouse)
                .WithMany()
                .HasForeignKey(o => o.SenderNpWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.RecipientNpWarehouse)
                .WithMany()
                .HasForeignKey(o => o.RecipientNpWarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(o => o.SenderNpCity)
                .WithMany()
                .HasForeignKey(o => o.SenderNpCityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.RecipientNpCity)
                .WithMany()
                .HasForeignKey(o => o.RecipientNpCityId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(d => d.SenderContactPerson)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(d => d.SenderContactPersonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.OrderRecipient)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(d => d.OrderRecipientId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);

            entity.HasOne(d => d.AddressInfo)
                  .WithOne()
                  .HasForeignKey<OrderAddressInfo>(d => d.Id)
                  .IsRequired(false);
        });

        builder.Entity<SewingHistory>()
            .HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SewingHistory>()
            .HasOne(x => x.SewedByUser)
            .WithMany()
            .HasForeignKey(x => x.SewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FinanceCategory>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.SortOrder).IsRequired();

            b.HasMany(x => x.Entries)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.Type, x.IsActive });
        });

        builder.Entity<FinanceEntry>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            b.Property(x => x.SourceType).IsRequired();
            b.Property(x => x.IsLocked).IsRequired();

            b.Property(x => x.Comment).HasMaxLength(2000);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.ReversedEntry)
                .WithMany()
                .HasForeignKey(x => x.ReversedEntryId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.EntryDate);
            b.HasIndex(x => new { x.CategoryId, x.EntryDate });
            b.HasIndex(x => x.OrderId);
        });

        builder.Entity<OrderTag>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Type).IsRequired();
            b.Property(t => t.CanBeDeleted).IsRequired();
            b.Property(t => t.Value).HasMaxLength(500);
            b.Property(t => t.CreatedAt).IsRequired();
            b.HasIndex(t => t.Type);
        });

        builder.Entity<OrderOrderTag>(b =>
        {
            b.HasKey(ot => new { ot.OrderId, ot.OrderTagId });
            b.Property(ot => ot.CreatedAt).IsRequired();

            b.HasOne(ot => ot.Order)
                .WithMany(o => o.OrderTags)
                .HasForeignKey(ot => ot.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ot => ot.OrderTag)
                .WithMany(t => t.OrderOrderTags)
                .HasForeignKey(ot => ot.OrderTagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariantSewer>(b =>
        {
            b.HasKey(pvs => new { pvs.ProductVariantId, pvs.SewerId });

            b.HasOne(pvs => pvs.ProductVariant)
                .WithMany(pv => pv.Sewers)
                .HasForeignKey(pvs => pvs.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pvs => pvs.Sewer)
                .WithMany()
                .HasForeignKey(pvs => pvs.SewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TelegramSettings>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.BotToken).HasMaxLength(512);
        });

        builder.Entity<TelegramChatConfig>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ChatId).HasMaxLength(64).IsRequired();
            b.Property(x => x.ChatName).HasMaxLength(256);
            b.Property(x => x.NotificationType).IsRequired();
            b.Property(x => x.NotificationsEnabled).IsRequired();

            b.HasOne(x => x.TelegramSettings)
                .WithMany(x => x.ChatConfigs)
                .HasForeignKey(x => x.TelegramSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Collection>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        builder.Entity<ProductCollection>(b =>
        {
            b.HasKey(pc => new { pc.CollectionId, pc.ProductId });

            b.HasOne(pc => pc.Collection)
                .WithMany(c => c.ProductCollections)
                .HasForeignKey(pc => pc.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pc => pc.Product)
                .WithMany()
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(builder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is Product && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((Product)entity.Entity).CreatedAt = now;
            }

            ((Product)entity.Entity).UpdatedAt = now;
        }

        var variantEntities = ChangeTracker.Entries()
            .Where(x => x.Entity is ProductVariant && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in variantEntities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                ((ProductVariant)entity.Entity).CreatedAt = now;
            }

            ((ProductVariant)entity.Entity).UpdatedAt = now;
        }
    }
}

