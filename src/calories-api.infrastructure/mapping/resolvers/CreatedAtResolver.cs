using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class CreatedAtResolver : IValueResolver<Meal, MealResponse, TimeOnly>
{
    public TimeOnly Resolve(Meal source, MealResponse destination, TimeOnly destMember, ResolutionContext context)
    {
        return TimeOnly.FromDateTime(source.DateTime);
    }
}
