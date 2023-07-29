using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class CreatedAtResolver : IValueResolver<CalorieEntry, CalorieEntryResponse, TimeOnly>
{
    public TimeOnly Resolve(CalorieEntry source, CalorieEntryResponse destination, TimeOnly destMember, ResolutionContext context)
    {
        return TimeOnly.FromDateTime(source.DateTime);
    }
}
