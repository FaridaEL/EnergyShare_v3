using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Web.Models.Partage
{
    public class PartageDetailsFormModel
    {
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PartageEnergieType EnergieType { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
