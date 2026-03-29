using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities.ProfilsEnergie
{
    public static class ProfilEnergieErrors
    {
        public static Result OffreOuDemandeRequise() =>
            Result.Invalid(new ValidationError(
                "",
                "Un profil doit contenir au moins une offre ou une demande d'énergie.",
                "ProfilEnergie.OffreOuDemandeRequise",
                ValidationSeverity.Error));

        public static Result ConsentementRequis() =>
            Result.Invalid(new ValidationError(
                "",
                "Le consentement est requis pour participer au matching.",
                "ProfilEnergie.ConsentementRequis",
                ValidationSeverity.Error));

        public static Result ValeurNegative(string champ) =>
            Result.Invalid(new ValidationError(
                champ,
                "La valeur ne peut pas être négative.",
                "ProfilEnergie.ValeurNegative",
                ValidationSeverity.Error));
    }
}

