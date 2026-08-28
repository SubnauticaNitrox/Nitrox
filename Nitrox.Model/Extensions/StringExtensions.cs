using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Nitrox.Model.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Gets the arguments passed to a command, given its name.
    /// </summary>
    /// <param name="args">All arguments passed to the program.</param>
    /// <param name="name">Name of the command, include the - or -- prefix.</param>
    /// <returns>All arguments passed to the given command name or empty if not found or no arguments passed.</returns>
    public static IEnumerable<string> GetCommandArgs(this string[] args, string name)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith(name))
            {
                continue;
            }
            if (arg.Length > name.Length && arg[name.Length] == '=')
            {
                yield return arg.Substring(name.Length + 1);
                continue;
            }
            for (i += 1; i < args.Length; i++)
            {
                arg = args[i];
                if (arg.StartsWith("-"))
                {
                    break;
                }
                yield return arg;
            }
        }
    }

    extension(string self)
    {
        public byte[] ToMd5Hash()
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(self);
            return md5.ComputeHash(inputBytes);
        }

        public int ToMd5HashedInt32() => BitConverter.ToInt32(self.ToMd5Hash(), 0);

        /// <summary>
        ///     Returns with title-case words and optionally omitted digits.
        /// </summary>
        public string ToSpacedTitleCase(bool keepDigits = true)
        {
            char[] chars = self.Trim().ToCharArray();
            for (int i = 1; i < chars.Length; i++)
            {
                ref char previous = ref chars[i - 1];
                ref char current = ref chars[i];

                if (i == 1 && char.IsLetter(previous))
                {
                    previous = char.ToUpperInvariant(previous);
                    continue;
                }
                if (char.IsWhiteSpace(previous) || !char.IsLetter(previous))
                {
                    if (char.IsLetter(current))
                    {
                        current = char.ToUpperInvariant(current);
                    }
                }
                if (!char.IsLetter(current) && (!keepDigits || !char.IsDigit(current)))
                {
                    current = ' ';
                }
                if (char.IsWhiteSpace(previous) && char.IsWhiteSpace(current))
                {
                    previous = '\0';
                }
            }
            return Regex.Replace(new string(chars), "(?!^)([A-Z])", " $1");;
        }
    }
}
