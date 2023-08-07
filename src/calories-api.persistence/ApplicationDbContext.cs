using calories_api.domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace calories_api.persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Meal> Meals { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Meal>().HasKey(e => e.Id);
        builder.Entity<Meal>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<Meal>().Property(e => e.Text).HasMaxLength(100);

        builder.Entity<User>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<User>().HasMany(e => e.Meals).WithOne().HasForeignKey(e => e.Id);

        Guid administratorRoleId = Guid.NewGuid();
        Guid administratorId = Guid.NewGuid();

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
                Id = administratorRoleId,
                Name = domain.Roles.Administrator.ToString(),
                NormalizedName = domain.Roles.Administrator.ToString().ToUpper()
            }
        );

        // Get the administrator's username, email and password from secrets.json
        IConfiguration configuration = new ConfigurationBuilder().AddUserSecrets("bc9385d3-59fe-460f-84f9-e815e12cdcbb").Build();

        string? username = configuration["Administrator:Username"] ?? throw new NullReferenceException("Username is not provided");
        string? email = configuration["Administrator:Email"] ?? throw new NullReferenceException("Email is not provided");
        string? password = configuration["Administrator:Password"] ?? throw new NullReferenceException("Password is not provided");

        // Create the administrator identity
        PasswordHasher<User> hasher = new();
        string passwordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(password, passwordSalt);

        User administrator = new()
        {
            Id = administratorId,
            FirstName = "Jonas",
            LastName = "Ababio",
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            PasswordSalt = passwordSalt
        };

        administrator.PasswordHash = hasher.HashPassword(administrator, saltedPassword);

        builder.Entity<User>().HasData(administrator);

        // Assign the administrator user to the administrator role
        builder.Entity<IdentityUserRole<Guid>>().HasData
        (
            new IdentityUserRole<Guid>
            {
                RoleId = administratorRoleId,
                UserId = administratorId
            }
        );

        base.OnModelCreating(builder);
    }
}
