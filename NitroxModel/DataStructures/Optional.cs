using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Util
{
    [Serializable]
    [ProtoContract]
    public struct Optional<T> : ISerializable
    {
        [ProtoMember(1)]
        public T Value { get; private set; }

        [ProtoMember(2)]
        public bool HasValue { get; private set; }

        private Optional(T value)
        {
            Value = value;
            HasValue = true;
        }

        public bool IsPresent()
        {
            return HasValue;
        }

        public bool IsEmpty()
        {
            return !HasValue;
        }

        public T Get()
        {
            return Value;
        }

        public T OrElse(T elseValue)
        {
            if (IsEmpty())
            {
                return elseValue;
            }

            return Value;
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

        private Optional(SerializationInfo info, StreamingContext context)
        {
            Value = (T)info.GetValue("value", typeof(T));
            HasValue = info.GetBoolean("hasValue");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Value);
            info.AddValue("hasValue", HasValue);
        }

        public static implicit operator Optional<T>(T obj)
        {
            if (obj == null) // null is passed when T is a reference type
            {
                return new Optional<T>();
            }
            return new Optional<T>(obj);
        }

        public static explicit operator T(Optional<T> value)
        {
            return value.Value;
        }
    }

    public sealed class OptionalNullException<T> : Exception
    {
        public OptionalNullException() : base($"Optional <{nameof(T)}> is null!")
        {}

        public OptionalNullException(string message) : base($"Optional <{nameof(T)}> is null:\n\t{message}")
        {}
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
