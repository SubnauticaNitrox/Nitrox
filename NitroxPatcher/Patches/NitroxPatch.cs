using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches;

/// <summary>
///     Base class for declaring a patch using <see cref="Harmony" />.
///     Implement either <see cref="IDynamicPatch" /> if your patch is unloadable or <see cref="IPersistentPatch" /> if it isn't.<br />
///     Either one or more of the following patch types are possible in one <see cref="NitroxPatch"/>:<br />
///     - Prefix patch, use <see cref="PatchPrefix" /><br />
///     - Postfix patch, use <see cref="PatchPostfix" /><br />
///     - Transpiler patch, use <see cref="PatchTranspiler" /><br />
///     - Finalizer patch, use <see cref="PatchFinalizer" /><br />
///     <p />
///     Documentation for patching can be found under https://harmony.pardeike.net/articles/patching.html and https://github.com/BepInEx/HarmonyX/wiki/Difference-between-Harmony-and-HarmonyX
/// </summary>
public abstract class NitroxPatch : INitroxPatch
{
    /// <summary>
    ///     List of patches defined by the class that inherits.
    /// </summary>
    private readonly List<MethodBase> activePatches = new();

    public abstract void Patch(Harmony harmony);

    /// <summary>
    ///     Removes the (previously applied) patches from the process.
    /// </summary>
    /// <param name="harmony"></param>
    public void Restore(Harmony harmony)
    {
        foreach (MethodBase targetMethod in activePatches)
        {
            harmony.Unpatch(targetMethod, HarmonyPatchType.All, harmony.Id);
        }
    }

    protected void PatchFinalizer(Harmony harmony, MethodBase targetMethod, MethodInfo finalizerMethod)
    {
        PatchMultiple(harmony, targetMethod, null, null, null, finalizerMethod);
    }

    protected void PatchTranspiler(Harmony harmony, MethodBase targetMethod, MethodInfo transpilerMethod)
    {
        PatchMultiple(harmony, targetMethod, null, null, transpilerMethod);
    }

    protected void PatchPrefix(Harmony harmony, MethodBase targetMethod, MethodInfo prefixMethod)
    {
        PatchMultiple(harmony, targetMethod, prefixMethod);
    }

    protected void PatchPostfix(Harmony harmony, MethodBase targetMethod, MethodInfo postfixMethod)
    {
        PatchMultiple(harmony, targetMethod, null, postfixMethod);
    }

    protected void PatchMultiple(
        Harmony harmony,
        MethodBase targetMethod,
        MethodInfo prefix = null,
        MethodInfo postfix = null,
        MethodInfo transpiler = null,
        MethodInfo finalizer = null,
        MethodInfo manipulator = null)
    {
        static HarmonyMethod AsHarmonyMethod(MethodInfo methodInfo) => methodInfo != null ? new HarmonyMethod(methodInfo) : null;

        Validate.NotNull(targetMethod, "Target method cannot be null");

        harmony.Patch(targetMethod, AsHarmonyMethod(prefix), AsHarmonyMethod(postfix), AsHarmonyMethod(transpiler), AsHarmonyMethod(finalizer), AsHarmonyMethod(manipulator));
        activePatches.Add(targetMethod); // Store our patched methods
    }

    /// <summary>
    ///     Resolves a type using <see cref="NitroxServiceLocator.LocateService{T}" />. If the result is not null it will cache and return the same type on future calls.
    /// </summary>
    /// <typeparam name="T">Type to get and cache from <see cref="NitroxServiceLocator" /></typeparam>
    /// <returns>The requested type or null if not available.</returns>
    protected static T Resolve<T>(bool prelifeTime = false) where T : class
    {
        return prelifeTime ? NitroxServiceLocator.Cache<T>.ValuePreLifetime : NitroxServiceLocator.Cache<T>.Value;
    }
}
