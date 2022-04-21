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

    public HarmonyMethod GetHarmonyMethod(string methodName)
    {
        MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        Validate.NotNull(method, $"Patcher: Patch method \"{methodName}\" cannot be found");
        return new HarmonyMethod(method);
    }

    /// <summary>
    ///     Resolves a type using <see cref="NitroxServiceLocator.LocateService{T}" />. If the result is not null it will cache and return the same type on future calls.
    /// </summary>
    /// <typeparam name="T">Type to get and cache from <see cref="NitroxServiceLocator" /></typeparam>
    /// <returns>The requested type or null if not available.</returns>
    protected static T Resolve<T>(bool prelifeTime = false) where T : class
    {
        return prelifeTime ? NitroxServiceLocator.Cache<T>.ValuePrelifetime : NitroxServiceLocator.Cache<T>.Value;
    }

    protected void PatchFinalizer(Harmony harmony, MethodBase targetMethod, string finalizerMethod = "Finalizer")
    {
        PatchMultiple(harmony, targetMethod, null, null, null, finalizerMethod);
    }

    protected void PatchTranspiler(Harmony harmony, MethodBase targetMethod, string transpilerMethod = "Transpiler")
    {
        PatchMultiple(harmony, targetMethod, null, null, transpilerMethod);
    }

    protected void PatchPrefix(Harmony harmony, MethodBase targetMethod, string prefixMethod = "Prefix")
    {
        PatchMultiple(harmony, targetMethod, prefixMethod);
    }

    protected void PatchPostfix(Harmony harmony, MethodBase targetMethod, string postfixMethod = "Postfix")
    {
        PatchMultiple(harmony, targetMethod, null, postfixMethod);
    }

    protected void PatchMultiple(Harmony harmony, MethodBase targetMethod, bool prefix = false, bool postfix = false, bool transpiler = false, bool finalizer = false, bool iLManipulator = false)
    {
        string prefixMethod = prefix ? "Prefix" : null;
        string postfixMethod = postfix ? "Postfix" : null;
        string transpilerMethod = transpiler ? "Transpiler" : null;
        string finalizerMethod = finalizer ? "Finalizer" : null;
        string iLManipulatorMethod = iLManipulator ? "ILManipulator" : null;

        PatchMultiple(harmony, targetMethod, prefixMethod, postfixMethod, transpilerMethod, finalizerMethod, iLManipulatorMethod);
    }

    protected void PatchMultiple(
        Harmony harmony,
        MethodBase targetMethod,
        string prefixMethod = null,
        string postfixMethod = null,
        string transpilerMethod = null,
        string finalizerMethod = null,
        string iLManipulatorMethod = null)
    {
        Validate.NotNull(targetMethod, "Target method cannot be null");

        HarmonyMethod harmonyPrefixMethod = prefixMethod != null ? GetHarmonyMethod(prefixMethod) : null;
        HarmonyMethod harmonyPostfixMethod = postfixMethod != null ? GetHarmonyMethod(postfixMethod) : null;
        HarmonyMethod harmonyTranspilerMethod = transpilerMethod != null ? GetHarmonyMethod(transpilerMethod) : null;
        HarmonyMethod harmonyFinalizerMethod = finalizerMethod != null ? GetHarmonyMethod(finalizerMethod) : null;
        HarmonyMethod harmonyILManipulatorMethod = iLManipulatorMethod != null ? GetHarmonyMethod(iLManipulatorMethod) : null;

        harmony.Patch(targetMethod, harmonyPrefixMethod, harmonyPostfixMethod, harmonyTranspilerMethod, harmonyFinalizerMethod, harmonyILManipulatorMethod);
        activePatches.Add(targetMethod); // Store our patched methods
    }
}
