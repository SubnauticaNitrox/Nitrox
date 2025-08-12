using System;
using System.IO;
using System.Threading.Tasks;

namespace Nitrox.Launcher.Models.Utils;

public class CacheFile
{
    private DateTimeOffset? creationTime;
    public string FileName { get; init; }
    public string TempFilePath => Path.Combine(Path.GetTempPath(), FileName);

    public DateTimeOffset? CreationTime
    {
        get
        {
            if (creationTime == null && File.Exists(TempFilePath))
            {
                using FileStream stream = File.OpenRead(TempFilePath);
                if (stream.Length < 8)
                {
                    return null;
                }
                Span<byte> buffer = stackalloc byte[8];
                stream.ReadExactly(buffer);
                creationTime = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(buffer));
            }
            return creationTime;
        }
    }

    public CacheFile(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        FileName = $"nitrox_{fileName.Trim()}.cache";
    }

    /// <summary>
    ///     Gets the cached data if not old or refreshes the cache using the <see cref="refreshedValueFactory"/>.
    /// </summary>
    public static async Task<T> GetOrRefreshAsync<T>(string name, Func<ValueReader, T> reader, Action<BinaryWriter, T> writer, Func<Task<T>> refreshedValueFactory = null, TimeSpan age = default)
    {
        if (age == default)
        {
            age = TimeSpan.FromDays(1);
        }

        CacheFile file = new(name);
        if (writer != null && (file.CreationTime == null || DateTimeOffset.UtcNow - file.CreationTime >= age))
        {
            await using BinaryWriter binaryWriter = new(File.Create(file.TempFilePath));
            binaryWriter.Write(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            T newValue = refreshedValueFactory == null ? default : await refreshedValueFactory();
            writer(binaryWriter, newValue);
            return newValue;
        }

        using ValueReader valueReader = new(file.GetStream());
        T readerResult = reader(valueReader);
        if (valueReader.ReachedEarlyEnd)
        {
            return refreshedValueFactory == null ? default : await refreshedValueFactory();
        }
        return readerResult;
    }

    private BinaryReader GetStream()
    {
        BinaryReader reader = new(File.OpenRead(TempFilePath));
        reader.ReadInt64(); // file creation in unix time
        return reader;
    }

    public class ValueReader : IDisposable
    {
        private readonly BinaryReader binaryReader;
        public bool ReachedEarlyEnd { get; private set; }

        public ValueReader(BinaryReader binaryReader)
        {
            this.binaryReader = binaryReader;
        }

        public T Read<T>(T defaultValue = default)
        {
            static T InnerRead<T2>(ValueReader reader, Func<BinaryReader, T2> read, T defaultValue = default)
            {
                try
                {
                    return (T)(object)read(reader.binaryReader);
                }
                catch (EndOfStreamException)
                {
                    reader.ReachedEarlyEnd = true;
                    return defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }

            // Return default values for future reads when end of file (EOF).
            if (ReachedEarlyEnd)
            {
                return defaultValue;
            }

            Type requestedType = typeof(T);
            if (requestedType == typeof(int))
            {
                return InnerRead(this, reader => reader.ReadInt32(), defaultValue);
            }
            if (requestedType == typeof(string))
            {
                return InnerRead(this, reader => reader.ReadString(), defaultValue);
            }
            if (requestedType == typeof(byte[]))
            {
                return InnerRead(this, reader =>
                {
                    int dataSize = reader.ReadInt32();
                    return reader.ReadBytes(dataSize);
                }, defaultValue);
            }
            throw new NotSupportedException($"Type: '{requestedType}' is not yet supported to be read from cache files");
        }

        public void Dispose() => binaryReader?.Dispose();
    }
}
