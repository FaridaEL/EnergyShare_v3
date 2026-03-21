using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnergyShare_v3.Domain.Enums
{
    public enum PartageEnergieStatutType
    {

        [Display(Name = "Inactif")] //par défaut, le partage est fraichement crée, Sibelga doit valider le partage
        Inactif = 1,
        [Display(Name = "En attente de validation")]   // Le partage est en cours de validation par l'organisme public (OP)
        EnAttenteValidation = 2,
        [Display(Name = " Actif")] //dès que  le partage est validé par OP, il passe à l'état actif et le partage d'énergie peut véritablement commencer
        Actif = 3,
        [Display(Name = "En attente de modification")] // Lors d'une demande de modificaiton (type ModificationPartage)
        EnAttenteModification = 4,
        [Display(Name = "Suspendu")]   // si la demande de modification du partage est refusée par l'OP, le partage passe à l'état suspendu et ne peut plus partager d'énergie tant que les problèmes ne sont pas résolus
        Suspendu = 5,
        [Display(Name = "En cours de cloture")]  // statut du partage après la fin de la période de partage prévue ou le début de préavis de 3 semaines pour un P2P
        EnCoursCloture = 6,
        [Display(Name = "Cloturé")]   // dès que la clôture du partage est confirmée par l'OP, le partage passe à l'état clôturé et ne peut plus partager d'énergie
        Cloture = 7

    }
}
