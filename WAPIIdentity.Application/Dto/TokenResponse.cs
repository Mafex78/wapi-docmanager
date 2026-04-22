namespace WAPIIdentity.Application.Dto;

public record TokenResponse
{ 
    public string Token  { get; set; } = string.Empty; 
    public DateTime Expiration  { get; set; }
}