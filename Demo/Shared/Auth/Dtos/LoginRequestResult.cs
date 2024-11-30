namespace Demo.Shared.Auth.Dtos;

public class LoginRequestResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}