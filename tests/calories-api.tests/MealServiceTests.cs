using Microsoft.Extensions.Configuration;

namespace calories_api.tests;

public class MealServiceTests
{
    private IMealService? _mealService;
    private readonly Fixture _fixture;
    private readonly Mock<IMealRepository> _mealRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private List<Meal> _meals = new();

    public MealServiceTests()
    {
        _fixture = new();
        _mealRepositoryMock = new(MockBehavior.Loose);
        _configurationMock = new(MockBehavior.Loose);
        _httpClientMock = new(MockBehavior.Loose);
    }

    #region AddMealAsyncTests

    [Fact]
    public async void AddMealAsync_WhenRepositoryFailsToAddMeal_ReturnNull()
    {
        // Given
        Meal? meal = null;
        CreateMealRequest request = _fixture.Create<CreateMealRequest>();

        _mealRepositoryMock.Setup(m => m.Create(It.IsAny<Meal>())).ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);

        // When
        MealResponse? actual = await _mealService.AddMealAsync(request);
    
        // Then
        _mealRepositoryMock.Verify(m => m.Create(It.IsAny<Meal>()), Times.Once);

        Assert.Null(actual);
    }

    [Fact]
    public async void AddMealAsync_WhenRepositorySuccessfullyAddsMeal_ReturnAddedMeal()
    {
        // Given
        Meal meal = _fixture.Create<Meal>();
        CreateMealRequest request = new()
        {
            UserId = meal.UserId,
            Text = meal.Text,
            NumberOfCalories = meal.NumberOfCalories
        };
        MealResponse expected = meal.ToMealResponse();

        _mealRepositoryMock.Setup(m => m.Create(It.IsAny<Meal>()))
                           .Callback<Meal>(x => { meal.Created = x.Created; _meals.Add(meal); })
                           .ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);

        // When
        MealResponse? actual = await _mealService.AddMealAsync(request);
    
        // Then
        _mealRepositoryMock.Verify(m => m.Create(It.IsAny<Meal>()), Times.Once);
        
        Assert.NotNull(actual);
        Assert.True(_meals.Count is 1);
        Assert.Contains(meal, _meals);
        Assert.Equal(expected.Id, actual.Id);
    }

    #endregion

    #region UpdateMealAsyncTests

    [Fact]
    public async void UpdateMealAsync_WhenRepositoryFailsToUpdateMeal_ReturnNull()
    {
        // Given
        Meal? meal = null;
        UpdateMealRequest request = new()
        {
            Text = "Waakye with fish",
            NumberOfCalories = 233
        };
        
        _mealRepositoryMock.Setup(m => m.Update(It.IsAny<Meal>())).ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);
    
        // When
        MealResponse? actual = await _mealService.UpdateMealAsync(Guid.NewGuid(), request);
    
        // Then
        _mealRepositoryMock.Verify(m => m.Update(It.IsAny<Meal>()), Times.Once);

        Assert.Null(actual);
    }

    //when number of calories is provided and database successfully updates meal, it should return the updated meal
    [Fact]
    public async void UpdateMealAsync_WhenRepositorySuccessfullyUpdatesMeal_ReturnUpdatedMeal()
    {
        // Given
        Meal meal = _fixture.Create<Meal>();
        UpdateMealRequest request = new() { Text = "Waakye with fish", NumberOfCalories = meal.NumberOfCalories };
        MealResponse expected = new();

        _mealRepositoryMock.Setup(m => m.Update(It.IsAny<Meal>()))
                           .Callback<Meal>(x => { meal.Created = x.Created; meal.Text = x.Text; expected = meal.ToMealResponse(); })
                           .ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);
    
        // When
        MealResponse? actual = await _mealService.UpdateMealAsync(meal.Id, request);
    
        // Then
        _mealRepositoryMock.Verify(m => m.Update(It.IsAny<Meal>()), Times.Once());

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Text, actual.Text);
    }

    #endregion

    #region GetMealsAsyncTests

    [Fact]
    public async void GetMealsAsync_WhenSearchStringIsProvided_ReturnAllMealsThatMatch()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        QueryParameters query = new() { SeachString = "Text", PageSize = 4 };

        _mealRepositoryMock.Setup(m => m.RetrieveAll()).ReturnsAsync(_meals);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);

        // When
        IEnumerable<MealResponse> actual = await _mealService.GetMealsAsync(query);
    
        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAll(), Times.Once());

        Assert.Equal(_meals.Count - 1, actual.Count());
        Assert.Equal(_meals.First().Id, actual.First().Id);
        Assert.Contains("Text", actual.Last().Text);
    }

    [Fact]
    public async void GetMealsAsync_WhenSearchStringIsNotProvided_ReturnAllMeals()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(10).ToList();
        QueryParameters query = new() { PageSize = 5 };

        _mealRepositoryMock.Setup(m => m.RetrieveAll()).ReturnsAsync(_meals);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object);

        // When
        IEnumerable<MealResponse> actual = await _mealService.GetMealsAsync(query);
    
        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAll(), Times.Once());

        Assert.Equal(_meals.Count / 2, actual.Count());
        Assert.Equal(_meals.First().Id, actual.First().Id);
    }

    #endregion

    #region GetMealsByUserAsyncTests

    //when a search string is provided, it should return meals by user that match the search string

    //when a search string is not provided, it should return all user meals

    #endregion

    #region GetMealByIdAsyncTests

    //when id matches a corresponding meal in the database, the meal is returned

    //when the id doesn't match a corresponding meal in the database, it should return null

    #endregion

    #region GetTotalUserCaloriesForTodayAsyncTests

    //when the userId is valid and user has added meals today, then the total number of calories for the day should be returned

    //when the userId is valid and user has not added meals today, then the total number of calories for the day should be 0

    //when the userId is not valid, then there would be no number of calories for the day (0)

    #endregion

    #region RemoveMealAsyncTests

    //when database successfully removes the meal, return true

    //when database fails to remove the meal, return false

    //if the id is invalid, return null

    #endregion
}