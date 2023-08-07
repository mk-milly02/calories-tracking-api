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

    [HttpGet("{id}")]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? NotFound("Invalid user id") : Ok(user);
    }

    [HttpGet]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserProfile>))]
    public IActionResult GetAllUsers([FromQuery] PagingFilter query)
    {
        return Ok(_userService.GetAllUsers(query));
    }

    [AllowAnonymous]
    [HttpPost("sign-up")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SignUp([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserRegistrationResponse? user = await _userService.RegisterAsync(request);
        return user is null ? BadRequest("Repository failed to create user") : CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

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


    [HttpPost("register-user")]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> RegisterRegularUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateRegularUserAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    [HttpPost("register-manager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> RegisterUserManager([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateUserManagerAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    [HttpPost("register-admin")]
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

    [HttpPut("{id}")]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        UserProfile? user = await _userService.UpdateUserAsync(id, request);
        return user is null ? BadRequest("Repository failed to update user") : Ok(user);
    }

    [HttpPut("settings")]
    [Authorize(Policy = "MustBeARegularUser")]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettings settings)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        UserProfile? user = await _userService.UpdateUserSettingsAsync(settings);
        return user is null ? BadRequest("Repository failed to update user settings") : Ok(user);
    }

    [HttpPut("add-password/{id}")]
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

    [HttpDelete("{id}")]
    [Authorize(Policy = "MustBeAUserManager")]
    [Authorize(Policy = "MustBeAnAdministrator")]
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
