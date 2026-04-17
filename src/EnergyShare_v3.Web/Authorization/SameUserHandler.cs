using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EnergyShare_v3.Web.Authorization
{
    /*"L’autorisation simple est gérée via des policies basées sur les rôles dans Program.cs.
Pour les règles plus complexes dépendantes du contexte (comme accéder uniquement à ses propres données),
    j’ai implémenté un AuthorizationHandler personnalisé dans la couche Web."
       
    Ce handler d'autorisation vérifie qu’un utilisateur ne peut accéder
    qu’à ses propres données.

    Fonctionnement :
    - Si l’utilisateur est Administrateur → accès autorisé (bypass)
    - Sinon :
        → on récupère l’Id du user connecté (dans les claims)
        → on compare avec l’Id de la ressource demandée
        → si c’est le même → accès autorisé
        → sinon → accès refusé

    Utilisation actuelle :
    - Accès à /api/users/{id} → un utilisateur ne peut voir que son propre profil

    Évolution prévue (plus tard dans le projet) :
    Ce même principe sera réutilisé pour :
    - ProfilEnergie (accès uniquement à son profil)
    - PointAccess (accès uniquement à ses points d’accès)
    - Partage (accès uniquement aux partages dont il est membre)

    Ce handler permet de gérer une autorisation basée sur la ressource
       (et pas uniquement sur les rôles).

     */
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameUserRequirement requirement,
            Guid resourceUserId)
        {
            if (context.User.IsInRole("Administrateur"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var currentUserId) &&
                currentUserId == resourceUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
