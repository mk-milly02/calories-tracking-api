using calories_tracking.domain;
using calories_tracking.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_tracking.presentation;

/// <summary>
/// Contains endpoints used for executing CRUD operations on users.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService"></param>
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets user by id.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <returns>The user's profile.</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// GET: api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? BadRequest($"User with id:{id} does not exist.") : Ok(user);
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="query">Query parameter for pagination</param>
    /// <returns>A paginated list of all users.</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    // GET: api/users?page=1&size=10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageList<UserProfile>))]
    public async Task<IActionResult> GetAllUsers([FromQuery] QueryParameters query)
    {
        return ModelState.IsValid ? Ok(await _userService.GetAllUsersAsync(query)) : BadRequest(ModelState);
    }

    /// <summary>
    /// Creates new user account.
    /// </summary>
    /// <param name="request">New user information</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// POST: api/users
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("The email address is already in use.");

        UserProfile? response = await _userService.CreateUserAsync(request);
        return response is null
            ? BadRequest("Failed to create user.")
            : CreatedAtAction(nameof(GetUserById), new { id = response.UserId }, response);
    }

    /// <summary>
    /// Updates a user's account.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <param name="request">Updated user information</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// PUT: api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        bool response = await _userService.UpdateUserAsync(id, request);
        return response ? NoContent() : BadRequest("Failed to update user profile.");
    }

    /// <summary>
    /// Deletes a user's account.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return await _userService.RemoveUserAsync(id) 
            ? NoContent() 
            : BadRequest("Failed to delete user.");
    }
}
