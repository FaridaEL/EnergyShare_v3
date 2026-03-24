using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Pipes;

namespace EnergyShare_v3.Domain.Entities
{
    public class MembrePartage
    {
        //règle 1 point EAN ne peut appartenir qu'a un seul partage à la fois.
        [Key]
        public Guid Id { get; set; }
        public bool IsInterlocuteurUnique { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExitAt { get; set; }
        public DateTime? DateCommunicationPreavis { get; set; }
        public DateTime? DateSortiePlanifiee { get; set; } //peut être calculé à partir de datePravisDonnées + 3 semaines? 

        //Enumérations
        public Guid UserId { get; set; } 
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public Guid PartageId { get; set; } 
        [ForeignKey("PartageId")]
        public Partage Partage { get; set; } = null!;
        public Guid PointAccessId { get; set; }
        [ForeignKey("PointAccessId")]
        public PointAccess PointAccess { get; set; } = null!;

    
        // Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        //interlocuteur unique : le vendeur-producteur est l'interlocuteur unique du partage, il est le seul à pouvoir créer le partage et à communiquer le préavis de sortie du partage.

        /*public void DesignerCommeInterlocuteurUnique()
        {
            if( UserId.UserRole != UserRole.Vendeur)
                throw new InvalidOperationException("Seul le vendeur-producteur peut être désigné comme interlocuteur unique du partage.");
            IsInterlocuteurUnique = true;
        } */

        //Règles de gestion RG-E018 à RG-E023 : adhésion et sortie du partage
        //Impact pour un pair-to-pair  sur le statut du partage qui passe en Démarrer cloture dès que
        //le préavis de 3 semaines est communiqué par un membre du partage
        //--> MAis statut partage ne peut pas être géré ici car un risque de couplage fort -- <gestion dans l'appication
        public void CommuniquerPreavis(DateTime dateCommunication)    
        {
            if (ExitAt.HasValue)
                throw new InvalidOperationException("Le membre a déjà quitté le partage.");

            if (dateCommunication < JoinedAt)
                throw new InvalidOperationException("La date de préavis ne peut pas être antérieure à la date d'entrée.");

            DateCommunicationPreavis = dateCommunication;
            DateSortiePlanifiee = dateCommunication.AddDays(21);
        }

        public void Quitter(DateTime dateSortie)    //Impact pour un pair-to-pair  : le partage passe en clôturé dès que le membre quitte le partage après les 3 semaines de préavis.
        {
            if (dateSortie < JoinedAt)
                throw new InvalidOperationException("La date de sortie ne peut pas être antérieure à la date d'entrée.");

            if (DateSortiePlanifiee.HasValue && dateSortie < DateSortiePlanifiee.Value)
                throw new InvalidOperationException("Le délai de préavis de 3 semaines n'est pas respecté.");

            ExitAt = dateSortie;
        }

        [NotMapped]
        public bool EstActif => ExitAt is null; // le membre est toujours actif tant qu'il n'y a pas de date de sortie.

    }
}
