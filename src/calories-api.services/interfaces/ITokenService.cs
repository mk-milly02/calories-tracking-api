using calories_api.domain;

namespace calories_api.services;

public interface ITokenService
{
    Task<AuthenticationResponse> GenerateTokenAsync(User user);
}
