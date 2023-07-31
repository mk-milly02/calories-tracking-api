using AutoMapper;
using calories_api.domain;
using Microsoft.AspNetCore.Identity;

namespace calories_api.services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public UserService(IMapper mapper, UserManager<User> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<UserProfile?> CreateUser(CreateUserRequest request, Roles userRole)
    {
        User user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, userRole.ToString());
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }

    public async Task<bool?> DeleteUser(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        IdentityResult result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public IEnumerable<UserProfile> GetAllUsers(PagingFilter query)
    {
        List<UserProfile> profiles = new();
        IEnumerable<User> users = _userManager.Users.ToList();

        foreach (User user in users)
        {
            UserProfile userProfile = _mapper.Map<UserProfile>(user);
            profiles.Add(userProfile);
        }

        return profiles.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
    }

    public async Task<UserProfile?> GetUser(Guid userId)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : _mapper.Map<UserProfile>(user);
    }

    public async Task<UserProfile?> UpdateUser(Guid userId, UpdateUserRequest request)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) return null;

        user = _mapper.Map<User>(request);
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? _mapper.Map<UserProfile>(user) : null;
    }
}
