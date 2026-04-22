namespace Shared.Infrastructure;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } =  string.Empty;
    
    public string Audience { get; set; } =  string.Empty;
    
    /// <summary>Symmetric signing key (HMAC-SHA256). Injected via Kubernetes Secret.</summary>
    public string SigningKey { get; set; } = string.Empty;
    
    public int ExpirationMinutes { get; set; } = 60;
}