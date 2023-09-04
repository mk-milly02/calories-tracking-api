using calories_tracking.domain;

namespace calories_tracking.services;

public interface IUserService
{
    Task<UserRegistrationResponse> RegisterAsync(UserRegistrationRequest request);
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
    Task<UserRegistrationResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserActionResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<UserProfile?> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserProfile>> GetAllUsers(PaginationQueryParameters query);
    Task<UserActionResponse> EditUserProfileAsync(Guid userId, EditUserProfileRequest request);
    Task<UserActionResponse> RemoveUserAsync(Guid userId);
    Task<bool> HasExceededDailyCalorieLimit(Guid userId);
    Task<bool> SetDailyCalorieLimit(Guid userId, UserSettings settings);
}
