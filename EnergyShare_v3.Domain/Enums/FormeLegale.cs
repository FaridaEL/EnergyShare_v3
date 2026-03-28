using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum FormeLegale
    {
      
        [Display(Name = "Ambassade")]
        Ambassade = 1,

        [Display(Name = "ASBL (Association sans but lucratif)")]
        ASBL = 2,

        [Display(Name = "Association de copropriétaires")]
        AssociationCoProprietaire = 3,

        [Display(Name = "Autre forme juridique")]
        Autre = 4,

        [Display(Name = "GMBH")]
        GMBH = 5,

        [Display(Name = "LTD (Limited Company)")]
        LTD = 6,

        [Display(Name = "Personne physique (indépendant)")]
        PersonnePhysique = 7,

        [Display(Name = "SA (Société anonyme)")]
        SA = 8,

        [Display(Name = "SARL")]
        SARL = 9,

        [Display(Name = "SC (Société coopérative)")]
        SC = 10,

        [Display(Name = "SCRI (Société coopérative à responsabilité illimitée)")]
        SCRI = 11,

        [Display(Name = "SCRL (Société coopérative à responsabilité limitée)")]
        SCRL = 12,

        [Display(Name = "SNC (Société en nom collectif)")]
        SNC = 13,

        [Display(Name = "SPA")]
        SPA = 14,

        [Display(Name = "SPRL (ancienne forme)")]
        SPRL = 15,

        [Display(Name = "SPRLU (ancienne forme unipersonnelle)")]
        SPRLU = 16,

        [Display(Name = "SRL (Société à responsabilité limitée)")]
        SRL = 17

    }
}
