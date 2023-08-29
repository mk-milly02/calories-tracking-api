namespace calories_tracking.tests;

public class UserControllerTests
{
    #region GetUserById

    [Fact]
    public async Task GetUserById_WhenUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        Guid userId = Guid.NewGuid();

        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/users/{userId}");

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ReturnsBadRequestWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        Guid userId = Guid.NewGuid();

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users/{userId}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal($"User with id:{userId} does not exist.", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("manager")]
    [InlineData("admin")]
    public async Task GetUserById_WhenUserDoesExist_ReturnsOkWithUserProfile(string role)
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users/{user!.UserId}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync(role)}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? profile = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(profile);
        Assert.Equal(user.Email, profile.Email);
        Assert.NotNull(profile.Role);
        Assert.True(profile.Role.Equals(nameof(Roles.RegularUser)));
    }
    
    #endregion

    #region GetAllUsers
    
    [Fact]
    public async Task GetAllUsers_WhenModelIsInValid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        PaginationQueryParameters query = new() { Page = 0, Size = 0 };
    
        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/users?page={query.Page}&size={query.Size}");
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_WhenModelIsValid_ReturnsOkWithUserProfiles()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        PaginationQueryParameters query = new() { Page = 1, Size = 10 };
    
        // When
        HttpResponseMessage response = await httpClient.GetAsync($"api/users?page={query.Page}&size={query.Size}");
    
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IEnumerable<UserProfile>? profiles = JsonConvert.DeserializeObject<IEnumerable<UserProfile>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(profiles);
        Assert.Equal(3, profiles.Count());
        Assert.NotNull(profiles.First().Role);
    }

    #endregion

    #region CreateUser
    
    [Fact]
    public async Task CreateUser_WhenCurrentUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        CreateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Password = null,
            Role = null,
            Email = null
        };

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users", model);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WhenCreateUserRequestIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
         httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        CreateUserRequest model = new()
        {
            FirstName = "null",
            LastName = "null",
            Password = "null.12345",
            Role = "null",
            Email = "null@gmail.com"
        };

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.Equal(2, details.Errors.Count);
    }

    [Fact]
    public async Task CreateUser_WhenNewUserEmailAlreadyExists_ReturnsBadRequestWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        CreateUserRequest model = new()
        {
            FirstName = "null",
            LastName = "null",
            Email = "jp@theloft.com",
            Password = "Brim@lEy1803",
            Role = nameof(Roles.Administrator)
        };

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        UserRegistrationResponse? result = JsonConvert.DeserializeObject<UserRegistrationResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.Null(result.Profile);
        Assert.Equal("DuplicateEmail", result.Errors.First().Code);
    }

    [Fact]
    public async Task CreateUser_WhenNewUserIsSuccessfullyCreated_ReturnsCreated()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        CreateUserRequest model = new()
        {
            FirstName = "jim",
            LastName = "halpert",
            Email = "jim@athelead.com",
            Password = "loNg/$jim99@",
            Role = nameof(Roles.RegularUser)
        };

        // When
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/users", model);

        // Then
        string test = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion
    
    #region UpdateUser

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        UpdateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Username = null,
            ExpectedNumberOfCaloriesPerDay = 0,
            IsCaloriesDeficient = false
        };

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{Guid.NewGuid()}", model);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenUserRequestModelIsInvalid_ReturnsBadRequestWithErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        UpdateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Username = null,
            ExpectedNumberOfCaloriesPerDay = -1,
            IsCaloriesDeficient = false
        };

        Guid userId = Guid.NewGuid();

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{userId}", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsABadRequestWithUserActionResponse()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        UpdateUserRequest model = new()
        {
            FirstName = "nullandvoid",
            LastName = "nullandvoid",
            Username = "nulldnad",
            ExpectedNumberOfCaloriesPerDay = 100,
            IsCaloriesDeficient = false
        };

        Guid userId = Guid.NewGuid();

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{userId}", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        UserActionResponse? result = JsonConvert.DeserializeObject<UserActionResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateUser_WhenRepositorySuccessfullyUpdatesUser_ReturnsNoContent()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        UserProfile? user = await factory.GetUserProfileAsync();

        UpdateUserRequest model = new()
        {
            FirstName = "scott",
            LastName = "mcall",
            Username = "scott.mcall",
            ExpectedNumberOfCaloriesPerDay = 135,
            IsCaloriesDeficient = false
        };

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{user!.UserId}", model);

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
        
    #endregion

    #region DeleteUser
    
    [Fact]
    public async Task DeleteUser_WhenUserDoesNotExist_ReturnBadRequestWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");
    
        // When
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/users/{Guid.NewGuid()}");
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        UserActionResponse? result = JsonConvert.DeserializeObject<UserActionResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task DeleteUser_WhenRepositorySuccessfullyDeletesUser_ReturnNoContent()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        UserProfile? user = await factory.GetUserProfileAsync();

        // When
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/users/{user!.UserId}");
    
        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion
}
