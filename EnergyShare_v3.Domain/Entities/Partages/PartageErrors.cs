using Ardalis.Result;
using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public static class PartageErrors
    {

        public static Result NomObligatoire() =>
          Result.Invalid(new ValidationError(
              nameof(Partage.Nom),
              "Le nom du partage ne peut pas être vide.",
              "Partage.NomObligatoire",
              ValidationSeverity.Error));

        public static Result PartageEnCoursDeCloture() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Impossible d'effectuer cette action sur un partage en cours de clôture.",
                "Partage.EnCoursCloture",
                ValidationSeverity.Error));

        public static Result PartageCloture() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Impossible d'effectuer cette action sur un partage clôturé.",
                "Partage.Cloture",
                ValidationSeverity.Error));

        public static Result NombreMembresPairToPairInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Membres),
                "Un partage pair-à-pair doit contenir exactement deux membres.",
                "Partage.NombreMembresPairToPairInvalide",
                ValidationSeverity.Error));

        public static Result NombreMembresMemeBatimentInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Membres),
                "Un partage de type même bâtiment doit contenir au moins deux membres.",
                "Partage.NombreMembresMemeBatimentInvalide",
                ValidationSeverity.Error));

        public static Result SoumissionGrdImpossible() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Seul un partage inactif peut être soumis au GRD.",
                "Partage.SoumissionGrdImpossible",
                ValidationSeverity.Error));

        public static Result ValidationGrdImpossible() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Le partage doit être en attente de validation.",
                "Partage.ValidationGrdImpossible",
                ValidationSeverity.Error));

        public static Result ModificationImpossible() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Seul un partage actif peut passer en attente de modification.",
                "Partage.ModificationImpossible",
                ValidationSeverity.Error));

        public static Result ValidationModificationGrdImpossible() =>
           Result.Invalid(new ValidationError(
               nameof(Partage.Statut),
               "Le partage doit être en attente de modification.",
               "Partage.ValidationModificationGrdImpossible",
               ValidationSeverity.Error));

        public static Result ClotureImpossible() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Le partage doit être en cours de clôture.",
                "Partage.ClotureImpossible",
                ValidationSeverity.Error));

        public static Result DemarrageClotureImpossible() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.Statut),
                "Seul un partage actif peut entrer en cours de clôture.",
                "Partage.DemarrageClotureImpossible",
                ValidationSeverity.Error));

        public static Result VendeurObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.VendeurId),
                "Le vendeur est obligatoire pour créer un partage.",
                "Partage.VendeurObligatoire",
                ValidationSeverity.Error));

        public static Result DateFinAvantDateDebut() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.DateFin),
                "La date de fin ne peut pas être antérieure à la date de début.",
                "Partage.DateFinAvantDateDebut",
                ValidationSeverity.Error));

        public static Result InvitationCodeIntrouvable() =>
            Result.Invalid(new ValidationError(
                nameof(Partage.InvitationCode),
                "Aucun partage ne correspond à ce code d’invitation.",
                "Partage.InvitationCodeIntrouvable",
                ValidationSeverity.Error));
        public static Result InvitationCodeExpire() =>
           Result.Invalid(new ValidationError(
               nameof(Partage.InvitationCode),
               "Ce code d’invitation a expiré. Demandez un nouveau code au créateur du partage.",
               "Partage.InvitationCodeExpire",
               ValidationSeverity.Error));


    }
}
