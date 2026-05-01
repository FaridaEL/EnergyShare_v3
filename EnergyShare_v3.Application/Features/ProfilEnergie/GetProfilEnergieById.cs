using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{    //intention : récupérer un profil énergie spécifique pour afficher les détails du profil ou pour effectuer des opérations
     //de matching et de simulation d'économie d'énergie réalisées grâce au partage,
     //en comparant le prix d'achat cible de l'acheteur avec le prix de vente cible du vendeur et en comparant le prix de vente cible du vendeur avec le prix d'achat auprès de son fournisseur d'énergie actuel
    public record GetProfilEnergieById(Guid Id) : IQuery<Result<ProfilEnergieDetailDto>>;
    public class GetProfilEnergieByIdHandler(
        IApplicationDbContext context,
        IUserContext userContext) // on s'assure que l'utilisateur connecté est bien le propriétaire du profil énergie demandé pour des raisons de sécurité et de confidentialité des données, 
        : IQueryHandler<GetProfilEnergieById, Result<ProfilEnergieDetailDto>>
    {
        //private readonly IApplicationDbContext _context;
        //public GetProfilEnergieHandler(IApplicationDbContext context)
        //{ _context = context;      }

        public async ValueTask<Result<ProfilEnergieDetailDto>> Handle(
            GetProfilEnergieById query,
            CancellationToken cancellationToken)
        {
            //v2 CQRS améliorée sans charger l'entité complète ProfilEnergie et ses relations, mais en projetant directement
            //les données nécessaires dans le ProfilEnergieSummaryDto à l'aide de LINQ et de la méthode Select.
            //Cela permet d'optimiser les performances en ne récupérant que les données nécessaires pour le DTO,tout en évitant
            //les problèmes liés au chargement paresseux (lazy loading) et en assurant que les données nécessaires
            //sont disponibles lors de la création du ProfilEnergieSummaryDto.
            //En ccl : plus léger, sans include, plus CQRS
            var profil = await context.ProfilsEnergie
            .AsNoTracking()
            .Where(pe => pe.Id == query.Id)
            .Select(pe => new ProfilEnergieDetailDto(
                pe.Id,
                pe.DemandeEnergie_kWh,
                pe.OffreEnergie_kWh,
                pe.PrixAchatCible_Eur,
                pe.PrixVenteCible_Eur,
                pe.PointAccessId,
                pe.PointAccess.UserId,
                pe.Audit.CreatedAt,
                pe.Audit.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);


            if (profil is null)
                return Result<ProfilEnergieDetailDto>.NotFound("Profil énergie introuvable.");
            
            var currentUserId = userContext.UserId;

            // Sécurité : l'utilisateur doit être authentifié.
            if (currentUserId is null || currentUserId == Guid.Empty)
                return Result<ProfilEnergieDetailDto>.Unauthorized();

            // Sécurité : seul le propriétaire ou un administrateur peut consulter le détail complet.
            // Les autres utilisateurs passent par SearchPotentialMatches, qui expose un DTO limité.
            if (profil.UserId != currentUserId && !userContext.IsInRole("Administrateur"))
                return Result<ProfilEnergieDetailDto>.Forbidden();

            return Result.Success(profil);

            /*Version 1 : chargement de l'entité
            var profilEnergie = await _context.ProfilsEnergie
                 .AsNoTracking()
                 .Include(pe => pe.PointAccess) //en effet on utilise dans Dto profilEnergie.PointAccess.UserId, profilEnergie.PointAccess.User.Role, profilEnergie.PointAccess.User.UserType
                     .ThenInclude(pa => pa.User)  //Il faut donc charger PointAccess puis User pour éviter les problèmes de chargement paresseux (lazy loading) et s'assurer que les données nécessaires sont disponibles lors de la création du ProfilEnergieSummaryDto
                 .FirstOrDefaultAsync(pe => pe.Id == query.Id, cancellationToken);

            if (profilEnergie is null)
                return null;

            return new ProfilEnergieSummaryDto(
                    profilEnergie.Id,
                    profilEnergie.DemandeEnergie_kWh,
                    profilEnergie.OffreEnergie_kWh,
                    profilEnergie.PrixAchatCible_Eur,
                    profilEnergie.PrixVenteCible_Eur,
                    profilEnergie.ConsommationAnnuelleEstime_kWh,
                    profilEnergie.ProductionAnnuelleEstime_kWh,
                    profilEnergie.PrixAchatEnergieFournisseur_Eur,
                    profilEnergie.PrixVenteInjectionFournisseurActuel_Eur,
                    profilEnergie.PointAccessId,
                    profilEnergie.PointAccess.UserId,
                    profilEnergie.PointAccess.User.Role,
                    profilEnergie.PointAccess.User.UserType,
                    profilEnergie.CreatedAt
            );
       
        */

        }
        

    }
}



