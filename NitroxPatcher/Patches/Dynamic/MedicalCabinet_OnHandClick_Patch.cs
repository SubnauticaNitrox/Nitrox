using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MedicalCabinet_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((MedicalCabinet t) => t.OnHandClick(default(GUIHand)));

    public static void Postfix(MedicalCabinet __instance)
    {
        Resolve<MedkitFabricator>().Clicked(__instance);
    }
}
