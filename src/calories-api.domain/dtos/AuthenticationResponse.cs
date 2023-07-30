namespace calories_api.domain;

public class AuthenticationResponse
{
    public string? Token { get; set; }
    public DateTime Expires { get; set; }
}
