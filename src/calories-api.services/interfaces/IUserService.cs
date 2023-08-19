using calories_api.domain;

namespace calories_api.services;

public interface IUserService
{
    Task<UserRegistrationResponse?> RegisterAsync(UserRegistrationRequest request);
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    Task<bool> EmailAlreadyExistsAsync(string email);
    Task<UserProfile?> CreateRegularUserAsync(CreateUserRequest request);
    Task<UserProfile?> CreateUserManagerAsync(CreateUserRequest request);
    Task<UserProfile?> CreateAdministratorAsync(CreateUserRequest request);
    Task<UserProfile?> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserProfile>> GetAllUsers(PagingFilter query);
    Task<UserProfile?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool?> RemoveUserAsync(Guid userId);
    Task CheckForCalorieDeficiencyAsync(Guid userId);
    Task<UserProfile?> UpdateUserSettingsAsync(UserSettings settings);
    Task<bool?> CreatePasswordAsync(Guid userId, CreatePasswordRequest request);
}
