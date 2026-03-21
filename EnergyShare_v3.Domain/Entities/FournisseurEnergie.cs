using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class FournisseurEnergie
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = null!; //Nom du fournisseur d'énergie
        public string? Description { get; set; } //Description du fournisseur d'énergie
        public string? SiteWeb { get; set; } //Site web du fournisseur d'énergie
        public string? LogoUrl { get; set; } //URL du logo du fournisseur d'énergie
        
        
        public ICollection<PointAccess> PointsAccess { get; set; } = new List<PointAccess>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
