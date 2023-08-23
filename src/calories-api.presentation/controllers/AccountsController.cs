using System.Net;
using System.Security.Claims;
using calories_api.domain;
using calories_api.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_api.presentation;

/// <summary>
/// Contains endpoints for managing user accounts.
/// </summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsController"/> class.
    /// </summary>
    /// <param name="userService">The user service providing account-related functionality.</param> 
    public AccountsController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// The user id for current authenticated user.
    /// </summary>
    public string? CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Registers new user.
    /// </summary>
    /// <param name="request">The user registration information.</param>
    /// <returns></returns>
    /// <remarks>Allows anonymous users.</remarks>
    /// POST: api/accounts/register
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(((int)HttpStatusCode.OK))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserRegistrationResponse? user = await _userService.RegisterAsync(request);
        return user is null ? BadRequest("Repository failed to create user") : Ok("User registration was successful");
    }

    /// <summary>
    /// Authenticates a registered user.
    /// </summary>
    /// <param name="request">User authentication credentials.</param>
    /// <returns>Authentication response with a token and expiration time.</returns>
    /// <remarks>Allows anonymous users.</remarks>
    /// POST: api/accounts/login
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AuthenticationResponse))]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        AuthenticationResponse? response = await _userService.AuthenticateAsync(request);
        return response is null ? Unauthorized("Invalid sign in credentials") : Ok(response);
    }

    /// <summary>
    /// Allows users to edit their account details.
    /// </summary>
    /// <param name="request">Update user request.</param>
    /// <returns>An updated user profile.</returns>
    /// <remarks>Allows authenticated users i.e regular users, user managers or administrators.</remarks>
    /// PUT: api/accounts/settings/profile
    [HttpPut("settings/profile")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> EditUserProfile([FromBody] EditUserProfileRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        UserProfile? user = await _userService.EditUserProfileAsync(new(CurrentUserId!), request);
        return user is null ? BadRequest("Repository failed to edit user profile") : Ok(user);
    }

    /// <summary>
    /// Allows users to set their expected number of calories per day.
    /// </summary>
    /// <param name="settings">User settings.</param>
    /// <returns>An updated user profile.</returns>
    /// <remarks>Allows authenticated users i.e only regular users.</remarks>
    /// PUT: api/accounts/settings/expected-calories-per-day
    [HttpPut("settings/expected-calories-per-day")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> SetExpectedNumberOfCaloriesPerDay([FromBody] UserSettings settings)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        UserProfile? user = await _userService.SetExpectedNumberOfCaloriesPerDayAsync(new(CurrentUserId!), settings);
        return user is null ? BadRequest("Repository failed to set expected number of calories") : Ok(user);
    }

    /// <summary>
    /// Checks if the current user has exceeded their calorie limit for the day.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e only regular users.</remarks>
    /// GET: api/accounts/check-calories-deficiency
    [HttpGet("check-calorie-deficiency")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CheckForCalorieDeficiency()
    {
        await _userService.CheckForCalorieDeficiencyAsync(new(CurrentUserId!));
        return NoContent();
    }
}
