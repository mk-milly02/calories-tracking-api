using calories_api.domain;

namespace calories_api.services;

public interface IAccountService
{
    Task<bool> IsUsernameOrEmailInUse(UserRegistrationRequest request);
    Task<UserRegistrationResponse?> Register(UserRegistrationRequest request);
    Task<AuthenticationResponse?> Authenticate(AuthenticationRequest request);
    Task<bool> SetExpectedNumberOfCaloriesPerDay(UserSettings settings);
    Task CheckForCalorieDeficiency(Guid userId);
}
