using Ardalis.Result;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public static class DemandeGRDErrors
    {
        public static Result PartageObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(DemandeGRD.PartageId),
                "Le partage est obligatoire pour créer une demande GRD.",
                "DemandeGRD.PartageObligatoire",
                ValidationSeverity.Error));

        public static Result DemandeurObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(DemandeGRD.DemandeurId),
                "Le demandeur est obligatoire.",
                "DemandeGRD.DemandeurObligatoire",
                ValidationSeverity.Error));

        //gestion erreurs réponse dde traitement 
        public static Result TypeDemandeInvalide() =>
            Result.Invalid(new ValidationError(
                nameof(DemandeGRD.DemandeType),
                "Cette action est réservée aux demandes d'information de périmètre.",
                "DemandeGRD.TypeDemandeInvalide",
                ValidationSeverity.Error));

        public static Result DemandeDejaTraitee() =>
            Result.Invalid(new ValidationError(
                nameof(DemandeGRD.ResponseStatus),
                "Cette demande a déjà été traitée.",
                "DemandeGRD.DemandeDejaTraitee",
                ValidationSeverity.Error));

        public static Result AgentTraitantObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(DemandeGRD.AgentTraitantId),
                "L'agent traitant est obligatoire pour répondre à une demande GRD.",
                "DemandeGRD.AgentTraitantObligatoire",
                ValidationSeverity.Error));

    }
}