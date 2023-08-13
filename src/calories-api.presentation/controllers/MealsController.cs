using calories_api.domain;
using calories_api.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace calories_api.presentation;

[ApiController]
[Route("api/meals")]
public class MealsController : ControllerBase
{
    private readonly IMealService _mealService;
    private readonly IUserService _userService;

    public MealsController(IMealService mealService, IUserService userService)
    {
        _mealService = mealService;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<MealResponse>))]
    public async Task<IActionResult> GetMeals([FromQuery] QueryParameters parameters)
    {
        return Ok(await _mealService.GetMealsAsync(parameters));
    }

    [HttpGet("{id}/{parameters}")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<MealResponse>))]
    public async Task<IActionResult> GetMealsByUser(Guid userId, QueryParameters parameters)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(userId);
        if (user is null) return BadRequest("Invalid user id");
        return Ok(await _mealService.GetMealsByUserAsync(userId, parameters));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(MealResponse))]
    public async Task<IActionResult> GetMeal(Guid id)
    {
        MealResponse? meal = await _mealService.GetMealByIdAsync(id);
        return meal is null ? BadRequest("Invalid request") : Ok(meal);
    }

    [HttpGet("total-calories/{id}")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    [Authorize(Policy = "MustBeARegularUser")]
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(double))]
    public async Task<IActionResult> GetTotalCaloriesForToday(Guid id)
    {
        UserProfile? user = await _userService.GetUserByIdAsync(id);
        return user is null ? BadRequest("Invalid request") : Ok(await _mealService.GetTotalUserCaloriesForTodayAsync(id));
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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
