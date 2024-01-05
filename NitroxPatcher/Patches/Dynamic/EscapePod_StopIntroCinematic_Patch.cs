using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_StopIntroCinematic_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((EscapePod e) => e.StopIntroCinematic(default(bool)));

    public static void Postfix(bool isInterrupted)
    {
        if (!isInterrupted) return;

        // This patch should fire before Multiplayer is loaded. To prevent sending as an unauthenticated packet we delay it.
        if (Multiplayer.Active)
        {
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
        }
        else
        {
            Multiplayer.OnLoadingComplete += () => Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
        }
    }
}
