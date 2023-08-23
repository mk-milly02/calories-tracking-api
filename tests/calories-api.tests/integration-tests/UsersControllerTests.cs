namespace calories_api.tests;

public class UsersControllerTests
{
    [Fact]
    public async Task CreateUser_WhenCurrentUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        CreateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Password = null,
            Role = null,
            Email = null
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WhenCreateCurrentUserRequestModelIsInvalid_ReturnsBadRequestWithErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        CreateUserRequest model = new()
        {
            FirstName = "null",
            LastName = "null",
            Password = "null.12345",
            Role = "null",
            Email = "null@gmail.com"
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WhenNewUserEmailAlreadyExists_ReturnsBadRequestWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        CreateUserRequest model = new()
        {
            FirstName = "null",
            LastName = "null",
            Email = "jp@theloft.com",
            Password = "12345678$",
            Role = nameof(Roles.Administrator)
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User already exists", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateUser_WhenNewUserIsSuccessfullyCreated_ReturnsUserProfile()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        CreateUserRequest model = new()
        {
            FirstName = "jim",
            LastName = "halpert",
            Email = "jim@athelead.com",
            Password = "long/$jim99@",
            Role = nameof(Roles.RegularUser)
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? user = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(user);
        Assert.Equal(model.FirstName, user.FirstName);
        Assert.NotNull(user.Role);
        Assert.Equal(nameof(Roles.RegularUser), user.Role);
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        Guid userId = Guid.NewGuid();

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users/{userId}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ReturnsNotFoundWithMessage()
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Invalid user id", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesExist_ReturnsOkWithUserProfile()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users/{user!.UserId}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("admin")}");

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

    [Fact]
    public async Task GetAllUsers_WhenModelIsInValid_ReturnsBadRequestWithErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        PagingFilter query = new() { Page = 0, Size = 0 };

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users?page={query.Page}&size={query.Size}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");
    
        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_WhenModelIsValid_ReturnsOkWithUserProfiles()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        PagingFilter query = new() { Page = 1, Size = 10 };

        HttpRequestMessage request = new(HttpMethod.Get, $"api/users?page={query.Page}&size={query.Size}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");
    
        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IEnumerable<UserProfile>? profiles = JsonConvert.DeserializeObject<IEnumerable<UserProfile>>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(profiles);
        Assert.Equal(3, profiles.Count());
        Assert.NotNull(profiles.First().Role);
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UpdateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Username = null,
            ExpectedNumberOfCaloriesPerDay = 0,
            IsCaloriesDeficient = false
        };

        HttpRequestMessage request = new(HttpMethod.Put, $"api/users/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenUserRequestModelIsInvalid_ReturnsBadRequestWithErrors()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UpdateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Username = null,
            ExpectedNumberOfCaloriesPerDay = -1,
            IsCaloriesDeficient = false
        };

        Guid userId = Guid.NewGuid();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{userId}", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ReturnsNotFoundWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UpdateUserRequest model = new()
        {
            FirstName = "nullandvoid",
            LastName = "nullandvoid",
            Username = "nulldnad",
            ExpectedNumberOfCaloriesPerDay = 100,
            IsCaloriesDeficient = false
        };

        Guid userId = Guid.NewGuid();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/users/{userId}", model);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User does not exist", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UpdateUser_WhenRepositorySuccessfullyUpdatesUser_ReturnsOkWithUpdatedUserProfile()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        UpdateUserRequest model = new()
        {
            FirstName = "scott",
            LastName = "mcall",
            Username = "scott.mcall",
            ExpectedNumberOfCaloriesPerDay = 135,
            IsCaloriesDeficient = false
        };

        HttpRequestMessage request = new(HttpMethod.Put, $"api/users/{user!.UserId}")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");

        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? profile = JsonConvert.DeserializeObject<UserProfile?>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(profile);
        Assert.Equal(135, profile.ExpectedNumberOfCaloriesPerDay);
        Assert.False(profile.LastName!.Equals(user.LastName));
    }

    [Fact]
    public async Task DeleteUser_WhenUserDoesNotExist_ReturnNotFoundWithErrorMessage()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        HttpRequestMessage request = new(HttpMethod.Delete, $"api/users/{Guid.NewGuid()}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");
    
        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User does not exist", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DeleteUser_WhenRepositorySuccessfullyDeletesUser_ReturnNoContent()
    {
        // Given
        using CustomWebApplicationFactory<Program> factory = new();
        HttpClient httpClient = factory.CreateDefaultClient(new Uri("https://localhost:7213"));

        UserProfile? user = await factory.GetUserProfileAsync();

        HttpRequestMessage request = new(HttpMethod.Delete, $"api/users/{user!.UserId}");
        request.Headers.Add("Authorization", $"Bearer {await factory.GenerateUserTokenAsync("manager")}");
    
        // When
        HttpResponseMessage response = await httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
