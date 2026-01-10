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
            entity.Property(e => e.Value).HasColumnType("decimal(5,2)");

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
            .WithOne()
            .HasForeignKey<CrmUserSettings>(x => x.NpCityId);

        builder.Entity<CrmUserSettings>()
            .HasOne(x => x.NpWarehouse)
            .WithOne()
            .HasForeignKey<CrmUserSettings>(x => x.NpWarehouseId);

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
                  .OnDelete(DeleteBehavior.Cascade);

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

