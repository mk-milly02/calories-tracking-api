using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateCalorieEntryRequest, CalorieEntry>();

        CreateMap<CalorieEntry, CalorieEntryResponse>()
            .ForMember(x => x.CreatedOn, options => options.MapFrom<CreatedOnResolver>())
            .ForMember(x => x.CreatedAt, options => options.MapFrom<CreatedAtResolver>());

        CreateMap<UpdateCalorieEntryRequest, CalorieEntry>();
    }
}
