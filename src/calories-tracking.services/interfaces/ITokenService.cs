using calories_tracking.domain;

namespace calories_tracking.services;

public interface ITokenService
{
    Task<AuthenticationResponse> GenerateTokenAsync(User user);
}
