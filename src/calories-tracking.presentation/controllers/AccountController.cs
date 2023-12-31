﻿using calories_tracking.domain;
using calories_tracking.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace calories_tracking.presentation;

/// <summary>
/// Contains endpoints for managing user accounts.
/// </summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMealService _mealService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="userService">The user service providing account-related functionality.</param>
    /// <param name="mealService">The meal service providing meal-related functionality.</param> 
    public AccountController(IUserService userService, IMealService mealService)
    {
        _userService = userService;
        _mealService = mealService;
    }

    /// <summary>
    /// The user id of the current authenticated user.
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("The email address is already in use.");

        UserProfile? response = await _userService.RegisterAsync(request);

        return response is null
            ? BadRequest("User registeration was unsuccesful.")
            : Created($"api/users/{response.UserId}", response);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        AuthenticationResponse? response = await _userService.AuthenticateAsync(request);
        return response is null ? Unauthorized("The email or password is invalid.") : Ok(response);
    }

    /// <summary>
    /// Allows the current user to edit account details.
    /// </summary>
    /// <param name="request">Update user request.</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e regular users, user managers or administrators.</remarks>
    /// PUT: api/accounts/settings/profile
    [HttpPut("settings/profile")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EditUserProfile([FromBody] EditUserProfileRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _userService.EditUserProfileAsync(new(CurrentUserId!), request) 
            ? NoContent() 
            : BadRequest("Failed to update user profile.");
    }

    /// <summary>
    /// Allows the current user to view account details.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e regular users, user managers or administrators.</remarks>
    /// GET: api/accounts/profile
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> GetUserProfile()
    {
        return Ok(await _userService.GetUserByIdAsync(new(CurrentUserId!)));
    }

    /// <summary>
    /// Allows the current user to view meals.
    /// </summary>
    /// <param name="parameters">Query parameters for pagination and filtering</param>
    /// <returns>A list of meals added by the current user.</returns>
    /// <remarks>Allows authenticated users i.e regular users only.</remarks>
    /// GET: api/accounts/meals
    [HttpGet("meals")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageList<MealResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetMeals([FromQuery] QueryParameters parameters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return Ok(_mealService.GetMealsByUserAsync(new(CurrentUserId!), parameters));
    }

    /// <summary>
    /// Allows the current user to set their daily calorie limit.
    /// </summary>
    /// <param name="settings">User settings.</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e only regular users.</remarks>
    /// PUT: api/accounts/settings/dialy-calorie-limit
    [HttpPut("settings/daily-calorie-limit")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDailyCalorieLimit([FromBody] UserSettings settings)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return await _userService.SetDailyCalorieLimitAsync(new(CurrentUserId!), settings) 
            ? NoContent() 
            : BadRequest("Failed to set daily calorie limit.");
    }

    /// <summary>
    /// Checks if the current user has exceeded their daily calorie limit.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e only regular users.</remarks>
    /// PATCH: api/accounts/settings/daily-calorie-limit-exceeded
    [HttpPatch("settings/daily-calorie-limit-exceeded")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CheckIfDailyCalorieLimitIsExceeded()
    {
        return await _userService.HasExceededDailyCalorieLimitAsync(new(CurrentUserId!)) ? NoContent() : BadRequest();
    }
}
