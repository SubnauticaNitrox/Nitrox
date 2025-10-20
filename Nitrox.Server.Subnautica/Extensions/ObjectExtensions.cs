using Nitrox.Server.Subnautica.Models.Logging.Redaction;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ObjectExtensions
{
    public static SensitiveData<T> ToSensitive<T>(this T value) => new(value);
}
