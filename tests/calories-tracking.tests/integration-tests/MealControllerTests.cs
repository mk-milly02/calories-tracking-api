namespace calories_tracking.tests;

public class MealControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public MealControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateDefaultClient(new Uri("https://localhost:7213"));
    }

    [Fact]
    public async void GetAllMeals_WhenQueryParamerterModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        FiltrationQueryParameters query = new() { Page = -2, S = "", Size = -133 };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await _httpClient.GetAsync($"api/meals?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.Equal(2, details.Errors.Count);
    }

    [Fact]
    public async void GetAllMeals_WhenQueryParamerterModelIsValid_ReturnsOk()
    {
        // Given
        FiltrationQueryParameters query = new() { Page = 1, S = "", Size = 5 };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await _httpClient.GetAsync($"api/meals?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IEnumerable<MealResponse>? meals = JsonConvert.DeserializeObject<IEnumerable<MealResponse>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(meals);
        Assert.Empty(meals);
    }

    [Fact]
    public async void GetAllMealsByUser_WhenQueryParamerterModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        FiltrationQueryParameters query = new() { Page = -2, S = "", Size = -133 };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await _httpClient.GetAsync($"api/meals/:user/{Guid.NewGuid()}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.Equal(2, details.Errors.Count);
    }

    [Fact]
    public async void GetAllMealsByUser_WhenUserDoesNotExist_ReturnsBadRequestWithErrorMessage()
    {
        // Given
        FiltrationQueryParameters query = new() { Page = 1, S = "", Size = 5 };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("admin")}");
        Guid userId = Guid.NewGuid();

        // When
        HttpResponseMessage response = await _httpClient.GetAsync($"api/meals/:user/{userId}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal($"User with id:{userId} does not exist.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async void GetAllMealsByUser_WhenModelIsValidAndTheUserExists_ReturnsOk()
    {
        // Given
        FiltrationQueryParameters query = new() { Page = 1, S = "", Size = 5 };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("admin")}");

        UserProfile? user = await _factory.GetUserProfileAsync();
        await _factory.AddMealsAsync(user!.UserId);

        // When
        HttpResponseMessage response = await _httpClient.GetAsync($"api/meals/:user/{user.UserId}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IEnumerable<MealResponse>? meals = JsonConvert.DeserializeObject<IEnumerable<MealResponse>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(meals);
        Assert.Equal(5, meals.Count());
    }

    [Fact]
    public async void AddMeal_WhenCreateMealRequestModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        UserProfile? user = await _factory.GetUserProfileAsync();

        CreateMealRequest model = new() { UserId = user!.UserId, NumberOfCalories = -1, Text = null };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/meals", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.Equal(2, details.Errors.Count);
    }

    [Fact]
    public async void AddMeal_WhenRespositorySuccessfullyCreatesMeal_ReturnsCreated()
    {
        // Given
        UserProfile? user = await _factory.GetUserProfileAsync();

        CreateMealRequest model = new() { UserId = user!.UserId, NumberOfCalories = 1000, Text = "waakye" };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/meals", model);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
