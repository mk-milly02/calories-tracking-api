using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class CreatedOnResolver : IValueResolver<CalorieEntry, CalorieEntryResponse, DateOnly>
{
    public DateOnly Resolve(CalorieEntry source, CalorieEntryResponse destination, DateOnly destMember, ResolutionContext context)
    {
        return DateOnly.FromDateTime(source.DateTime);
    }
}
