using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Helper
{
    public class Validate
    {
        public static void NotNull<T>(T o)
            // Prevent non-nullable valuetypes from getting boxed to object.
            // In other words: Error when trying to assert non-null on something that can't be null in the first place.
            where T : class
        {
            if (o == null)
            {
                throw new ArgumentNullException();
            }
        }

        public static void NotNull<T>(T o, string message)
            where T : class
        {
            if (o == null)
            {
                throw new ArgumentNullException(message);
            }
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

        public static void IsPresent<T>(Optional<T> opt)
        {
            if (opt.IsEmpty())
            {
                throw new OptionalEmptyException<T>();
            }
        }

        public static void IsPresent<T>(Optional<T> opt, string message)
        {
            if (opt.IsEmpty())
            {
                throw new OptionalEmptyException<T>(message);
            }
        }
    }
}
