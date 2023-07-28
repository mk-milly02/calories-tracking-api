using calories_api.domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace calories_api.persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CalorieEntry> Calories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CalorieEntry>().HasKey(e => e.EntryId);
        builder.Entity<CalorieEntry>().Property(e => e.Text).HasMaxLength(100);

        builder.Entity<User>().HasMany(e => e.Calories).WithOne().HasForeignKey(e => e.EntryId);

        builder.Entity<Role>().HasData
        (
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Regular User"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "User Manager"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "Administrator"
            }
        );

        base.OnModelCreating(builder);
    }
}
