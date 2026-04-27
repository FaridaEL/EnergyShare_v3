using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities.PointsAccesses
{
    public static class PointAccessErrors
    {
        public static Result ConsentementRequis() =>
            Result.Invalid(new ValidationError(
                "",
                "Le consentement est requis pour participer au matching.",
                "PointAccess.ConsentementRequis",
                ValidationSeverity.Error));

        public static Result UserObligatoire() =>
           Result.Invalid(new ValidationError(
               nameof(PointAccess.UserId),
               "L'utilisateur est obligatoire.",
               "PointAccess.UserObligatoire",
               ValidationSeverity.Error));

        public static Result AdresseObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(PointAccess.AdresseLine1),
                "L'adresse est obligatoire.",
                "PointAccess.AdresseObligatoire",
                ValidationSeverity.Error));

        public static Result CodePostalInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(PointAccess.CodePostal),
                "Le code postal doit contenir 4 chiffres.",
                "PointAccess.CodePostalInvalide",
                ValidationSeverity.Error));

        public static Result FournisseurObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(PointAccess.Fournisseur),
                "Le fournisseur d'énergie est obligatoire.",
                "PointAccess.FournisseurObligatoire",
                ValidationSeverity.Error));

        public static Result SmartMeterInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(PointAccess.SmartMeter_Encrypted),
                "Le numéro de compteur intelligent doit commencer par 1SJ et contenir maximum 20 caractères.",
                "PointAccess.SmartMeterInvalide",
                ValidationSeverity.Error));

        public static Result EanInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(PointAccess.EAN_Encrypted),
                "Le code EAN doit commencer par 5414489 et contenir 18 chiffres.",
                "PointAccess.EanInvalide",
                ValidationSeverity.Error));
    }
}
