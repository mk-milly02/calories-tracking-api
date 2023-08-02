using System.Text;
using calories_api.domain;
using calories_api.persistence;
using calories_api.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace calories_api.infrastructure;

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

    public static void RegisterMappingProfile(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
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
                policy.RequireRole(nameof(Roles.RegularUser));
            });
            
            options.AddPolicy("MustBeAUserManager", policy =>
            {
                policy.RequireRole(nameof(Roles.UserManager));
            });

            options.AddPolicy("MustBeAnAdministrator", policy => 
            {
                policy.RequireRole(nameof(Roles.Administrator));
            });

            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("default");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentity<User, Role>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
    }
}
