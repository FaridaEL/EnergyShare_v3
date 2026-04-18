using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Users
{
    /*
    RefreshToken = jeton long de durée de vie plus longue que l'access token.
    Il permet d'obtenir une nouvelle paire de tokens sans redemander le login.
    Lors d'un refresh, l'ancien token sera révoqué puis remplacé par un nouveau.
*/
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;
    }
}
