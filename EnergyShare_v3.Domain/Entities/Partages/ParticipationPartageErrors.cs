using Ardalis.Result;
using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public static class ParticipationPartageErrors
    {
        public static Result InterlocuteurUniqueDoitEtreVendeur(Guid pointAccessId) =>
            Result.Invalid(new ValidationError(
                pointAccessId.ToString(),
                "Seul un vendeur peut être interlocuteur unique.",
                "ParticipationPartage.InterlocuteurUniqueDoitEtreVendeur",
                ValidationSeverity.Error));

        public static Result PointInjectionRequis(Guid pointAccessId) =>
          Result.Invalid(new ValidationError(
              pointAccessId.ToString(),
              "L'interloctueur unique doit disposer d'un point d'injection.",
              "ParticipationPartage.PointInjectionRequis",
              ValidationSeverity.Error));

        public static Result MembreDejaSorti(Guid membreId) =>
        Result.Invalid(new ValidationError(
            membreId.ToString(),
            "Le membre a déjà quitté le partage.",
            "ParticipationPartage.MembreDejaSorti",
            ValidationSeverity.Error));

        public static Result DatePreavisAvantEntree(Guid membreId) =>
           Result.Invalid(new ValidationError(
               membreId.ToString(),
               "La date de préavis ne peut pas être antérieure à la date d'entrée.",
               "ParticipationPartage.DatePreavisAvantEntree",
               ValidationSeverity.Error));

        public static Result DateSortieAvantEntree(Guid membreId) =>
            Result.Invalid(new ValidationError(
                membreId.ToString(),
                "La date de sortie ne peut pas être antérieure à la date d'entrée.",
                "ParticipationPartage.DateSortieAvantEntree",
                ValidationSeverity.Error));

        public static Result PreavisNonRespecte(Guid membreId) =>
            Result.Invalid(new ValidationError(
                membreId.ToString(),
                "Le délai de préavis de 3 semaines n'est pas respecté.",
                "ParticipationPartage.PreavisNonRespecte",
                ValidationSeverity.Error));

        //Todo Cette erreur sert surtout à être utilisée dans le handler / service applicatif qui ajoute un membre à un partage
        // pas directement dans MembrePartage seul.
        // Car pour savoir si un PointAccess est déjà dans un partage actif, il faut regarder :
        // - les autres MembrePartage , - leur ExitAt , - le Statut du Partage

        public static Result PointAccessDejaDansUnPartageActif(Guid pointAccessId) =>
           Result.Invalid(new ValidationError(
               pointAccessId.ToString(),
               "Ce point d'accès participe déjà à un partage actif.",
               "ParticipationPartage.PointAccessDejaDansUnPartageActif",
               ValidationSeverity.Error));

        public static Result PartageObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(ParticipationPartage.PartageId),
                "Le partage est obligatoire.",
                "ParticipationPartage.PartageObligatoire",
                ValidationSeverity.Error));

        public static Result PointAccessObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(ParticipationPartage.PointAccessId),
                "Le point d'accès est obligatoire.",
                "ParticipationPartage.PointAccessObligatoire",
                ValidationSeverity.Error));

    }
}
