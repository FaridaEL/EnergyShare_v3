using EnergyShare_v3.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EnergyShare_v3.Domain.Entities
{
    public class Message
    {
        /*  but de l'entité message : 
         *  Premier pas : Suite à un match,le user clique sur "Contacter" et remplit le formulaire de message.
         *  Notification : Le système envoie un e-mail au Vendeur pour dire "Vous avez un nouveau message sur EnergyShare".
         *  Échange de coordonnées : Une fois que le courant passe (littéralement!),
         *  ils utilisent le message pour se donner rendez-vous, s'échanger leurs numéros, signer la convention de partage.
            Conforme RGPD : On évite d'afficher des données de contact (téléphone, mail) lors de la recherche de match.
         */

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Objet { get; set; } = null!; // ex: "Demande de partage d'énergie"
        [Required]
        public string Contenu { get; set; } = null!; // Le corps du message
        [Required]
        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
        public bool? IsLu { get; set; } = false;

        [Required]
        public Guid ExpediteurId { get; set; }
        [ForeignKey("ExpediteurId")]
        public User Expediteur { get; set; } = null!;

        public Guid DestinataireId { get; set; }
        [ForeignKey("DestinataireId")]
        public User Destinataire { get; set; } = null!;

        public Guid? MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match? Match { get; set; }

    }

   
}
