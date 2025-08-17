using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NitroxModel.Extensions;

public static class StringExtensions
{
    public static byte[] AsMd5Hash(this string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        return md5.ComputeHash(inputBytes);
    }

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
}
