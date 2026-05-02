using Ardalis.Result;
using EnergyShare_v3.Domain.Entities.Matchs;
using EnergyShare_v3.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnergyShare_v3.Domain.Entities.Messages
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
        public string ObjetMessage { get; set; } = null!; // ex: "Demande de partage d'énergie"
        [Required]
        public string Contenu { get; set; } = null!; // Le corps du message
        [Required]
        public DateTime DateEnvoi { get; private set; } = DateTime.UtcNow;
        public bool IsLu { get; private set; } = false;

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

        //Méhtodes
        public void MarquerCommeLu() => IsLu = true;
        public void MarquerCommeNonLu() => IsLu = false;

        //Constructeurs
        //Constructeur
        private Message() { } // Constructeur privé pour EF Core
        private Message(string objet, string contenu, Guid expediteurId, Guid destinataire, Guid? matchId = null)
        {
            Id = Guid.NewGuid();
            ObjetMessage = objet;
            Contenu = contenu;
            ExpediteurId = expediteurId;
            DestinataireId = destinataire;
            MatchId = matchId;

        }

        public static Result<Message> Create(string objet, string contenu, Guid expediteurId, Guid destinataireId, Guid? matchId = null)
        {
            if (expediteurId == Guid.Empty)
                return MessageErrors.ExpediteurObligatoire().Map();

            if (destinataireId == Guid.Empty)
                return MessageErrors.DestinataireObligatoire().Map();

            if (expediteurId == destinataireId)
                return MessageErrors.ExpediteurEgaleDestinataire().Map();

            if (string.IsNullOrWhiteSpace(objet))
                return MessageErrors.ObjetObligatoire().Map();

            if (string.IsNullOrWhiteSpace(contenu))
                return MessageErrors.ContenuObligatoire().Map();

            return Result.Success(new Message(objet, contenu, expediteurId, destinataireId, matchId));
        }

       


    }
}
