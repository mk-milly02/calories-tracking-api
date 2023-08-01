using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using calories_api.domain;
using Microsoft.AspNetCore.Identity;

namespace calories_api.services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IMealService _mealService;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public UserService(IMapper mapper, IMealService mealService, UserManager<User> userManager, ITokenService tokenService)
    {
        _mapper = mapper;
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
            UserProfile userProfile = _mapper.Map<UserProfile>(user);
            profiles.Add(userProfile);
        }

        return profiles.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<UserProfile?> GetUserByIdAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : _mapper.Map<UserProfile>(user);
    }

    public async Task<UserProfile?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) return null;

        user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task<UserProfile?> UpdateUserSettingsAsync(UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(settings.UserId.ToString());
        user!.ExpectedNumberOfCaloriesPerDay = settings.ExpectedNumberOfCaloriesPerDay;
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task CheckForCalorieDeficiencyAsync(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        double totalUserCaloriesForToday = await _mealService.GetTotalUserCaloriesForToday(userId);
        user!.IsCaloriesDeficient = totalUserCaloriesForToday < user.ExpectedNumberOfCaloriesPerDay;
        await _userManager.UpdateAsync(user);
    }

    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        User? user = await _userManager.FindByEmailAsync(request.Email!);

        if(user is null) { return null; }
        string saltedPassword = GenerateSaltedPassword(request.Password!, user.PasswordSalt!);
        return await _userManager.CheckPasswordAsync(user, saltedPassword) ? await _tokenService.GenerateTokenAsync(user) : null;
    }

    public async Task<UserRegistrationResponse?> RegisterAsync(UserRegistrationRequest request)
    {
        User user = new() { PasswordSalt = GenerateSalt() };
        _mapper.Map(request, user);

        string saltedPassword = GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult result = await _userManager.CreateAsync(user, saltedPassword);
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        return result.Succeeded ? _mapper.Map<UserRegistrationResponse>(user) : null;
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        return user is not null;
    }

    private static string GenerateSalt()
    {
        //"using" ensures that RNG is well disposed
        using RandomNumberGenerator generator = RandomNumberGenerator.Create();
        byte[] randomNumberBytes = new byte[32];
        generator.GetBytes(randomNumberBytes);
        return Convert.ToBase64String(randomNumberBytes);
    }

    private static string GenerateSaltedPassword(string password, string salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        byte[] saltedPasswordBytes = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
        Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);
        return Convert.ToBase64String(saltedPasswordBytes);
    }

    public async Task<UserProfile?> CreateRegularUserAsync(CreateUserRequest request)
    {
        User user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task<UserProfile?> CreateUserManagerAsync(CreateUserRequest request)
    {
        User user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.UserManager.ToString());
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task<UserProfile?> CreateAdministratorAsync(CreateUserRequest request)
    {
        User user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, Roles.Administrator.ToString());
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task<bool?> CreatePasswordAsync(Guid userId, CreatePasswordRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) return null;

        user.PasswordSalt = GenerateSalt();
        string saltedPassword = GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult created = await _userManager.AddPasswordAsync(user, saltedPassword);
        return created.Succeeded;
    }
}
