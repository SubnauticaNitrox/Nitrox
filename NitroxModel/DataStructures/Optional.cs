using System;

namespace NitroxModel.DataStructures.Util
{
    [Serializable]
    public class HasValueOptional<T> : Optional<T>
    {
        private readonly T value;

        public HasValueOptional(T value)
        {
            this.value = value;
        }

        public override T Get()
        {
            return value;
        }

        public override bool IsPresent()
        {
            return true;
        }

        public override bool IsEmpty()
        {
            return false;
        }

        public override T OrElse(T elseValue)
        {
            return value;
        }

        public override string ToString()
        {
            return $"Optional[{Get()}]";
        }
    }

    [Serializable]
    public class NoValueOptional<T> : Optional<T>
    {
        public override T Get()
        {
            throw new InvalidOperationException("Optional did not have a value");
        }

        public override bool IsPresent()
        {
            return false;
        }

        public override bool IsEmpty()
        {
            return true;
        }

        public override T OrElse(T elseValue)
        {
            return elseValue;
        }

        public override string ToString()
        {
            return $"Optional<{typeof(T)}>.Empty()";
        }
    }

    [Serializable]
    public abstract class Optional<T>
    {
        public static Optional<T> Empty()
        {
            return new NoValueOptional<T>();
        }

        public static Optional<T> Of(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null");
            }

            return new HasValueOptional<T>(value);
        }

        public static Optional<T> OfNullable(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                return new NoValueOptional<T>();
            }

            return new HasValueOptional<T>(value);
        }

        public abstract T Get();
        public abstract bool IsPresent();
        public abstract bool IsEmpty();
        public abstract T OrElse(T elseValue);
    }
    
    [Serializable]
    public sealed class OptionalEmptyException<T> : Exception
    {
        public OptionalEmptyException() : base($"Optional <{nameof(T)}> is empty.")
        {
        }

        public OptionalEmptyException(string message) : base($"Optional <{nameof(T)}> is empty:\n\t{message}")
        {
        }
    }
}
