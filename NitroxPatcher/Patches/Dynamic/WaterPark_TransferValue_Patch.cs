using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Transfers the NitroxEntity to the new main module when two WaterParks are merged.
/// </summary>
public class WaterPark_TransferValue_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => WaterPark.TransferValue(default, default));

    public static void Prefix(WaterPark srcWaterPark, WaterPark dstWaterPark)
    {
        if (NitroxEntity.TryGetEntityFrom(srcWaterPark.gameObject, out NitroxEntity topEntity))
        {
            NitroxEntity.SetNewId(dstWaterPark.gameObject, topEntity.Id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
