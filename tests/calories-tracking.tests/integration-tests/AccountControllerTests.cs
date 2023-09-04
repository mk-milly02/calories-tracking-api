namespace calories_tracking.tests;

public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AccountControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();
    }

    #region Register

    [Theory]
    [InlineData(null, null, null, null, null, null)]
    [InlineData("test", "project", "test.project", "abbentley", "abbentley", "test@tests.com")] //password complexity
    [InlineData("test", "project", "test.project", "94Ben@tley", "89", "test@tests.com")] //passwords match
    [InlineData("test", "project", "test.project", "94Ben@tley", "94Ben@tley", "testtests.com")] //email validity
    public async Task Register_WhenUserRegistrationRequestModelIsNotValid_ReturnsBadRequestWithValidationErrors(string fName, string lName, string uName, string password, string cpassword, string email)
    {
        // Given
        UserRegistrationRequest model = new()
        {
            FirstName = fName,
            LastName = lName,
            Username = uName,
            Password = password,
            ConfirmPassword = cpassword,
            Email = email
        };

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/register", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.True(details.Errors.Count > 0);
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
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            Email = email
        };

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/register", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        UserRegistrationResponse? result = JsonConvert.DeserializeObject<UserRegistrationResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.Null(result.Profile);
        Assert.Equal("DuplicateEmail", result.Errors.First().Code);
    }

    [Fact]
    public async Task Register_WhenUserIsSuccessfullyRegistered_ReturnsCreated()
    {
        // Given
        UserRegistrationRequest model = new()
        {
            FirstName = "imagine",
            LastName = "dragons",
            Username = "imagine.dragons",
            Password = "Pa$$word@1234!",
            ConfirmPassword = "Pa$$word@1234!",
            Email = "mercury@act.com"
        };

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/register", model);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region Login

    [Theory]
    [InlineData(null, null)]
    [InlineData("test", null)]
    public async Task Login_WhenAuthenticationRequestModelIsNotValid_ReturnsBadRequestWithValidationErrors(string email, string password)
    {
        // Given
        AuthenticationRequest model = new()
        {
            Password = password,
            Email = email
        };

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/login", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.True(details.Errors.Count is 2);
    }

    [Theory]
    [InlineData("admin@calories-tracker.com", "1234")]
    [InlineData("jp@theloft.com", "qwerty")]
    [InlineData("manager@calories-tracker.com", "hTtp$://")]
    public async Task Login_WhenLoginCredentialsAreInvalid_ReturnsBadRequestWithErrorMessage(string email, string password)
    {
        // Given
        AuthenticationRequest model = new()
        {
            Password = password,
            Email = email
        };

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/login", model);

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("The email or password is invalid.", await response.Content.ReadAsStringAsync());
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

        // When
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/accounts/login", model);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuthenticationResponse? authentication = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(authentication);
        Assert.NotNull(authentication.Token);
    }

    #endregion

    #region EditUserProfile

    [Fact]
    public async Task EditUserProfile_WhenUserHasNotBeenAuthenticated_ReturnsForbidden()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = null, LastName = null, Username = null };

        // When
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/accounts/settings/profile", model);

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditUserProfile_WhenUpdateUserRequestIsNotValid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = null, LastName = null, Username = null };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/accounts/settings/profile", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? details = JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(details);
        Assert.True(details.Errors.Count is 3);
    }

    [Fact]
    public async Task EditUserProfile_WhenUserProfileIsSuccessfullyUpdated_ReturnsNoContent()
    {
        // Given
        EditUserProfileRequest model = new() { FirstName = "nick", LastName = "miller", Username = "julius.pepperwood" };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/accounts/settings/profile", model);

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region SetDailyCalorieLimit

    [Fact]
    public async Task SetDailyCalorieLimit_WhenUserSettingsModelIsNotValid_ReturnsBadRequestWithValidationErrors()
    {
        // Given
        UserSettings model = new() { DailyCalorieLimit = -300 };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/accounts/settings/daily-calorie-limit", model);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetDailyCalorieLimit_WhenUserSettingsModelIsValid_ReturnsNoContent()
    {
        // Given
        UserSettings model = new() { DailyCalorieLimit = 300 };

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {await _factory.GenerateUserTokenAsync("regular")}");

        // When
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/accounts/settings/daily-calorie-limit", model);

        // Then
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion
}
