using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace NitroxModel;

public static class Extensions
{
    public static string GetSavesFolderDir(this IKeyValueStore store)
    {
        if (store == null)
        {
            return Path.Combine(NitroxUser.AppDataPath, "saves");
        }
        return store.GetValue("SavesFolderDir", Path.Combine(NitroxUser.AppDataPath, "saves"));
    }

    public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);

        return type.GetField(name)
                   .GetCustomAttributes(false)
                   .OfType<TAttribute>()
                   .SingleOrDefault();
    }

    /// <summary>
    ///     Gets only the unique flags of the given enum value that aren't part of a different flag in the same enum type,
    ///     excluding the 0 flag.
    /// </summary>
    public static IEnumerable<T> GetUniqueNonCombinatoryFlags<T>(this T flags) where T : Enum
    {
        ulong flagCursor = 1;
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if (!flags.HasFlag(value))
            {
                continue;
            }

            ulong definedFlagBits = Convert.ToUInt64(value);
            while (flagCursor < definedFlagBits)
            {
                flagCursor <<= 1;
            }

            if (flagCursor == definedFlagBits && value.HasFlag(value))
            {
                yield return value;
            }
        }
    }

    /// <inheritdoc cref="Enum.IsDefined" />
    public static bool IsDefined<TEnum>(this TEnum value) where TEnum : Enum => Enum.IsDefined(typeof(TEnum), value);

    /// <summary>
    ///     Removes all items from the list where the predicate returns true.
    /// </summary>
    /// <param name="list">The list to remove items from.</param>
    /// <param name="extraParameter">An extra parameter to supply to the predicate.</param>
    /// <param name="predicate">The predicate that tests each item in the list for removal.</param>
    public static void RemoveAllFast<TItem, TParameter>(this IList<TItem> list, TParameter extraParameter, Func<TItem, TParameter, bool> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            TItem item = list[i];
            if (predicate.Invoke(item, extraParameter))
            {
                // Optimization for Unity mono: swap item to end and remove it. This reduces GC pressure for resizing arrays.
                list[i] = list[^1];
                list.RemoveAt(list.Count - 1);
            }
        }
    }

    public static int GetIndex<T>(this T[] list, T itemToFind) => Array.IndexOf(list, itemToFind);

    public static string AsByteUnitText(this uint byteSize)
    {
        // Uint can't go past 4GiB, so we don't need to worry about overflow.
        string[] suf = { "B", "KiB", "MiB", "GiB" };
        if (byteSize == 0)
        {
            return $"0{suf[0]}";
        }
        int place = Convert.ToInt32(Math.Floor(Math.Log(byteSize, 1024)));
        double num = Math.Round(byteSize / Math.Pow(1024, place), 1);
        return num + suf[place];
    }

    /// <summary>
    ///     Calls an action if an error happens.
    /// </summary>
    /// <remarks>
    ///     Use this for fire-and-forget tasks so that errors aren't hidden when they happen.
    /// </remarks>
    public static Task ContinueWithHandleError(this Task task, Action<Exception> onError) =>
        task.ContinueWith(t =>
        {
            if (t is not { IsFaulted: true, Exception: { } ex })
            {
                return;
            }

            onError(ex);
        });

    /// <summary>
    ///    Logs any exception/error of the task.
    /// </summary>
    /// <remarks>
    ///    <inheritdoc cref="ContinueWithHandleError(System.Threading.Tasks.Task,System.Action{System.Exception})"/>
    /// </remarks>
    public static Task ContinueWithHandleError(this Task task, bool alsoLogIngame = false) => task.ContinueWithHandleError(ex =>
    {
        Log.Error(ex);
        if (alsoLogIngame)
        {
            Log.InGame(ex.Message);
        }
    });

    public static string GetFirstNonAggregateMessage(this Exception exception) => exception switch
    {
        AggregateException ex => ex.InnerExceptions.FirstOrDefault(e => e is not AggregateException)?.Message ?? ex.Message,
        _ => exception.Message
    };

    /// <returns>
    ///     <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" /><br />
    ///     <see langword="true" /> if both IEnumerables are null.
    /// </returns>
    /// <remarks>
    ///     <see cref="ArgumentNullException" /> can't be thrown because of <paramref name="first" /> or
    ///     <paramref name="second" /> being null.
    /// </remarks>
    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
    public static bool SequenceEqualOrBothNull<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
        if (first != null && second != null)
        {
            return first.SequenceEqual(second);
        }
        return first == second;
    }

    public static void RemoveWhere<TKey, TValue, TParameter>(this IDictionary<TKey, TValue> dictionary, TParameter extraParameter, Func<TValue, TParameter, bool> predicate)
    {
        int toRemoveIndex = 0;
        TKey[] toRemove = ArrayPool<TKey>.Shared.Rent(dictionary.Count);
        try
        {
            foreach (KeyValuePair<TKey, TValue> item in dictionary)
            {
                if (predicate.Invoke(item.Value, extraParameter))
                {
                    toRemove[toRemoveIndex++] = item.Key;
                }
            }
            for (int i = 0; i < toRemoveIndex; i++)
            {
                dictionary.Remove(toRemove[i]);
            }
        }
        finally
        {
            ArrayPool<TKey>.Shared.Return(toRemove, true);
        }
    }

    public static byte[] AsMd5Hash(this string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        return md5.ComputeHash(inputBytes);
    }

    /// <summary>
    ///     Reads the exact amount of bytes from the stream.
    /// </summary>
    public static byte[] ReadStreamExactly(this Stream stream, byte[] buffer, int count)
    {
        int start;
        int num;
        for (start = 0; start < count; start += num)
        {
            num = stream.Read(buffer, start, count);
            if (num == 0)
            {
                throw new EndOfStreamException();
            }
        }
        return buffer;
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

    public static bool IsHardcore(this SubnauticaServerConfig config) => config.GameMode == NitroxGameMode.HARDCORE;
    public static bool IsPasswordRequired(this SubnauticaServerConfig config) => config.ServerPassword != "";
}
