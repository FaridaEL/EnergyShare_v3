using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.PointsAccesses;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EnergyShare_v3.Application.Features.Partage
{          /// <summary>
           /// Récupère la liste des membres liés à un partage.
           /// Cette Query est utilisée par la page PartageMembres.
           /// </summary>
    public record GetMembresPartage(Guid PartageId)
        : IQuery<Result<IReadOnlyList<MembrePartageDto>>> ;

    public class GetMembresPartagesHandler(IApplicationDbContext context, IUserContext userContext)
        : IQueryHandler<GetMembresPartage, Result<IReadOnlyList<MembrePartageDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<MembrePartageDto>>> Handle(
            GetMembresPartage query,
            CancellationToken cancellationToken)
        {
            // 1. Vérifie qu'un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
            {
                return Result<IReadOnlyList<MembrePartageDto>>
                    .Unauthorized();
            }

            var userId = userContext.UserId.Value;
            // 2. Vérifie que le partage existe.
           // le vendeur/interlocuteur unique et l'administrateur peuvent consulter la liste des membres d'un partage.

            var partage = await context.Partages
                 .FirstOrDefaultAsync(
                    p => p.Id == query.PartageId,
                    cancellationToken);
            if (partage is null)
            {
                return Result<IReadOnlyList<MembrePartageDto>>.NotFound();
            }


            // 3. Contrôle simple d'accès.  -->  La liste des membres peut être consultée :
            // - par le vendeur / interlocuteur unique ;
            // - par un membre actif du partage ;
            // - par un administrateur.
            var estMembreActif = await context.MembresPartage
                .AnyAsync(m =>
                    m.PartageId == query.PartageId  && m.PointAccess.UserId == userId && m.ExitAt == null,
                    cancellationToken);

            var canRead =  partage.VendeurId == userId || estMembreActif  || userContext.IsInRole("Administrateur");

            if (!canRead)
            {
                 return Result<IReadOnlyList<MembrePartageDto>> .Forbidden();
            }
            // 4. Récupère les participations du partage
            // avec les données du point d'accès associées.
            var membres = await context.MembresPartage
                .Where(m => m.PartageId == query.PartageId)

                // ParticipationPartage -> PointAccess -> User
                .Include(m => m.PointAccess)
                    .ThenInclude(pa => pa.User)

                .OrderBy(m => m.JoinedAt)

                .Select(m => new MembrePartageDto(
                    m.Id,
                    m.PointAccessId,

                    // Nom du membre.
                    // Si prénom et nom sont absents, on utilise l'e-mail.
                    string.IsNullOrWhiteSpace(m.PointAccess.User.FirstName)
                        && string.IsNullOrWhiteSpace(m.PointAccess.User.LastName)
                            ? (m.PointAccess.User.Email ?? "Utilisateur")
                            : ((m.PointAccess.User.FirstName ?? "")
                                + " "
                                + (m.PointAccess.User.LastName ?? "")).Trim(),

                    // Informations du point d'accès
                    m.PointAccess.EAN_Encrypted ?? "Non renseigné",

                    $"{m.PointAccess.AdresseLine1}, {m.PointAccess.CodePostal}",

                    // Rôle
                    m.UserRolePartage.ToString(),
                    m.IsInterlocuteurUnique,

                    // Dates
                    m.JoinedAt,
                    m.DateCommunicationPreavis,
                    m.DateSortiePlanifiee,
                    m.ExitAt,

                    // Une participation reste active tant qu'elle
                    // ne possède pas de date de sortie.
                    m.ExitAt == null
                ))
                .ToListAsync(cancellationToken);
            // 5. Retourne la liste.
            // Une liste vide n'est pas considérée comme une erreur.
            return Result.Success<IReadOnlyList<MembrePartageDto>>(
                membres);    
        }
    }
}
