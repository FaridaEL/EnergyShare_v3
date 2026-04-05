using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities.Users
{
    public static class UserErrors
    {
        public static Result EmailObligatoire() =>
           Result.Invalid(new ValidationError(
               nameof(User.Email),
               "L'email est obligatoire.",
               "User.EmailObligatoire",
               ValidationSeverity.Error));

        public static Result PasswordHashObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(User.PasswordHash),
                "Le mot de passe hashé est obligatoire.",
                "User.PasswordHashObligatoire",
                ValidationSeverity.Error));

        public static Result SocieteReserveeAuProfessionnel() =>
            Result.Invalid(new ValidationError(
                nameof(User.SocieteName),
                "Seul un utilisateur professionnel peut avoir un nom de société.",
                "User.SocieteReserveeAuProfessionnel",
                ValidationSeverity.Error));

        /*public static Result NumeroEntrepriseReserveAuProfessionnel() =>
            Result.Invalid(new ValidationError(
                nameof(User.NumeroEntreprise),
                "Seul un utilisateur professionnel peut avoir un numéro d’entreprise.",
                "User.NumeroEntrepriseReserveAuProfessionnel",
                ValidationSeverity.Error));  */

        public static Result NomSocieteRequisSiNumeroEntreprise() =>
            Result.Invalid(new ValidationError(
                nameof(User.SocieteName),
                "Le nom de société est requis lorsqu'un numéro d'entreprise est renseigné.",
                "User.NomSocieteRequisSiNumeroEntreprise",
                ValidationSeverity.Error));

        public static Result NomSocieteObligatoirePourPersonneMorale() =>
            Result.Invalid(new ValidationError(
                nameof(User.SocieteName),
                "Le nom de société est obligatoire pour une personne morale.",
                "User.NomSocieteObligatoirePourPersonneMorale",
                ValidationSeverity.Error));
    }
}
