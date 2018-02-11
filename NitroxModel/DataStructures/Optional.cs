using System;
using NitroxModel.Helper;

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
            if (value.Equals(default(T)))
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
            if (IsEmpty())
            {
                throw new InvalidOperationException("Optional did not have a value");
            }

            return value;
        }

        public bool IsPresent()
        {
            return !IsEmpty();
        }

        public bool IsEmpty()
        {
            return value == null || value.Equals(default(T));
        }

        public T OrElse(T elseValue)
        {
            if (IsPresent())
            {
                return value;
            }

            return elseValue;
        }

        public override string ToString()
        {
            if (IsEmpty())
            {
                return "Optional<" + typeof(T) + ">.Empty()";
            }

            return "Optional[" + Get().ToString() + "]";
        }

        public Optional<T> Then(Func<T, Optional<T>> continueFunc)
        {
            Validate.NotNull(continueFunc);

            if (IsPresent())
            {
                return continueFunc(value);
            }

            return Empty();
        }

        public bool Then(Action<T> continueAction)
        {
            Validate.NotNull(continueAction);

            if (IsPresent())
            {
                continueAction(value);
                return true;
            }

            return false;
        }
    }

    [Serializable]
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
