namespace calories_api.tests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateDefaultClient(new Uri("https://localhost:7213"));
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
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };

        // When
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
