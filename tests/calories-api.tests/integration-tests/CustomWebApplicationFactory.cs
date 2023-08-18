namespace calories_api.tests;

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
            
            //register application dbcontext which uses in memory database for testing purposes
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("CaloriesTrackerTestsDB"));

            ApplicationDbContext dbContext = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            RoleManager<Role> manager = services.BuildServiceProvider().GetRequiredService<RoleManager<Role>>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            await manager.CreateAsync(new() { Id = Guid.NewGuid(), Name = nameof(Roles.RegularUser), NormalizedName = nameof(Roles.RegularUser).ToUpper() });
            await manager.CreateAsync(new() { Id = Guid.NewGuid(), Name = nameof(Roles.UserManager), NormalizedName = nameof(Roles.UserManager).ToUpper() });
            await manager.CreateAsync(new() { Id = Guid.NewGuid(), Name = nameof(Roles.Administrator), NormalizedName = nameof(Roles.Administrator).ToUpper() });
        });
    }
}
