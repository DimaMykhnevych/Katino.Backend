using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Katino.Domain.Entities;

namespace Katino.Infrastructure.Persistance.Context;

public class KatinoDbContext : IdentityDbContext<AppUser, UserRole, Guid>
{
    public KatinoDbContext(DbContextOptions<KatinoDbContext> options) : base(options)
    {

    }

    public DbSet<AppUser> AppUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}

