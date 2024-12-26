using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_StopIntroCinematic_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((EscapePod e) => e.StopIntroCinematic(default(bool)));

    public static void Postfix(bool isInterrupted)
    {
        if (!isInterrupted) return;

        Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
    }
}
