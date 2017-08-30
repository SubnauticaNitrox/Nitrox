using System;

namespace NitroxModel.DataStructures.Util
{
    [Serializable]
    public struct Optional<T>
    {
        private T value;

        private Optional(T value)
        {
            this.value = value;
        }

        public static Optional<T> Empty()
        {
            return new Optional<T>();
        }

        public static Optional<T> Of(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Value cannot be null");
            }

            return new Optional<T>(value);
        }

        public static Optional<T> OfNullable(T value)
        {
            return new Optional<T>(value);
        }

        public T Get()
        {
            if (value == null)
            {
                throw new InvalidOperationException("Optional did not have a value");
            }

            return value;
        }

        public bool IsPresent()
        {
            return value != null;
        }

        public bool IsEmpty()
        {
            return value == null;
        }

        public T OrElse(T elseValue)
        {
            if (value != null)
            {
                return value;
            }

            return elseValue;
        }

        // TODO: Discuss whether these functions should be in Validate (as .NotNull overload or .NotEmpty).
        public void AssertPresent()
        {
            if (IsEmpty())
            {
                throw new OptionalEmptyException<T>();
            }
        }

        public void AssertPresent(string message)
        {
            if (IsEmpty())
            {
                throw new OptionalEmptyException<T>(message);
            }
        }

        public override string ToString()
        {
            if (IsEmpty())
            {
                return "Optional<" + typeof(T) + ">.Empty()";
            }

            return "Optional[" + Get().ToString() + "]";
        }
    }

    public sealed class OptionalEmptyException<T> : Exception
    {
        public OptionalEmptyException() : base($"Optional <{nameof(T)}> is empty.")
        {
        }

        public OptionalEmptyException(string message) : base($"Optional <{nameof(T)}> is empty:\n\t" + message)
        {
        }
    }
}
