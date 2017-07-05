using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.DataStructures.Util
{
    public class Optional<T>
    {
        private T value;

        private Optional()
        {

        }

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
            if(value == null)
            {
                throw new InvalidOperationException("Value cannot be null");
            }

            return new Optional<T>(value);
        }

        public static Optional<T> OfNullable(T value)
        {
            return new Optional<T>(value);
        }
        
        public T Get()
        {
            if(value == null)
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
            if(value != null)
            {
                return value;
            }

            return elseValue;
        }
    }
}
