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

    public async Task<IEnumerable<UserProfile>> GetAllUsers(PagingFilter query)
    {
        List<UserProfile> profiles = new();
        IEnumerable<User> users = _userManager.Users.ToList();

        foreach (User user in users)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);
            UserProfile profile = user.ToUserProfile(roles.First());
            profiles.Add(profile);
        }

        return profiles.Skip((query.Page - 1) * query.Size).Take(query.Size);
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

    public async Task<UserProfile?> EditUserProfileAsync(Guid userId, EditUserProfileRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;
        
        IdentityResult result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) { IList<string> roles = await _userManager.GetRolesAsync(user); return user.ToUserProfile(roles.First());  }
        
        return null;
    }

    public async Task<UserProfile?> SetExpectedNumberOfCaloriesPerDayAsync(Guid userId, UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.ExpectedNumberOfCaloriesPerDay = settings.ExpectedNumberOfCaloriesPerDay;
        IdentityResult result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) { IList<string> roles = await _userManager.GetRolesAsync(user); return user.ToUserProfile(roles.First());  }
        return null;
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

    public async Task<UserProfile?> CreateUserAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();

        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult result = await _userManager.CreateAsync(user, saltedPassword);
        await _userManager.AddToRoleAsync(user, request.Role!);

        UserProfile profile = user.ToUserProfile(request.Role!);
        
        return result.Succeeded ? profile : null;
    }

    public async Task<UserProfile?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;
        user.ExpectedNumberOfCaloriesPerDay = request.ExpectedNumberOfCaloriesPerDay;
        user.IsCaloriesDeficient = request.IsCaloriesDeficient;

        IdentityResult result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) { IList<string> roles = await _userManager.GetRolesAsync(user); return user.ToUserProfile(roles.First());  }
        
        return null;
    }
}
