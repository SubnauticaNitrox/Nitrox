using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Player_SetCurrentEscapePod_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Property((Player p) => p.currentEscapePod).GetSetMethod();

    public static void Prefix(EscapePod value)
    {
        Resolve<LocalPlayer>().BroadcastEscapePodChange(value.GetId());
    }
}
