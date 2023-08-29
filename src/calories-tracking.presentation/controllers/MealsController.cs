using System.Net;
using calories_tracking.domain;
using calories_tracking.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_tracking.presentation;

/// <summary>
/// Contains endpoints to performing CRUD operations on meals.
/// </summary>
[ApiController]
[Route("api/meals")]
public class MealsController : ControllerBase
{
    private readonly IMealService _mealService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MealsController"/> class.
    /// </summary>
    /// <param name="mealService">The meal service providing meal-related functionality.</param>
    /// <param name="userService">The user service providing account-related functionality.</param>
    public MealsController(IMealService mealService, IUserService userService)
    {
        _mealService = mealService;
        _userService = userService;
    }

    /// <summary>
    /// Gets all meals.
    /// </summary>
    /// <param name="parameters">Query parameters for pagination and filtering</param>
    /// <returns>A list of all meals.</returns>
    /// <remarks>Allows authenticated users i.e administrators only.</remarks>
    /// GET: api/meals?s=toast/-page=1/-size=10
    [HttpGet]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<MealResponse>))]
    public async Task<IActionResult> GetAllMeals([FromQuery] QueryParameters parameters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _mealService.GetMealsAsync(parameters));
    }

    [HttpGet("user/{id}")] // api/meals/user/e48c46a6-2287-468b-8abc-9ae4ab75e7b6?s=toast&page=1&size=10
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<MealResponse>))]
    public async Task<IActionResult> GetAllMealsByUser(Guid userId, [FromQuery] QueryParameters parameters)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(userId);
        if (user is null) return BadRequest("Invalid user id");
        return Ok(await _mealService.GetMealsByUserAsync(userId, parameters));
    }

    [HttpGet("{id}")] // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(MealResponse))]
    public async Task<IActionResult> GetMeal(Guid id)
    {
        MealResponse? meal = await _mealService.GetMealByIdAsync(id);
        return meal is null ? BadRequest("Invalid request") : Ok(meal);
    }

    [HttpGet("calories/{id}")] // api/meals/calories/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(double))]
    public async Task<IActionResult> GetTotalCaloriesForToday(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? BadRequest("Invalid request") : Ok(await _mealService.GetTotalUserCaloriesForTodayAsync(id));
    }

    [HttpPost] // api/meals
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(201)]
    public async Task<IActionResult> AddMeal([FromBody] CreateMealRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        MealResponse? meal = await _mealService.AddMealAsync(request);
        return meal is null ? BadRequest("Repository failed to add meal") : CreatedAtAction(nameof(GetMeal), new { id = meal.Id }, meal);
    }

    [HttpPut("{id}")] // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(MealResponse))]
    public async Task<IActionResult> UpdateMeal(Guid id, [FromBody] UpdateMealRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        MealResponse? meal = await _mealService.UpdateMealAsync(id, request);
        return meal is null ? BadRequest("Repository failed to update meal") : Ok(meal);
    }

    [HttpDelete("{id}")] // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(MealResponse))]
    public async Task<IActionResult> DeleteMeal(Guid id)
    {
        bool? deleted = await _mealService.RemoveMealAsync(id);
        if (deleted is null) return BadRequest("Invalid request");
        return deleted.Value ? NoContent() : BadRequest("Repository failed to delete meal");
    }
}
