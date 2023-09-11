using calories_tracking.domain;

namespace calories_tracking.services;

public interface IUserService
{
    Task<PageList<UserProfile>> GetAllUsersAsync(QueryParameters query);
    Task<UserProfile?> RegisterAsync(UserRegistrationRequest request);
    Task<UserProfile?> CreateUserAsync(CreateUserRequest request);
    Task<UserProfile?> GetUserByIdAsync(Guid userId);
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> EditUserProfileAsync(Guid userId, EditUserProfileRequest request);
    Task<bool> RemoveUserAsync(Guid userId);
    Task<bool> HasExceededDailyCalorieLimitAsync(Guid userId);
    Task<bool> SetDailyCalorieLimitAsync(Guid userId, UserSettings settings);
    Task<bool> EmailAlreadyExistsAsync(string email);
}
