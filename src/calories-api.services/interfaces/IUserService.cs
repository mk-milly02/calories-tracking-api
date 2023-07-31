using calories_api.domain;

namespace calories_api.services;

public interface IUserService
{
    Task<UserProfile?> CreateUser(CreateUserRequest request, Roles userRole);
    Task<UserProfile?> GetUser(Guid userId);
    IEnumerable<UserProfile> GetAllUsers(PagingFilter query);
    Task<UserProfile?> UpdateUser(Guid userId, UpdateUserRequest request);
    Task<bool?> DeleteUser(Guid userId);
}
