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
    }
}