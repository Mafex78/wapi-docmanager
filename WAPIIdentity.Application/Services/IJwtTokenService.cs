using WAPIIdentity.Application.Dto;
using WAPIIdentity.Domain.Entities;

namespace WAPIIdentity.Application.Services;

public interface IJwtTokenService
{
    TokenResponse Generate(User user);
}