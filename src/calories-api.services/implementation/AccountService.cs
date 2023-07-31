using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using calories_api.domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace calories_api.services;

public class AccountService : IAccountService
{
    private readonly IMapper _mapper;
    private readonly IMealService _mealService;
    private readonly IAuthenticationConfigurationProvider _authenticationConfigurationProvider;
    private readonly UserManager<User> _userManager;

    public AccountService(IMapper mapper, IMealService mealService, UserManager<User> userManager, IAuthenticationConfigurationProvider authenticationConfigurationProvider)
    {
        _mapper = mapper;
        _mealService = mealService;
        _authenticationConfigurationProvider = authenticationConfigurationProvider;
        _userManager = userManager;
    }

    public async Task<AuthenticationResponse?> Authenticate(AuthenticationRequest request)
    {
        User? user = await _userManager.FindByEmailAsync(request.Email!);

        if(user is null) { return null; }
        string saltedPassword = GenerateSaltedPassword(request.Password!, user.PasswordSalt!);
        return await _userManager.CheckPasswordAsync(user, saltedPassword) ? await GenerateToken(user) : null;
    }

    public async Task<UserResponse?> Register(UserRegistrationRequest request)
    {
        User user = new() { PasswordSalt = GenerateSalt() };
        _mapper.Map(request, user);

        string saltedPassword = GenerateSaltedPassword(request.Password!, user.PasswordSalt);
        IdentityResult result = await _userManager.CreateAsync(user, saltedPassword);
        await _userManager.AddToRoleAsync(user, Roles.RegularUser.ToString());
        return result.Succeeded ? _mapper.Map<UserResponse>(user) : null;
    }

    private static string GenerateSalt()
    {
        //using ensures that RNG is well disposed
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

    private async Task<AuthenticationResponse> GenerateToken(User user)
    {
        IConfiguration bearer = _authenticationConfigurationProvider.GetSchemeConfiguration(JwtBearerDefaults.AuthenticationScheme);

        string issuer = bearer.GetSection("issuer").Value ?? throw new InvalidOperationException("Issuer is not specified");
        string audience = bearer.GetSection("audience").Value ?? throw new InvalidOperationException("Audience is not specified");
        string secretKey = bearer.GetSection("secret-key").Value ?? throw new InvalidOperationException("Secret key is not specified");

        byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        SymmetricSecurityKey symmetricSecurityKey = new(secretKeyBytes);
        SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

        IList<string>? userRoles = await _userManager.GetRolesAsync(user);

        List<Claim> claims = new() 
        {
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Aud, audience),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Role, userRoles.First())
        };

        JwtSecurityToken securityToken = new(issuer,
                                             audience,
                                             claims,
                                             expires: DateTime.UtcNow.AddMinutes(30),
                                             signingCredentials: signingCredentials);

        return new() { Token = new JwtSecurityTokenHandler().WriteToken(securityToken), Expires = securityToken.ValidTo };
    }

    public async Task<bool> IsUsernameOrEmailInUse(UserRegistrationRequest request)
    {
        User? existingEmail = await _userManager.FindByEmailAsync(request.Email!);
        User? existingUsername = await _userManager.FindByNameAsync(request.Username!);
        return existingEmail != null && existingUsername != null;
    }

    public async Task<bool> SetExpectedNumberOfCaloriesPerDay(UserSettings settings)
    {
        User? user = await _userManager.FindByIdAsync(settings.UserId.ToString());
        user!.ExpectedNumberOfCaloriesPerDay = settings.ExpectedNumberOfCaloriesPerDay;
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task CheckForCalorieDeficiency(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        double totalUserCaloriesForToday = await _mealService.GetTotalUserCaloriesForToday(userId);
        user!.IsCaloriesDeficient = totalUserCaloriesForToday < user.ExpectedNumberOfCaloriesPerDay;
        await _userManager.UpdateAsync(user);
    }
}
