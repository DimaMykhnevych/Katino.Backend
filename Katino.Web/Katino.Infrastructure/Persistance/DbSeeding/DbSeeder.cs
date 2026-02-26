using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Katino.Infrastructure.Persistance.DbSeeding;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KatinoDbContext>();

        var saveReqruied = false;
        if (!await db.FinanceCategories.AnyAsync(c => c.Name == FinanceCategoryNames.Revenue, ct))
        {
            db.FinanceCategories.AddRange(
                new FinanceCategory
                {
                    Id = Guid.NewGuid(),
                    Name = FinanceCategoryNames.Revenue,
                    Type = FinanceCategoryType.Income,
                    IsActive = true,
                    SortOrder = 0
                }
            );

            saveReqruied = true;
        }

        if (!await db.MeasurementTypes.AnyAsync(ct))
        {
            db.MeasurementTypes.AddRange(
                new MeasurementType
                {
                    Id = Guid.NewGuid(),
                    Name = "Загальні",
                    Unit = "см"
                }
            );

            saveReqruied = true;
        }

        if (saveReqruied)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}