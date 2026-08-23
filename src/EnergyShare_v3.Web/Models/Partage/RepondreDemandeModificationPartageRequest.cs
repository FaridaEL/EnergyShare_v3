namespace EnergyShare_v3.Web.Models.Partage
{
    public record RepondreDemandeModificationPartageRequest(
        bool IsValide,
        string? CommentaireReponseGRD
    );
}