using Microsoft.AspNetCore.Identity;

namespace calories_tracking.domain;

public class UserActionResponse
{
    public UserActionResponse(IdentityResult result)
    {
        Succeeded = result.Succeeded;
        Error = result.Errors.FirstOrDefault();
    }

    public UserActionResponse() { }

    public bool Succeeded { get; set; }
    public IdentityError? Error { get; set; }
}
