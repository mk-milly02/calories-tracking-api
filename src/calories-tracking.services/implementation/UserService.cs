using calories_tracking.domain;
using Microsoft.AspNetCore.Identity;

namespace calories_tracking.services;

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

    public async Task<UserActionResponse> RemoveUserAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) 
        { 
            return new() 
            { 
                Succeeded = false, 
                Error = new() 
                { 
                    Code = "NotFound", 
                    Description = $"User with id:{userId} does not exist." 
                } 
            }; 
        }   
        return new(await _userManager.DeleteAsync(user!));
    }

    public async Task<IEnumerable<UserProfile>> GetAllUsers(PaginationQueryParameters query)
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

    public async Task<UserActionResponse> EditUserProfileAsync(Guid userId, EditUserProfileRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;
        
        return new(await _userManager.UpdateAsync(user));
    }

    public async Task<bool> SetDailyCalorieLimit(Guid userId, UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        user!.DailyCalorieLimit = settings.DailyCalorieLimit;
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> HasExceededDailyCalorieLimit(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        double totalUserCaloriesForToday = await _mealService.GetTotalUserCaloriesForTodayAsync(userId);
        user!.HasExceededDailyCalorieLimit = totalUserCaloriesForToday < user.DailyCalorieLimit;
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        User? user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null) { return null; }
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt!);
        return await _userManager.CheckPasswordAsync(user, saltedPassword) ? await _tokenService.GenerateTokenAsync(user) : null;
    }

    public async Task<UserRegistrationResponse> RegisterAsync(UserRegistrationRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(user, saltedPassword);
        UserRegistrationResponse response = new();

        if (!userCreatedResult.Succeeded) { response.Errors.AddRange(userCreatedResult.Errors); return response; }
        
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        response.Profile = user.ToUserProfile(nameof(Roles.RegularUser));
        return response;
    }

    public async Task<UserRegistrationResponse> CreateUserAsync(CreateUserRequest request)
    {
        User user = request.ToUser();
        user.PasswordSalt = Security.GenerateSalt();
        string saltedPassword = Security.GenerateSaltedPassword(request.Password!, user.PasswordSalt);

        IdentityResult userCreatedResult = await _userManager.CreateAsync(user, saltedPassword);
        UserRegistrationResponse response = new();

        if (!userCreatedResult.Succeeded) { response.Errors.AddRange(userCreatedResult.Errors); return response; }
        
        await _userManager.AddToRoleAsync(user, request.Role!);
        response.Profile = user.ToUserProfile(request.Role!);
        return response;
    }

    public async Task<UserActionResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) 
        { 
            return new() 
            { 
                Succeeded = false, 
                Error = new() 
                { 
                    Code = "NotFound", 
                    Description = $"User with id:{userId} does not exist." 
                } 
            }; 
        }   

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.Username;
        user.DailyCalorieLimit = request.ExpectedNumberOfCaloriesPerDay;
        user.HasExceededDailyCalorieLimit = request.IsCaloriesDeficient;

        return new(await _userManager.UpdateAsync(user));
    }
}
