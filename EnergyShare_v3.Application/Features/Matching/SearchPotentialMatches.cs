using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{      /* Todo : 
        * Le calcul de distance
         la recherche des profils compatibles
         la règle “disponible / non disponible”
    
        Logique MVP : 
    1. Je récupère mon point d’accès source
    2. Je récupère son profil énergie
    3. Je cherche les profils opposés
    4. J’exclus les points déjà dans un partage actif
    5. Je calcule une distance simplifiée
    6. Je retourne des PotentialMatchDto

        Règle : Un point d’accès est disponible s’il n’est pas déjà membre d’un partage actif.
    Todo UI : 
    Page Trouver un match   : SearchPotentialMatchesQuery -> → affichre les suggestions

    Bouton “Contacter” ou “Enregistrer”    :
        → CreateMatch
        → sauvegarde le match si absent
        → retourne MatchId
        */

    public record SearchPotentialMatchesQuery(Guid SourcePointAccessId)
            : IQuery<Result<IReadOnlyList<PotentialMatchDto>>>;

    public class SearchPotentialMatchesHandler(IApplicationDbContext context)
        : IQueryHandler<SearchPotentialMatchesQuery, Result<IReadOnlyList<PotentialMatchDto>>>
    {
        public async ValueTask<Result<IReadOnlyList<PotentialMatchDto>>> Handle(
            SearchPotentialMatchesQuery query,
            CancellationToken cancellationToken)
        {
            // 1. Point d’accès de départ : c’est lui qui porte l’adresse, le user et le consentement.
            var sourcePointAccess = await context.PointAccesses
                .AsNoTracking()
                .Include(pa => pa.User)
                .Include(pa => pa.Membres)
                .FirstOrDefaultAsync(pa => pa.Id == query.SourcePointAccessId, cancellationToken);

            if (sourcePointAccess is null)
                return Result.NotFound("Point d'accès introuvable.");

            // 2. Sans consentement, pas de matching.
            if (!sourcePointAccess.AccordConsentement)
                return Result.Invalid(new ValidationError("Le consentement au partage des données est requis."));

            // 3. Profil énergie source : il contient l’offre, la demande et les prix.
            var sourceProfil = await context.ProfilsEnergie
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PointAccessId == sourcePointAccess.Id, cancellationToken);

            if (sourceProfil is null)
                return Result.Invalid(new ValidationError("Le profil énergie source est introuvable."));

            // 4. On récupère les autres profils dont le point d’accès est actif et consentant.
            var candidats = await context.ProfilsEnergie
                .AsNoTracking()
                .Join(
                    context.PointAccesses
                        .AsNoTracking()
                        .Include(pa => pa.User)
                        .Include(pa => pa.Membres),
                    profil => profil.PointAccessId,
                    pointAccess => pointAccess.Id,
                    (profil, pointAccess) => new
                    {
                        Profil = profil,
                        PointAccess = pointAccess
                    })
                .Where(c =>
                    c.PointAccess.Id != sourcePointAccess.Id
                    && c.PointAccess.EstActif
                    && c.PointAccess.AccordConsentement)
                .ToListAsync(cancellationToken);

            var resultats = candidats
                // 5. MVP : on garde uniquement les profils complémentaires.
                // Un vendeur voit des acheteurs, un acheteur voit des vendeurs.
                .Where(c => EstCompatible(sourceProfil, c.Profil))
                .Select(c =>
                {   // On remet toujours les données dans le bon ordre pour le DTO : vendeur puis acheteur.
                    var sourceEstVendeur = EstVendeur(sourceProfil);

                    var vendeurProfil = sourceEstVendeur ? sourceProfil : c.Profil;
                    var acheteurProfil = sourceEstVendeur ? c.Profil : sourceProfil;

                    var vendeurPoint = sourceEstVendeur ? sourcePointAccess : c.PointAccess;
                    var acheteurPoint = sourceEstVendeur ? c.PointAccess : sourcePointAccess;

                    var distance = CalculerDistanceKm(
                        sourcePointAccess.Latitude,
                        sourcePointAccess.Longitude,
                        c.PointAccess.Latitude,
                        c.PointAccess.Longitude);

                    // 6. MVP : un point déjà lié à une participation n’est pas disponible.
                    // Todo, plus tard affiner avec DateSortie / Statut si l'entité ParticipationPartage les expose.
                    var estDisponible = !c.PointAccess.Membres.Any();

                    return new PotentialMatchDto(
                        vendeurPoint.Id,
                        acheteurPoint.Id,
                        vendeurPoint.UserId,
                        acheteurPoint.UserId,
                        GetDisplayFirstName(vendeurPoint.User.FirstName),
                        GetDisplayFirstName(acheteurPoint.User.FirstName),
                        vendeurProfil.OffreEnergie_kWh,
                        acheteurProfil.DemandeEnergie_kWh,
                        vendeurProfil.PrixVenteCible_Eur,
                        acheteurProfil.PrixAchatCible_Eur,
                        distance,
                        estDisponible
                    );
                })
                .OrderBy(m => m.DistanceCalculee)
                .ToList();

            return Result.Success<IReadOnlyList<PotentialMatchDto>>(resultats);
        }

        private static bool EstCompatible(dynamic sourceProfil, dynamic candidatProfil)
        {
            return EstVendeur(sourceProfil) && EstAcheteur(candidatProfil)
                || EstAcheteur(sourceProfil) && EstVendeur(candidatProfil);
        }

        private static bool EstVendeur(dynamic profil)
            => profil.OffreEnergie_kWh is not null && profil.OffreEnergie_kWh > 0;

        private static bool EstAcheteur(dynamic profil)
            => profil.DemandeEnergie_kWh is not null && profil.DemandeEnergie_kWh > 0;

        private static decimal CalculerDistanceKm(
            double? lat1,
            double? lon1,
            double? lat2,
            double? lon2)
        {
            if (lat1 is null || lon1 is null || lat2 is null || lon2 is null)
                return 0;

            // Distance simplifiée pour MVP : suffisante pour trier les résultats par proximité
            //todo v2 : heversine ( -> limité à Bxl donc courbure de la terre négligable à cette échelle) ou API GoogleMaps/openRoute
            var dLat = lat2.Value - lat1.Value;
            var dLon = lon2.Value - lon1.Value;
            var distance = Math.Sqrt((dLat * dLat) + (dLon * dLon)) * 111; //*111 pour convertir les degrés en kilomètres (approximation) 1 degré = 111km sur Terre

            return Math.Round((decimal)distance, 2);
        }

        private static string GetDisplayFirstName(string? firstName)
        {
            return string.IsNullOrWhiteSpace(firstName)
                ? "Utilisateur"
                : firstName;
        }
    }
}