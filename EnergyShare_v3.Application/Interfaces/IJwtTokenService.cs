using EnergyShare_v3.Domain.Entities.Users;

namespace EnergyShare_v3.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, IList<string> roles);
        string GenerateRefreshToken();
    }
}