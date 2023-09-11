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
public class MealController : ControllerBase
{
    private readonly IMealService _mealService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MealController"/> class.
    /// </summary>
    /// <param name="mealService">The meal service providing meal-related functionality.</param>
    /// <param name="userService">The user service providing account-related functionality.</param>
    public MealController(IMealService mealService, IUserService userService)
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
    // GET: api/meals?s=toast&page=1&size=10
    [HttpGet]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageList<MealResponse>))]
    public IActionResult GetAllMeals([FromQuery] QueryParameters parameters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(_mealService.GetMealsAsync(parameters));
    }

    /// <summary>
    /// Gets all meals by a particular user.
    /// </summary>
    /// <param name="id">The user's id</param>
    /// <param name="parameters">Query parameters for pagination and filtering</param>
    /// <returns>A list of meals by a user.</returns>
    /// <remarks>Allows authenticated users i.e administrators only.</remarks>
    // GET: api/meals/:user/e48c46a6-2287-468b-8abc-9ae4ab75e7b6?s=toast&page=1&size=10
    [HttpGet(":user/{id}")]
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MealResponse>))]
    public async Task<IActionResult> GetAllMealsByUser(Guid id, [FromQuery] QueryParameters parameters)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        UserProfile? user = await _userService.GetUserByIdAsync(id);
        if (user is null) return BadRequest($"User with id:{id} does not exist.");

        return Ok(_mealService.GetMealsByUserAsync(id, parameters));
    }

    /// <summary>
    /// Gets a meal.
    /// </summary>
    /// <param name="id">The meal's id</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e administrators only.</remarks>
    // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpGet("{id}")]
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MealResponse))]
    public async Task<IActionResult> GetMeal(Guid id)
    {
        MealResponse? meal = await _mealService.GetMealByIdAsync(id);
        return meal is null ? BadRequest($"Meal with id:{id} does not exist.") : Ok(meal);
    }

    /// <summary>
    /// Calculates total number of calories cosumed by the user today.
    /// </summary>
    /// <param name="id">User's id</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e regular users only.</remarks>
    // api/meals/calories/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpGet("calories/today/{id}")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    public async Task<IActionResult> GetTotalCaloriesForToday(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? BadRequest("Invalid request") : Ok(_mealService.GetTotalUserCaloriesForTodayAsync(id));
    }

    /// <summary>
    /// Adds a new meal.
    /// </summary>
    /// <param name="request">New meal</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e administrators and regular users.</remarks>
    // api/meals
    [HttpPost]
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(201)]
    public async Task<IActionResult> AddMeal([FromBody] CreateMealRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        MealResponse? meal = await _mealService.AddMealAsync(request);
        return meal is null ? BadRequest("Failed to add meal") : CreatedAtAction(nameof(GetMeal), new { id = meal.Id }, meal);
    }

    /// <summary>
    /// Updates a meal.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e administrators and regular users.</remarks>
    // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpPut("{id}")]
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MealResponse))]
    public async Task<IActionResult> UpdateMeal(Guid id, [FromBody] UpdateMealRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        bool result = await _mealService.UpdateMealAsync(id, request);
        return result ? Ok($"Meal with id:{id} was successfully updated.") : BadRequest("Failed to update meal.");
    }

    /// <summary>
    /// Deletes a meal.
    /// </summary>
    /// <param name="id">Meal's id</param>
    /// <returns></returns>
    /// <remarks>Allows authenticated users i.e administrators and regular users.</remarks>
    // api/meals/e48c46a6-2287-468b-8abc-9ae4ab75e7b6
    [HttpDelete("{id}")] 
    [Authorize(Policy = "MustBeAnAdministratorOrARegularUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMeal(Guid id)
    {
        bool deleted = await _mealService.RemoveMealAsync(id);
        return deleted ? NoContent() : BadRequest("Failed to delete meal");
    }
}
