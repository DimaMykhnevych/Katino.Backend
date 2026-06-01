using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Katino.Infrastructure.Persistance.Context;

//public class KatinoDbContextFactory : IDesignTimeDbContextFactory<KatinoDbContext>
//{
//    public KatinoDbContext CreateDbContext(string[] args)
//    {
//        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
//            ?? throw new InvalidOperationException("ConnectionStrings__Default environment variable is not set.");

//        var optionsBuilder = new DbContextOptionsBuilder<KatinoDbContext>();
//        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

//        return new KatinoDbContext(optionsBuilder.Options);
//    }
//}
