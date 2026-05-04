using EnergyShare_v3.Bricks.Model;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Entities.Users
{
    public class OrganismePublic   :IAuditable
    {

        [Key]    
        public Guid Id { get; set; } 
        
        [Required]
        public string Nom { get; set; } = "Sibelga"; // "Sibelga" ou "Brugel"  --> défaut = Sibelga dans cette v1 Brugel valida les communautés d'energie

        public ICollection<DemandeGRD> DemandesGrd { get; set; } = [];
        public ICollection<User> Employes {  get; set; } = [];
        //Données d'audit
        public AuditInfo Audit { get; private set; } = new AuditInfo();
    }
  
}
