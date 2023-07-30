using calories_api.domain;

namespace calories_api.services;

public interface IAccountService
{
    Task<bool> IsUsernameOrEmailInUse(UserRegistrationRequest request);
    Task<UserResponse?> Register(UserRegistrationRequest request);
    Task<AuthenticationResponse?> Authenticate(AuthenticationRequest request);
}
