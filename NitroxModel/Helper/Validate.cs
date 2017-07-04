using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Helper
{
    public class Validate
    {
        public static void NotNull(Object o)
        {
            if(o == null)
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
    }
}
