using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class UsernameResolver : IValueResolver<CreateUserRequest, User, string?>
{
    public string Resolve(CreateUserRequest source, User destination, string? destMember, ResolutionContext context)
    {
        return $"{source.FirstName!.ToLower()}.{source.LastName!.ToLower()}";
    }
}
