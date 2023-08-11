using System.Text;
using calories_api.domain;
using calories_api.infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                                ?? throw new InvalidOperationException("Bearer scheme is not configured");
        
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
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(nameof(Roles.RegularUser));
            });
            
            options.AddPolicy("MustBeAUserManager", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(nameof(Roles.UserManager));
            });

            options.AddPolicy("MustBeAnAdministrator", policy => 
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole(nameof(Roles.Administrator));
            });
        });
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("default");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentity<User, Role>(options => 
        {
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@ ";
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
    }

    public static async void SeedAdministrator(this IApplicationBuilder application)
    {
        using IServiceScope services = application.ApplicationServices.CreateScope();

        IConfiguration configuration = services.ServiceProvider.GetRequiredService<IConfiguration>();
        UserManager<User> userManager = services.ServiceProvider.GetRequiredService<UserManager<User>>();

        if (await userManager.FindByNameAsync("admin") is not null) return;

        // Create the administrator identity
        User administrator = configuration.GetSection("Administrator").Get<User>()
                             ?? throw new NullReferenceException("Administrator details not provided");
                             
        string password = configuration["Administrator:Password"]
                          ?? throw new NullReferenceException("Administrator password not provided");

        PasswordHasher<User> hasher = new();
        string passwordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(password, passwordSalt);
        administrator.PasswordSalt = passwordSalt;
        administrator.PasswordHash = hasher.HashPassword(administrator, saltedPassword);

        await userManager.CreateAsync(administrator);
        await userManager.AddToRoleAsync(administrator, nameof(Roles.Administrator));
    }
}
