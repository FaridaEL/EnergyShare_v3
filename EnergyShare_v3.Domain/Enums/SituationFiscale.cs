using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum SituationFiscale
    {
        /*  Assujeti TVA : // Une personne physique ou morale assujettie à la TVA facture l'électricité partagée avec des montant HTVA et des montants TVAC.
         *  Elle peut appliquer un % de 6% sur les différentes composantes de son prix aux consommateurs résidentiels. Les professionnels eux paieront 21%. 
         
            Non Assujetti :Un non-assujetti (personne physique ou morale) ne peut pas inclure de TVA dans les factures qu'il émet.
        Les factures produites indiqueront uniquement des montants TTC et ne pourront pas être utilisées par les consommateurs assujettis pour déduire la TVA. 
        Les particuliers ou les Associations de copropriétaire jouissant d'une installation PV de moins de 10 kVA, sont non assujettis à la TVA. 

         Franchisé TVA : La franchise est un régime accessible aux personnes physiques ou morales assujetties à la TVA,
        dont le chiffre d'affaires annuel (vente élec + vente CV) est inférieur à 25.000 €/an. 
        Ce régime permet de réduire les démarches administratives des assujettis et impose un format spécifique pour les factures émises
        (sans mention du montant HTVA ou TVAC, mais avec un montant TTC).
        En conséquence, cette facture ne pourra pas être utilisée par les consommateurs assujettis qui déclarent la TVA de leurs factures.

         */
        [Display(Name = "Assujetti à la TVA")]  
        AssujettiTVA = 1,
        [Display(Name = "Non Assujeti")]    // uniquement communauté d'énergie
        NonAssujetti = 2,
        [Display(Name = "Franchisé à la TVA")]
        FranchiseTVA = 3

    }
}
