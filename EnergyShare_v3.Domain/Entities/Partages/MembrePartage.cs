using Ardalis.Result;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Partages
{
    public class MembrePartage
    {
        //règle 1 point EAN ne peut appartenir qu'a un seul partage à la fois.
        [Key]
        public Guid Id { get; set; }
        public bool IsInterlocuteurUnique { get; private set; } = false;
        public UserRolePartage UserRolePartage { get; set; } //acheteur ou vendeur dans le partage.
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExitAt { get; private set; }
        public DateTime? DateCommunicationPreavis { get; private set; }
        public DateTime? DateSortiePlanifiee { get; private set; } //peut être calculé à partir de datePravisDonnées + 3 semaines? 

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

        /*  pour info avant result : 
         public void DefinirCommeInterlocuteurUnique()
      {
          if (UserRolePartage != UserRolePartage.Vendeur)
              throw new InvalidOperationException("Seul un vendeur peut être interlocuteur unique.");

          if (!PointAccess.IsInjectionPoint)
              throw new InvalidOperationException("L'interlocuteur unique doit disposer d'un point d'injection.");

          IsInterlocuteurUnique = true;
      }*/

        public Result DefinirCommeInterlocuteurUnique()
        {
            if (UserRolePartage != UserRolePartage.Vendeur)
               return MembrePartageErrors.InterlocuteurUniqueDoitEtreVendeur(UserId);

           if (!PointAccess.IsInjectionPoint)
                return MembrePartageErrors.PointInjectionRequis(PointAccessId);
           
            IsInterlocuteurUnique = true;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }


        //Règles de gestion RG-E018 à RG-E023 : adhésion et sortie du partage
        //Impact pour un pair-to-pair  sur le statut du partage qui passe en Démarrer cloture dès que
        //le préavis de 3 semaines est communiqué par un membre du partage
        //--> MAis statut partage ne peut pas être géré ici car un risque de couplage fort -- <gestion dans l'appication
        public Result CommuniquerPreavis(DateTime dateCommunication)    
        {
            if (ExitAt.HasValue)
               return MembrePartageErrors.MembreDejaSorti(UserId);

            if (dateCommunication < JoinedAt)
                return MembrePartageErrors.DatePreavisAvantEntree(UserId);
            
            DateCommunicationPreavis = dateCommunication;
            DateSortiePlanifiee = dateCommunication.AddDays(21);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Quitter(DateTime dateSortie)    //Impact pour un pair-to-pair  : le partage passe en clôturé dès que le membre quitte le partage après les 3 semaines de préavis.
        {
            if (dateSortie < JoinedAt)
                return MembrePartageErrors.DateSortieAvantEntree(UserId);

            if (DateSortiePlanifiee.HasValue && dateSortie < DateSortiePlanifiee.Value)
                return MembrePartageErrors.PreavisNonRespecte(UserId);

            ExitAt = dateSortie;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        [NotMapped]
        public bool EstActif => ExitAt is null; // le membre est toujours actif tant qu'il n'y a pas de date de sortie.

    }
}
