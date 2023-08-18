using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using calories_api.domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace calories_api.services;

public class TokenService : ITokenService
{
    private readonly IAuthenticationConfigurationProvider _authenticationConfigurationProvider;
    private readonly UserManager<User> _userManager;

    public TokenService(IAuthenticationConfigurationProvider authenticationConfigurationProvider, UserManager<User> userManager)
    {
        _authenticationConfigurationProvider = authenticationConfigurationProvider;
        _userManager = userManager;
    }

    public async Task<AuthenticationResponse> GenerateTokenAsync(User user)
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
            new Claim(ClaimTypes.Role, userRoles.First()),
            new Claim("test", "testing")
        };

        JwtSecurityToken securityToken = new(issuer,
                                             audience,
                                             claims,
                                             expires: DateTime.UtcNow.AddMinutes(30),
                                             signingCredentials: signingCredentials);

        return new() { Token = new JwtSecurityTokenHandler().WriteToken(securityToken), Expires = securityToken.ValidTo };
    }
}
