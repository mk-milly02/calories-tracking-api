using calories_api.domain;
using calories_api.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_api.presentation;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets user by id
    /// </summary>
    /// <param name="id">User id</param>
    /// <returns>The user's profile</returns>
    /// api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpGet("{id}")]
    [Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? NotFound("Invalid user id") : Ok(user);
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <param name="query">Query parameter for pagination</param>
    /// <returns>All users</returns>
    /// api/users?page=1&size=10
    [HttpGet]
    [Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserProfile>))]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAllUsers([FromQuery] PagingFilter query)
    {
        return ModelState.IsValid ? Ok(await _userService.GetAllUsers(query)) : BadRequest(ModelState);
    }

    /// <summary>
    /// Allows users (anonymous) to create accounts
    /// </summary>
    /// <param name="request">User registration request</param>
    /// <returns>Newly create user profile</returns>
    /// api/users/register
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SignUp([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserRegistrationResponse? user = await _userService.RegisterAsync(request);
        return user is null ? BadRequest("Repository failed to create user") : CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Allows users (anonymous) to sign in
    /// </summary>
    /// <param name="request">User authentication request</param>
    /// <returns>Authentication response with a token and expiration time</returns>
    /// api/users/sign-in
    [AllowAnonymous]
    [HttpPost("sign-in")]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(200, Type = typeof(AuthenticationResponse))]
    public async Task<IActionResult> SignIn([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        AuthenticationResponse? response = await _userService.AuthenticateAsync(request);
        return response is null ? Unauthorized("Invalid sign in credentials") : Ok(response);
    }

    /// <summary>
    /// Allows and administrator or a user manager to create an account for a regular user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>A user profile</returns>
    /// api/users/register/user
    [HttpPost("register/user")]
    [Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> RegisterRegularUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateRegularUserAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    /// <summary>
    /// Allows the administrator to create an account for a user managers
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>A user profile</returns>
    /// api/users/register/manager
    [HttpPost("register/manager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> RegisterUserManager([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateUserManagerAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    [HttpPost("register/admin")] // api/users/register/admin
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> RegisterAdministrator([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateAdministratorAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    [HttpPut("{id}")] // api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        UserProfile? user = await _userService.UpdateUserAsync(id, request);
        return user is null ? BadRequest("Repository failed to update user") : Ok(user);
    }

    [HttpPut("settings")] // api/users/settings
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettings settings)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        UserProfile? user = await _userService.UpdateUserSettingsAsync(settings);
        return user is null ? BadRequest("Repository failed to update user settings") : Ok(user);
    }

    [HttpPut("password/{id}")] // api/users/password/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddPassword(Guid id, [FromBody] CreatePasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        bool? added = await _userService.CreatePasswordAsync(id, request);
        if (added is null) return NotFound("User does not exist");
        return added.Value? NoContent() : BadRequest("Repository failed to add password");
    }

    [HttpDelete("{id}")] // api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        bool? deleted = await _userService.RemoveUserAsync(id);
        if (deleted is null) return NotFound("User does not exist");
        return deleted.Value ? NoContent() : BadRequest("Repository failed to remove user");
    }
}
