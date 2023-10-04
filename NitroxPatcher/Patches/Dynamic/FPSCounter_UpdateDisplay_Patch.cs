#if DEBUG
using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FPSCounter_UpdateDisplay_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((FPSCounter t) => t.UpdateDisplay());

    public static void Postfix(FPSCounter __instance)
    {
        if (!Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        __instance.strBuffer.AppendLine($"Loading entities: {Resolve<Entities>().EntitiesToSpawn.Count}");
        __instance.text.SetText(__instance.strBuffer);
    }
}
#endif