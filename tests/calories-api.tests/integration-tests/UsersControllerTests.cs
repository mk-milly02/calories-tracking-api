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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
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
            Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuthenticationResponse? authentication = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(authentication);
        Assert.NotNull(authentication.Token);
    }
}
