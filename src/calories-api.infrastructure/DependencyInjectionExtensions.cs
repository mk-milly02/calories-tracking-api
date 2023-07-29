using calories_api.services;
using Microsoft.Extensions.DependencyInjection;

namespace calories_api.infrastructure;

public static class DependencyInjectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddHttpClient<IMealService, MealService>();
    }

    public static void RegisterMappingProfile(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }
}
