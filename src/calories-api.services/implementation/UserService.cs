using calories_api.domain;
using Microsoft.AspNetCore.Identity;

namespace calories_api.services;

public class UserService : IUserService
{
    private readonly IMealService _mealService;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public UserService(IMealService mealService, UserManager<User> userManager, ITokenService tokenService)
    {
        _mealService = mealService;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<bool?> RemoveUserAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        IdentityResult result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public IEnumerable<UserProfile> GetAllUsers(PagingFilter query)
    {
        List<UserProfile> profiles = new();
        IEnumerable<User> users = _userManager.Users.ToList();

        foreach (User user in users)
        {
            profiles.Add(user.ToUserProfile());
        }

        return profiles.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<UserProfile?> GetUserByIdAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.ToUserProfile();
    }

    public async Task<UserProfile?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) return null;

        user = request.ToUser();
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? user.ToUserProfile() : null;
    }

    public async Task<UserProfile?> UpdateUserSettingsAsync(UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(settings.UserId.ToString());
        user!.ExpectedNumberOfCaloriesPerDay = settings.ExpectedNumberOfCaloriesPerDay;
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? user.ToUserProfile() : null;
    }

    public async Task CheckForCalorieDeficiencyAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        double totalUserCaloriesForToday = await _mealService.GetTotalUserCaloriesForTodayAsync(userId);
        user!.IsCaloriesDeficient = totalUserCaloriesForToday < user.ExpectedNumberOfCaloriesPerDay;
        await _userManager.UpdateAsync(user);
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        User? user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null) { return null; }
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt!);
        return await _userManager.CheckPasswordAsync(user, saltedPassword) ? await _tokenService.GenerateTokenAsync(user) : null;
    }

    public async Task<UserRegistrationResponse?> RegisterAsync(UserRegistrationRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();

        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult result = await _userManager.CreateAsync(user, saltedPassword);
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        return result.Succeeded ? user.ToUserRegistrationResponse() : null;
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        return user is not null;
    }

    public async Task<UserProfile?> CreateRegularUserAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        return result.Succeeded ? user.ToUserProfile() : null;
    }

    public async Task<UserProfile?> CreateUserManagerAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.UserManager.ToString());
        return result.Succeeded ? user.ToUserProfile() : null;
    }

    public async Task<UserProfile?> CreateAdministratorAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.Administrator.ToString());
        return result.Succeeded ? user.ToUserProfile() : null;
    }

    public async Task<bool?> CreatePasswordAsync(Guid userId, CreatePasswordRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) return null;

        user.PasswordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult created = await _userManager.AddPasswordAsync(user, saltedPassword);
        return created.Succeeded;
    }
}
