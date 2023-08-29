using Microsoft.AspNetCore.Identity;

namespace calories_tracking.domain;

public class UserRegistrationResponse
{
    public UserProfile? Profile { get; set; }
    public List<IdentityError> Errors { get; set; } = new();
}
