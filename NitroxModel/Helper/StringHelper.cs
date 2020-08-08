using System;
using System.Text;

namespace NitroxModel.Helper
{
    public static class StringHelper
    {
        private static readonly Random random = new Random();

        public static string GenerateRandomString(int size)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
