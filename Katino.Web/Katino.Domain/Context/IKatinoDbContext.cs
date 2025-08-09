using Katino.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Katino.Domain.Context;

public interface IKatinoDbContext
{
    DbSet<AppUser> AppUsers { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<ProductVariant> ProductVariants { get; set; }
    DbSet<Size> Sizes { get; set; }
    DbSet<Color> Colors { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<ProductPhoto> ProductPhotos { get; set; }
    DbSet<ProductVariantMeasurement> ProductVariantMeasurements { get; set; }
    DbSet<MeasurementType> MeasurementTypes { get; set; }
}
