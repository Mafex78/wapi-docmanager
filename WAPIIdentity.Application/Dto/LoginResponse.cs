namespace WAPIIdentity.Application.Dto;

public record LoginResponse
{
    public string UserId  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty; 
    public DateTime TokenExpiration  { get; set; }
}