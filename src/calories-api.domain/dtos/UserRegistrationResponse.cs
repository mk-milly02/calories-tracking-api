namespace calories_api.domain;

public class UserRegistrationResponse
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set;}
    public string? Email { get; set;}
}
