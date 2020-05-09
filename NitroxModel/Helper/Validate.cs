using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;

namespace NitroxModel.Helper
{
    public static class Validate
    {
        public static void NotNull<T>(T o)
            // Prevent non-nullable valuetypes from getting boxed to object.
            // In other words: Error when trying to assert non-null on something that can't be null in the first place.
            where T : class
        {
            if (o != null)
            {
                return;
            }
            
            Optional<string> paramName = GetParameterName<T>();
            if (paramName.HasValue)
            {
                throw new ArgumentNullException(paramName.Value);
            }
            throw new ArgumentNullException();
        }

        public static void NotNull<T>(T o, string message)
            where T : class
        {
            if (o != null)
            {
                return;
            }
            
            Optional<string> paramName = GetParameterName<T>();
            if (paramName.HasValue)
            {
                throw new ArgumentNullException(paramName.Value, message);
            }
            throw new ArgumentNullException(message);
        }

        public static void IsTrue(bool b)
        {
            if (!b)
            {
                throw new ArgumentException();
            }
        }

        public static void IsTrue(bool b, string message)
        {
            if (!b)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsFalse(bool b)
        {
            if (b)
            {
                throw new ArgumentException();
            }
        }

        public static void IsFalse(bool b, string message)
        {
            if (b)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsPresent<T>(Optional<T> opt) where T : class
        {
            if (!opt.HasValue)
            {
                throw new OptionalEmptyException<T>();
            }
        }

        public static void IsPresent<T>(Optional<T> opt, string message) where T : class
        {
            if (!opt.HasValue)
            {
                throw new OptionalEmptyException<T>(message);
            }
        }

        public static void PacketCorrelation<T>(T packet, string expectedCorrelationId)
            where T : CorrelatedPacket
        {
            if (!expectedCorrelationId.Equals(packet.CorrelationId))
            {
                throw new UncorrelatedPacketException(packet, expectedCorrelationId);
            }
        }

        private static Optional<string> GetParameterName<TParam>()
        {
            ParameterInfo[] parametersOfMethodBeforeValidate = new StackFrame(2).GetMethod().GetParameters();
            return Optional.OfNullable(parametersOfMethodBeforeValidate.SingleOrDefault(pi => pi.ParameterType == typeof(TParam))?.Name);
        }
    }
}
