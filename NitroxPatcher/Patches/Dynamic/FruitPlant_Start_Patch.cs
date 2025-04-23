using System.Reflection;
using NitroxClient.Helpers;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Gives its own reference (<see cref="FruitPlant"/>) to its children <see cref="PickPrefab"/>s
/// </summary>
public sealed partial class FruitPlant_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FruitPlant t) => t.Start());

    public static void Prefix(FruitPlant __instance)
    {
        foreach (PickPrefab pickPrefab in __instance.fruits)
        {
            pickPrefab.AddReference(__instance);
        }
    }
}
