using calories_api.domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace calories_api.persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Meal> Meals { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Meal>().HasKey(e => e.Id);
        builder.Entity<Meal>().Property(e => e.Text).HasMaxLength(100);

        builder.Entity<User>().HasMany(e => e.Meals).WithOne().HasForeignKey(e => e.Id);

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
