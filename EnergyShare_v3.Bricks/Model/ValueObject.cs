namespace EnergyShare_v3.Bricks.Model
{     // Classe de base pour les ValueObjects : égalité basée sur les valeurs et non un Id
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetAtomicValues();  // Classe de base pour les ValueObjects : égalité basée sur les valeurs et non un Id

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())   // Vérifie même type et non null
                return false;

            var other = (ValueObject)obj;

            return GetAtomicValues().SequenceEqual(other.GetAtomicValues());   // Compare chaque valeur (ValueObject = égalité par contenu)
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()      // Combine les hash des propriétés pour créer un hash unique
                .Aggregate(0, (current, obj) => HashCode.Combine(current, obj));
        }

        protected static bool EqualOperator(ValueObject? left, ValueObject? right)  // Permet d'utiliser == entre ValueObjects
        {      
            if (left is null ^ right is null)  // ou excusif ^ : XOR : un null et pas l'autre → false
                return false;

            return left is null || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)   // Permet d'utiliser != entre ValueObjects
        {
            return !EqualOperator(left, right);
        }
    }


}
