using System;

namespace NitroxModel.Helper
{
    public class Validate
    {
        public static void NotNull(Object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException();
            }
        }

        public static void NotNull(Object o, String message)
        {
            if (o == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        public static void Istrue(bool b)
        {
            if (!b)
            {
                throw new ArgumentException();
            }
        }

        public static void Istrue(bool b, String message)
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

        public static void IsFalse(bool b, String message)
        {
            if (b)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
