using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Util
{
    [Serializable]
    [ProtoContract]
    public class Optional<T>
    {
        public T Value
        { 
            get
            {
                if (!HasValue)
                {
                    throw new OptionalEmptyException<T>();
                }
                return value;
            }
        }

        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        [ProtoMember(1)]
        private T value;

        [ProtoMember(2)]
        private bool hasValue;

        private Optional()
        {}

        private Optional(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public static Optional<T> Empty()
        {
            return new Optional<T>();
        }

        public static Optional<T> Of(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null");
            }

            return new Optional<T>(value);
        }

        public static Optional<T> OfNullable(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                return new Optional<T>();
            }

            return new Optional<T>(value);
        }

        public T Get()
        {
            return Value;
        }

        public static implicit operator Optional<T>(T obj)
        {
            return new Optional<T>(obj);
        }

        public static explicit operator T(Optional<T> value)
        {
            return value.Value;
        }
    }

    public static class OptionalExtensions
    {
        public static bool IsPresent<T>(this Optional<T> optional)
        {
            return optional != null ? (optional).HasValue : false;
        }

        public static bool IsEmpty<T>(this Optional<T> optional)
        {
            return optional != null ? !((Optional<T>)optional).HasValue : true;
        }

        public static T OrElse<T>(this Optional<T> optional, T elseValue)
        {
            if (optional.IsEmpty())
            {
                return elseValue;
            }

            return (optional).Value;
        }
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
