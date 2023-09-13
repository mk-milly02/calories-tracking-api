namespace calories_tracking.tests;

public class MealControllerTests
{
    [Fact]
    public async void GetAllMeals_WhenQueryParamerterModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = -2, S = "", Size = -133 };

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals?s={query.S}&page={query.Page}&size={query.Size}");

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
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = 1, S = "", Size = 5 };

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PageList<MealResponse>? meals = JsonConvert.DeserializeObject<PageList<MealResponse>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(meals);
        Assert.Empty(meals.Items);
    }

    [Fact]
    public async void GetAllMealsByUser_WhenQueryParamerterModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = -2, S = "", Size = -133 };

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/:user/{Guid.NewGuid()}?s={query.S}&page={query.Page}&size={query.Size}");

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
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = 1, S = "", Size = 5 };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");
        Guid userId = Guid.NewGuid();

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/:user/{userId}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal($"User with id:{userId} does not exist.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async void GetAllMealsByUser_WhenModelIsValidAndTheUserExists_ReturnsOk()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = 1, S = "", Size = 5 };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        UserProfile? user = await factory.GetUserProfileAsync();
        await factory.AddMealsAsync(user!.UserId);

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/:user/{user.UserId}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PageList<MealResponse>? meals = JsonConvert.DeserializeObject<PageList<MealResponse>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(meals);
        Assert.Equal(5, meals.Items.Count);
    }

    [Fact]
    public async void GetAllMealsByUser_WhenModelIsValidAndTheUserExistsAndSearchStringHasNoMatch_ReturnsOk()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        QueryParameters query = new() { Page = 1, S = "Kenkey", Size = 5 };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        UserProfile? user = await factory.GetUserProfileAsync();
        await factory.AddMealsAsync(user!.UserId);

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/:user/{user.UserId}?s={query.S}&page={query.Page}&size={query.Size}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PageList<MealResponse>? meals = JsonConvert.DeserializeObject<PageList<MealResponse>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(meals);
        Assert.Empty(meals.Items);
    }

    [Fact]
    public async void AddMeal_WhenCreateMealRequestModelIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        CreateMealRequest model = new() { UserId = user!.UserId, NumberOfCalories = -1, Text = null };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"api/meals", model);

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
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        CreateMealRequest model = new() { UserId = user!.UserId, NumberOfCalories = 1000, Text = "waakye" };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"api/meals", model);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async void GetMeal_MealIdDoesNotExist_ReturnsBadRequestWithAnErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        Guid mealId = Guid.NewGuid();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/{mealId}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal($"Meal with id:{mealId} does not exist.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async void GetMeal_MealIdExists_ReturnsOk()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();
        CreateMealRequest model = new() { UserId = user!.UserId, NumberOfCalories = 1000, Text = "waakye" };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage addMealResponse = await httpClient.PostAsJsonAsync("api/meals", model);
        Assert.Equal(HttpStatusCode.Created, addMealResponse.StatusCode);

        MealResponse? meal = JsonConvert.DeserializeObject<MealResponse?>(await addMealResponse.Content.ReadAsStringAsync());
        Assert.NotNull(meal);

        HttpResponseMessage response = await httpClient.GetAsync($"api/meals/{meal.Id}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        MealResponse? result = JsonConvert.DeserializeObject<MealResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.Equivalent(meal, result, true);
    }

    [Fact]
    public async Task UpdateMeal_WhenUpdateMealRequestIsInvalid_ReturnsBadRequestWithErrorMessages()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UpdateMealRequest model = new() { NumberOfCalories = -2, Text = null };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/meals/{Guid.NewGuid()}", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.Equal(2, details.Errors.Count);
    }

    [Fact]
    public async Task UpdateMeal_WhenMealDoesNotExist_ReturnsBadRequestWithErrorMessages()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UpdateMealRequest model = new() { NumberOfCalories = 2000, Text = "null" };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/meals/{Guid.NewGuid()}", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMeal_WhenRepositorySuccessfullyUpdatesMeal_ReturnsOk()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();
        CreateMealRequest create = new() { UserId = user!.UserId, NumberOfCalories = 1000, Text = "waakye" };

        UpdateMealRequest update = new() { NumberOfCalories = 2000, Text = "null" };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage addMealResponse = await httpClient.PostAsJsonAsync("api/meals", create);
        Assert.Equal(HttpStatusCode.Created, addMealResponse.StatusCode);

        HttpResponseMessage getMealsresponse = await httpClient.GetAsync("api/meals?page=1&size=10");
        Assert.Equal(HttpStatusCode.OK, getMealsresponse.StatusCode);
        PageList<MealResponse>? meals = JsonConvert.DeserializeObject<PageList<MealResponse>>(await getMealsresponse.Content.ReadAsStringAsync());
        Assert.NotNull(meals);

        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/meals/{meals.Items.First().Id}", update);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMeal_WhenRepositoryFailsToDeleteMeal_ReturnsBadRequest()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/meals/{Guid.NewGuid()}");

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMeal_WhenRepositorySuccessfullyDeletesMeal_ReturnsNoContent()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();
        CreateMealRequest create = new() { UserId = user!.UserId, NumberOfCalories = 1000, Text = "waakye" };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage addMealResponse = await httpClient.PostAsJsonAsync("api/meals", create);
        Assert.Equal(HttpStatusCode.Created, addMealResponse.StatusCode);

        HttpResponseMessage getMealsresponse = await httpClient.GetAsync("api/meals?page=1&size=10");
        Assert.Equal(HttpStatusCode.OK, getMealsresponse.StatusCode);
        PageList<MealResponse>? meals = JsonConvert.DeserializeObject<PageList<MealResponse>>(await getMealsresponse.Content.ReadAsStringAsync());
        Assert.NotNull(meals);

        HttpResponseMessage response = await httpClient.DeleteAsync($"api/meals/{meals.Items.First().Id}");

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
