using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyShare_v3.Domain.Entities
{
    public class OrganismePublic
    {

        [Key]    
        public Guid Id { get; set; } 
        
        [Required]
        public string Nom { get; set; } = "Sibelga"; // "Sibelga" ou "Brugel"  --> défaut = Sibelga dans cette v1 Brugel valida les communautés d'energie

        //Enumérations
        public OrganismePublicType TypeOrganisme { get; set; } = OrganismePublicType.GRD; // Valeurs : "GRD" par défaut  si type organisme = même batiment ou pair to pair
       
        public ICollection<DdeValidationPartage> DdesValidationPartages { get; set; } = new List<DdeValidationPartage>();
        public ICollection<DdeInfoPerimetre> DdesInfosPerimetre { get; set; } = new List<DdeInfoPerimetre>();
        public ICollection<User> Employes {  get; set; } = new List<User>();

        
        //Données d'audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

   
}
