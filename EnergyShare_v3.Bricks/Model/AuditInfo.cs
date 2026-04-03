using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Bricks.Model
// Contient les infos de traçabilité (création / modification).
// C’est un ValueObject car il n’a pas d’identité propre, seulement des valeurs.
{
    [Owned] // [Owned] indique que cet objet n'est pas une entité indépendante.Il est stocké dans la même table que l'entité qui le possède (ex: User, Match, Partage).
    
    public class AuditInfo
    {
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string? CreatedBy { get; private set; }

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; private set; }

        public void SetCreated(string? user)
        {
            CreatedBy = user;
        }

        public void Touch(string? user)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = user;
        }
    }
}
