using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateMealRequest, Meal>();

        CreateMap<Meal, MealResponse>()
            .ForMember(x => x.CreatedOn, options => options.MapFrom<CreatedOnResolver>())
            .ForMember(x => x.CreatedAt, options => options.MapFrom<CreatedAtResolver>());

        CreateMap<UpdateMealRequest, Meal>();

        CreateMap<UserRegistrationRequest, User>();

        CreateMap<User, UserResponse>();
    }
}
