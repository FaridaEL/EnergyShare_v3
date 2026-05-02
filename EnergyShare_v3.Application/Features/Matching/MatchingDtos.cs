using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{
    
    
    public record SavedMatchSummaryDto( 
         /*On récupère un match existant et enregistré*/
        Guid MatchId,
        Guid PointAccessVendeurId,
        Guid PointAccessAcheteurId,

        Guid VendeurUserId,
        Guid AcheteurUserId,
        string PrenomVendeurMatch,
        string PrenomAcheteurMatch,

        Guid ContactUserId,     // pour envoyer le message  : dépend du context qui est connecté c'est soit le vendeur soit l'acheteur
        string ContactPrenom,    // pour afficher dans l'UI
        
        decimal DistanceCalculee,
        DateTime CreatedAt
    );

    public record PotentialMatchDto(
       //DTO pour afficher le résulat d'une recherche de match
       /*pas d'Id : un match potentiel ne peut pas encore être enregistré en base de données.
        D'abord un calcul est fait à la volée et qui est non persisté, on affiche donc des suggestions de match à l'utilisateur,
       Puis, si il y a un intéret de l'utilisateur, ce dernier clique sur je suis interessé, "sauver match pour contacter plus tard" ou "contacter"*/
       Guid PointAccessVendeurId,
       Guid PointAccessAcheteurId,
       Guid VendeurUserId,
       Guid AcheteurUserId,
       string PrenomVendeurMatch,
       string PrenomAcheteurMatch,
       decimal? OffreEnergie_kWh,
       decimal? DemandeEnergie_kWh,
       decimal? PrixVenteCible_Eur,
       decimal? PrixAchatCible_Eur,
       decimal DistanceCalculee,
       bool EstDisponible
   );

}


