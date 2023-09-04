namespace calories_tracking.tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseEnvironment("Development");

        builder.ConfigureServices(async services =>
        {
            //find the registered application dbcontext that uses PostgreSQL and remove it
            ServiceDescriptor? service = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (service is not null) services.Remove(service);

            //register application dbcontext which uses in memory database for testing
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("CaloriesTrackerTestDatabase"));

            ApplicationDbContext dbContext = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();

            //reset database to isolate tests
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        });
    }

    public async Task<string> GenerateUserTokenAsync(string role)
    {
        using IServiceScope services = Services.CreateScope();
        UserManager<User> userManager = services.ServiceProvider.GetRequiredService<UserManager<User>>();
        IConfiguration configuration = services.ServiceProvider.GetRequiredService<IConfiguration>();
        ITokenService tokenService = services.ServiceProvider.GetRequiredService<ITokenService>();

        User? user = new();

        switch (role)
        {
            case "regular":
                user = await userManager.FindByEmailAsync(configuration["Identity:RegularUser:Email"]!);
                break;
            case "manager":
                user = await userManager.FindByEmailAsync(configuration["Identity:UserManager:Email"]!);
                break;
            case "admin":
                user = await userManager.FindByEmailAsync(configuration["Identity:Administrator:Email"]!);
                break;
            default:
                break;
        }

        Assert.NotNull(user);
        AuthenticationResponse response = await tokenService.GenerateTokenAsync(user);
        Assert.NotNull(response.Token);
        return response.Token;
    }

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        using IServiceScope services = Services.CreateScope();
        UserManager<User> userManager = services.ServiceProvider.GetRequiredService<UserManager<User>>();
        IConfiguration configuration = services.ServiceProvider.GetRequiredService<IConfiguration>();

        User? user = await userManager.FindByEmailAsync(configuration["Identity:RegularUser:Email"]!);
        Assert.NotNull(user);
        return user.ToUserProfile(nameof(Roles.RegularUser));
    }

    public async Task AddMealsAsync(Guid userId)
    {
        using IServiceScope services = Services.CreateScope();
        IMealService mealService = services.ServiceProvider.GetRequiredService<IMealService>();

        Fixture fixture = new();
        IEnumerable<CreateMealRequest> meals = fixture.CreateMany<CreateMealRequest>(5);

        foreach (CreateMealRequest meal in meals)
        {
            meal.UserId = userId;
            await mealService.AddMealAsync(meal);
        }
    }
}
