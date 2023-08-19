namespace calories_api.tests;

public class UsersControllerUnitTests
{
    private UsersController? _controller;
    private readonly Mock<IUserService> _userServiceMock;

    public UsersControllerUnitTests()
    {
        _userServiceMock = new();
    }

    [Fact]
    public async void SignUp_WhenRepositoryFailsToAddUser_ReturnBadRequestWithErrorMessage()
    {
        // Given
        UserRegistrationRequest request = new() 
        {
            FirstName = "fName",
            LastName = "lName",
            Username = "uName",
            Password = "password",
            Email = "email"
        };

        UserRegistrationResponse? response = null;

        _userServiceMock.Setup(x => x.RegisterAsync(It.IsAny<UserRegistrationRequest>())).ReturnsAsync(response);
        _controller = new UsersController(_userServiceMock.Object);
    
        // When
        IActionResult actual = await _controller.SignUp(request);
    
        // Then
        BadRequestObjectResult result = Assert.IsAssignableFrom<BadRequestObjectResult>(actual);
        Assert.Equal("Repository failed to create user", result.Value);
    }

    [Fact]
    public async void RegisterRegularUser_WhenRepositoryFailsToAddUser_ReturnBadRequestWithErrorMessage()
    {
        // Given
        CreateUserRequest request = new() 
        {
            FirstName = "fName",
            LastName = "lName",
            Email = "email"
        };

        UserProfile? response = null;

        _userServiceMock.Setup(x => x.CreateRegularUserAsync(It.IsAny<CreateUserRequest>())).ReturnsAsync(response);
        _controller = new UsersController(_userServiceMock.Object);
    
        // When
        IActionResult actual = await _controller.RegisterRegularUser(request);
    
        // Then
        BadRequestObjectResult result = Assert.IsAssignableFrom<BadRequestObjectResult>(actual);
        Assert.Equal("Repository failed to create user", result.Value);
    }

    [Fact]
    public async Task UpdateUser_WhenRepositoryFailsToUpdateUser_ReturnsBadRequwstWithErroeMessage()
    {
        // Given
        UpdateUserRequest request = new() 
        {
            FirstName = "fName",
            LastName = "lName",
            Username = "username"
        };

        UserProfile? response = null;

        _userServiceMock.Setup(x => x.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>())).ReturnsAsync(response);
        _controller = new UsersController(_userServiceMock.Object);
    
        // When
        IActionResult actual = await _controller.UpdateUser(request, Guid.NewGuid());
    
        // Then
        BadRequestObjectResult result = Assert.IsAssignableFrom<BadRequestObjectResult>(actual);
        Assert.Equal("Repository failed to update user", result.Value);
    }
}
