namespace EnergyShare_v3.Bricks.Model
{
   /* public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetAtomicValues();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;

            return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()
                .Aggregate(0, (current, obj) => HashCode.Combine(current, obj));
        }

        protected static bool EqualOperator(ValueObject? left, ValueObject? right)
        {      
            if (left is null ^ right is null)  // ou excusif ^
                return false;

            return left is null || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
        {
            return !EqualOperator(left, right);
        }
    }

*/
}
