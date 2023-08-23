using System.Net;
using calories_api.domain;
using calories_api.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_api.presentation;

/// <summary>
/// Contains endpoints used for executing CRUD operations on users.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = "MustBeAnAdministratorOrAUserManager")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService"></param>
    public UsersController(IUserService userService)
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
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? NotFound("Invalid user id") : Ok(user);
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="query">Query parameter for pagination</param>
    /// <returns>A paginated list of all users.</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// GET: api/users?page=1/-size=10
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UserProfile>))]
    public async Task<IActionResult> GetAllUsers([FromQuery] PagingFilter query)
    {
        return ModelState.IsValid ? Ok(await _userService.GetAllUsers(query)) : BadRequest(ModelState);
    }

    /// <summary>
    /// Creates new user account.
    /// </summary>
    /// <param name="request">New user information</param>
    /// <returns>A newly created user profile.</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// POST: api/users
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(200, Type = typeof(UserProfile))]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _userService.EmailAlreadyExistsAsync(request.Email!)) return BadRequest("User already exists");

        UserProfile? userProfile = await _userService.CreateUserAsync(request);
        return userProfile is null ? BadRequest("Repository failed to create user") : Ok(userProfile);
    }

    /// <summary>
    /// Updates a user's account.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <param name="request">Updated user information</param>
    /// <returns>An updated user profile.</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// PUT: api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(UserProfile))]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        UserProfile? existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser is null) return NotFound("User does not exist");

        UserProfile? updatedUser = await _userService.UpdateUserAsync(id, request);
        return updatedUser is null ? BadRequest("Repository failed to update user") : Ok(updatedUser);
    }

    /// <summary>
    /// Deletes a user's account.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <returns>Void</returns>
    /// <remarks>Allows authenticated users i.e user managers and administrators.</remarks>
    /// api/users/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        bool? deleted = await _userService.RemoveUserAsync(id);
        if (deleted is null) return NotFound("User does not exist");
        return deleted.Value ? NoContent() : BadRequest("Repository failed to remove user");
    }
}
