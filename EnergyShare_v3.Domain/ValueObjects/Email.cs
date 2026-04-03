using Ardalis.Result;
using EnergyShare_v3.Bricks.Model;
using System.Text.RegularExpressions;

namespace EnergyShare_v3.Domain.ValueObjects
{
    // Value Object : un email est défini par sa valeur, pas par un Id.
    public sealed class Email : ValueObject
    {
        public string Value { get; private set; } = null!;

        private Email() { } // Pour EF Core

        private Email(string value)  // Constructeur privé → création contrôlée via Create()
        {
            Value = value;
        }

        // Factory : valide + normalise avant création
        public static Result<Email> Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Result<Email>.Invalid(new ValidationError(
                    nameof(Email),
                    "L'email est obligatoire."));

            var normalized = input.Trim().ToLowerInvariant();

            if (!IsValid(normalized))
                return Result<Email>.Invalid(new ValidationError(
                    nameof(Email),
                    "Le format de l'email est invalide."));

            return Result.Success(new Email(normalized));
        }

        // Vérifie le format email (regex simple)
        private static bool IsValid(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        // Définit les valeurs utilisées pour comparer deux Email
        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;   // Conversion en string (pratique pour affichage) -> permet de faire Console.WriteLine(user.Email) au lieu de Console.WriteLine(user.Email.Value);;

        public static implicit operator string(Email email) => email.Value;  // Permet d'utiliser Email comme une string automatiquement  -> permet de string email = user.Email; au lieu de string email = user.Email.Value; 
    }
}
