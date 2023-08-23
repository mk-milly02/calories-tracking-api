using calories_api.domain;

namespace calories_api.services;

public interface IUserService
{
    Task<UserRegistrationResponse?> RegisterAsync(UserRegistrationRequest request);
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    Task<bool> EmailAlreadyExistsAsync(string email);
    Task<UserProfile?> CreateUserAsync(CreateUserRequest request);
    Task<UserProfile?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<UserProfile?> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserProfile>> GetAllUsers(PagingFilter query);
    Task<UserProfile?> EditUserProfileAsync(Guid userId, EditUserProfileRequest request);
    Task<bool?> RemoveUserAsync(Guid userId);
    Task CheckForCalorieDeficiencyAsync(Guid userId);
    Task<UserProfile?> SetExpectedNumberOfCaloriesPerDayAsync(Guid userId, UserSettings settings);
}
