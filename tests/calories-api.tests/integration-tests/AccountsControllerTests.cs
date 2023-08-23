namespace calories_api.tests;

public class AccountsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AccountsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();
    }

    [Theory]
    [InlineData(null, null, null, null, null)]
    [InlineData("test", "project", "test.project", "94bentley", "null")]
    public async Task Register_WhenUserRegistrationRequestModelIsNotValid_ReturnsBadRequestWithErrors(string fName, string lName, string uName, string password, string email)
    {
        // Given
        UserRegistrationRequest model = new() 
        {
            FirstName = fName,
            LastName = lName,
            Username = uName,
            Password = password,
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/register")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("admin@calories-tracker.com")]
    [InlineData("jp@theloft.com")]
    [InlineData("manager@calories-tracker.com")]
    public async Task Register_WhenEmailAlreadyExists_ReturnsBadRequestWithErrorMessage(string email)
    {
        // Given
        UserRegistrationRequest model = new() 
        {
            FirstName = "fName",
            LastName = "lName",
            Username = "uName",
            Password = "password",
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/register")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User already exists", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_WhenRepositorySuccessfullyCreatesUserAccount_ReturnsOk()
    {
        // Given
        UserRegistrationRequest model = new() 
        {
            FirstName = "imagine",
            LastName = "dragons",
            Username = "imagine.dragons",
            Password = "selene/1234",
            Email = "mercury@act.com"
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/register")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("User registration was successful", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("test", "project")]
    public async Task Login_WhenAuthenticationRequestModelIsNotValid_ReturnsBadRequestWithErrors(string email, string password)
    {
        // Given
        AuthenticationRequest model = new() 
        {
            Password = password,
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/login")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("admin@calories-tracker.com", "1234")]
    [InlineData("jp@theloft.com", "qwerty")]
    [InlineData("manager@calories-tracker.com", "hTtp$://")]
    public async Task Login_WhenSignInCredentialsAreInvalid_ReturnsBadRequestWithErrorMessage(string email, string password)
    {
        // Given
        AuthenticationRequest model = new() 
        {
            Password = password,
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/login")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Invalid sign in credentials", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_WhenUserIsSuccessfullyAuthenticated_ReturnsOkWithAuthenticationResponse()
    {
        // Given
        
        AuthenticationRequest model = new() 
        {
            Password = _configuration["Identity:RegularUser:Password"],
            Email = _configuration["Identity:RegularUser:Email"]
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/accounts/login")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuthenticationResponse? authentication = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(authentication);
        Assert.NotNull(authentication.Token);
    }

    [Fact]
    public async Task EditUserProfile_WhenUserHasNotBeenAuthenticated_ReturnsForbidden()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = null, LastName = null, Username = null };

        HttpRequestMessage request = new(HttpMethod.Put, "api/accounts/settings/profile")
        {
            Content = JsonContent.Create(model)
        };
    
        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditUserProfile_WhenUpdateUserRequestIsInvalid_ReturnsBadRequestWithErrors()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = null, LastName = null, Username = null };

        HttpRequestMessage request = new(HttpMethod.Put, "api/accounts/settings/profile")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");
    
        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditUserProfile_WhenUserIsSuccessfullyUpdated_ReturnsOkWithUserProfile()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = "nick", LastName = "miller", Username = "julius.pepperwood" };
        UserProfile? profile = await _factory.GetUserProfileAsync();

        HttpRequestMessage request = new(HttpMethod.Put, "api/accounts/settings/profile")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");
    
        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? updatedProfile = JsonConvert.DeserializeObject<UserProfile?>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(updatedProfile);
        Assert.NotEqual("pepperwood", updatedProfile.LastName);
        Assert.Equal(profile!.Username, updatedProfile.Username);
    }

    [Fact]
    public async Task SetExpectedNumberOfCaloriesPerDay_WhenUserSettingsModelIsInvalid_ReturnsBadRequestWithErrors()
    {
        // Given
        UserSettings model = new() { ExpectedNumberOfCaloriesPerDay = -300 };

        HttpRequestMessage request = new(HttpMethod.Put, "api/accounts/settings/expected-calories-per-day")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");
    
        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetExpectedNumberOfCaloriesPerDay_WhenUserSettingsModelIsValid_ReturnsOkWithUserProfile()
    {
        // Given
        UserSettings model = new() { ExpectedNumberOfCaloriesPerDay = 300 };

        HttpRequestMessage request = new(HttpMethod.Put, "api/accounts/settings/expected-calories-per-day")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");
    
        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);
    
        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? profile = JsonConvert.DeserializeObject<UserProfile?>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(profile);
        Assert.True(profile.ExpectedNumberOfCaloriesPerDay.Equals(300));
    }
}
