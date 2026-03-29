using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Domain.Entities.Messages
{
    public static class MessageErrors
    {
        public static Result ObjetObligatoire() =>
          Result.Invalid(new ValidationError(
              nameof(Message.ObjetMessage),
              "Veuillez renseigner un objet.",
              "Message.ObjetObligatoire",
              ValidationSeverity.Error));

        public static Result ContenuObligatoire() =>
            Result.Invalid(new ValidationError(
                nameof(Message.Contenu),
                "Veuillez indiquer votre message.",
                "Message.ContenuObligatoire",
                ValidationSeverity.Error));

    }
}
