using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Web.Models.Partage
{
    // Requête envoyée par le GRD pour répondre à une demande de périmètre
    public record RepondreDemandePerimetreRequest(
        PerimetreType PerimetreConfirme,   // A, B, C ou D
        string? CommentaireReponseGRD      // commentaire optionnel du GRD
    );
}
