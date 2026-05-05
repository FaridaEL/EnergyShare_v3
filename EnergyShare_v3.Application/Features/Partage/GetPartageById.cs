using Ardalis.Result;
using EnergyShare_v3.Application.Features.PointAccess;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Partage
{              /*Todo implémenter logique CQRS/Mediator*/
    public record GetPartageById(Guid Id) : 
        IQuery<Result<PartageDetailsDto>>;

    public class GetPartageByIdHandler(
        IApplicationDbContext context,
        IUserContext userContext) :
        IQueryHandler<GetPartageById, Result<PartageDetailsDto>>
    {
        public async ValueTask<Result<PartageDetailsDto>> Handle(
            GetPartageById query,
            CancellationToken cancellationToken)
        {   // partage n'est pas lié directement à l'utilisateur -> on teste l'accès avant de projeter en DTO 
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<PartageDetailsDto>.Unauthorized();

            var currentUserId = userContext.UserId.Value;

            var isAdmin = userContext.IsInRole("Administrateur");
            

            var hasAccess = await context.Partages
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == query.Id &&
                    (
                        isAdmin ||
                        p.VendeurId == currentUserId ||
                        p.Membres.Any(m =>
                            m.ExitAt == null &&
                            m.PointAccess.UserId == currentUserId)
                    ),
                    cancellationToken);

            if (!hasAccess)
                return Result<PartageDetailsDto>.Forbidden();


            //var dto = await context.Partages
            //.AsNoTracking()
            //.Where(p => p.Id == query.Id)
            //.Select(p => new PartageDetailsDto(
            //    p.Id,
            //    p.Nom,
            //    p.Description,
            //    p.EnergieType,
            //    p.Statut,
            //    p.Membres.Count(m => m.ExitAt == null),
            //    p.DateDebut,
            //    p.DateFin,
            //    p.Audit.CreatedAt,
            //    p.Audit.UpdatedAt,

            //    // CanEdit :  Pour le MVP, l'admin ou le vendeur/interlocuteur peut modifier.Plus tard, on pourra ajouter le gestionnaire de partage.
            //    isAdmin || p.VendeurId == currentUserId,

            //    // IsInterlocuteurUnique : Dans le MVP, le vendeur créateur est considéré comme interlocuteur unique.
            //    p.VendeurId == currentUserId,

            //    // Progression UI : Permet d'afficher une barre de progression dans l'UI.
            //    p.Statut == PartageEnergieStatutType.Inactif ? 20 :
            //    p.Statut == PartageEnergieStatutType.EnAttenteValidation ? 60 :
            //    p.Statut == PartageEnergieStatutType.Actif ? 100 :
            //    40,
            //    // On récupère la dernière dde d'info de périmètre liée au partage.
            //    p.DemandesGrd
            //        .Where(d => d.DemandeType == DemandeGRDType.DdeInfoPerimetre)
            //        .OrderByDescending(d => d.DateDemande)
            //        .Select(d => new DemandePerimetreDto(
            //            d.Id,
            //            p.Id,
            //            d.DateDemande,
            //            d.ResponseStatus.ToString(),
            //            d.DetailsDemande ?? string.Empty
            //        ))
            //        .FirstOrDefault()

            //))
            //.FirstOrDefaultAsync(cancellationToken);

            //if (dto is null)
            //{
            //    return Result<PartageDetailsDto>.NotFound("Partage introuvable");
            //}

            // On charge le partage complet puis on fait le mapping DTO en mémoire.
            // Cela évite les projections EF trop complexes, notamment avec ToString()
            // sur les enums dans DerniereDemandePerimetre.
            var partage = await context.Partages
                .AsNoTracking()
                .Include(p => p.Membres)
                    .ThenInclude(m => m.PointAccess)
                .Include(p => p.DemandesGrd)
                .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

            if (partage is null)
                return Result<PartageDetailsDto>.NotFound("Partage introuvable");

            // Dernière demande d'information de périmètre liée au partage.
            // Le ToString() est fait ici en mémoire, pas dans SQL.
            var derniereDemandePerimetre = partage.DemandesGrd
                .Where(d => d.DemandeType == DemandeGRDType.DdeInfoPerimetre)
                .OrderByDescending(d => d.DateDemande)
                .Select(d => new DemandePerimetreDto(
                    d.Id,
                    partage.Id,
                    d.DateDemande,
                    d.ResponseStatus.ToString(),
                    d.DetailsDemande ?? string.Empty
                ))
                .FirstOrDefault();

            var dto = new PartageDetailsDto(
                partage.Id,
                partage.Nom,
                partage.Description,
                partage.EnergieType,
                partage.Statut,
                partage.Membres.Count(m => m.ExitAt == null),
                partage.DateDebut,
                partage.DateFin,
                partage.Audit.CreatedAt,
                partage.Audit.UpdatedAt,

                // CanEdit : Pour le MVP, l'admin ou le vendeur/interlocuteur peut modifier.
                isAdmin || partage.VendeurId == currentUserId,

                // IsInterlocuteurUnique : Dans le MVP, le vendeur créateur est considéré comme interlocuteur unique.
                partage.VendeurId == currentUserId,

                // Progression UI : permet d'afficher une barre de progression dans l'UI.
                partage.Statut == PartageEnergieStatutType.Inactif ? 20 :
                partage.Statut == PartageEnergieStatutType.EnAttenteValidation ? 60 :
                partage.Statut == PartageEnergieStatutType.Actif ? 100 :
                40,

                derniereDemandePerimetre
            );

            return Result.Success(dto);

            
        }
    }
}
