namespace calories_api.tests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateDefaultClient(new Uri("https://localhost:7213"));
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();
    }

    [Theory]
    [InlineData(null, null, null, null, null)]
    [InlineData("test", "project", "test.project", "94bentley", "null")]
    public async Task SignUp_WhenUserRegistrationRequestModelIsNotValid_ReturnBadRequestWithErrors(string fName, string lName, string uName, string password, string email)
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

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register")
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
    public async Task SignUp_WhenEmailAlreadyExists_ReturnBadRequestWithErrorMessage(string email)
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

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register")
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
    public async Task SignUp_WhenRepositorySuccessfullyCreatesUserAccount_ReturnCreatedWithUserProfile()
    {
        // Given
        UserRegistrationRequest model = new() 
        {
            FirstName = "imagine",
            LastName = "dragons",
            Username = "imagine.dragons",
            Password = "selene",
            Email = "mercury@act.com"
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register")
        {
            Content = JsonContent.Create(model)
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("test", "project")]
    public async Task SignIn_WhenAuthenticationRequestModelIsNotValid_ReturnBadRequestWithErrors(string email, string password)
    {
        // Given
        AuthenticationRequest model = new() 
        {
            Password = password,
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/sign-in")
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
    public async Task SignIn_WhenSignInCredentialsAreInvalid_ReturnBadRequestWithErrorMessage(string email, string password)
    {
        // Given
        AuthenticationRequest model = new() 
        {
            Password = password,
            Email = email
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/sign-in")
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
    public async Task SignIn_WhenUserIsSuccessfullyAuthenticated_ReturnOkWithAuthenticationResponse()
    {
        // Given
        
        AuthenticationRequest model = new() 
        {
            Password = _configuration["Identity:RegularUser:Password"],
            Email = _configuration["Identity:RegularUser:Email"]
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/sign-in")
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
    public async Task RegisterRegularUser_WhenUserDoesNotHavePermission_ReturnsForbidden()
    {
        // Given
        CreateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Email = null
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register/user")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await SignIn("user")}");

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("manager")]
    [InlineData("administrator")]
    public async Task RegisterRegularUser_WhenCreateUserRequestModelIsNotValid_ReturnsBadRequestWithErrors(string role)
    {
        // Given
        CreateUserRequest model = new()
        {
            FirstName = null,
            LastName = null,
            Email = null
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register/user")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await SignIn(role)}");

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("manager")]
    [InlineData("administrator")]
    public async Task RegisterRegularUser_WhenUserEmailAlreadyExists_ReturnsBadRequestWithErrorMessage(string role)
    {
        // Given
        CreateUserRequest model = new()
        {
            FirstName = "null",
            LastName = "null",
            Email = "jp@theloft.com"
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register/user")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await SignIn(role)}");

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("User already exists", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task RegisterRegularUser_WhenUserIsSuccessfullyCreated_ReturnsUserProfile()
    {
        // Given
        CreateUserRequest model = new()
        {
            FirstName = "jim",
            LastName = "halpert",
            Email = "jim@athelead.com"
        };

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/register/user")
        {
            Content = JsonContent.Create(model)
        };
        request.Headers.Add("Authorization", $"Bearer {await SignIn("administrator")}");

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserProfile? user = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(user);
        Assert.Equal(model.FirstName, user.FirstName);
        Assert.NotNull(user.Role);
        Assert.Equal(nameof(Roles.RegularUser), user.Role);
    }

    private async Task<string> SignIn(string role)
    {
        AuthenticationRequest model = new();

        switch (role)
        {
            case "administrator":
                model.Email = _configuration["Identity:Administrator:Email"];
                model.Password = _configuration["Identity:Administrator:Password"];
                break;
            case "manager":
                model.Email = _configuration["Identity:UserManager:Email"];
                model.Password = _configuration["Identity:UserManager:Password"];
                break;
            case "user":
                model.Email = _configuration["Identity:RegularUser:Email"];
                model.Password = _configuration["Identity:RegularUser:Password"];
                break;
            default:
                break;
        }

        HttpRequestMessage request = new(HttpMethod.Post, "api/users/sign-in")
        {
            Content = JsonContent.Create(model)
        };

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        AuthenticationResponse authentication = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync())!;
        return authentication.Token!;
    }
}
