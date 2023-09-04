using calories_tracking.domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace calories_tracking.infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Meal> Meals { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Meal>().HasKey(x => x.Id);
        builder.Entity<Meal>().Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Entity<Meal>().Property(x => x.Text).HasMaxLength(100);

        builder.Entity<User>().Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Entity<User>().HasMany(x => x.Meals).WithOne(x => x.User).HasForeignKey(x => x.Id);

        builder.Entity<Role>().HasData
        (
            new Role
            {
                Id = Guid.NewGuid(),
                Name = domain.Roles.RegularUser.ToString(),
                NormalizedName = domain.Roles.RegularUser.ToString().ToUpper()
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = domain.Roles.UserManager.ToString(),
                NormalizedName = domain.Roles.UserManager.ToString().ToUpper()
            },
            new Role
            {
                Id = Guid.NewGuid(),
                Name = domain.Roles.Administrator.ToString(),
                NormalizedName = domain.Roles.Administrator.ToString().ToUpper()
            }
        );

        base.OnModelCreating(builder);
    }
}
