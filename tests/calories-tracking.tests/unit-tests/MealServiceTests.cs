namespace calories_tracking.tests;

public class MealServiceTests
{
    private IMealService? _mealService;
    private readonly Fixture _fixture;
    private readonly Mock<IMealRepository> _mealRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly ILogger<MealService> _logger;
    private List<Meal> _meals = new();

    public MealServiceTests()
    {
        _fixture = new();

        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mealRepositoryMock = new(MockBehavior.Loose);
        _configurationMock = new(MockBehavior.Loose);
        _httpClientMock = new(MockBehavior.Loose);
        _logger = new NullLogger<MealService>();
    }

    #region AddMealAsyncTests

    [Fact]
    public async void AddMealAsync_WhenRepositoryFailsToAddMeal_ReturnNull()
    {
        // Given
        CreateMealRequest request = _fixture.Create<CreateMealRequest>();

        _mealRepositoryMock.Setup(m => m.CreateAsync(It.IsAny<Meal>())).ThrowsAsync(new InvalidOperationException());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        MealResponse? actual = await _mealService.AddMealAsync(request);

        // Then
        _mealRepositoryMock.Verify(m => m.CreateAsync(It.IsAny<Meal>()), Times.Once);

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

        _mealRepositoryMock.Setup(m => m.CreateAsync(It.IsAny<Meal>()))
                           .Callback<Meal>(x => { meal.Created = x.Created; _meals.Add(meal); })
                           .ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        MealResponse? actual = await _mealService.AddMealAsync(request);

        // Then
        _mealRepositoryMock.Verify(m => m.CreateAsync(It.IsAny<Meal>()), Times.Once);

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
        Meal meal = _fixture.Create<Meal>();
        UpdateMealRequest request = new()
        {
            Text = "Waakye with fish",
            NumberOfCalories = 233
        };

        _mealRepositoryMock.Setup(m => m.UpdateAsync(It.IsAny<Meal>())).ThrowsAsync(new InvalidOperationException());
        _mealRepositoryMock.Setup(m => m.RetrieveAsync(It.IsAny<Guid>())).ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        bool? actual = await _mealService.UpdateMealAsync(Guid.NewGuid(), request);

        // Then
        _mealRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<Meal>()), Times.Once);

        Assert.False(actual);
    }

    //when number of calories is provided and database successfully updates meal, it should return the updated meal
    [Fact]
    public async void UpdateMealAsync_WhenRepositorySuccessfullyUpdatesMeal_ReturnUpdatedMeal()
    {
        // Given
        Meal meal = _fixture.Create<Meal>();
        UpdateMealRequest request = new() { Text = "Waakye with fish", NumberOfCalories = 1000 };

        _mealRepositoryMock.Setup(m => m.UpdateAsync(It.IsAny<Meal>()));
        _mealRepositoryMock.Setup(m => m.RetrieveAsync(It.IsAny<Guid>())).ReturnsAsync(meal);
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        bool? actual = await _mealService.UpdateMealAsync(Guid.NewGuid(), request);

        // Then
        _mealRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<Meal>()), Times.Once());

        Assert.True(actual);
    }

    #endregion

    #region GetMealsAsyncTests

    [Fact]
    public void GetMealsAsync_WhenSearchStringIsProvided_ReturnAllMealsThatMatch()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        QueryParameters query = new() { S = "Text", Size = 4 };

        _mealRepositoryMock.Setup(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()))
            .Returns(_meals.Where(x => x.Text!.Contains(query.S, StringComparison.OrdinalIgnoreCase)).AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        PageList<MealResponse> actual = _mealService.GetMealsAsync(query);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Once());
        _mealRepositoryMock.Verify(m => m.RetrieveAll(), Times.Never());

        Assert.Equal(_meals.Count - 1, actual.Items.Count);
        Assert.Equal(_meals.First().Id, actual.Items.First().Id);
        Assert.Contains("Text", actual.Items.Last().Text);
    }

    [Fact]
    public void GetMealsAsync_WhenSearchStringIsNotProvided_ReturnAllMeals()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(10).ToList();
        QueryParameters query = new() { Size = 5 };

        _mealRepositoryMock.Setup(m => m.RetrieveAll()).Returns(_meals.AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        PageList<MealResponse> actual = _mealService.GetMealsAsync(query);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Never());
        _mealRepositoryMock.Verify(m => m.RetrieveAll(), Times.Once());

        Assert.Equal(_meals.First().Id, actual.Items.First().Id);
    }

    #endregion

    #region GetMealsByUserAsyncTests

    [Fact]
    public void GetMealsByUserAsync_WhenSearchStringIsProvided_ReturnsMealsThatMatch()
    {
        // Given
        Guid userId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6");
        List<Meal> mealsByUser = new()
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "French toast",
                NumberOfCalories = 400
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Rice and beans",
                NumberOfCalories = 400
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Toasted bread with orange juice",
                NumberOfCalories = 400
            }
        };
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        _meals.AddRange(mealsByUser);
        QueryParameters query = new() { Size = 5, S = "toast" };

        _mealRepositoryMock.Setup(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()))
            .Returns(_meals.Where(x => x.UserId.Equals(userId) && x.Text!.Contains(query.S, StringComparison.OrdinalIgnoreCase)).AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        PageList<MealResponse> actual = _mealService.GetMealsByUserAsync(userId, query);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Once);

        Assert.NotNull(actual);
        Assert.Equal(2, actual.Items.Count);
    }

    [Fact]
    public void GetMealsByUserAsync_WhenSearchStringIsNotProvided_ReturnsAllMeals()
    {
        // Given
        Guid userId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6");
        List<Meal> mealsByUser = new()
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "French toast",
                NumberOfCalories = 400
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Rice and beans",
                NumberOfCalories = 400
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Toasted bread with orange juice",
                NumberOfCalories = 400
            }
        };
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        _meals.AddRange(mealsByUser);
        QueryParameters query = new() { Size = 5, };

        _mealRepositoryMock.Setup(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>())).Returns(_meals.Where(x => x.UserId == userId).AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        PageList<MealResponse> actual = _mealService.GetMealsByUserAsync(userId, query);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Once);

        Assert.NotNull(actual);
        Assert.True(actual.Items.Count is 3);
    }

    #endregion

    #region GetMealByIdAsyncTests

    [Fact]
    public async void GetMealByIdAsync_WhenMealExists_ReturnMeal()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        Guid id = _meals.First().Id;
        MealResponse expected = _meals.First().ToMealResponse();

        _mealRepositoryMock.Setup(m => m.RetrieveAsync(It.IsAny<Guid>())).ReturnsAsync(_meals.SingleOrDefault(x => x.Id == id));
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        MealResponse? actual = await _mealService.GetMealByIdAsync(id);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAsync(It.IsAny<Guid>()), Times.Once);

        Assert.NotNull(actual);
        Assert.Equal(expected.Text, actual.Text);
    }

    [Fact]
    public async void GetMealByIdAsync_WhenMealDoesNotExists_ReturnNull()
    {
        // Given
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        Guid id = Guid.NewGuid();

        _mealRepositoryMock.Setup(m => m.RetrieveAsync(It.IsAny<Guid>())).ReturnsAsync(_meals.SingleOrDefault(x => x.Id == id));
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        MealResponse? actual = await _mealService.GetMealByIdAsync(id);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAsync(It.IsAny<Guid>()), Times.Once);

        Assert.Null(actual);
    }

    #endregion

    #region GetTotalUserCaloriesForTodayAsyncTests

    [Fact]
    public void GetTotalUserCaloriesForTodayAsync_WhenMealsHaveBeenAddedToday_ReturnTotalCaloriesForToday()
    {
        // Given
        Guid userId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6");
        List<Meal> mealsByUser = new()
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "French toast",
                NumberOfCalories = 400
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Rice and beans",
                NumberOfCalories = 700
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Toasted bread with orange juice",
                NumberOfCalories = 1000
            }
        };
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        _meals.AddRange(mealsByUser);
        double expected = 2100;

        _mealRepositoryMock.Setup(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>())).Returns(_meals.Where(x => x.UserId == userId).AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        double actual = _mealService.GetTotalUserCaloriesForTodayAsync(userId);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Once());

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetTotalUserCaloriesForTodayAsync_WhenNoMealsHaveBeenAddedToday_ReturnZero()
    {
        // Given
        Guid userId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6");
        List<Meal> mealsByUser = new()
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "French toast",
                NumberOfCalories = 400,
                Created = DateTime.MinValue
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Rice and beans",
                NumberOfCalories = 700,
                Created = DateTime.MinValue
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = new("e48c46a6-2287-468b-8abc-9ae4ab75e7b6"),
                Text = "Toasted bread with orange juice",
                NumberOfCalories = 1000,
                Created = DateTime.MinValue
            }
        };
        _meals = _fixture.CreateMany<Meal>(5).ToList();
        _meals.AddRange(mealsByUser);
        double expected = 0;

        _mealRepositoryMock.Setup(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>())).Returns(_meals.Where(x => x.UserId.Equals(userId) && x.Created.Date.Equals(DateTime.Today)).AsQueryable());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        double actual = _mealService.GetTotalUserCaloriesForTodayAsync(userId);

        // Then
        _mealRepositoryMock.Verify(m => m.RetrieveAllByCondition(It.IsAny<Expression<Func<Meal, bool>>>()), Times.Once());

        Assert.Equal(expected, actual);
    }

    #endregion

    #region RemoveMealAsyncTests

    [Fact]
    public async void RemoveMealAsync_WhenRepositorySuccessfullyRemovesMeal_ReturnsTrue()
    {
        // Given
        _mealRepositoryMock.Setup(m => m.DeleteAsync(It.IsAny<Guid>()));
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        bool actual = await _mealService.RemoveMealAsync(Guid.NewGuid());

        // Then
        _mealRepositoryMock.Verify(m => m.DeleteAsync(It.IsAny<Guid>()), Times.Once());

        Assert.True(actual);
    }

    [Fact]
    public async void RemoveMealAsync_WhenRepositoryFailsToRemoveMeal_ReturnsFalse()
    {
        // Given
        _mealRepositoryMock.Setup(m => m.DeleteAsync(It.IsAny<Guid>())).ThrowsAsync(new InvalidOperationException());
        _mealService = new MealService(_mealRepositoryMock.Object, _httpClientMock.Object, _configurationMock.Object, _logger);

        // When
        bool actual = await _mealService.RemoveMealAsync(Guid.NewGuid());

        // Then
        _mealRepositoryMock.Verify(m => m.DeleteAsync(It.IsAny<Guid>()), Times.Once());

        Assert.False(actual);
    }

    #endregion
}