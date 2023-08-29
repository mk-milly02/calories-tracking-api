namespace calories_tracking.domain;

public class AuthenticationResponse
{
    public string? Token { get; set; }
    public DateTime Expires { get; set; }
}
