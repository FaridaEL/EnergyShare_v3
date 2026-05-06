namespace EnergyShare_v3.Web.Models.Partage
{
    public record RepondreDemandeValidationPartageRequest(
        bool IsValide,
        string? CommentaireReponseGRD
    );
}