using EnergyShare_v3.Application.Interfaces;
using System.Security.Claims;

namespace EnergyShare_v3.Web.Infrastructure
{
    /*
      CurrentUserContext centralise l'accès à l'utilisateur connecté.
      Il lit les claims du HttpContext courant et expose les infos utiles à l'application : 
        UserId, Email, rôles, OrganismePublicId, etc.
      Cela évite de manipuler directement HttpContext.User dans les handlers ou services métier.
  */
    public class CurrentUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Email =>
            User?.FindFirstValue(ClaimTypes.Email);

        public string? UserName =>
            User?.FindFirstValue(ClaimTypes.Name);

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;

        public IReadOnlyList<string> Roles =>
            User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            ?? [];

        public Guid? OrganismePublicId
        {
            get
            {
                var value = User?.FindFirstValue("OrganismePublicId");
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public bool IsInRole(string role) =>
            User?.IsInRole(role) ?? false;
    }
}
