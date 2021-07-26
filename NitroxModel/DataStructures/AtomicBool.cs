using System.Threading;

namespace NitroxModel.DataStructures
{
    public sealed class AtomicBool
    {
        private int backingValue;

        public bool Value
        {
            get => Interlocked.CompareExchange(ref backingValue, 1, 1) == 1;
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(ref backingValue, 1, 0);
                }
                else
                {
                    Interlocked.CompareExchange(ref backingValue, 0, 1);
                }
            }
        }

        public static implicit operator bool(AtomicBool b)
        {
            return b.Value;
        }

        /// <summary>
        ///     Sets the bool to true if it's false.
        /// </summary>
        /// <returns>Returns true if the bool was just set to true and wasn't already true.</returns>
        public bool GetAndSet()
        {
            return Interlocked.CompareExchange(ref backingValue, 1, 0) == 0;
        }
    }
}
