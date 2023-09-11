using calories_tracking.domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace calories_tracking.services;

public class UserService : IUserService
{
    private readonly IMealService _mealService;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(IMealService mealService, UserManager<User> userManager, ITokenService tokenService, ILogger<UserService> logger)
    {
        _mealService = mealService;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<bool> RemoveUserAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        try
        {
            await _userManager.DeleteAsync(user!);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles removing user with id:{id}", userId);
            return false;
        }
    }

    public async Task<PageList<UserProfile>> GetAllUsersAsync(QueryParameters query)
    {
        List<UserProfile> profiles = new();
        IEnumerable<User> users = _userManager.Users.ToList();

        foreach (User user in users)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);
            UserProfile profile = user.ToUserProfile(roles.First());
            profiles.Add(profile);
        }

        return PageList<UserProfile>.Create(profiles, query.Page, query.Size);
    }

    public async Task<UserProfile?> GetUserByIdAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is not null)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);
            UserProfile profile = user.ToUserProfile(roles.First());
            return profile;
        }

        return null;
    }

    public async Task<bool> EditUserProfileAsync(Guid userId, EditUserProfileRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;

        try
        {
            await _userManager.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles editing user with id:{id}", userId);
            return false;
        }
    }

    public async Task<bool> SetDailyCalorieLimitAsync(Guid userId, UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.DailyCalorieLimit = settings.DailyCalorieLimit;

        try
        {
            await _userManager.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles setting daily calorie limit of user with id:{id}", userId);
            return false;
        }
    }

    public async Task<bool> HasExceededDailyCalorieLimitAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        double totalUserCaloriesForToday = _mealService.GetTotalUserCaloriesForTodayAsync(userId);
        user!.HasExceededDailyCalorieLimit = totalUserCaloriesForToday < user.DailyCalorieLimit;

        try
        {
            await _userManager.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles updating HasExceededDailyCalorieLimit of user with id:{id}", userId);
            return false;
        }
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        User? user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null) { return null; }
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt!);
        return await _userManager.CheckPasswordAsync(user, saltedPassword) ? await _tokenService.GenerateTokenAsync(user) : null;
    }

    public async Task<UserProfile?> RegisterAsync(UserRegistrationRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);

        try
        {
            await _userManager.CreateAsync(user, saltedPassword);
            await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
            return user.ToUserProfile(nameof(Roles.RegularUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles registering user.");
            return null;
        }
    }

    public async Task<UserProfile?> CreateUserAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);

        try
        {
            await _userManager.CreateAsync(user, saltedPassword);
            await _userManager.AddToRoleAsync(user, request.Role!);
            return user.ToUserProfile(request.Role!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles creating user.");
            return null;
        }
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;
        user.DailyCalorieLimit = request.ExpectedNumberOfCaloriesPerDay;
        user.HasExceededDailyCalorieLimit = request.IsCaloriesDeficient;

        try
        {
            await _userManager.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured whiles updating user with id:{id}", userId);
            return false;
        }
    }
    
    public async Task<bool> EmailAlreadyExistsAsync(string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        return user is not null;
    }
}
