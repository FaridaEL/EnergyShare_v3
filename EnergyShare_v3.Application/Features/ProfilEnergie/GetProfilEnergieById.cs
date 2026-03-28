using EnergyShare_v3.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.ProfilEnergie
{    //intention : récupérer un profil énergie spécifique pour afficher les détails du profil ou pour effectuer des opérations de matching et de simulation d'économie d'énergie réalisées grâce au partage, en comparant le prix d'achat cible de l'acheteur avec le prix de vente cible du vendeur et en comparant le prix de vente cible du vendeur avec le prix d'achat auprès de son fournisseur d'énergie actuel
    public record GetProfilEnergieByIdQuery(Guid Id);
    public class GetProfilEnergieHandler
    {
        private readonly IApplicationDbContext _context;
        public GetProfilEnergieHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfilEnergieSummaryDto?> HandleAsync(
        GetProfilEnergieByIdQuery query,
        CancellationToken cancellationToken = default)
        {
            //v2 CQRS améliorée sans charger l'entité complète ProfilEnergie et ses relations, mais en projetant directement
            //les données nécessaires dans le ProfilEnergieSummaryDto à l'aide de LINQ et de la méthode Select.
            //Cela permet d'optimiser les performances en ne récupérant que les données nécessaires pour le DTO,tout en évitant
            //les problèmes liés au chargement paresseux (lazy loading) et en assurant que les données nécessaires
            //sont disponibles lors de la création du ProfilEnergieSummaryDto.
            //En ccl : plus léger, sans include, plus CQRS
            return await _context.ProfilsEnergie
            .AsNoTracking()
            .Where(pe => pe.Id == query.Id)
            .Select(pe => new ProfilEnergieSummaryDto(
                pe.Id,
                pe.DemandeEnergie_kWh,
                pe.OffreEnergie_kWh,
                pe.PrixAchatCible_Eur,
                pe.PrixVenteCible_Eur,
                pe.ConsommationAnnuelleEstime_kWh,
                pe.ProductionAnnuelleEstime_kWh,
                pe.PrixAchatEnergieFournisseur_Eur,
                pe.PrixVenteInjectionFournisseurActuel_Eur,
                pe.PointAccessId,
                pe.PointAccess.UserId,
                pe.PointAccess.User.Role,
                pe.PointAccess.User.UserType,
                pe.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);


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



