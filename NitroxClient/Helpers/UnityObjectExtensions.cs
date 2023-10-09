using NitroxModel.Core;

namespace NitroxClient.Helpers;

public static class UnityObjectExtensions
{
    /// <summary>
    ///     Resolves a type using <see cref="NitroxServiceLocator.LocateService{T}" />. If the result is not null it will cache and return the same type on future calls.
    /// </summary>
    /// <remarks>
    ///     Dependency Injection should be limited to UnityEngine object types as in other cases it should be injected as constructor parameter.
    ///     This is the reason for having UnityEngine.Object as first parameter.
    /// </remarks>
    /// <typeparam name="T">Type to get and cache from <see cref="NitroxServiceLocator" /></typeparam>
    /// <returns>The requested type or null if not available.</returns>
    public static T Resolve<T>(this UnityEngine.Object _, bool prelifeTime = false) where T : class
    {
        return prelifeTime ? NitroxServiceLocator.Cache<T>.ValuePreLifetime : NitroxServiceLocator.Cache<T>.Value;
    }
}
