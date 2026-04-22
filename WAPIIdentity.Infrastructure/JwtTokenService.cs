using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using WAPIIdentity.Application.Dto;
using WAPIIdentity.Application.Services;
using WAPIIdentity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure;

namespace WAPIIdentity.Infrastructure;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    
    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }
    
    public TokenResponse Generate(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.Roles.Any())
        {
            claims.AddRange(user.Roles
                .Select(x => new Claim(ClaimTypes.Role, x.ToString())));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime tokenExpiration = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);  

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: tokenExpiration,
            signingCredentials: creds);
        
        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        
        return new TokenResponse
        {
            Token = encoded,
            Expiration = tokenExpiration
        };
    }
}