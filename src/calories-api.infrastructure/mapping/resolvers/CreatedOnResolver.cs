using AutoMapper;
using calories_api.domain;

namespace calories_api.infrastructure;

public class CreatedOnResolver : IValueResolver<Meal, MealResponse, DateOnly>
{
    public DateOnly Resolve(Meal source, MealResponse destination, DateOnly destMember, ResolutionContext context)
    {
        return DateOnly.FromDateTime(source.DateTime);
    }
}
