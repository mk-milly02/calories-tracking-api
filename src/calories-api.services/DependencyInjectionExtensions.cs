using System.Text;
using calories_api.domain;
using calories_api.infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace calories_api.services;

public static class DependencyInjectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddHttpClient<IMealService, MealService>();

        services.AddScoped<IMealRepository, MealRepository>();
        
        services.AddTransient<IMealService, MealService>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IUserService, UserService>();
    }

    public static void AddTokenBasedAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        IConfiguration bearer = configuration.GetRequiredSection("Authentication:Schemes:Bearer")
                                ?? throw new NullReferenceException("Bearer scheme is not configured");
        
        services.AddAuthentication(options => 
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => 
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,

                ValidIssuer = bearer.GetValue<string>("issuer"),
                ValidAudience = bearer.GetValue<string>("audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearer.GetValue<string>("secret-key")!))
            };
        });
    }

    public static void AddPolicyBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("MustBeARegularUser", policy => 
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(new[] { nameof(Roles.RegularUser) });
            });

            options.AddPolicy("MustBeAnAdministrator", policy => 
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(new[] { nameof(Roles.Administrator) });
            });

            options.AddPolicy("MustBeAnAdministratorOrAUserManager", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(new[] { nameof(Roles.Administrator), nameof(Roles.UserManager) });
            });

            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(new[]
            {
                nameof(Roles.RegularUser), nameof(Roles.UserManager), nameof(Roles.Administrator)
            }).Build();
        });
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("default")
                                  ?? throw new NullReferenceException("Connection string is not configured");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentity<User, Role>(options => 
        {
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@ ";
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
    }

    public static async void SeedIdentityUsers(this IApplicationBuilder application)
    {
        using IServiceScope services = application.ApplicationServices.CreateScope();

        IConfiguration configuration = services.ServiceProvider.GetRequiredService<IConfiguration>();
        UserManager<User> userManager = services.ServiceProvider.GetRequiredService<UserManager<User>>();
        PasswordHasher<User> hasher = new();

        if (await userManager.FindByNameAsync("admin") is null)
        {
            // Create the administrator identity
            User administrator = configuration.GetSection("Identity:Administrator").Get<User>()
                                 ?? throw new NullReferenceException("Administrator details not provided");

            string password = configuration["Identity:Administrator:Password"]
                              ?? throw new NullReferenceException("Administrator password not provided");

            string passwordSalt = Security.GenerateSalt();
            string saltedPassword = Security.GenerateSaltedPassword(password, passwordSalt);
            administrator.PasswordSalt = passwordSalt;
            administrator.PasswordHash = hasher.HashPassword(administrator, saltedPassword);

            await userManager.CreateAsync(administrator);
            await userManager.AddToRoleAsync(administrator, nameof(Roles.Administrator));
        }

        if (await userManager.FindByNameAsync("mj.scott") is null)
        {
            // Create the user manager identity
            User manager = configuration.GetSection("Identity:UserManager").Get<User>()
                           ?? throw new NullReferenceException("User manager's details not provided");

            string mpassword = configuration["Identity:UserManager:Password"]
                              ?? throw new NullReferenceException("User manager's password not provided");

            string mpasswordSalt = Security.GenerateSalt();
            string msaltedPassword = Security.GenerateSaltedPassword(mpassword, mpasswordSalt);
            manager.PasswordSalt = mpasswordSalt;
            manager.PasswordHash = hasher.HashPassword(manager, msaltedPassword);

            await userManager.CreateAsync(manager);
            await userManager.AddToRoleAsync(manager, nameof(Roles.UserManager));
        }

        if (await userManager.FindByNameAsync("julius.pepperwood") is null)
        {
            // Create the regular user identity
            User user = configuration.GetSection("Identity:RegularUser").Get<User>()
                        ?? throw new NullReferenceException("Regular user's details not provided");

            string upassword = configuration["Identity:RegularUser:Password"]
                              ?? throw new NullReferenceException("Regular user's password not provided");

            string upasswordSalt = Security.GenerateSalt();
            string usaltedPassword = Security.GenerateSaltedPassword(upassword, upasswordSalt);
            user.PasswordSalt = upasswordSalt;
            user.PasswordHash = hasher.HashPassword(user, usaltedPassword);

            await userManager.CreateAsync(user);
            await userManager.AddToRoleAsync(user, nameof(Roles.RegularUser));
        }
    }
}
