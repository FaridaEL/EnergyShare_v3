using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Enums
{
    public enum DdeGRDResponseStatus
    {

        [Display(Name = "Valide")]  // traité   : dde info périmètre --> réponse apportée
        Valide = 1,
        [Display(Name = "Refus")]   //traité  ; si dde info périmetre mais que les addresses ne sont pas correctes, ou autre 
        Refus = 2,
        [Display(Name = "En Attente")] //EnAttente
        EnAttente = 3

    }
}
